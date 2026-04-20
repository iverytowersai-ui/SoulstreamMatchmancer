# Matchmancer — Level 10 Blueprint & Checklist
**Goal:** Reach a playable Stage 1 (Levels 1–10) from the current empty Unity project, using Claude Code (or Antigravity) as your pair-dev and the Matchmancer plugin skills as the build recipe.

---

## 1. The Setup (one-time, before any coding)

### Tooling
- [ ] **Unity 2022.3 LTS or newer** — open `SoulstreamMatchmancer/` and let it import.
- [ ] **Claude Code CLI** installed — run it from inside `SoulstreamMatchmancer/` so it sees the Assets folder.
- [ ] **Matchmancer plugin active** (it is — I can see all 25 skills loaded).
- [ ] **Git** — `git init` in the project root. Commit after every completed skill. You will thank yourself.
- [ ] **TextMesh Pro** — already installed ✔

### Project hygiene
- [ ] Delete the `Matchmancer/9781839215070_Code/` tutorial junk (fruit ninja, brick breaker, etc.). It's noise.
- [ ] Create the folder structure Skill 01 expects: `Assets/_Matchmancer/{Scripts,Prefabs,ScriptableObjects,Art,Audio,Scenes}`.

---

## 2. What Level 10 Actually Requires

Level 10 = end of Stage 1 = first mini-boss fight. To make that playable and **not crash**, you need these systems working end-to-end. Anything beyond this list is post-Level-10 work.

**Required for Level 10:**
1. A working match-3 board (swap, match, cascade, refill)
2. 6 tile types that do different combat things
3. An enemy that takes damage and attacks back
4. A player character with HP, shield, and an ultimate
5. 10 levels of data (enemies, move limits, star thresholds)
6. A results screen (stars earned, win/lose)
7. A save file so Level 10 actually unlocks
8. Minimum viable UI (HP bars, move counter, level select)

**NOT required for Level 10** (skip until this works):
- Gear, inventory, boosters, shop, lore codex, achievements, story dialogue, character affinity, cosmetics, Addressables, bark system, SQLite — all of that is Stage 2+ work.

---

## 3. Build Order — The Checklist

Run each skill in Claude Code by saying *"Load matchmancer skill XX and build it"*. Each one builds on the previous. **Do not skip order** — every skill assumes the prior ones exist.

### Phase A — Match-3 Foundation (Skills 01–06)
These come from the base `match-3` skill set. This is your playable board with no combat yet.

- [ ] **Skill 01** — Project setup, folder structure, ObjectPool, GameManager/ScoreManager/LevelManager singletons
- [ ] **Skill 02** — GameBoard grid + Tile behavior + initial randomization (no pre-existing matches)
- [ ] **Skill 03** — MatchFinder (3+ horizontal/vertical, L/T shapes, deadlock detection)
- [ ] **Skill 04** — Input system (touch/mouse swap, swap-back on invalid)
- [ ] **Skill 05** — Gravity + refill (tiles fall, new tiles spawn from top)
- [ ] **Skill 06** — MatchResolver cascade loop + combo multiplier + auto-shuffle

**🎯 Checkpoint A:** You can swap tiles, they match, cascade, and refill. No combat yet — it's a Candy Crush board. **Commit.**

### Phase B — Combat Layer (Skills 11–14)
This is what makes it Matchmancer and not just match-3.

- [ ] **Skill 11** — 6 Tile Types (Damage, Energy, Defense, Debuff, Break, Luck) + CombatFormula + CombatStats
- [ ] **Skill 12** — CombatResolver wires matched groups to damage/energy/shield/debuff effects
- [ ] **Skill 13** — EnemyController (HP, armor, intent, status effects, enemy turn)
- [ ] **Skill 14** — CharacterRuntime (player HP, shield, ultimate meter) + UltimateSystem

**🎯 Checkpoint B:** You match Damage tiles → enemy HP drops. Enemy attacks back → your HP drops. Fight is real. **Commit.**

