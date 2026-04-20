---
name: "Matchmancer Skill 25: Story & Dialogue System"
description: "Build the narrative layer: StorySceneData ScriptableObject (lines, speaker, portrait, pose, audio), DialogueController with typewriter effect and auto-advance, StoryTriggerSystem that fires scenes before/after specific levels, and save-aware replay so scenes don't repeat. Load this skill when the user asks about story, dialogue, cutscene, narrative, between-level transitions, portraits, or the story system. Requires Skills 20, 21, 23."
---

# Skill 25: Story & Dialogue System

## Objective
Story scenes are short, character-driven moments between levels. A scene is a list of lines — each line has a speaker, portrait, optional pose, and optional audio. The system shows one line at a time with a typewriter effect, auto-advances or waits for tap, and fires triggers on story beats (unlock lore, hand out rewards).

After this skill:
- Designers author scenes as ScriptableObjects.
- `StoryTriggerSystem` fires the right scene at the right moment (pre-battle, post-battle, first-boot).
- `DialogueController` renders dialogue over any screen.
- Scenes that have already played don't replay unless flagged.

## Prerequisites
- Skill 20 — SaveManager (for `seenSceneIds`)
- Skill 21 — ScreenRouter (for overlay)
- Skill 23 — LoreUnlockTracker (scene effects can unlock lore)

---

## Step 1: StorySceneData SO

**File:** `Assets/Scripts/Story/StorySceneData.cs`

```csharp
using UnityEngine;

namespace Matchmancer.Story
{
    public enum SpeakerSide { Left, Right, Center }

    [System.Serializable]
    public class DialogueLine
    {
        public string       speakerName;
        public Sprite       portrait;
        public SpeakerSide  side = SpeakerSide.Left;
        [TextArea(2, 6)] public string text;
        public AudioClip    voiceClip;
        public float        autoAdvanceSeconds = 0f;  // 0 = wait for tap
    }

    [CreateAssetMenu(fileName = "Story_", menuName = "Matchmancer/Story Scene")]
    public class StorySceneData : ScriptableObject
    {
        public string           sceneId;
        public DialogueLine[]   lines;

        [Header("On-complete effects")]
        public string[]         unlocksLoreIds;
        public bool             replayable = false;
    }
}
```

---

## Step 2: Trigger data

**File:** `Assets/Scripts/Story/StoryTrigger.cs`

```csharp
using UnityEngine;

namespace Matchmancer.Story
{
    public enum StoryTriggerPoint { FirstBoot, BeforeLevel, AfterLevelVictory, AfterLevelDefeat }

    [CreateAssetMenu(fileName = "Trigger_", menuName = "Matchmancer/Story Trigger")]
    public class StoryTrigger : ScriptableObject
    {
        public StoryTriggerPoint point;
        public string            levelId;       // for per-level triggers
        public StorySceneData    scene;
    }
}
```

---

## Step 3: StoryTriggerSystem

**File:** `Assets/Scripts/Story/StoryTriggerSystem.cs`

```csharp
using System.Collections.Generic;
using Matchmancer.Save;
using UnityEngine;

namespace Matchmancer.Story
{
    public class StoryTriggerSystem : MonoBehaviour
    {
        public static StoryTriggerSystem Instance { get; private set; }

        [SerializeField] private StoryTrigger[] triggers;
        [SerializeField] private DialogueController dialogue;

        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void HandleFirstBoot()      => FirePoint(StoryTriggerPoint.FirstBoot, null);
        public void HandleBeforeLevel(string levelId) => FirePoint(StoryTriggerPoint.BeforeLevel, levelId);
        public void HandleAfterVictory(string levelId) => FirePoint(StoryTriggerPoint.AfterLevelVictory, levelId);
        public void HandleAfterDefeat(string levelId)  => FirePoint(StoryTriggerPoint.AfterLevelDefeat, levelId);

        private void FirePoint(StoryTriggerPoint point, string levelId)
        {
            foreach (var t in triggers)
            {
                if (t == null || t.point != point) continue;
                if (!string.IsNullOrEmpty(t.levelId) && t.levelId != levelId) continue;
                if (t.scene == null) continue;
                if (HasSeen(t.scene.sceneId) && !t.scene.replayable) continue;

                dialogue.Play(t.scene, () => MarkSeen(t.scene));
                return; // only one scene per trigger point — queue if you need more
            }
        }

        private bool HasSeen(string id)
        {
            var list = SaveManager.Instance.Data.seenSceneIds;
            return list != null && list.Contains(id);
        }

        private void MarkSeen(StorySceneData scene)
        {
            var data = SaveManager.Instance.Data;
            if (data.seenSceneIds == null) data.seenSceneIds = new List<string>();
            if (!data.seenSceneIds.Contains(scene.sceneId)) data.seenSceneIds.Add(scene.sceneId);
            foreach (var id in scene.unlocksLoreIds) Lore.LoreUnlockTracker.Instance?.Unlock(id);
            SaveManager.Instance.Save();
        }
    }
}
```

