---
name: "Matchmancer Skill 13: Enemy System"
description: "Build the enemy side of combat: EnemyData ScriptableObject, StatusEffect / StatusEffectSystem (poison, vulnerability, armor break), and EnemyController runtime that consumes CombatResolver.OnEffectResolved, subtracts defense, processes armor, applies debuffs, ticks status effects, and runs the enemy's attack turn. Load this skill when the user asks about enemies, enemy HP, enemy armor, enemy turns, status effects, poison, vulnerability, armor break, or why matches aren't actually hurting the enemy. Requires Skills 11–12. Build before Skill 14 (character system) — Skill 14's CharacterRuntime receives the attack damage this controller produces."
---

# Skill 13: Enemy System

## Objective
Make matches actually hurt something. `CombatResolver` already fires `CombatEffect` events (Skill 12). This skill builds the enemy that listens to those events, subtracts its defense, absorbs hits with armor, applies debuffs, ticks poison, and attacks the player on its own turn.

After this skill:
- The enemy has HP, armor, defense, and a turn counter visible in logs.
- Damage tiles reduce HP (after defense + armor).
- Break tiles chip armor.
- Debuff tiles apply poison and vulnerability.
- Every N player moves, the enemy attacks back with basic or special damage.
- On death, `OnDeath` fires — ready for the victory flow in Skill 18.

## Prerequisites
- Skill 11 (`TileType`, `CombatRole`, `CombatConfig`) complete
- Skill 12 (`CombatResolver`, `CombatEffect`) complete and fires `OnEffectResolved`
- `CombatEffect` exposes `Role` (CombatRole), `DamageDealt`, `IsCriticalHit`, `ArmorDamageDealt`, `AppliesPoison`, `AppliesVulnerability`, `DebuffDuration`

> **Namespace note:** the reference files in `_reference/` use `Soulstream.Matchmancer.Combat`. The live project uses `Matchmancer.Combat`. Use the live-project namespace — change `namespace Soulstream.Matchmancer.Combat` → `namespace Matchmancer.Combat` when lifting the files.
>
> Also rename the field `effect.SourceRole` → `effect.Role` (the active `CombatEffect` uses `Role`).

---

## Step 1: StatusEffect — Value Type

**File:** `Assets/Scripts/Combat/StatusEffect.cs`

A plain struct plus the enum of types. Kept separate from the system so other code (UI icons, save files) can reference the type without pulling in the manager.

```csharp
namespace Matchmancer.Combat
{
    /// <summary>Types of status effects that can be applied to enemies (or the player).</summary>
    public enum StatusEffectType
    {
        Poison,         // damage-over-time each enemy turn
        Vulnerability,  // incoming damage multiplied
        ArmorBreak      // armor reduced to zero for N turns
    }

    /// <summary>One active status effect instance with remaining duration.</summary>
    public struct StatusEffect
    {
        public StatusEffectType type;
        public float            value;          // poison DPS, vuln multiplier, etc.
        public int              turnsRemaining;

        public StatusEffect(StatusEffectType type, float value, int duration)
        {
            this.type           = type;
            this.value          = value;
            this.turnsRemaining = duration;
        }

        /// <summary>Tick one turn. Returns true if the effect has expired.</summary>
        public bool Tick()
        {
            turnsRemaining--;
            return turnsRemaining <= 0;
        }
    }
}
```

---

## Step 2: StatusEffectSystem — Per-Target Manager

**File:** `Assets/Scripts/Combat/StatusEffectSystem.cs`

A pure-C# class (no MonoBehaviour). `EnemyController` owns one; `CharacterRuntime` (Skill 14) will own another. Keeping it pure makes it unit-testable without Unity.

Key design choices:
- **Refresh semantics**: re-applying a status of the same type refreshes duration/value to the *higher* of old and new — stacking poison from multiple Debuff matches doesn't stomp a longer-lasting one.
- **`TickAll` returns poison damage** — the enemy owes itself this damage on its turn. Letting the system return a number instead of applying it keeps the system decoupled from whatever owns the HP.