### Phase C — Level 10 Scaffolding (Skills 17, 18, 20)
Skip 15 (gear), 16 (boosters), 19 (achievements) for now.

- [ ] **Skill 17** — LevelData + StageData ScriptableObjects + author Levels 1–10 with scaling enemies/move limits
- [ ] **Skill 18** — StarCriteria + StarRatingSystem + ResultsScreenController
- [ ] **Skill 20** — SaveManager (PlayerPrefs+JSON) + level unlocking + progression gating

**🎯 Checkpoint C:** Clear Level 1 → Level 2 unlocks. Clear Level 10 → Stage 1 complete. **Commit.**

### Phase D — Minimum UI (Skills 21, 24)
Just enough to not look like a debugger.

- [ ] **Skill 21** — LoadScreen → MainHub → StageSelect → LevelSelect → Battle (ScreenRouter)
- [ ] **Skill 24** — Battle HUD (player HP bar, enemy HP bar, moves left, ultimate meter, combo banner)

**🎯 Checkpoint D:** Full loop from launch screen to battle to results to next level. **This is your Level 10 MVP. Ship it to yourself and play it.**

---

## 4. Skills You're SKIPPING for Level 10

These stay locked until after Checkpoint D is solid:
- Skill 07 (VFX) — placeholder Unity particles are fine
- Skill 08 (Audio) — silent is fine for MVP
- Skill 09/10 (polish, mobile build) — wait until gameplay works
- Skills 15, 16, 19, 22, 23, 25 — all post-Level-10

---

## 5. How to Work with Claude Code on This

**Per-skill workflow (copy this):**
1. `cd` into `SoulstreamMatchmancer/`
2. Start Claude Code
3. Say: *"Read matchmancer skill [NUMBER] and implement it in this Unity project. Follow the skill exactly. When done, give me a list of Unity Editor steps I need to take (scene setup, prefab wiring, ScriptableObject creation)."*
4. Claude writes the C# scripts into `Assets/_Matchmancer/Scripts/`
5. You do the Unity Editor wiring it tells you to do
6. You test in Play mode
7. If it breaks: *"Load engineering:debug skill. Here's the error: [paste]"*
8. Commit when the checkpoint passes
9. Move to next skill

**Rules of engagement:**
- One skill per session. Don't try to stack them.
- Always test in Play mode before moving on.
- If a skill says "Requires Skills 11–14" — believe it. Don't jump ahead.
- When Claude suggests architecture decisions, load `engineering:architecture` skill.
- When you merge your own changes, load `engineering:code-review` skill.

---

## 6. Art Assets for Level 10 (minimum viable)

You do **not** need final art. You need placeholders that read clearly:
- 6 tile sprites (colored squares with a symbol) — sword, bolt, shield, skull, hammer, clover
- 1 player portrait (pick Blue or Kaery from your roster)
- 5 enemy sprites (Stage 1 enemies — can reuse colors/palette swaps)
- 1 background per stage
- HP bar, button, panel 9-slices

Use the `matchmancer-art-director` skill + `elite-fantasy-image-director` skill to generate prompts when you're ready for real art. For MVP, solid color squares work.

---

## 7. Definition of Done for Level 10

The MVP is "done" when you can, in one continuous session without restarting Unity:
1. Launch game → Main Hub → Stage 1 → Level 1
2. Win Level 1 with a star rating shown on results
3. Return to Stage Select and see Level 2 unlocked
4. Play all the way through Level 10
5. Win Level 10 against its mini-boss
6. Quit the app, relaunch, and see progress preserved

When that works, come back and we'll plan Stage 2 — which is when gear, boosters, story dialogue, and the full OZONE narrative layer come online.

---

**Next action:** Open Claude Code in `SoulstreamMatchmancer/`, tell it *"Load matchmancer skill 01 and set up the project,"* and work the checklist top-to-bottom.
