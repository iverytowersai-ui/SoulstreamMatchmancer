---
name: "Matchmancer Skill 21: UI Screens & Flow"
description: "Build the navigation shell: LoadScreen, LoginScreen, MainHub, StageSelect, LevelSelect, ScreenRouter for transitions. Wires save state to button states (locked/unlocked stages and levels). Load this skill when the user asks about UI flow, main menu, load screen, stage select, level select, or screen navigation. Requires Skills 17, 20."
---

# Skill 21: UI Screens & Flow

## Objective
Build the non-battle screens and the router that ties them together. Every screen reads `SaveManager.Data` and `LevelProgressionManager` to render the correct state — no separate UI cache.

After this skill:
- App boots → Load → Login (guest/new) → Main Hub.
- Main Hub has buttons: Play, Shop, Gear, Lore, Settings.
- Stage Select shows 10 stages; locked stages greyed out.
- Level Select shows 10 levels for a stage; star ratings on cleared ones.
- All transitions go through a single `ScreenRouter`.

## Prerequisites
- Skill 17 — StageData/LevelData assets exist
- Skill 20 — SaveManager + LevelProgressionManager unlock queries

---

## Step 1: ScreenRouter

**File:** `Assets/Scripts/UI/ScreenRouter.cs`

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace Matchmancer.UI
{
    public enum ScreenId
    {
        Load, Login, MainHub, StageSelect, LevelSelect, Battle, Shop, Gear, Lore, Settings, Results
    }

    public class ScreenRouter : MonoBehaviour
    {
        public static ScreenRouter Instance { get; private set; }

        [System.Serializable] public class ScreenEntry { public ScreenId id; public GameObject root; }
        [SerializeField] private ScreenEntry[] screens;

        private readonly Stack<ScreenId> history = new Stack<ScreenId>();
        public ScreenId Current { get; private set; }

        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            foreach (var e in screens) if (e.root) e.root.SetActive(false);
            Go(ScreenId.Load, pushHistory: false);
        }

        public void Go(ScreenId id, bool pushHistory = true)
        {
            foreach (var e in screens) if (e.root) e.root.SetActive(e.id == id);
            if (pushHistory && Current != default) history.Push(Current);
            Current = id;
        }

        public void Back()
        {
            if (history.Count == 0) return;
            var prev = history.Pop();
            Go(prev, pushHistory: false);
        }
    }
}
```

---

## Step 2: LoadScreen

**File:** `Assets/Scripts/UI/LoadScreenController.cs`

```csharp
using System.Collections;
using Matchmancer.Save;
using UnityEngine;
using UnityEngine.UI;

namespace Matchmancer.UI
{
    public class LoadScreenController : MonoBehaviour
    {
        [SerializeField] private Slider progressBar;

        private void OnEnable() => StartCoroutine(Boot());

