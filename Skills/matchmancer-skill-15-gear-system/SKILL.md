---
name: "Matchmancer Skill 15: Gear System"
description: "Build the gear layer: GearData ScriptableObject, GearSlot enum (Weapon/Armor/Trinket/Sigil), GearInstance runtime rolled affixes, and GearSystem manager that equips gear onto CharacterRuntime and injects stat bonuses into the combat pipeline. Load this skill when the user asks about gear, equipment, weapon, armor, trinket, sigil, stat bonuses from items, equip flow, or item drops. Requires Skills 11–14. Build before Skill 16 (inventory) — gear persistence flows through the same save layer."
---

# Skill 15: Gear System

## Objective
Add a gear/equipment layer on top of `CharacterRuntime`. Four slots (Weapon, Armor, Trinket, Sigil), each holding one `GearInstance`. Equipping a piece adds its stat bonuses to the character's effective stats that feed into `CombatFormula` and `CharacterRuntime`. Unequipping removes them. All bonuses recalculate atomically — no stale snapshots.

After this skill:
- Character has four gear slots that can hold one item each.
- Equipping gear immediately boosts attack, HP, defense, luck, or crit.
- Unequipping reverts stats cleanly.
- Rolled affixes give designers a stat-budget knob per rarity.
- UI can read `GearSystem.Equipped[slot]` for inventory screens.

## Prerequisites
- Skill 11 — `CombatConfig`, stat plumbing
- Skill 12 — `CombatResolver` reading character stats
- Skill 14 — `CharacterRuntime` exposes mutable base stats + `RecalculateStats()` hook

---

## Step 1: GearSlot & GearRarity enums

**File:** `Assets/Scripts/Character/GearSlot.cs`

```csharp
namespace Matchmancer.Character
{
    public enum GearSlot  { Weapon, Armor, Trinket, Sigil }
    public enum GearRarity { Common, Uncommon, Rare, Epic, Legendary }
}
```

---

## Step 2: GearData — ScriptableObject archetype

**File:** `Assets/Scripts/Character/GearData.cs`

```csharp
using UnityEngine;

namespace Matchmancer.Character
{
    [CreateAssetMenu(fileName = "Gear_", menuName = "Matchmancer/Gear Data")]
    public class GearData : ScriptableObject
    {
        [Header("Identity")]
        public string    gearName = "Rusted Blade";
        public Sprite    icon;
        public GearSlot  slot    = GearSlot.Weapon;
        public GearRarity rarity = GearRarity.Common;

        [Header("Flat Bonuses")]
        public float attackBonus;
        public float hpBonus;
        public float defenseBonus;

        [Header("Percent Bonuses (0–1)")]
        public float critChanceBonus;
        public float luckBonus;
        public float ultimateChargeBonus;

        [Header("Affix Budget")]
        [Tooltip("Total points the roller can distribute across random affixes.")]
        public int affixBudget = 0;

        [TextArea] public string flavourText;
    }
}
```

---

## Step 3: GearInstance — runtime rolled item

**File:** `Assets/Scripts/Character/GearInstance.cs`

A serializable struct so it saves cleanly via `JsonUtility`. Stores the archetype ID plus the rolled affix values — the archetype provides the base stats, the instance provides the variance.

```csharp
using System;

namespace Matchmancer.Character
{
    [Serializable]
    public class GearInstance
    {
        public string   dataId;           // GearData.name — lookup key
        public int      rolledLevel = 1;
        public float    rolledAttack;
        public float    rolledHP;
        public float    rolledDefense;
        public float    rolledCrit;
        public float    rolledLuck;

        [NonSerialized] public GearData cachedData;

        public float TotalAttack  => (cachedData?.attackBonus ?? 0f) + rolledAttack;
        public float TotalHP      => (cachedData?.hpBonus ?? 0f) + rolledHP;
        public float TotalDefense => (cachedData?.defenseBonus ?? 0f) + rolledDefense;
        public float TotalCrit    => (cachedData?.critChanceBonus ?? 0f) + rolledCrit;
        public float TotalLuck    => (cachedData?.luckBonus ?? 0f) + rolledLuck;
    }
}
```

---

## Step 4: GearSystem — equip manager