> **Save schema note:** add `public List<string> seenSceneIds = new List<string>();` to `MatchmancerSaveData`.

---

## Step 4: DialogueController

**File:** `Assets/Scripts/Story/DialogueController.cs`

```csharp
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Matchmancer.Story
{
    public class DialogueController : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Image      leftPortrait, rightPortrait, centerPortrait;
        [SerializeField] private TMP_Text   speakerLabel, bodyLabel;
        [SerializeField] private Button     advanceButton;
        [SerializeField] private AudioSource voice;
        [SerializeField] private float      charactersPerSecond = 40f;

        private StorySceneData currentScene;
        private int             lineIndex;
        private Action          onComplete;
        private Coroutine       typingRoutine;
        private bool            lineComplete;

        private void Awake()
        {
            root.SetActive(false);
            advanceButton.onClick.AddListener(Advance);
        }

        public void Play(StorySceneData scene, Action completed)
        {
            currentScene = scene;
            onComplete   = completed;
            lineIndex    = 0;
            root.SetActive(true);
            ShowLine();
        }

        private void ShowLine()
        {
            if (currentScene == null || lineIndex >= currentScene.lines.Length) { Finish(); return; }
            var line = currentScene.lines[lineIndex];
            speakerLabel.text = line.speakerName;

            leftPortrait  .gameObject.SetActive(line.side == SpeakerSide.Left   && line.portrait);
            rightPortrait .gameObject.SetActive(line.side == SpeakerSide.Right  && line.portrait);
            centerPortrait.gameObject.SetActive(line.side == SpeakerSide.Center && line.portrait);

            var slot = line.side == SpeakerSide.Left   ? leftPortrait
                     : line.side == SpeakerSide.Right  ? rightPortrait
                     :                                   centerPortrait;
            if (line.portrait) slot.sprite = line.portrait;

            if (line.voiceClip && voice) voice.PlayOneShot(line.voiceClip);

            if (typingRoutine != null) StopCoroutine(typingRoutine);
            typingRoutine = StartCoroutine(Type(line));
        }

        private IEnumerator Type(DialogueLine line)
        {
            lineComplete = false;
            bodyLabel.text = "";
            float perChar = 1f / Mathf.Max(1f, charactersPerSecond);
            for (int i = 0; i < line.text.Length; i++)
            {
                bodyLabel.text += line.text[i];
                yield return new WaitForSeconds(perChar);
            }
            lineComplete = true;
            if (line.autoAdvanceSeconds > 0f)
            {
                yield return new WaitForSeconds(line.autoAdvanceSeconds);
                Advance();
            }
        }

        public void Advance()
        {
            if (!lineComplete)
            {
                // Skip typing: fill text instantly
                if (typingRoutine != null) StopCoroutine(typingRoutine);
                bodyLabel.text = currentScene.lines[lineIndex].text;
                lineComplete   = true;
                return;
            }
            lineIndex++;
            ShowLine();
        }

        private void Finish()
        {
            root.SetActive(false);
            var cb = onComplete;
            onComplete   = null;
            currentScene = null;
            cb?.Invoke();
        }
    }
}
```

---

## Step 5: Integration

In `LevelProgressionManager.RecordResult` (Skill 20):

```csharp
if (result.victory) StoryTriggerSystem.Instance?.HandleAfterVictory(result.levelId);
else                StoryTriggerSystem.Instance?.HandleAfterDefeat (result.levelId);
```

In battle bootstrap (when level starts):
```csharp
StoryTriggerSystem.Instance?.HandleBeforeLevel(level.levelId);
```

In `AppInitializer`:
```csharp
if (SaveManager.Instance.Data.seenSceneIds == null || SaveManager.Instance.Data.seenSceneIds.Count == 0)
    StoryTriggerSystem.Instance?.HandleFirstBoot();
```

---

## Validation checklist

- [ ] Scripts compile
- [ ] First boot shows the intro scene; subsequent boots don't
- [ ] Tapping advance before text finishes completes the line instantly
- [ ] Tapping advance when complete moves to next line
- [ ] `autoAdvanceSeconds > 0` advances automatically
- [ ] `replayable = true` replays every trigger
- [ ] `unlocksLoreIds` adds entries to LoreUnlockTracker on scene finish
- [ ] Voice clip plays exactly once per line
- [ ] Save file persists `seenSceneIds` across app restarts

---

## Report

**Built:** StorySceneData SO, StoryTrigger SO, StoryTriggerSystem singleton, DialogueController with typewriter + skip + auto-advance.
**Files:** under `Assets/Scripts/Story/`: `StorySceneData.cs`, `StoryTrigger.cs`, `StoryTriggerSystem.cs`, `DialogueController.cs`.
**Modified:** `MatchmancerSaveData.seenSceneIds`; `LevelProgressionManager` + `AppInitializer` + battle bootstrap call the triggers.
**Assumptions:** Only one scene per trigger point fires (first match wins). Portraits are static sprites — pose animation is future work. Audio uses a single `AudioSource` routed through SFX bus.
**Final:** This closes the 11–25 Matchmancer skill set. Master prompt's build-order is complete.
