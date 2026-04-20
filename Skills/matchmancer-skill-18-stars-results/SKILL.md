---
name: "Matchmancer Skill 18: Stars & Results System"
description: "Build the completion layer: StarCriteria ScriptableObject (HP remaining, moves used, combo threshold), StarRatingSystem that evaluates per-level 3-star and 5-star results, and ResultsScreenController that animates the outcome. Load this skill when the user asks about stars, 3-star rating, 5-star, level results, victory screen, defeat screen, or completion criteria. Requires Skills 11–17."
---

# Skill 18: Stars & Results System

## Objective
After each battle, evaluate performance against per-level criteria and award 1–3 stars (base) plus up to 2 bonus "mastery" stars. Persist the highest star count per level. Present a results screen with rewards.

After this skill:
- Victory triggers `StarRatingSystem.Evaluate(level, result)` → returns stars earned.
- Highest star rating stored per-level in `SaveManager`.
- Results screen shows stars, XP gained, gold, gear drops.
- Defeat screen offers retry.

## Prerequisites
- Skill 13 — `EnemyController.OnDeath` fires on victory
- Skill 14 — `CharacterRuntime.OnDeath` fires on defeat
- Skill 17 — `LevelData` holds `StarCriteria`

---

## Step 1: BattleResult — result DTO

**File:** `Assets/Scripts/Progression/BattleResult.cs`

```csharp
namespace Matchmancer.Progression
{
    public struct BattleResult
    {
        public bool   victory;
        public int    movesUsed;
        public int    movesLimit;
        public float  hpRemainingPct;   // 0–1
        public int    maxCombo;
        public int    goldEarned;
        public int    xpEarned;
        public string levelId;
    }
}
```

---

## Step 2: StarCriteria SO

**File:** `Assets/Scripts/Progression/StarCriteria.cs`

```csharp
using UnityEngine;

namespace Matchmancer.Progression
{
    [CreateAssetMenu(fileName = "StarCriteria_", menuName = "Matchmancer/Star Criteria")]
    public class StarCriteria : ScriptableObject
    {
        [Header("Base 3-star thresholds")]
        [Tooltip("Star 1 is awarded for victory. Stars 2 & 3 require these:")]
        public float star2_minHpPct = 0.33f;   // must finish with ≥ 33% HP
        public float star3_minHpPct = 0.66f;   // must finish with ≥ 66% HP

        [Header("Bonus mastery stars (up to 2)")]
        public int   bonus_maxMoves = 0;       // 0 = disabled
        public int   bonus_minCombo = 0;       // 0 = disabled
    }
}
```

---

## Step 3: StarRatingSystem

**File:** `Assets/Scripts/Progression/StarRatingSystem.cs`

```csharp
using UnityEngine;

namespace Matchmancer.Progression
{
    public static class StarRatingSystem
    {
        /// <summary>Returns total stars earned (0–5). 0 if defeat.</summary>
        public static int Evaluate(BattleResult result, StarCriteria criteria)
        {
            if (!result.victory) return 0;
            int stars = 1; // victory
            if (result.hpRemainingPct >= criteria.star2_minHpPct) stars++;
            if (result.hpRemainingPct >= criteria.star3_minHpPct) stars++;

            // Bonus mastery
            if (criteria.bonus_maxMoves > 0 && result.movesUsed <= criteria.bonus_maxMoves) stars++;
            if (criteria.bonus_minCombo > 0 && result.maxCombo  >= criteria.bonus_minCombo) stars++;

            return Mathf.Clamp(stars, 0, 5);
        }
    }
}
```

---

## Step 4: Persist best stars

Add to `MatchmancerSaveData`:

```csharp
[Serializable]
public class LevelRecord
{
    public string levelId;
    public int    bestStars;
    public bool   fiveStarCleared;
}
public List<LevelRecord> levelRecords = new List<LevelRecord>();
```

`LevelProgressionManager.RecordResult(result, stars)` finds-or-creates the entry and takes the max of old vs new.

---

## Step 5: ResultsScreenController

**File:** `Assets/Scripts/UI/ResultsScreenController.cs`

```csharp
using Matchmancer.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Matchmancer.UI
{
    public class ResultsScreenController : MonoBehaviour
    {
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject defeatPanel;
        [SerializeField] private Image[]    starIcons;       // 5 icons
        [SerializeField] private Sprite     starFilled, starEmpty;
        [SerializeField] private TMP_Text   xpLabel, goldLabel, comboLabel;
        [SerializeField] private Button     continueButton, retryButton;

        public void Show(BattleResult r, int stars)
        {
            victoryPanel.SetActive(r.victory);
            defeatPanel .SetActive(!r.victory);

            for (int i = 0; i < starIcons.Length; i++)
                starIcons[i].sprite = i < stars ? starFilled : starEmpty;

            xpLabel   .text = $"XP +{r.xpEarned}";
            goldLabel .text = $"Gold +{r.goldEarned}";
            comboLabel.text = $"Max Combo ×{r.maxCombo}";
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
```

---

## Step 6: Wiring

Battle scene orchestrator subscribes to `EnemyController.OnDeath` and `CharacterRuntime.OnDeath`:

```csharp
void HandleVictory()
{
    var result = battleTracker.BuildResult(victory: true);
    int stars  = StarRatingSystem.Evaluate(result, currentLevel.starCriteria);
    progression.RecordResult(result, stars);
    resultsScreen.Show(result, stars);
}
```

`battleTracker` is a tiny MonoBehaviour that watches move count and max combo during play.

---

## Validation checklist

- [ ] Scripts compile
- [ ] Perfect run (100% HP, under move limit, high combo) → 5 stars
- [ ] Win with 10% HP → 1 star
- [ ] Defeat → 0 stars, defeat panel shows
- [ ] `levelRecords` stores max stars, never regresses
- [ ] Retry button reloads the battle scene
- [ ] Continue button returns to level select (Skill 21)

---

## Report

**Built:** BattleResult DTO, StarCriteria SO, StarRatingSystem pure class, LevelRecord persistence, ResultsScreenController.
**Files:** `BattleResult.cs`, `StarCriteria.cs`, `StarRatingSystem.cs` under `Progression/`; `ResultsScreenController.cs` under `UI/`.
**Modified:** `MatchmancerSaveData.levelRecords`, `LevelProgressionManager.RecordResult`.
**Assumptions:** Battle orchestrator exists and can build a `BattleResult`. Rewards amounts come from `LevelData` or enemy drops.
**Next:** Skill 19 — achievements & titles hook `RecordResult` to track cumulative wins/perfects/combos.
