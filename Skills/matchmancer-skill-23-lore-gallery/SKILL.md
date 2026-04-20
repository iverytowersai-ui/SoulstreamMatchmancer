---
name: "Matchmancer Skill 23: Lore & Character Gallery"
description: "Build the codex: LoreEntryData ScriptableObject (characters, enemies, world lore, schools of magick), LoreUnlockTracker tied to story triggers and level clears, and LorePageController with a side-nav + reading pane. Load this skill when the user asks about lore, codex, character gallery, bestiary, world encyclopedia, or Soulstream universe entries. Requires Skills 17, 20, 25."
---

# Skill 23: Lore & Character Gallery

## Objective
Give the player a codex that unlocks as they progress. Each entry is a ScriptableObject with a title, portrait, body text, and unlock condition. The gallery groups entries by category (Characters, Enemies, Schools of Magick, World).

After this skill:
- Designers add entries via Create menu.
- Story triggers (Skill 25) and level clears call `LoreUnlockTracker.Unlock(id)`.
- Gallery UI lists categories on the left, entries in a scroll list, the body in a reading pane.
- Locked entries show silhouettes.

## Prerequisites
- Skill 20 — SaveManager for unlocked IDs
- Skill 21 — ScreenRouter for Lore screen

---

## Step 1: LoreEntryData SO

**File:** `Assets/Scripts/Lore/LoreEntryData.cs`

```csharp
using UnityEngine;

namespace Matchmancer.Lore
{
    public enum LoreCategory { Character, Enemy, School, World, Boss }

    [CreateAssetMenu(fileName = "Lore_", menuName = "Matchmancer/Lore Entry")]
    public class LoreEntryData : ScriptableObject
    {
        public string        id;
        public LoreCategory  category;
        public string        title;
        public Sprite        portrait;
        public Sprite        lockedSilhouette;
        [TextArea(6, 20)] public string body;

        [Header("Unlock condition (optional)")]
        public string        requiredLevelClearId;   // unlocks on level clear
        public bool          unlockedByDefault;
    }
}
```

---

## Step 2: LoreUnlockTracker

**File:** `Assets/Scripts/Lore/LoreUnlockTracker.cs`

```csharp
using System;
using System.Collections.Generic;
using Matchmancer.Save;
using UnityEngine;

namespace Matchmancer.Lore
{
    public class LoreUnlockTracker : MonoBehaviour
    {
        public static LoreUnlockTracker Instance { get; private set; }

        [SerializeField] private LoreEntryData[] catalog;

        public event Action<LoreEntryData> OnUnlocked;

        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureDefaults();
        }

        public IReadOnlyList<LoreEntryData> Catalog => catalog;

        public bool IsUnlocked(string id)
        {
            var set = GetUnlockedSet();
            return set.Contains(id);
        }

        public void Unlock(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            var data = SaveManager.Instance.Data;
            if (data.unlockedLore == null) data.unlockedLore = new List<string>();
            if (data.unlockedLore.Contains(id)) return;
            data.unlockedLore.Add(id);
            SaveManager.Instance.Save();

            var entry = FindById(id);
            if (entry != null) OnUnlocked?.Invoke(entry);
        }

        public void OnLevelCleared(string levelId)
        {
            foreach (var e in catalog)
                if (e != null && e.requiredLevelClearId == levelId)
                    Unlock(e.id);
        }

        private HashSet<string> GetUnlockedSet()
        {
            var data = SaveManager.Instance.Data;
            if (data.unlockedLore == null) data.unlockedLore = new List<string>();
            return new HashSet<string>(data.unlockedLore);
        }

        private void EnsureDefaults()
        {
            foreach (var e in catalog)
                if (e != null && e.unlockedByDefault) Unlock(e.id);
        }

        public LoreEntryData FindById(string id)
        {
            foreach (var e in catalog) if (e != null && e.id == id) return e;
            return null;
        }
    }
}
```

> **Save schema note:** add `public List<string> unlockedLore = new List<string>();` to `MatchmancerSaveData` (Skill 20).

---

## Step 3: LorePageController UI

**File:** `Assets/Scripts/UI/LorePageController.cs`