```csharp
using System.Collections.Generic;

namespace Matchmancer.Combat
{
    /// <summary>
    /// Manages active status effects on a single target (enemy or player).
    /// EnemyController owns one instance; CharacterRuntime (Skill 14) will own another.
    /// Call TickAll() at the start of each enemy turn to process poison etc.
    /// </summary>
    public class StatusEffectSystem
    {
        private readonly List<StatusEffect> activeEffects = new List<StatusEffect>();

        public IReadOnlyList<StatusEffect> ActiveEffects => activeEffects;

        public bool HasEffect(StatusEffectType type)
        {
            foreach (var e in activeEffects)
                if (e.type == type) return true;
            return false;
        }

        /// <summary>
        /// Apply a new status effect. If the same type is already active,
        /// refreshes duration and value to the higher of old vs new.
        /// </summary>
        public void Apply(StatusEffect effect)
        {
            for (int i = 0; i < activeEffects.Count; i++)
            {
                if (activeEffects[i].type == effect.type)
                {
                    var existing = activeEffects[i];
                    if (effect.turnsRemaining > existing.turnsRemaining)
                        existing.turnsRemaining = effect.turnsRemaining;
                    if (effect.value > existing.value)
                        existing.value = effect.value;
                    activeEffects[i] = existing;
                    return;
                }
            }
            activeEffects.Add(effect);
        }

        /// <summary>
        /// Tick all effects by one turn. Returns total poison damage dealt this tick.
        /// Removes expired effects.
        /// </summary>
        public float TickAll()
        {
            float poisonDamage = 0f;

            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                var e = activeEffects[i];

                if (e.type == StatusEffectType.Poison)
                    poisonDamage += e.value;

                if (e.Tick())
                    activeEffects.RemoveAt(i);
                else
                    activeEffects[i] = e;
            }

            return poisonDamage;
        }

        /// <summary>Returns the vulnerability damage multiplier (1.0 if not vulnerable).</summary>
        public float GetVulnerabilityMultiplier()
        {
            foreach (var e in activeEffects)
                if (e.type == StatusEffectType.Vulnerability)
                    return e.value;
            return 1f;
        }

        public void ClearAll() => activeEffects.Clear();
    }
}
```

---

## Step 3: EnemyData — ScriptableObject

**File:** `Assets/Scripts/Combat/EnemyData.cs`

The designer-facing archetype. One asset per enemy (Shadow Fiend, Hollow Hound, Vampyl Captain, etc.). `GetScaled*` helpers apply level scaling so the same asset can be reused across levels 1–100.

```csharp
using UnityEngine;

namespace Matchmancer.Combat
{
    /// <summary>
    /// ScriptableObject defining an enemy archetype.
    /// Create one asset per enemy via Create → Matchmancer → Enemy Data.
    /// EnemyController reads this at battle start to initialize runtime state.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyData_", menuName = "Matchmancer/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        public string   enemyName = "Shadow Fiend";
        public Sprite   portrait;
        public Sprite   battleSprite;

        [Header("Base Stats")]
        public float maxHP     = 100f;
        public float attack    = 8f;
        public float defense   = 2f;
        public float maxArmor  = 0f;     // 0 = no armor phase

        [Header("Turn Behavior")]
        [Tooltip("How many player moves before the enemy attacks.")]
        public int   turnInterval   = 3;
        [Tooltip("Chance (0–1) the enemy uses a special attack instead of basic.")]
        public float specialChance  = 0.2f;

        [Header("Special Attack")]
        public float specialDamage  = 15f;
        public string specialName   = "Dark Slash";

        [Header("Scaling")]
        [Tooltip("Per-level multiplier applied to HP, attack, defense, armor.")]
        public float levelScaling   = 0.08f;

        [Header("Rewards")]
        public int   xpReward       = 25;
        public int   goldReward     = 10;

        [Header("Flavour")]
        [TextArea] public string loreDescription;

        /// <summary>Returns stats scaled for a given absolute level (1–100).</summary>
        public float GetScaledHP(int level)       => maxHP    * (1f + levelScaling * (level - 1));
        public float GetScaledAttack(int level)   => attack   * (1f + levelScaling * (level - 1));
        public float GetScaledDefense(int level)  => defense  * (1f + levelScaling * (level - 1));
        public float GetScaledArmor(int level)    => maxArmor * (1f + levelScaling * (level - 1));
    }
}
```

---

