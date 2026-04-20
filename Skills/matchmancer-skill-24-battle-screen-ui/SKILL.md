---
name: "Matchmancer Skill 24: Battle Screen UI"
description: "Build the battle HUD: EnemyUIController (HP bar, armor bar, intent indicator, turn counter, status icons), player HUD (HP, shield, ultimate meter, moves left), booster bar, combo banner. Subscribes to events from EnemyController, CharacterRuntime, UltimateSystem, and CombatResolver. Load this skill when the user asks about battle UI, HP bar, enemy HP, ultimate meter, status icons, intent indicator, or in-battle HUD. Requires Skills 11\u201320."
---

# Skill 24: Battle Screen UI

## Objective
The visible battle: bind every runtime value from combat to a UI element with smooth tweens. Everything is event-driven — no per-frame polling of the runtime.

After this skill:
- Enemy portrait + HP bar + armor bar + turn counter + status icons update live.
- Player HP, shield, ultimate meter, moves remaining update live.
- Booster bar shows counts, greys out at 0.
- Combo banner appears on chains ≥3.
- Damage numbers float off tiles (hooks Skill 07 VFX).

## Prerequisites
- Skill 13 — EnemyController events
- Skill 14 — CharacterRuntime + UltimateSystem
- Skill 16 — InventoryManager booster counts
- Skill 07 — floating popup helpers

---

## Step 1: BarAnimator utility

**File:** `Assets/Scripts/UI/BarAnimator.cs`

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Matchmancer.UI
{
    [RequireComponent(typeof(Image))]
    public class BarAnimator : MonoBehaviour
    {
        [SerializeField] private float duration = 0.35f;
        private Image img;
        private Coroutine routine;

        private void Awake() => img = GetComponent<Image>();

        public void Set(float target01, bool instant = false)
        {
            target01 = Mathf.Clamp01(target01);
            if (instant) { img.fillAmount = target01; return; }
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(Tween(target01));
        }

        private IEnumerator Tween(float target)
        {
            float start = img.fillAmount;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / duration);
                img.fillAmount = Mathf.Lerp(start, target, k);
                yield return null;
            }
            img.fillAmount = target;
        }
    }
}
```

---

## Step 2: EnemyUIController

**File:** `Assets/Scripts/UI/EnemyUIController.cs`

```csharp
using Matchmancer.Combat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Matchmancer.UI
{
    public class EnemyUIController : MonoBehaviour
    {
        [SerializeField] private EnemyController enemy;
        [SerializeField] private Image           portrait;
        [SerializeField] private BarAnimator     hpBar, armorBar;
        [SerializeField] private TMP_Text        hpText, armorText, turnCounterText, nameText;
        [SerializeField] private Transform       statusIconRoot;
        [SerializeField] private GameObject      statusIconPrefab;

        private void OnEnable()
        {
            enemy.OnDamageTaken        += HandleDamage;
            enemy.OnArmorDamaged       += HandleArmor;
            enemy.OnStatusApplied      += HandleStatus;
            enemy.OnTurnCounterChanged += HandleTurnCounter;
            enemy.OnPoisonTick         += HandlePoison;
            enemy.OnDeath              += HandleDeath;
            Refresh(instant: true);
        }

        private void OnDisable()
        {
            enemy.OnDamageTaken        -= HandleDamage;
            enemy.OnArmorDamaged       -= HandleArmor;
            enemy.OnStatusApplied      -= HandleStatus;
            enemy.OnTurnCounterChanged -= HandleTurnCounter;
            enemy.OnPoisonTick         -= HandlePoison;
            enemy.OnDeath              -= HandleDeath;
        }

        public void Refresh(bool instant = false)
        {
            hpBar   .Set(enemy.CurrentHP    / Mathf.Max(1f, enemy.MaxHP),    instant);
            armorBar.Set(enemy.CurrentArmor / Mathf.Max(1f, enemy.MaxArmor), instant);
            hpText    .text = $"{Mathf.CeilToInt(enemy.CurrentHP)} / {Mathf.CeilToInt(enemy.MaxHP)}";
            armorText .text = enemy.MaxArmor > 0 ? $"🛡 {Mathf.CeilToInt(enemy.CurrentArmor)}" : "";
            turnCounterText.text = enemy.MovesUntilAttack > 0 ? $"{enemy.MovesUntilAttack}" : "!";
        }

        private void HandleDamage(float dmg, bool crit, float newHp) => Refresh();
        private void HandleArmor(float dmg, float remaining)        => Refresh();
        private void HandlePoison(float dmg, float newHp)           => Refresh();
        private void HandleTurnCounter(int n)                       => turnCounterText.text = n > 0 ? $"{n}" : "!";

        private void HandleStatus(StatusEffectType t)
        {
            var go = Instantiate(statusIconPrefab, statusIconRoot);
            var img = go.GetComponent<Image>();
            // Map enum -> icon in a real build; here we just label it
            go.GetComponentInChildren<TMP_Text>().text = t.ToString().Substring(0, 1);
        }

        private void HandleDeath()
        {
            portrait.color = new Color(0.3f, 0.3f, 0.3f, 0.6f);
            turnCounterText.text = "✕";
        }
    }
}
```

---

## Step 3: PlayerHUDController

**File:** `Assets/Scripts/UI/PlayerHUDController.cs`

```csharp
using Matchmancer.Character;
using TMPro;
using UnityEngine;

namespace Matchmancer.UI
{
    public class PlayerHUDController : MonoBehaviour
    {
        [SerializeField] private CharacterRuntime character;
        [SerializeField] private UltimateSystem   ultimate;
        [SerializeField] private BarAnimator      hpBar, shieldBar, ultimateBar;
        [SerializeField] private TMP_Text         hpText, movesText;