```csharp
using System.Linq;
using Matchmancer.Lore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Matchmancer.UI
{
    public class LorePageController : MonoBehaviour
    {
        [SerializeField] private Transform       categoryRoot;
        [SerializeField] private GameObject      categoryBtnPrefab;
        [SerializeField] private Transform       entryListRoot;
        [SerializeField] private GameObject      entryBtnPrefab;
        [SerializeField] private Image           readerPortrait;
        [SerializeField] private TMP_Text        readerTitle, readerBody;
        [SerializeField] private Button          backButton;

        private LoreCategory currentCategory = LoreCategory.Character;

        private void OnEnable()
        {
            BuildCategoryButtons();
            RebuildEntryList();
            backButton.onClick.AddListener(() => ScreenRouter.Instance.Go(ScreenId.MainHub, false));
        }

        private void OnDisable()
        {
            backButton.onClick.RemoveAllListeners();
            Clear(categoryRoot);
            Clear(entryListRoot);
        }

        private void BuildCategoryButtons()
        {
            Clear(categoryRoot);
            foreach (LoreCategory cat in System.Enum.GetValues(typeof(LoreCategory)))
            {
                var go = Instantiate(categoryBtnPrefab, categoryRoot);
                go.GetComponentInChildren<TMP_Text>().text = cat.ToString();
                var localCat = cat;
                go.GetComponent<Button>().onClick.AddListener(() =>
                {
                    currentCategory = localCat;
                    RebuildEntryList();
                });
            }
        }

        private void RebuildEntryList()
        {
            Clear(entryListRoot);
            var entries = LoreUnlockTracker.Instance.Catalog
                .Where(e => e != null && e.category == currentCategory);

            foreach (var entry in entries)
            {
                var go  = Instantiate(entryBtnPrefab, entryListRoot);
                var lbl = go.GetComponentInChildren<TMP_Text>();
                var btn = go.GetComponent<Button>();
                bool unlocked = LoreUnlockTracker.Instance.IsUnlocked(entry.id);
                lbl.text = unlocked ? entry.title : "???";
                btn.interactable = unlocked;
                btn.onClick.AddListener(() => Open(entry));
            }
        }

        private void Open(LoreEntryData entry)
        {
            bool unlocked = LoreUnlockTracker.Instance.IsUnlocked(entry.id);
            readerPortrait.sprite = unlocked ? entry.portrait : entry.lockedSilhouette;
            readerTitle.text = unlocked ? entry.title : "Unknown";
            readerBody .text = unlocked ? entry.body  : "This memory has not yet returned to you.";
        }

        private static void Clear(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--) Destroy(t.GetChild(i).gameObject);
        }
    }
}
```

---

## Step 4: Wire level clears

In `LevelProgressionManager.RecordResult` (Skill 20), after unlocking next level:

```csharp
LoreUnlockTracker.Instance?.OnLevelCleared(result.levelId);
```

Story triggers (Skill 25) call `LoreUnlockTracker.Instance.Unlock(id)` directly.

---

## Validation checklist

- [ ] Scripts compile
- [ ] Entries flagged `unlockedByDefault` show on first boot
- [ ] Clearing a level that matches `requiredLevelClearId` unlocks the tied entry
- [ ] Locked entries show the silhouette + "???" title
- [ ] Category buttons filter correctly
- [ ] Save/load preserves unlocked IDs
- [ ] `OnUnlocked` fires exactly once per newly-unlocked entry

---

## Report

**Built:** LoreEntryData SO, LoreUnlockTracker singleton with save integration, LorePageController with category nav + reading pane.
**Files:** `LoreEntryData.cs`, `LoreUnlockTracker.cs` under `Assets/Scripts/Lore/`; `LorePageController.cs` under `UI/`.
**Modified:** `MatchmancerSaveData.unlockedLore` added. `LevelProgressionManager.RecordResult` calls `OnLevelCleared`.
**Assumptions:** Categories enumerated from enum values; designers can add new categories later. Body uses plain text with newlines — rich text via TMP tags if desired.
**Next:** Skill 24 — Battle screen UI is the last visible layer; Skill 25 fires story scenes that unlock lore.
