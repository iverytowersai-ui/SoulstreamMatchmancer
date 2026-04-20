---
name: "Matchmancer Skill 20: Progression System & Save"
description: "Build the unlock gating + save backbone: ISaveBackend interface, LocalSaveBackend (PlayerPrefs + JSON), MatchmancerSaveData aggregate schema, LevelProgressionManager for level unlocking and reward distribution, and character XP curve. Load this skill when the user asks about saving, loading, save schema, level unlocking, stage gates, XP, level-ups, or reward granting. Requires Skills 11\u201319."
---

# Skill 20: Progression System & Save

## Objective
Tie every persistent system together. One save schema, one save backend (swappable to server later), one progression manager that answers "can the player enter this level?" and "give the player their rewards".

After this skill:
- `ISaveBackend` abstracts PlayerPrefs so we can swap in a server backend without touching game code.
- `MatchmancerSaveData` is the single source of truth for all persistent state.
- `LevelProgressionManager` enforces unlock rules and distributes rewards.
- `CharacterXPCurve` turns XP into levels with diminishing returns.

## Prerequisites
- Skills 14–19 — character, gear, inventory, level data, stars, achievements, titles

---

## Step 1: ISaveBackend

**File:** `Assets/Scripts/Save/ISaveBackend.cs`

```csharp
namespace Matchmancer.Save
{
    public interface ISaveBackend
    {
        bool HasSave();
        string LoadRaw();
        void SaveRaw(string json);
        void Delete();
    }
}
```

---

## Step 2: LocalSaveBackend

**File:** `Assets/Scripts/Save/LocalSaveBackend.cs`

```csharp
using UnityEngine;

namespace Matchmancer.Save
{
    public class LocalSaveBackend : ISaveBackend
    {
        private const string Key = "matchmancer.save.v1";

        public bool HasSave() => PlayerPrefs.HasKey(Key);
        public string LoadRaw() => PlayerPrefs.GetString(Key, string.Empty);
        public void SaveRaw(string json)
        {
            PlayerPrefs.SetString(Key, json);
            PlayerPrefs.Save();
        }
        public void Delete() => PlayerPrefs.DeleteKey(Key);
    }
}
```

---

## Step 3: MatchmancerSaveData

**File:** `Assets/Scripts/Save/MatchmancerSaveData.cs`

```csharp
using System;
using System.Collections.Generic;
using Matchmancer.Character;
using Matchmancer.Inventory;
using Matchmancer.Progression;

namespace Matchmancer.Save
{
    [Serializable]
    public class MatchmancerSaveData
    {
        public int    schemaVersion = 1;
        public string profileName = "Guest";
        public string createdAtIso;

        // Progression
        public List<string>       unlockedStages = new List<string>();
        public List<string>       unlockedLevels = new List<string>();
        public List<LevelRecord>  levelRecords   = new List<LevelRecord>();

        // Character
        public int    characterLevel = 1;
        public long   characterXP    = 0;
        public float  currentHP;               // for retry mid-stage if needed

        // Gear — equipped IDs per slot
        public List<string> equippedGearIds = new List<string>();

        // Subsystem snapshots
        public InventoryManager.Snapshot     inventory     = new InventoryManager.Snapshot();
        public AchievementTracker.Snapshot   achievements  = new AchievementTracker.Snapshot();
        public TitleSystem.Snapshot          titles        = new TitleSystem.Snapshot();

        // Settings
        public float musicVolume = 0.7f;
        public float sfxVolume   = 0.8f;
        public int   gold        = 0;
    }
}
```

---

## Step 4: SaveManager

**File:** `Assets/Scripts/Save/SaveManager.cs`

```csharp
using UnityEngine;

namespace Matchmancer.Save
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private ISaveBackend backend;
        public MatchmancerSaveData Data { get; private set; }

        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            backend = new LocalSaveBackend();
            Load();
        }

        public void Load()
        {
            if (backend.HasSave())
            {
                try { Data = JsonUtility.FromJson<MatchmancerSaveData>(backend.LoadRaw()); }
                catch { Data = new MatchmancerSaveData(); }
            }
            else Data = new MatchmancerSaveData { createdAtIso = System.DateTime.UtcNow.ToString("o") };
        }

        public void Save()
        {
            // Sync live subsystem state into Data before serializing
            Data.inventory    = Inventory.InventoryManager.Instance?.CaptureSnapshot()   ?? Data.inventory;
            Data.achievements = Progression.AchievementTracker.Instance?.Capture()       ?? Data.achievements;
            Data.titles       = Progression.TitleSystem.Instance?.Capture()              ?? Data.titles;

            backend.SaveRaw(JsonUtility.ToJson(Data));
        }

        public void WipeForTesting() => backend.Delete();
    }
}
```

---

## Step 5: CharacterXPCurve

**File:** `Assets/Scripts/Character/CharacterXPCurve.cs`

```csharp
using UnityEngine;

namespace Matchmancer.Character
{
    public static class CharacterXPCurve
    {
        /// <summary>XP required to reach `level` from level 1. Quadratic curve.</summary>
        public static long XPForLevel(int level) => (long)(100 * Mathf.Pow(level - 1, 1.6f));

        public static int LevelForXP(long xp)
        {
            int lvl = 1;
            while (XPForLevel(lvl + 1) <= xp && lvl < 100) lvl++;
            return lvl;
        }
    }
}
```

