---
name: "Matchmancer Skill 19: Achievement & Title System"
description: "Build the long-term retention layer: AchievementData ScriptableObject, AchievementTracker for cumulative stat counters (total wins, total damage, total crits, max combo, poisons applied), TitleData + TitleSystem for unlockable display titles. Load this skill when the user asks about achievements, titles, nameplates, milestones, retention hooks, or unlock trophies. Requires Skills 11–18."
---

# Skill 19: Achievement & Title System

## Objective
Track every player stat that matters across all battles. Unlock achievements and titles when thresholds cross. Let the player pick one title to display on their profile.

After this skill:
- `AchievementTracker` stores a lifetime counter for each stat key.
- Achievements unlock automatically as counters tick past thresholds.
- `TitleSystem` holds the list of unlocked titles; player picks `equippedTitle`.
- UI can enumerate `achievements` for a trophies page.

## Prerequisites
- Skill 18 — `BattleResult` populated per battle
- Skill 14 — character level/XP events
- SaveManager extended with achievement fields

---

## Step 1: AchievementData SO

**File:** `Assets/Scripts/Progression/AchievementData.cs`

```csharp
using UnityEngine;

namespace Matchmancer.Progression
{
    [CreateAssetMenu(fileName = "Achv_", menuName = "Matchmancer/Achievement Data")]
    public class AchievementData : ScriptableObject
    {
        public string   id;
        public string   displayName;
        [TextArea] public string description;
        public Sprite   icon;
        public string   counterKey;       // e.g. "totalWins", "maxCombo", "totalCrits"
        public int      threshold = 10;
        public string   unlocksTitleId;   // optional — auto-grant title on unlock
        public int      rewardGold;
    }
}
```

---

## Step 2: Counter keys

**File:** `Assets/Scripts/Progression/StatKeys.cs`

```csharp
namespace Matchmancer.Progression
{
    public static class StatKeys
    {
        public const string TotalWins       = "totalWins";
        public const string TotalDefeats    = "totalDefeats";
        public const string TotalDamage     = "totalDamage";
        public const string TotalCrits      = "totalCrits";
        public const string MaxCombo        = "maxCombo";
        public const string PoisonsApplied  = "poisonsApplied";
        public const string PerfectClears   = "perfectClears";   // 5-star clears
        public const string GoldEarned      = "goldEarned";
    }
}
```

---

## Step 3: AchievementTracker