        private void OnEnable()
        {
            character.OnHealthChanged   += HandleHP;
            character.OnShieldChanged   += HandleShield;
            ultimate .OnEnergyChanged   += HandleUltimate;
            ultimate .OnUltimateReady   += HandleUltReady;
            Refresh(instant: true);
        }

        private void OnDisable()
        {
            character.OnHealthChanged   -= HandleHP;
            character.OnShieldChanged   -= HandleShield;
            ultimate .OnEnergyChanged   -= HandleUltimate;
            ultimate .OnUltimateReady   -= HandleUltReady;
        }

        public void Refresh(bool instant = false)
        {
            hpBar.Set(character.CurrentHP / Mathf.Max(1f, character.EffectiveMaxHP), instant);
            shieldBar.Set(character.CurrentShield / Mathf.Max(1f, character.MaxShield), instant);
            ultimateBar.Set(ultimate.CurrentEnergy / Mathf.Max(1f, ultimate.MaxEnergy), instant);
            hpText.text = $"{Mathf.CeilToInt(character.CurrentHP)}/{Mathf.CeilToInt(character.EffectiveMaxHP)}";
        }

        public void SetMovesRemaining(int n) => movesText.text = $"Moves: {n}";

        private void HandleHP(float newHp)       => Refresh();
        private void HandleShield(float newSh)   => Refresh();
        private void HandleUltimate(float e)     => Refresh();
        private void HandleUltReady()            { /* punch anim here */ }
    }
}
```

---

## Step 4: BoosterBarController

**File:** `Assets/Scripts/UI/BoosterBarController.cs`

```csharp
using Matchmancer.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Matchmancer.UI
{
    public class BoosterBarController : MonoBehaviour
    {
        [System.Serializable] public class Slot { public string id; public Button btn; public TMP_Text countLbl; }
        [SerializeField] private Slot[] slots;
        [SerializeField] private CharacterRuntimeRef charRef;   // small helper — expose CharacterRuntime + EnemyController + IBoardBoosterHost

        private void OnEnable()
        {
            foreach (var s in slots)
            {
                var id = s.id;
                s.btn.onClick.AddListener(() =>
                    InventoryManager.Instance.UseBooster(id, charRef.Character, charRef.Enemy, charRef.BoardHost));
            }
            InventoryManager.Instance.OnBoosterCountChanged += HandleChanged;
            RefreshAll();
        }

        private void OnDisable()
        {
            foreach (var s in slots) s.btn.onClick.RemoveAllListeners();
            InventoryManager.Instance.OnBoosterCountChanged -= HandleChanged;
        }

        private void HandleChanged(string id, int count)
        {
            foreach (var s in slots)
                if (s.id == id) { s.countLbl.text = $"{count}"; s.btn.interactable = count > 0; }
        }

        private void RefreshAll()
        {
            foreach (var s in slots)
                HandleChanged(s.id, InventoryManager.Instance.GetCount(s.id));
        }
    }

    // Plain holder MonoBehaviour — scene-wired
    public class CharacterRuntimeRef : MonoBehaviour
    {
        public Matchmancer.Character.CharacterRuntime Character;
        public Matchmancer.Combat.EnemyController     Enemy;
        public Matchmancer.Inventory.IBoardBoosterHost BoardHost;
    }
}
```

---

## Step 5: ComboBanner

**File:** `Assets/Scripts/UI/ComboBanner.cs`

```csharp
using System.Collections;
using TMPro;
using UnityEngine;

namespace Matchmancer.UI
{
    public class ComboBanner : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private float    showTime = 0.9f;

        public void ShowCombo(int comboCount)
        {
            if (comboCount < 3) return;
            label.text = $"COMBO ×{comboCount}";
            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(HideAfter());
        }

        private IEnumerator HideAfter()
        {
            yield return new WaitForSeconds(showTime);
            gameObject.SetActive(false);
        }
    }
}
```

Subscribe in battle orchestrator: `MatchResolver.OnComboCompleted += combo => comboBanner.ShowCombo(combo);`

---

## Step 6: Scene layout

```
BattleCanvas
├── TopBar
│   └── EnemyPanel (EnemyUIController)
├── MiddleBoard   (game world — board, tiles)
├── BottomBar
│   ├── PlayerHUD (PlayerHUDController)
│   ├── BoosterBar (BoosterBarController)
│   └── MovesLabel
└── Overlays
    ├── ComboBanner
    └── FloatingDamageRoot (for Skill 07 popups)
```

---

## Validation checklist

- [ ] Enemy HP bar tweens smoothly on damage
- [ ] Armor bar hidden when enemy has no armor
- [ ] Turn counter shows "!" on attack turn, number otherwise
- [ ] Status icon spawns when poison/vuln applied
- [ ] Player HP, shield, ultimate bars update live
- [ ] Booster button disables at 0 count, re-enables when restocked
- [ ] Combo banner pops on 3+ cascades, hides after ~1s
- [ ] Death state: greyed portrait, no further ticks

---

## Report

**Built:** BarAnimator, EnemyUIController, PlayerHUDController, BoosterBarController, ComboBanner.
**Files:** all under `Assets/Scripts/UI/`.
**Assumptions:** `CharacterRuntime` exposes `OnHealthChanged`, `OnShieldChanged`; `UltimateSystem` exposes `OnEnergyChanged`, `OnUltimateReady`, `CurrentEnergy`, `MaxEnergy`. `MatchResolver` emits `OnComboCompleted(int)`.
**Next:** Skill 25 — Story scenes overlay the battle/hub stack with dialogue.