        private IEnumerator Boot()
        {
            float t = 0f;
            while (t < 1.2f)
            {
                t += Time.deltaTime;
                if (progressBar) progressBar.value = Mathf.Clamp01(t / 1.2f);
                yield return null;
            }

            var next = SaveManager.Instance.Data != null &&
                       !string.IsNullOrEmpty(SaveManager.Instance.Data.createdAtIso)
                ? ScreenId.MainHub
                : ScreenId.Login;

            ScreenRouter.Instance.Go(next);
        }
    }
}
```

---

## Step 3: LoginScreen

**File:** `Assets/Scripts/UI/LoginScreenController.cs`

```csharp
using Matchmancer.Save;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Matchmancer.UI
{
    public class LoginScreenController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameField;
        [SerializeField] private Button         startButton;

        private void OnEnable() => startButton.onClick.AddListener(HandleStart);
        private void OnDisable() => startButton.onClick.RemoveListener(HandleStart);

        private void HandleStart()
        {
            var name = string.IsNullOrWhiteSpace(nameField.text) ? "Guest" : nameField.text.Trim();
            SaveManager.Instance.Data.profileName = name;
            SaveManager.Instance.Data.createdAtIso = System.DateTime.UtcNow.ToString("o");
            SaveManager.Instance.Save();
            ScreenRouter.Instance.Go(ScreenId.MainHub);
        }
    }
}
```

---

## Step 4: MainHub

**File:** `Assets/Scripts/UI/MainHubController.cs`

```csharp
using Matchmancer.Save;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Matchmancer.UI
{
    public class MainHubController : MonoBehaviour
    {
        [SerializeField] private TMP_Text profileLabel, goldLabel, levelLabel;
        [SerializeField] private Button playButton, shopButton, gearButton, loreButton, settingsButton;

        private void OnEnable()
        {
            Refresh();
            playButton    .onClick.AddListener(() => ScreenRouter.Instance.Go(ScreenId.StageSelect));
            shopButton    .onClick.AddListener(() => ScreenRouter.Instance.Go(ScreenId.Shop));
            gearButton    .onClick.AddListener(() => ScreenRouter.Instance.Go(ScreenId.Gear));
            loreButton    .onClick.AddListener(() => ScreenRouter.Instance.Go(ScreenId.Lore));
            settingsButton.onClick.AddListener(() => ScreenRouter.Instance.Go(ScreenId.Settings));
        }

        private void OnDisable()
        {
            playButton    .onClick.RemoveAllListeners();
            shopButton    .onClick.RemoveAllListeners();
            gearButton    .onClick.RemoveAllListeners();
            loreButton    .onClick.RemoveAllListeners();
            settingsButton.onClick.RemoveAllListeners();
        }

        private void Refresh()
        {
            var d = SaveManager.Instance.Data;
            profileLabel.text = d.profileName;
            goldLabel   .text = $"{d.gold} Gold";
            levelLabel  .text = $"Lv. {d.characterLevel}";
        }
    }
}
```

---

## Step 5: StageSelect

**File:** `Assets/Scripts/UI/StageSelectController.cs`

```csharp
using Matchmancer.Progression;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Matchmancer.UI
{
    public class StageSelectController : MonoBehaviour
    {
        [SerializeField] private StageData[]  stages;
        [SerializeField] private GameObject   stageButtonPrefab;
        [SerializeField] private Transform    container;
        [SerializeField] private Button       backButton;

        private void OnEnable()
        {
            Clear();
            foreach (var stage in stages)
            {
                var go = Instantiate(stageButtonPrefab, container);
                var label = go.GetComponentInChildren<TMP_Text>();
                var btn   = go.GetComponent<Button>();

                bool unlocked = LevelProgressionManager.Instance.IsStageUnlocked(stage.stageId);
                label.text = $"{stage.stageName} {(unlocked ? "" : "🔒")}";
                btn.interactable = unlocked;
                btn.onClick.AddListener(() =>
                {
                    LevelSelectController.SelectedStage = stage;
                    ScreenRouter.Instance.Go(ScreenId.LevelSelect);
                });
            }
            backButton.onClick.AddListener(() => ScreenRouter.Instance.Go(ScreenId.MainHub, false));
        }

        private void OnDisable()
        {
            backButton.onClick.RemoveAllListeners();
            Clear();
        }

        private void Clear()
        {
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }
    }
}
```

---

## Step 6: LevelSelect

**File:** `Assets/Scripts/UI/LevelSelectController.cs`

```csharp
using Matchmancer.Progression;
using Matchmancer.Save;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Matchmancer.UI
{
    public class LevelSelectController : MonoBehaviour
    {
        public static StageData SelectedStage;

        [SerializeField] private GameObject levelCardPrefab;
        [SerializeField] private Transform  container;
        [SerializeField] private Button     backButton;

        private void OnEnable()
        {
            Rebuild();
            backButton.onClick.AddListener(() => ScreenRouter.Instance.Go(ScreenId.StageSelect, false));
        }

        private void OnDisable()
        {
            backButton.onClick.RemoveAllListeners();
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }

        private void Rebuild()
        {
            if (SelectedStage == null) return;
            var save = SaveManager.Instance.Data;

            foreach (var level in SelectedStage.levels)
            {
                var go    = Instantiate(levelCardPrefab, container);
                var label = go.GetComponentInChildren<TMP_Text>();
                var btn   = go.GetComponent<Button>();

                bool unlocked = LevelProgressionManager.Instance.IsLevelUnlocked(level.levelId);
                int  stars    = save.levelRecords.Find(r => r.levelId == level.levelId)?.bestStars ?? 0;

                label.text = $"{level.displayName} — {new string('★', stars)}{new string('☆', 5 - stars)} {(unlocked ? "" : "🔒")}";
                btn.interactable = unlocked;
                btn.onClick.AddListener(() =>
                {
                    BattleBootstrapPayload.Selected = level;
                    ScreenRouter.Instance.Go(ScreenId.Battle);
                });
            }
        }
    }

    public static class BattleBootstrapPayload
    {
        public static LevelData Selected;
    }
}
```

---

## Validation checklist

- [ ] Cold boot lands on Load → Login
- [ ] After entering a name, subsequent boots skip Login and go to MainHub
- [ ] Locked stages are greyed; clicking them does nothing
- [ ] Cleared levels show correct star count (1–5)
- [ ] Back buttons walk up the hierarchy correctly
- [ ] Selecting a level sets `BattleBootstrapPayload.Selected` and routes to Battle

---

## Report

**Built:** ScreenRouter with history stack, LoadScreen, LoginScreen, MainHub, StageSelect, LevelSelect, BattleBootstrapPayload handoff.
**Files:** `ScreenRouter.cs`, `LoadScreenController.cs`, `LoginScreenController.cs`, `MainHubController.cs`, `StageSelectController.cs`, `LevelSelectController.cs` under `Assets/Scripts/UI/`.
**Assumptions:** TMP_Text + UI Toolkit-free UGUI. One canvas per screen, all parented under a single root. Battle scene is loaded additively or shown via its own panel.
**Next:** Skill 22 — Shop screen reuses ScreenRouter and SaveManager gold.