Call `CharacterRuntime.AddXP(amount)` after victory; it updates `characterLevel` via `CharacterXPCurve.LevelForXP`.

---

## Step 6: LevelProgressionManager

**File:** `Assets/Scripts/Progression/LevelProgressionManager.cs`

```csharp
using System.Collections.Generic;
using Matchmancer.Character;
using Matchmancer.Inventory;
using Matchmancer.Save;
using UnityEngine;

namespace Matchmancer.Progression
{
    public class LevelProgressionManager : MonoBehaviour
    {
        public static LevelProgressionManager Instance { get; private set; }

        [SerializeField] private List<StageData> stages;
        [SerializeField] private CharacterRuntime character;

        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public bool IsLevelUnlocked(string levelId)
        {
            var data = SaveManager.Instance.Data;
            return data.unlockedLevels.Contains(levelId);
        }

        public bool IsStageUnlocked(string stageId)
        {
            var data = SaveManager.Instance.Data;
            return data.unlockedStages.Contains(stageId);
        }

        public void RecordResult(BattleResult result, int stars)
        {
            var data = SaveManager.Instance.Data;
            var rec  = data.levelRecords.Find(r => r.levelId == result.levelId);
            if (rec == null)
            {
                rec = new LevelRecord { levelId = result.levelId };
                data.levelRecords.Add(rec);
            }
            if (stars > rec.bestStars) rec.bestStars = stars;
            if (stars == 5) rec.fiveStarCleared = true;

            if (result.victory)
            {
                character?.AddXP(result.xpEarned);
                data.gold += result.goldEarned;
                UnlockNext(result.levelId);
            }

            SaveManager.Instance.Save();
        }

        private void UnlockNext(string clearedLevelId)
        {
            var data = SaveManager.Instance.Data;
            for (int s = 0; s < stages.Count; s++)
            {
                var stage = stages[s];
                for (int i = 0; i < stage.levels.Count; i++)
                {
                    if (stage.levels[i].levelId != clearedLevelId) continue;

                    // Unlock next level in this stage
                    if (i + 1 < stage.levels.Count)
                    {
                        var next = stage.levels[i + 1].levelId;
                        if (!data.unlockedLevels.Contains(next)) data.unlockedLevels.Add(next);
                    }
                    else if (s + 1 < stages.Count)
                    {
                        // Stage cleared — unlock next stage + its first level
                        var nextStage = stages[s + 1];
                        if (!data.unlockedStages.Contains(nextStage.stageId)) data.unlockedStages.Add(nextStage.stageId);
                        if (nextStage.levels.Count > 0)
                        {
                            var first = nextStage.levels[0].levelId;
                            if (!data.unlockedLevels.Contains(first)) data.unlockedLevels.Add(first);
                        }
                    }
                    return;
                }
            }
        }

        public void InitializeFirstRun()
        {
            var data = SaveManager.Instance.Data;
            if (data.unlockedStages.Count == 0 && stages.Count > 0)
            {
                data.unlockedStages.Add(stages[0].stageId);
                if (stages[0].levels.Count > 0)
                    data.unlockedLevels.Add(stages[0].levels[0].levelId);
            }
        }
    }
}
```

---

## Step 7: Boot sequence

`AppInitializer.Start()`:
1. `SaveManager.Instance.Load()` (runs in Awake)
2. `InventoryManager.Instance.RestoreSnapshot(SaveManager.Instance.Data.inventory)`
3. `AchievementTracker.Instance.Restore(SaveManager.Instance.Data.achievements)`
4. `TitleSystem.Instance.Restore(SaveManager.Instance.Data.titles)`
5. `LevelProgressionManager.Instance.InitializeFirstRun()`
6. `SaveManager.Instance.Save()` (persist first-run defaults)

---

## Validation checklist

- [ ] Clean install → first level + stage 1 unlocked
- [ ] Clear level 1 → level 2 unlocked, save file updates
- [ ] Clear last level of a stage → next stage unlocks
- [ ] XP curve: level 1 = 0 XP, level 10 is reachable in reasonable grind
- [ ] Wipe save (`WipeForTesting`) resets everything on next launch
- [ ] Snapshot round-trip preserves inventory, achievements, titles
- [ ] Schema version bump path documented (migrate on load if `schemaVersion < current`)

---

## Report

**Built:** ISaveBackend + LocalSaveBackend, MatchmancerSaveData, SaveManager, CharacterXPCurve, LevelProgressionManager with unlock gating.
**Files:** under `Save/`: `ISaveBackend.cs`, `LocalSaveBackend.cs`, `MatchmancerSaveData.cs`, `SaveManager.cs`. Under `Character/`: `CharacterXPCurve.cs`. Under `Progression/`: `LevelProgressionManager.cs`.
**Modified:** `AppInitializer` boot sequence, `CharacterRuntime.AddXP` hook.
**Assumptions:** JsonUtility can round-trip the schema (no polymorphism required). Schema version field ready for future migrations. Gold is a plain `int` on save data.
**Next:** Skill 21 — UI screens read `LevelProgressionManager` and `SaveManager.Data` for their display.