## Step 4: EnemyController — The Runtime

**File:** `Assets/Scripts/Combat/EnemyController.cs`

The heart of the skill. Order of operations on incoming damage matters — this is the contract the UI and balance designers will rely on:

1. **Vulnerability multiplier** applied first (raw × vuln multiplier)
2. **Defense** subtracted next (flat)
3. **Armor** absorbs what's left before HP
4. **HP** takes the remainder

Break tiles skip steps 1–3 and go straight to armor. Debuff tiles don't deal damage; they apply status effects.

```csharp
using System;
using UnityEngine;

namespace Matchmancer.Combat
{
    /// <summary>
    /// Runtime state and logic for one enemy in battle.
    /// Consumes CombatEffect events from CombatResolver.OnEffectResolved,
    /// applies damage/armor/debuffs, and runs the enemy's attack turn.
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        #region Inspector
        [SerializeField] private EnemyData       enemyData;
        [SerializeField] private CombatResolver  combatResolver;
        [SerializeField] private CombatConfig    combatConfig;
        #endregion

        #region Runtime State
        public float CurrentHP        { get; private set; }
        public float MaxHP            { get; private set; }
        public float CurrentArmor     { get; private set; }
        public float MaxArmor         { get; private set; }
        public float Defense          { get; private set; }
        public float Attack           { get; private set; }
        public bool  IsDead           => CurrentHP <= 0f;
        public int   MovesUntilAttack { get; private set; }

        public StatusEffectSystem StatusEffects { get; private set; }
        #endregion

        #region Events
        /// <summary>Args: (damage, isCrit, newHP).</summary>
        public event Action<float, bool, float> OnDamageTaken;
        /// <summary>Args: (armorDamage, remainingArmor).</summary>
        public event Action<float, float>       OnArmorDamaged;
        /// <summary>Args: (attackDamage, isSpecial).</summary>
        public event Action<float, bool>        OnEnemyAttack;
        /// <summary>Fired when HP reaches zero.</summary>
        public event Action                     OnDeath;
        /// <summary>Args: (statusType).</summary>
        public event Action<StatusEffectType>   OnStatusApplied;
        /// <summary>Args: (poisonDamage, newHP).</summary>
        public event Action<float, float>       OnPoisonTick;
        /// <summary>Args: new MovesUntilAttack value. Fires on decrement AND reset.</summary>
        public event Action<int>                OnTurnCounterChanged;
        #endregion

        #region Init
        /// <summary>
        /// Initialize the enemy for a new battle.
        /// Call once at level start with the absolute level (1–100) for stat scaling.
        /// </summary>
        public void InitForBattle(EnemyData data, int absoluteLevel)
        {
            enemyData = data;

            MaxHP        = data.GetScaledHP(absoluteLevel);
            CurrentHP    = MaxHP;
            Attack       = data.GetScaledAttack(absoluteLevel);
            Defense      = data.GetScaledDefense(absoluteLevel);
            MaxArmor     = data.GetScaledArmor(absoluteLevel);
            CurrentArmor = MaxArmor;

            MovesUntilAttack = data.turnInterval;

            StatusEffects = new StatusEffectSystem();

            Debug.Log($"[Enemy] {data.enemyName} spawned — HP:{MaxHP:F0} ATK:{Attack:F1} " +
                      $"DEF:{Defense:F1} Armor:{MaxArmor:F0} Turns:{data.turnInterval}");
        }

        private void OnEnable()
        {
            if (combatResolver != null)
                combatResolver.OnEffectResolved += HandleEffect;
        }

        private void OnDisable()
        {
            if (combatResolver != null)
                combatResolver.OnEffectResolved -= HandleEffect;
        }
        #endregion

        #region Consume Combat Effects
        private void HandleEffect(CombatEffect effect)
        {
            if (IsDead) return;

            switch (effect.Role)
            {
                case CombatRole.Damage:
                    ApplyDamage(effect.DamageDealt, effect.IsCriticalHit);
                    break;

                case CombatRole.Break:
                    ApplyArmorDamage(effect.ArmorDamageDealt);
                    break;

                case CombatRole.Debuff:
                    if (effect.AppliesPoison && combatConfig != null)
                    {
                        StatusEffects.Apply(new StatusEffect(
                            StatusEffectType.Poison,
                            combatConfig.poisonDamagePerTurn,
                            effect.DebuffDuration));
                        OnStatusApplied?.Invoke(StatusEffectType.Poison);
                    }
                    if (effect.AppliesVulnerability && combatConfig != null)
                    {
                        StatusEffects.Apply(new StatusEffect(
                            StatusEffectType.Vulnerability,
                            combatConfig.vulnerabilityMult,
                            effect.DebuffDuration));
                        OnStatusApplied?.Invoke(StatusEffectType.Vulnerability);
                    }
                    break;

                // Energy, Defense, Luck → affect the player, not the enemy.
                // CharacterRuntime (Skill 14) will handle those.
            }
        }

        private void ApplyDamage(float rawDamage, bool isCrit)
        {
            // 1. Vulnerability multiplier
            float vulnMult = StatusEffects.GetVulnerabilityMultiplier();
            float afterVuln = rawDamage * vulnMult;

            // 2. Subtract flat defense
            float afterDef = Mathf.Max(0f, afterVuln - Defense);

            // 3. Armor absorbs what's left before HP
            if (CurrentArmor > 0f)
            {
                float absorbed = Mathf.Min(CurrentArmor, afterDef);
                CurrentArmor -= absorbed;
                afterDef     -= absorbed;
                OnArmorDamaged?.Invoke(absorbed, CurrentArmor);
            }

            // 4. HP takes the remainder
            CurrentHP = Mathf.Max(0f, CurrentHP - afterDef);

            Debug.Log($"[Enemy] Took {afterDef:F1} dmg" +
                      $"{(isCrit ? " CRIT" : "")}" +
                      $"{(vulnMult > 1f ? " VULN" : "")} " +
                      $"→ HP {CurrentHP:F0}/{MaxHP:F0}");

            OnDamageTaken?.Invoke(afterDef, isCrit, CurrentHP);

            if (IsDead)
            {
                Debug.Log($"[Enemy] {enemyData.enemyName} defeated!");
                OnDeath?.Invoke();
            }
        }

        private void ApplyArmorDamage(float breakDamage)
        {
            if (CurrentArmor <= 0f) return;

            float before = CurrentArmor;
            CurrentArmor = Mathf.Max(0f, CurrentArmor - breakDamage);

            Debug.Log($"[Enemy] Armor {before:F0} → {CurrentArmor:F0} (-{breakDamage:F1})");
            OnArmorDamaged?.Invoke(breakDamage, CurrentArmor);
        }
        #endregion

        #region Enemy Turn
        /// <summary>
        /// Call this AFTER the player finishes a move (after cascades settle).
        /// Decrements the turn counter. When it reaches 0:
        ///   1. Ticks status effects (poison damages the enemy first).
        ///   2. Rolls basic vs special attack.
        ///   3. Fires OnEnemyAttack with the damage value — CharacterRuntime
        ///      (Skill 14) subscribes and applies it to the player.
        ///   4. Resets the counter.
        /// Returns the damage the player should take this call (0 if not attack turn yet).
        /// </summary>
        public float ProcessPlayerMove()
        {
            if (IsDead) return 0f;

            MovesUntilAttack--;
            OnTurnCounterChanged?.Invoke(MovesUntilAttack);

            if (MovesUntilAttack > 0) return 0f;

            // --- Enemy attacks ---

            // 1. Tick status effects (poison damages enemy before it attacks)
            float poisonDmg = StatusEffects.TickAll();
            if (poisonDmg > 0f)
            {
                CurrentHP = Mathf.Max(0f, CurrentHP - poisonDmg);
                Debug.Log($"[Enemy] Poison tick: -{poisonDmg:F1} → HP {CurrentHP:F0}");
                OnPoisonTick?.Invoke(poisonDmg, CurrentHP);

                if (IsDead)
                {
                    Debug.Log($"[Enemy] {enemyData.enemyName} killed by poison!");
                    OnDeath?.Invoke();
                    return 0f;
                }
            }

            // 2. Decide basic vs special
            bool isSpecial = UnityEngine.Random.value < enemyData.specialChance;
            float damage   = isSpecial ? enemyData.specialDamage : Attack;

            Debug.Log($"[Enemy] {enemyData.enemyName} uses " +
                      $"{(isSpecial ? enemyData.specialName : "Basic Attack")} " +
                      $"for {damage:F1} damage!");

            OnEnemyAttack?.Invoke(damage, isSpecial);

            // 3. Reset counter
            MovesUntilAttack = enemyData.turnInterval;
            OnTurnCounterChanged?.Invoke(MovesUntilAttack);

            return damage;
        }
        #endregion
    }
}
```