**File:** `Assets/Scripts/Character/GearSystem.cs`

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Matchmancer.Character
{
    public class GearSystem : MonoBehaviour
    {
        [SerializeField] private CharacterRuntime character;

        private readonly Dictionary<GearSlot, GearInstance> equipped =
            new Dictionary<GearSlot, GearInstance>();

        public IReadOnlyDictionary<GearSlot, GearInstance> Equipped => equipped;

        public event Action<GearSlot, GearInstance> OnGearChanged;

        public GearInstance GetEquipped(GearSlot slot) =>
            equipped.TryGetValue(slot, out var g) ? g : null;

        public void Equip(GearInstance gear)
        {
            if (gear?.cachedData == null) return;
            equipped[gear.cachedData.slot] = gear;
            character?.RecalculateStats();
            OnGearChanged?.Invoke(gear.cachedData.slot, gear);
            Debug.Log($"[Gear] Equipped {gear.cachedData.gearName} → {gear.cachedData.slot}");
        }

        public GearInstance Unequip(GearSlot slot)
        {
            if (!equipped.TryGetValue(slot, out var g)) return null;
            equipped.Remove(slot);
            character?.RecalculateStats();
            OnGearChanged?.Invoke(slot, null);
            return g;
        }

        /// <summary>Aggregates all equipped gear bonuses into a single stat block.</summary>
        public GearStatBlock AggregateBonuses()
        {
            var block = new GearStatBlock();
            foreach (var kv in equipped)
            {
                var g = kv.Value;
                block.attack  += g.TotalAttack;
                block.hp      += g.TotalHP;
                block.defense += g.TotalDefense;
                block.crit    += g.TotalCrit;
                block.luck    += g.TotalLuck;
            }
            return block;
        }
    }

    public struct GearStatBlock
    {
        public float attack, hp, defense, crit, luck;
    }
}
```

---

## Step 5: Hook into CharacterRuntime.RecalculateStats

In `CharacterRuntime.RecalculateStats()` (Skill 14), after applying base stats from `CharacterData`, add the gear block:

```csharp
var gearBlock = gearSystem != null ? gearSystem.AggregateBonuses() : default;
EffectiveAttack  = baseAttack  + gearBlock.attack;
EffectiveMaxHP   = baseMaxHP   + gearBlock.hp;
EffectiveDefense = baseDefense + gearBlock.defense;
EffectiveCrit    = baseCrit    + gearBlock.crit;
EffectiveLuck    = baseLuck    + gearBlock.luck;
// Clamp CurrentHP if MaxHP shrunk
CurrentHP = Mathf.Min(CurrentHP, EffectiveMaxHP);
```

Add `[SerializeField] private GearSystem gearSystem;` to `CharacterRuntime`.

---

## Step 6: Affix roller (optional helper)

**File:** `Assets/Scripts/Character/GearRoller.cs`

```csharp
using UnityEngine;

namespace Matchmancer.Character
{
    public static class GearRoller
    {
        public static GearInstance Roll(GearData data, int level = 1)
        {
            var inst = new GearInstance { dataId = data.name, rolledLevel = level, cachedData = data };
            int budget = data.affixBudget;
            // Distribute budget randomly across stat channels — 5 points = 1 attack, etc.
            while (budget-- > 0)
            {
                int pick = Random.Range(0, 5);
                switch (pick)
                {
                    case 0: inst.rolledAttack  += 1f;   break;
                    case 1: inst.rolledHP      += 5f;   break;
                    case 2: inst.rolledDefense += 0.5f; break;
                    case 3: inst.rolledCrit    += 0.01f;break;
                    case 4: inst.rolledLuck    += 0.01f;break;
                }
            }
            return inst;
        }
    }
}
```

---

## Step 7: Scene wiring

Add a `GearSystem` component on the Character GameObject. Assign `character` to the `CharacterRuntime`. Assign `gearSystem` on `CharacterRuntime`.

For testing, create `Assets/Data/Gear/Gear_RustedBlade.asset`:

| Field | Value |
|---|---|
| gearName | Rusted Blade |
| slot | Weapon |
| rarity | Common |
| attackBonus | 2 |
| affixBudget | 3 |

---

## Validation checklist

- [ ] All files compile
- [ ] Create a GearData asset via Create → Matchmancer → Gear Data
- [ ] `GearRoller.Roll(data)` returns an instance with non-zero affix rolls when budget > 0
- [ ] Calling `GearSystem.Equip(instance)` boosts `CharacterRuntime.EffectiveAttack`
- [ ] `Unequip` reverts stats exactly
- [ ] Equipping a new Weapon replaces the old one (dictionary overwrites)
- [ ] `OnGearChanged` fires on both equip and unequip paths

---

## Report

**Built:** GearSlot/GearRarity enums, GearData SO, GearInstance serializable runtime, GearSystem manager, GearRoller helper, wiring into CharacterRuntime.
**Files:** `GearSlot.cs`, `GearData.cs`, `GearInstance.cs`, `GearSystem.cs`, `GearRoller.cs` — all under `Assets/Scripts/Character/`. Plus `Gear_RustedBlade.asset` under `Assets/Data/Gear/`.
**Modified:** `CharacterRuntime.cs` — added `gearSystem` field + aggregation in `RecalculateStats`.
**Assumptions:** `CharacterRuntime` has `baseAttack`, `baseMaxHP`, `baseDefense`, `baseCrit`, `baseLuck` and exposes `Effective*` versions. `RecalculateStats()` is idempotent and safe to call during battle.
**Next:** Skill 16 — inventory for consumables + gear storage; save/load of equipped map.