**File:** `Assets/Scripts/Progression/AchievementTracker.cs`

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Matchmancer.Progression
{
    public class AchievementTracker : MonoBehaviour
    {
        public static AchievementTracker Instance { get; private set; }

        [SerializeField] private AchievementData[] catalog;

        private readonly Dictionary<string, long> counters = new Dictionary<string, long>();
        private readonly HashSet<string>          unlocked = new HashSet<string>();

        public event Action<AchievementData> OnAchievementUnlocked;
        public event Action<string, long>    OnCounterChanged;

        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public long Get(string key) => counters.TryGetValue(key, out var v) ? v : 0;

        public void Add(string key, long amount)
        {
            if (amount == 0) return;
            counters[key] = Get(key) + amount;
            OnCounterChanged?.Invoke(key, counters[key]);
            CheckUnlocks(key);
        }

        public void Max(string key, long value)
        {
            if (value > Get(key))
            {
                counters[key] = value;
                OnCounterChanged?.Invoke(key, value);
                CheckUnlocks(key);
            }
        }

        private void CheckUnlocks(string key)
        {
            foreach (var a in catalog)
            {
                if (a == null || a.counterKey != key) continue;
                if (unlocked.Contains(a.id)) continue;
                if (Get(key) >= a.threshold)
                {
                    unlocked.Add(a.id);
                    OnAchievementUnlocked?.Invoke(a);
                    if (!string.IsNullOrEmpty(a.unlocksTitleId))
                        TitleSystem.Instance?.Unlock(a.unlocksTitleId);
                }
            }
        }

        public IReadOnlyCollection<string> UnlockedIds => unlocked;

        // ---------- Save snapshot ----------

        [Serializable] public class Snapshot
        {
            public List<string> keys   = new List<string>();
            public List<long>   values = new List<long>();
            public List<string> unlockedIds = new List<string>();
        }

        public Snapshot Capture()
        {
            var s = new Snapshot();
            foreach (var kv in counters) { s.keys.Add(kv.Key); s.values.Add(kv.Value); }
            s.unlockedIds.AddRange(unlocked);
            return s;
        }

        public void Restore(Snapshot s)
        {
            counters.Clear(); unlocked.Clear();
            if (s == null) return;
            for (int i = 0; i < s.keys.Count; i++) counters[s.keys[i]] = s.values[i];
            foreach (var id in s.unlockedIds) unlocked.Add(id);
        }

        // ---------- Integration hook ----------

        public void RecordBattle(BattleResult r)
        {
            Add(r.victory ? StatKeys.TotalWins : StatKeys.TotalDefeats, 1);
            Add(StatKeys.GoldEarned, r.goldEarned);
            Max(StatKeys.MaxCombo, r.maxCombo);
        }
    }
}
```

---

## Step 4: TitleSystem

**File:** `Assets/Scripts/Progression/TitleSystem.cs`

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Matchmancer.Progression
{
    [CreateAssetMenu(fileName = "Title_", menuName = "Matchmancer/Title Data")]
    public class TitleData : ScriptableObject
    {
        public string id;
        public string displayName = "Apprentice";
        public Color  tint = Color.white;
    }

    public class TitleSystem : MonoBehaviour
    {
        public static TitleSystem Instance { get; private set; }

        [SerializeField] private TitleData[] catalog;
        private readonly HashSet<string> unlockedIds = new HashSet<string>();
        public string EquippedId { get; private set; }

        public event Action<string> OnTitleUnlocked;
        public event Action<string> OnEquippedChanged;

        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool IsUnlocked(string id) => unlockedIds.Contains(id);

        public void Unlock(string id)
        {
            if (string.IsNullOrEmpty(id) || !unlockedIds.Add(id)) return;
            OnTitleUnlocked?.Invoke(id);
        }

        public bool Equip(string id)
        {
            if (!IsUnlocked(id)) return false;
            EquippedId = id;
            OnEquippedChanged?.Invoke(id);
            return true;
        }

        public TitleData Find(string id)
        {
            foreach (var t in catalog)
                if (t != null && t.id == id) return t;
            return null;
        }

        [Serializable] public class Snapshot
        {
            public List<string> unlockedIds = new List<string>();
            public string equippedId;
        }
        public Snapshot Capture() => new Snapshot
        {
            unlockedIds = new List<string>(unlockedIds),
            equippedId  = EquippedId
        };
        public void Restore(Snapshot s)
        {
            unlockedIds.Clear();
            if (s == null) return;
            foreach (var id in s.unlockedIds) unlockedIds.Add(id);
            EquippedId = s.equippedId;
        }
    }
}
```

---

## Step 5: Integration points

In `CombatResolver.ResolveWave` (Skill 12) — tick crit/damage counters:
```csharp
if (effect.IsCriticalHit) AchievementTracker.Instance?.Add(StatKeys.TotalCrits, 1);
AchievementTracker.Instance?.Add(StatKeys.TotalDamage, (long)effect.DamageDealt);
```

In `EnemyController.HandleEffect` (Skill 13) — when poison applied:
```csharp
AchievementTracker.Instance?.Add(StatKeys.PoisonsApplied, 1);
```

In Skill 18 `HandleVictory`:
```csharp
AchievementTracker.Instance?.RecordBattle(result);
if (stars == 5) AchievementTracker.Instance?.Add(StatKeys.PerfectClears, 1);
```

---

## Validation checklist

- [ ] Scripts compile
- [ ] Winning 10 battles unlocks the "Ten Wins" achievement
- [ ] Unlock fires `OnAchievementUnlocked` exactly once per achievement
- [ ] Achievements that grant a title auto-unlock that title in `TitleSystem`
- [ ] `Equip(unknownId)` returns false, no change
- [ ] Save/restore round-trips counters and unlocked sets
- [ ] UI can render all unlocked achievements by iterating catalog & `UnlockedIds`

---

## Report

**Built:** AchievementData SO, StatKeys constants, AchievementTracker singleton, TitleData SO, TitleSystem singleton.
**Files:** under `Assets/Scripts/Progression/`: `AchievementData.cs`, `StatKeys.cs`, `AchievementTracker.cs`, `TitleSystem.cs`.
**Modified:** CombatResolver (+crit/damage counters), EnemyController (+poison counter), ResultsScreenController (calls `RecordBattle`).
**Assumptions:** Catalog assets assigned in Inspector at boot. Singletons live on a persistent Bootstrap GameObject. Save hook lands in Skill 20.
**Next:** Skill 20 — progression manager unlocks levels and saves everything.