---

## Step 5: Hook ProcessPlayerMove Into the Match Loop

The enemy turn counter needs to tick once per *player move*, not once per cascade wave. A cascade that clears 6 chains is still one move. The right place is the moment `MatchResolver` finishes resolving a swap — either at the tail of `ResolutionLoop()` or in the input controller after the swap settles.

**Option A — simplest, no input changes.** In `MatchResolver.ResolutionLoop()`, after the `while` loop exits (all cascades done), add:

```csharp
// After all cascades resolved — tell the enemy the player's move is over
enemyController?.ProcessPlayerMove();
```

Add the corresponding inspector field:

```csharp
[SerializeField] private EnemyController enemyController;
```

This keeps the enemy's turn advancement co-located with the combat pipeline — one place to reason about.

**Option B — cleaner.** Add an `OnPlayerMoveResolved` event to `MatchResolver` and let `EnemyController` subscribe. This decouples them. Do this if you expect more listeners (UI move counter, achievement tracker) to want the same signal. If it's only one listener, A is fine.

---

## Step 6: Update BattleBridge (or retire it)

`BattleBridge` from Skill 12 logged effects to the console. Now that `EnemyController` consumes the same events and logs them more usefully, you have two choices:

- **Retire it** — remove `BattleBridge` from the scene. The enemy's own `Debug.Log` lines cover the pipeline.
- **Keep it scoped** — leave it to log Energy / Defense / Luck (the rows the enemy ignores), so you still see the player-side half of the pipeline until Skill 14 lands.

If keeping it, narrow the switch:

```csharp
switch (e.Role)
{
    case CombatRole.Energy:  Debug.Log($"[Bridge] ENERGY +{e.EnergyGenerated:F1}"); break;
    case CombatRole.Defense: Debug.Log($"[Bridge] SHIELD +{e.ShieldGenerated:F1}"); break;
    case CombatRole.Luck:    Debug.Log($"[Bridge] LUCK crit+{e.LuckCritBonus:P1}"); break;
    // Damage / Break / Debuff are handled by EnemyController now.
}
```

---

## Step 7: Scene Setup

```
--- COMBAT --- (existing GameObject)
├── CombatStats
├── CombatResolver
├── BattleBridge (scoped or removed)
└── EnemyController        ← add here
```

**Inspector wiring on EnemyController:**

| Field | Assign |
|---|---|
| `Enemy Data`       | An `EnemyData` asset (create one at `Assets/Data/Enemies/EnemyData_ShadowFiend.asset`) |
| `Combat Resolver`  | The CombatResolver component in the scene |
| `Combat Config`    | `CombatConfig.asset` |

**If using Option A from Step 5:**

| Component | Field | Assign |
|---|---|---|
| `MatchResolver` | `Enemy Controller` | The EnemyController component |

**At level start**, something needs to call `enemyController.InitForBattle(data, level)`. For now, a simple bootstrapper works:

```csharp
// Assets/Scripts/Combat/BattleBootstrapper.cs — temporary, replaced by LevelManager in Skill 20
using UnityEngine;

namespace Matchmancer.Combat
{
    public class BattleBootstrapper : MonoBehaviour
    {
        [SerializeField] private EnemyController enemy;
        [SerializeField] private EnemyData       startingEnemy;
        [SerializeField] private int             absoluteLevel = 1;

        private void Start()
        {
            if (enemy != null && startingEnemy != null)
                enemy.InitForBattle(startingEnemy, absoluteLevel);
        }
    }
}
```

---

## Step 8: Create a Test Enemy Asset

In Unity: **Create → Matchmancer → Enemy Data**. Save as `Assets/Data/Enemies/EnemyData_ShadowFiend.asset`.

Recommended starter values for manual testing:

| Field            | Value |
|------------------|-------|
| enemyName        | Shadow Fiend |
| maxHP            | 80 |
| attack           | 6 |
| defense          | 2 |
| maxArmor         | 20 |
| turnInterval     | 3 |
| specialChance    | 0.25 |
| specialDamage    | 12 |
| specialName      | Dark Slash |
| levelScaling     | 0.08 |

These numbers let you kill the enemy in ~3–5 moves and see all four damage paths (armor hit, armor broken, HP hit, vulnerability boost) in a single playtest.

---

## Validation Checklist

- [ ] `StatusEffect.cs`, `StatusEffectSystem.cs`, `EnemyData.cs`, `EnemyController.cs` all compile — zero errors
- [ ] `EnemyData` asset creates via right-click Create menu
- [ ] `BattleBootstrapper.Start()` prints the `[Enemy] spawned` line with correct stats
- [ ] Make a Damage-tile match — console shows `[Enemy] Took X.X dmg → HP XX/80`
- [ ] Enemy armor absorbs before HP — first few Damage hits only reduce armor, then HP
- [ ] Make a Break-tile match — console shows `[Enemy] Armor XX → YY`
- [ ] Make a Debuff-tile match with ≥2 tiles — both Poison AND Vulnerability apply
- [ ] Next Damage match after vulnerability — damage is noticeably higher (`VULN` appears in log)
- [ ] Perform 3 player moves — enemy logs `[Enemy] uses Basic Attack for X.X damage!`
- [ ] Enemy turn counter resets to `turnInterval` after attacking
- [ ] Poison ticks at the start of the enemy's attack turn (HP drops before attack log)
- [ ] Kill enemy — `[Enemy] defeated!` logs and `OnDeath` event fires
- [ ] Zero NullReferenceExceptions with `enemyData` unset (guards work)

---

## Report

**What was built:**
- `StatusEffect` + `StatusEffectType` — value type + enum for all debuffs
- `StatusEffectSystem` — pure-C# manager, unit-testable, refresh-on-stack semantics
- `EnemyData` — ScriptableObject archetype with level scaling
- `EnemyController` — MonoBehaviour runtime; subscribes to `CombatResolver.OnEffectResolved`, applies damage through the vuln → defense → armor → HP pipeline, runs the enemy turn
- `BattleBootstrapper` — temporary init harness; retired when Skill 20 lands

**Files created:**
- `Assets/Scripts/Combat/StatusEffect.cs`
- `Assets/Scripts/Combat/StatusEffectSystem.cs`
- `Assets/Scripts/Combat/EnemyData.cs`
- `Assets/Scripts/Combat/EnemyController.cs`
- `Assets/Scripts/Combat/BattleBootstrapper.cs` (temporary)
- `Assets/Data/Enemies/EnemyData_ShadowFiend.asset` (designer asset)

**Files modified:**
- `Assets/Scripts/Grid/MatchResolver.cs` — one `EnemyController` field, one `ProcessPlayerMove()` call at the end of `ResolutionLoop`
- `Assets/Scripts/Combat/BattleBridge.cs` — narrowed to Energy/Defense/Luck only (or removed)

**Assumptions made:**
- `CombatConfig` exposes `poisonDamagePerTurn` and `vulnerabilityMult`. If it doesn't, add them: `public float poisonDamagePerTurn = 3f;` and `public float vulnerabilityMult = 1.5f;`.
- `CombatEffect.Role` is a `CombatRole` enum with cases `Damage`, `Energy`, `Defense`, `Debuff`, `Break`, `Luck`. Matches the live project's `CombatEffect.cs`.
- The enemy attacks *after* the full cascade resolves, not after each match. Prevents 6-chain cascades from triggering 6 enemy turns.
- `OnEnemyAttack` fires damage as raw number. Player-side mitigation (shield, armor) happens in Skill 14.

**What to test next:**
1. Playtest with the Shadow Fiend asset — confirm all four damage paths behave
2. Confirm `OnDeath` fires exactly once (not per frame while HP ≤ 0)
3. Ready for **Skill 14: Character System** — `CharacterRuntime` will subscribe to `OnEnemyAttack`, apply shield/defense, and feed real `characterAttack`/`characterLuck` into `CombatResolver.ResolveWave`
