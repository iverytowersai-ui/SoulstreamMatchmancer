# Soulstream Matchmancer — Claude Master Build Pack

> **Version:** 1.0 · **Date:** March 2026 · **Engine:** Unity (C#) · **AI:** Claude Code + Unity MCP
> **Rule:** 80% pre-planning, 20% building. Never one-shot the whole game.

---

## Table of Contents

1. [The Method][def]
2. [Tool Stack][def2]
3. [Project Setup][def3]
4. [Reference Pack](#4-reference-pack)
5. [The 20 Claude Prompts — In Order][def10]
6. [9-Character Roster Spec][def9]
7. [Milestone Commit Ladder][def8]
8. [Session Management Rules][def7]
9. [Bug Reporting Patterns][def6]
10. [Folder Structure][def5]
11. [What NOT To Do][def4]

---

## 1. The Method

From the `buildagame.mp4` transcript — the workflow that actually ships games:

| Step | Rule |
|------|------|
| **1** | Plan a specific game design **before** touching AI |
| **2** | Break the game into the **smallest possible** individual systems |
| **3** | Give Claude **one system at a time** — never the whole game |
| **4** | Test every change in Unity **immediately** |
| **5** | Commit every working state to version control |
| **6** | Describe bugs in **plain language** — Claude can diagnose |
| **7** | Polish and art come **after** core mechanics work |
| **8** | Watch context window — `/compact` below 15-20%, then restate context |

> **The biggest mistake:** Prompting "build me a match-3 game" and letting Claude make all design decisions. That produces uncontrollable spaghetti.

---

## 2. Tool Stack

| Tool | Role |
|------|------|
| **Unity** (2022 LTS, 2D template) | Game engine — run, test, wire |
| **Claude Code** (CLI, Opus model) | AI engineer — writes/edits code |
| **Unity MCP** (Coplay) | Editor bridge — lets Claude create GameObjects, attach scripts, play scenes, read error logs, self-correct |
| **GitHub Desktop** or Git | Version control — commit every milestone |
| **DOTween** (optional) | Animation tweening |
| **Feel** (optional) | Juice: particles, haptics, camera shake |

### Setup Checklist

- [ ] Unity Hub installed, Unity 2022 LTS with 2D template
- [ ] Claude Code installed (`claude` CLI + `/login`)
- [ ] Unity MCP (Coplay) package installed via Package Manager
- [ ] MCP connected to Claude Code (follow Coplay docs)
- [ ] Git repo initialized, first commit with empty project

---

## 3. Project Setup

Before Claude writes **any** gameplay code, create this structure:

```
Assets/
  References/           ← annotated screenshots + notes
  Docs/                 ← GDD, TechPlan, Milestones
  Scripts/
    Core/               ← BoardManager, Tile, MatchDetector, etc.
    Specials/           ← SpecialTile, SpecialFactory
    Blockers/           ← GlassBlocker, ChainBlocker, StoneBlocker
    Characters/         ← CharacterData, UltimateSystem
    Levels/             ← LevelData, LevelLoader, ObjectiveSystem
    UI/                 ← HUDController, WinPanel, LosePanel
    Data/               ← BoardConfig, TileDatabase
  ScriptableObjects/
  Prefabs/
    Tiles/
    Blockers/
    Characters/
    UI/
  Scenes/
    Boot
    Gameplay
    Map
    Roster
  Art/
    Tiles/
    Characters/
    Backgrounds/
    UI/
```

> Claude behaves better when your project looks like it has adult supervision.

---

## 4. Reference Pack

Create a `/References` folder with annotated screenshots. This is **exactly** what the video teaches.

### What to include

| Type | Content | Purpose |
|------|---------|---------|
| **Structural screenshots** | Other match-3 games (Hero Emblems, Royal Match, etc.) | Board scale, HUD zones, button hierarchy |
| **Annotated notes** | Your own labels on each screenshot | Exact mechanics per screen |
| **Soulstream visuals** | Blue, Crux, Kaery portraits; color palette; tile concepts | IP identity |
| **Gameplay wireframe** | Your ideal 9×9 board layout with HUD positions | North-star screenshot |

### Example annotation (save as `gameplay_notes.txt`)

```
Gameplay Screen Notes:
- 9x9 board, portrait orientation
- Top bar = moves counter (left) + objective icons (right)
- Bottom left = Blue character portrait
- Bottom center = Blue ultimate charge meter
- Bottom right = 3 booster slots (Bomb, Line, Swap)
- Pause button top-right corner
- Background = stormy urban rooftop, electric-blue atmosphere
- Tiles must be high-contrast, readable at 60x60px
- Match 3+ horizontal or vertical
- Adjacent swap only, no diagonal
- Moves-based fail state
```

---

## 5. The 20 Claude Prompts — In Order

> **Critical rule:** Do NOT skip ahead. Do NOT combine prompts. Build one system, test it, commit it, then move to the next.

---

### Prompt 0 — Context Establishment

```
Please read the reference images and notes inside the References folder.
Tell me what game we are building and confirm your understanding before
making any changes.

This is a portrait mobile match-3 game called Soulstream Matchmancer.
```

**Why:** Establishes shared context. The video does this before anything else.

---

### Prompt 1 — Game Design Document

```
Based on the references and your understanding, create a Game Design
Document and place it inside a Docs folder.

This is a portrait mobile match-3 game called Soulstream Matchmancer.

Core constraints:
- Portrait mobile orientation
- 9x9 board
- Adjacent tile swap only (no diagonal)
- Match 3+ horizontally or vertically
- Moves-based gameplay (out of moves = lose)
- 6 base tile types: Port Rune, OZONE Mark, Coven Seal,
  Witchbreed Thorn, Soulstream Shard, Petsha Charm
- 3 special tiles: Line Sigil (match-4), Star Sigil (match-5),
  Nova Sigil (L/T shape)
- 3 blocker types: Glass (1-2 hit), Chain (locks tile), Stone (2-3 hit)
- 25 levels for Chapter 1
- Blue is the default starter character
- 9 total characters in roster (6 locked at launch)
- Character system: 1 Ultimate + 1 Passive per character
- No monetization implementation yet
- Dark fantasy / tech-arcana aesthetic
- Chapter 1 theme: modern urban storm / OZONE conspiracy

Also create:
- TechPlan.md
- SystemBreakdown.md
- Milestones.md

Do not implement any code yet.
Summarize the recommended build order afterward.
```

**Commit:** `docs: initial GDD and planning documents`

---

### Prompt 2 — Project Inspection

```
Inspect the current Unity project and tell me:

1. What already exists
2. What is missing for a match-3 MVP
3. What scripts should be created next
4. What scene should be used (use the Gameplay scene)
5. What GameObjects and prefabs are needed
6. What should NOT be changed yet

Do not implement anything yet.
Make use of the Coplay MCP.
```

**Why:** Prevents Claude from hallucinating files that don't exist.

---

### Prompt 3 — Board Data Model

```
Based on the GDD, implement the foundational data model only.

Create:
- TileType enum (PortRune, OzoneMark, CovenSeal,
  WitchbreedThorn, SoulstreamShard, PetshaCharm)
- GridPosition struct
- BoardCell class
- BoardConfig ScriptableObject (width, height, tile set)

Place scripts in Assets/Scripts/Core/ and Assets/Scripts/Data/.
Keep it clean and modular.
Do NOT implement visuals, matching, or input yet.
Explain how each script is used.

Make use of the Coplay MCP.
The game scene is Gameplay.
```

**Commit:** `feat: board data model`

---

### Prompt 4 — Board Generation

```
Implement a BoardManager that generates a 9x9 board using the
TileType system from the previous step.

Requirements:
- No null cells — every cell gets a tile
- Use BoardConfig ScriptableObject for size and tile set
- Avoid starting matches if possible (re-roll tiles that
  would create instant 3-matches)
- Keep logic separate from presentation

Place in Assets/Scripts/Core/BoardManager.cs.
Return all file changes and explain how to test in Unity.
Make use of the Coplay MCP. Scene: Gameplay.
```

**Commit:** `feat: board generation with no-match check`

---

### Prompt 5 — Board Visuals

```
Create a simple tile visual system for the current board state.

Requirements:
- Spawn tile prefabs at board positions
- Each TileType shows a different placeholder color or sprite
- Keep visual representation separate from board logic (TileView)
- No animation yet
- Board should be centered and readable in portrait orientation

Place tile prefab in Assets/Prefabs/Tiles/.
Explain what needs to be assigned in the Inspector.
Make use of the Coplay MCP. Scene: Gameplay.
```

**Commit:** `feat: tile visuals and board display`

---

### Prompt 6 — Input & Tile Selection

```
Add player input for selecting and swapping tiles.

Requirements:
- Tap first tile to select, tap adjacent tile to swap
- Only allow adjacent swaps (up/down/left/right)
- No diagonal swaps
- Support mobile-friendly touch input
- Keep input system modular and separate from board logic
- Do NOT implement match resolution yet — just swap positions

Place in Assets/Scripts/Core/SwapController.cs.
Include test instructions.
Make use of the Coplay MCP. Scene: Gameplay.
```

**Commit:** `feat: tile selection and swap input`

---

### Prompt 7 — Swap Validation

```
Implement swap validation.

Requirements:
- When two adjacent tiles are swapped, temporarily swap them
- Check if the move creates at least one valid match
- If valid match: keep the swap
- If no match: animate swap back (revert)
- Keep validation logic deterministic and testable
- Separate validation from visual animation

Do NOT implement clearing or gravity yet.
Make use of the Coplay MCP. Scene: Gameplay.
```

**Commit:** `feat: swap validation with revert`

---

### Prompt 8 — Match Detection

```
Implement match detection for the board.

Requirements:
- Detect horizontal matches of 3 or more
- Detect vertical matches of 3 or more
- Support overlapping matches (a tile can be part of both
  horizontal and vertical matches)
- Return grouped coordinate sets
- Add debug logging so I can verify detection in Play Mode

Do NOT clear matched tiles yet.
Place in Assets/Scripts/Core/MatchDetector.cs.
Make use of the Coplay MCP. Scene: Gameplay.
```

**Commit:** `feat: match detection horizontal + vertical`

---

### Prompt 9 — Core Resolution Loop ⭐

> This is the most important prompt. If this loop doesn't feel right, the game doesn't work.

```
Implement the full match resolution loop:

1. Detect matches
2. Clear matched tiles (mark as empty)
3. Apply gravity — collapse tiles downward to fill gaps
4. Refill empty spaces from the top with new random tiles
5. Check for new matches from cascades
6. Repeat steps 1-5 until no matches remain

Requirements:
- Keep this as a clean board state pipeline
- Process one full cycle before checking cascades
- Use coroutines with slight delays (0.05-0.1s) between steps
  so cascades are visible
- Do NOT add special tiles or blocker logic yet
- Explain where animation hooks should be added later

Place in Assets/Scripts/Core/ResolutionPipeline.cs.
Make use of the Coplay MCP. Scene: Gameplay.
```

**Commit:** `feat: core resolution loop (clear/gravity/refill/cascade)`

---

### Prompt 10 — Animation Hooks

```
Add animation hooks to the existing board flow.

Requirements:
- Swap animation (0.3-0.4 seconds, smooth tween)
- Invalid swap bounce-back animation
- Tile pop/scale effect on clear
- Falling animation during gravity
- Refill fade-in animation from top
- Use a structure that can work with DOTween if installed later

Do NOT change game rules or board logic.
Make use of the Coplay MCP. Scene: Gameplay.
```

**Commit:** `feat: animation hooks for board flow`

---

### Prompt 11 — Score System

```
Implement a scoring system.

Requirements:
- Base points per tile matched (e.g., 50 per tile)
- Combo multiplier: each cascade step increases multiplier
  (1x → 1.5x → 2x → 2.5x etc.)
- Reset combo on player's next manual move
- Track and expose current score and combo level
- Add score popup text at match position (simple, white text)

Place in Assets/Scripts/Core/ScoreManager.cs.
Make use of the Coplay MCP. Scene: Gameplay.
```

**Commit:** `feat: scoring with combo multiplier`

---

### Prompt 12 — HUD

```
Implement a portrait mobile HUD for Soulstream Matchmancer.

Requirements:
- Top bar: moves remaining (left), objective display (right)
- Bottom-left: Blue character portrait placeholder
- Bottom-center: Blue ultimate charge meter (0 to 10)
- Bottom-right: 3 booster slots (placeholder icons)
- Top-right corner: pause button
- Score display below top bar
- Combo text that shows during cascades

Use placeholder visuals only. Do not final-style it.
Use the reference images for layout proportions.
Make use of the Coplay MCP. Scene: Gameplay.
```

**Commit:** `feat: basic portrait HUD`

---

### Prompt 13 — Objective System

```
Implement a first-pass objective system.

Support these objective types:
- Collect a target number of a specific tile type
- Reach a target score
- Clear all blockers (for future use)

Requirements:
- Moves counter: decrement on each valid player swap
- Out-of-moves = lose condition trigger
- All objectives met = win condition trigger
- Connect objective tracking to the HUD displays
- Keep it modular for future level expansion

Place in Assets/Scripts/Levels/ObjectiveSystem.cs.
Make use of the Coplay MCP. Scene: Gameplay.
```

**Commit:** `feat: objective system with moves/collect/score`

---

### Prompt 14 — Win/Lose Flow

```
Implement win and lose screens.

Requirements:
- Win panel: 1-3 star rating, score display,
  "Next Level" button, "Replay" button
- Lose panel: "Retry" button, "Quit" button,
  moves-used display
- Panels appear over the gameplay with a dark overlay
- Use placeholder UI styling only

Place in Assets/Scripts/UI/.
Make use of the Coplay MCP. Scene: Gameplay.
```

**Commit:** `feat: win/lose screens`

---

### Prompt 15 — Special Tiles (Sigils)

```
Add special tile generation to the existing match system.

Rules:
- Match 4 in a line → Line Sigil (clears entire row OR column)
- Match 5 in a line → Star Sigil (clears all tiles of one chosen type)
- T or L shaped match → Nova Sigil (3x3 explosion at center)

Requirements:
- Integrate into the current resolution pipeline
- Do NOT rewrite the core board architecture
- When a sigil is matched normally, it activates its effect
- Add placeholder visual indicators for each sigil type
- Explain how activation is triggered and resolved

Place in Assets/Scripts/Specials/.
Make use of the Coplay MCP. Scene: Gameplay.
```

**Commit:** `feat: Line/Star/Nova sigils`

---

### Prompt 16A — Glass Blocker

```
Add the first blocker type only: Glass.

Rules:
- Glass sits on a cell as an overlay
- Has 1 or 2 durability layers
- Takes damage when a match is made on that cell or adjacent
- When durability reaches 0, the glass is destroyed
- Show visual cracking between layer 1 and layer 2

Requirements:
- Implement as a separate blocker layer system
- Connect blocker clearing to the objective system
- Use placeholder visuals
- Do NOT add any other blocker types yet

Place in Assets/Scripts/Blockers/GlassBlocker.cs.
Make use of the Coplay MCP. Scene: Gameplay.
```

**Commit:** `feat: Glass blocker`

---

### Prompt 16B — Chain Blocker

```
Add the second blocker type: Chain.

Rules:
- Locks a tile in place — it cannot be swapped
- Chain is removed when a match is made adjacent to it
  or on the chained cell
- Once chain is removed, the tile becomes normal

Requirements:
- Do NOT rewrite the Glass blocker system
- Integrate with existing blocker layer
- Use placeholder visuals

Place in Assets/Scripts/Blockers/ChainBlocker.cs.
Make use of the Coplay MCP. Scene: Gameplay.
```

**Commit:** `feat: Chain blocker`

---

### Prompt 16C — Stone Blocker

```
Add the third blocker type: Stone/Armor.

Rules:
- Requires 2-3 hits from adjacent matches
- Track hit count and show progressive cracking
- Cannot be swapped or moved
- When fully destroyed, cell becomes empty

Requirements:
- Do NOT rewrite Glass or Chain systems
- Integrate with existing blocker layer
- Use placeholder visuals

Place in Assets/Scripts/Blockers/StoneBlocker.cs.
Make use of the Coplay MCP. Scene: Gameplay.
```

**Commit:** `feat: Stone blocker`

---

### Prompt 17 — Character Power Framework + Blue

```
Implement the character power system framework and Blue's abilities.

Framework:
- CharacterData ScriptableObject: name, portrait, ultimate info,
  passive info, charge cost, role, rarity, unlock state
- CharacterManager: tracks active character, charge meter
- Charge meter: +1 per normal match made
- When charge reaches the character's charge cost, enable
  Ultimate button in HUD

Blue — "Soulstream Pulse Caster":
- Charge cost: 10
- Ultimate: player taps a cell, clears a 4x4 area centered on it
- Passive "First Spark": first match each level grants +1 bonus charge

Requirements:
- Build framework so future characters use the same system
- Connect charge meter to the existing HUD
- Keep ultimate activation modular

Place in Assets/Scripts/Characters/.
Make use of the Coplay MCP. Scene: Gameplay.
```

**Commit:** `feat: character system + Blue ultimate`

---

### Prompt 18 — Level Data & Loader

```
Create a LevelData system with ScriptableObjects.

LevelData fields:
- Level number / ID
- Board width and height (default 9x9)
- Move limit
- Allowed tile types (subset of 6)
- Objective list (type + target amount)
- Blocker placements (position + type + durability)
- Star thresholds (1-star, 2-star, 3-star scores)
- Required character (optional, for tutorial levels)

Also create:
- LevelLoader: reads LevelData and initializes the Gameplay scene
- Create 5 test levels as ScriptableObject assets:
  - Level 1-3: basic matching only, no blockers
  - Level 4: introduces one Glass blocker
  - Level 5: introduces Chain blocker

Place in Assets/Scripts/Levels/ and Assets/ScriptableObjects/.
Make use of the Coplay MCP. Scene: Gameplay.
```

**Commit:** `feat: LevelData + LevelLoader + 5 test levels`

---

### Prompt 19 — Map Screen

```
Implement a Chapter 1 level map screen.

Requirements:
- 25 nodes in a clean path (linear or lightly curved)
- Each node shows: level number, lock/unlock state,
  star count if completed
- Tapping an unlocked node loads that level
- Simple scrollable layout
- Use placeholder visuals
- Chapter title: "Chapter 1: Stormbreak"

Create a new scene called Map in Assets/Scenes/.
Make use of the Coplay MCP.
```

**Commit:** `feat: Chapter 1 map screen`

---

### Prompt 20 — Character Roster Screen

```
Implement a character roster/selection screen.

Requirements:
- Grid of 9 character cards
- Each card shows: portrait placeholder, name, role label,
  rarity badge, lock/unlock state
- Blue = unlocked (Free tier)
- Crux and Kaery = shown as "unlockable" with preview
- Remaining 6 = shown as locked silhouettes with rarity badge
- Tapping an unlocked card selects that character for gameplay
- Each card supports future: price label, "Buy" button

Roster data:
1. Blue — Burst — Free — Unlocked
2. Crux — Precision — Earnable — Preview-locked
3. Kaery — Setup — Earnable — Preview-locked
4. Kimmy — Combo — Rare — Locked
5. Surge-Overload — Board Rewrite — Epic — Locked
6. Mr. Violet — Support — Rare — Locked
7. Guardian — Objective Killer — Epic — Locked
8. Yanoro — Trap/Burst — Legendary — Locked
9. Chanel — Corruption — Legendary — Locked

Create a new scene called Roster in Assets/Scenes/.
Make use of the Coplay MCP.
```

**Commit:** `feat: 9-character roster screen`

---

## 6. 9-Character Roster Spec

### Roster Overview

| # | Name | Role | Difficulty | Charge | Tier | Unlock |
|---|------|------|-----------|--------|------|--------|
| 1 | **Blue** | Burst / All-rounder | Easy | 10 | Free | Default starter |
| 2 | **Crux** | Precision / Line | Easy-Med | 10 | Earnable | Ch.1 clear or early progression |
| 3 | **Kaery** | Setup / Blocker-control | Medium | 12 | Earnable | Ch.2 entry or mid progression |
| 4 | **Kimmy** | Combo / Tempo | Medium | 10 | Rare | Store / Event |
| 5 | **Surge-Overload** | Board Rewrite | Med-High | 14 | Epic | Store / Event |
| 6 | **Mr. Violet** | Support / Cleanse | Easy | 10 | Rare | Store / Event |
| 7 | **Guardian** | Surgical Objective | High | 12 | Epic | Store / Event |
| 8 | **Yanoro** | Trap / Delayed Burst | High | 14 | Legendary | Event banner |
| 9 | **Chanel** | Corruption / Engine | High | 16 | Legendary | Event banner |

### Detailed Character Cards

---

#### 1. Blue — Soulstream Pulse Caster

- **Book:** Storm/lightning Mancer, blue eyes, urban energy, protagonist
- **Tile affinity:** Soulstream Shard
- **Ultimate — Soulstream Pulse:** Clear a 4×4 area at tapped location (~16 tiles max)
- **Passive — First Spark:** First match each level grants +1 bonus charge
- **Monetization:** Free forever. Blue is the face of the game.

---

#### 2. Crux — Port Cross Navigator

- **Book:** Controls density, velocity, momentum of small objects; absurdly accurate
- **Tile affinity:** Port Rune
- **Ultimate — Port Cross:** Clear 1 full row + 1 full column at tap point (~17 tiles max)
- **Passive — Pathfinder:** Once per level, first Match-4 upgrades more reliably into Line Sigil
- **Monetization:** Earnable early, soft-unlock. Too core to paywall hard.

---

#### 3. Kaery — Oracle Enchantress

- **Book:** Golden eyes, oracular truth, magick immunity, barrier identity
- **Tile affinity:** Coven Seal
- **Ultimate — Glamour Shift:** Choose Tile A → Tile B, convert up to 8 of A into B
- **Passive — Sealbreaker:** Specials deal +1 extra hit to Stone/Armor, up to 3× per level
- **Monetization:** Earnable. Completes the starter trio (burst + precision + setup).

---

#### 4. Kimmy — Feral Shift Spiraar

- **Book:** Child Spiraar, pink-furred werewolf form, parents died protecting the Grove
- **Tile affinity:** Petsha Charm
- **Ultimate — Feral Shift:** Next 5 moves, first match each turn spawns 1 Claw Mark; Claw Mark = mini-bomb when matched
- **Passive — Cub Courage:** First 2+ cascade each turn grants +1 bonus charge
- **Monetization:** Rare tier. Very marketable, cute, sellable, still fair.

---

#### 5. Surge-Overload — Technomancer

- **Book:** Elite hacker with unparalleled tech control, holographic interfaces
- **Tile affinity:** OZONE Mark
- **Ultimate — Ghost Touch:** Choose up to 6 tiles anywhere, rewrite them into one chosen type
- **Passive — Tech Savant:** Every Match-4 or Match-5 gives +1 extra charge
- **Monetization:** Epic tier. Premium feel, unique board manipulation.

---

#### 6. Mr. Violet — Gaia Guardian

- **Book:** Ancient creature, protective, purple Gaia-tailed squirrel, strong love/protection energy
- **Tile affinity:** Coven Seal
- **Ultimate — Gaia Blessing:** Create 4 Bloom Seeds around tapped cell. Bloom Seeds = wild support tiles, +1 blocker damage when cleared
- **Passive — Comfort Tail:** First special created each level gets +1 extra blocker hit
- **Monetization:** Rare tier. Mascot power, merch angle, non-edgy roster relief.

---

#### 7. Guardian — OZONE Assassin

- **Book:** OZONE's deadliest, enhanced speed/agility/marksmanship, secret Jreamer
- **Tile affinity:** OZONE Mark
- **Ultimate — Kill Zone:** Choose 5 cells anywhere — they clear, ignoring Chains and Armor
- **Passive — Predator Instinct:** First objective blocker damaged each turn takes +1 extra damage
- **Monetization:** Epic tier. Cool military fantasy, strong anti-blocker niche.

---

#### 8. Yanoro — Shadow Ambusher

- **Book:** Shandor/Vampyl darkness user, globe of darkness, vampiric speed
- **Tile affinity:** Witchbreed Thorn
- **Ultimate — Dark Globe:** Place a 3×3 shadow zone. After your next manual match, zone detonates + clears, bonus damage to locked cells
- **Passive — Vampiric Edge:** First shadow detonation each level grants +2 charge refund
- **Monetization:** Legendary. Strong ceiling but setup-dependent = fair premium.

---

#### 9. Chanel — Nightmare Empress

- **Book:** Darkness, blood-red nightmare power, seductive dominant presence
- **Tile affinity:** Witchbreed Thorn
- **Ultimate — Blood Eclipse:** Choose one tile type → drain up to 10 scattered copies → create 2 Crimson Sigils. Crimson Sigils explode 3×3 when matched
- **Passive — Nightmare Pressure:** First special matched near a blocker each level deals +1 extra splash damage
- **Monetization:** Legendary. Flashy, high-fantasy, delayed payoff = not instant-win.

---

### Power Budget Rule

> No ultimate should exceed ~16-18 tiles of value, or the equivalent in setup/control benefit.

| Character | Max tile value | Notes |
|-----------|---------------|-------|
| Blue | 16 (4×4) | Direct, reliable |
| Crux | ~17 (row+col) | Precise, cross-shaped |
| Kaery | 8 conversions | Setup, no direct clear |
| Kimmy | 5 mini-bombs | Tempo, spread over turns |
| Surge | 6 rewrites | Board manipulation, no clear |
| Mr. Violet | 4 Bloom Seeds | Support utility |
| Guardian | 5 cells | Surgical, ignores blockers |
| Yanoro | 9 (3×3) | Delayed, conditional |
| Chanel | ~10 drain + 2×9 sigil | Highest ceiling, most setup needed |

### Monetization Rules

**Good monetization — charge more for:**

- Fantasy appeal, animation quality, rarity, complexity, skins, early access, bundles

**Bad monetization — do NOT charge more because:**

- "This character just wins more" — that's lazy and rots the game

**Premium should mean:** More specialized, higher skill ceiling, cooler cast animations, more satisfying VFX — not raw stat inflation.

---

## 7. Milestone Commit Ladder

Commit after **every** working feature. This is non-negotiable.

| # | Milestone | Commit message |
|---|-----------|---------------|
| 1 | Docs created | `docs: GDD and planning` |
| 2 | Board data model | `feat: board data model` |
| 3 | Board generation | `feat: board generation` |
| 4 | Board visuals | `feat: tile visuals` |
| 5 | Input & selection | `feat: tile input` |
| 6 | Swap validation | `feat: swap validation` |
| 7 | Match detection | `feat: match detection` |
| 8 | Core loop (clear/gravity/refill) | `feat: core resolution loop` |
| 9 | Animations | `feat: animation hooks` |
| 10 | Score + combo | `feat: scoring system` |
| 11 | HUD | `feat: HUD` |
| 12 | Objectives | `feat: objectives` |
| 13 | Win/Lose | `feat: win/lose screens` |
| 14 | Sigils | `feat: special tiles` |
| 15 | Glass blocker | `feat: Glass blocker` |
| 16 | Chain blocker | `feat: Chain blocker` |
| 17 | Stone blocker | `feat: Stone blocker` |
| 18 | Blue ultimate | `feat: character system + Blue` |
| 19 | Level data + loader | `feat: level system` |
| 20 | Map screen | `feat: map screen` |
| 21 | Roster screen | `feat: roster screen` |

---

## 8. Session Management Rules

### Starting a session — always restate context

```
We are building a portrait mobile match-3 game called Soulstream
Matchmancer in Unity.

Current scene: Gameplay

Already working:
- [list what's done]

Next task:
- [single next system]

Constraints:
- Do not rename existing scripts
- Do not move files between folders
- Do not change scene structure
- Only modify files needed for this task
- Preserve current board behavior

Make use of the Coplay MCP.
```

### After `/compact` — restate everything

When context drops below 15-20%, run `/compact`, then immediately:

```
After compacting, please note:
- The game scene is called Gameplay (Assets/Scenes/Gameplay)
- The current working systems are: [list them]
- The next task is: [one thing]
- Do NOT modify: [list protected systems]
```

### Play Mode warning

> If Claude adjusts something while Unity is in Play Mode, changes may revert when you stop playing. If something "worked and then disappeared," check this first.

---

## 9. Bug Reporting Patterns

You do **not** need technical jargon. Plain language works.

### Good bug reports (one at a time)

```
"The board starts with too many free matches — I see matches
before my first move."

"The refill leaves gaps in the top row."

"The objective count updates one match late."

"Blue's meter fills too quickly — it should need 10 matches,
it seems to fill in 5."

"The Glass blocker visual does not change between layer 1 and 2."

"When I lose, the panel appears before the last animation finishes."

"The swap feels delayed — it should be snappier, around 0.3 seconds."

"Cascades happen but the score isn't increasing correctly."

"The roster screen lost track of which scene to load."

"Special tiles are not triggering when I match them —
they behave like normal tiles."
```

### Bad bug reports (do NOT do this)

```
"Everything is broken, the whole board doesn't work,
matches are wrong, score is wrong, and the UI overlaps."
```

> **One bug. One prompt. One fix. One test. One commit.**

---

## 10. Folder Structure

```
Soulstream-Matchmancer/
├── Assets/
│   ├── Art/
│   │   ├── Tiles/          (6 base + 3 special + 3 blocker)
│   │   ├── Characters/     (9 portraits)
│   │   ├── Backgrounds/    (chapter BGs)
│   │   └── UI/             (HUD icons, panels)
│   ├── Docs/
│   │   ├── GDD.md
│   │   ├── TechPlan.md
│   │   ├── SystemBreakdown.md
│   │   └── Milestones.md
│   ├── Prefabs/
│   │   ├── Tiles/
│   │   ├── Blockers/
│   │   ├── Characters/
│   │   └── UI/
│   ├── References/
│   │   ├── gameplay_notes.txt
│   │   ├── reference_screenshot_01.png
│   │   └── ...
│   ├── Scenes/
│   │   ├── Boot
│   │   ├── Gameplay
│   │   ├── Map
│   │   └── Roster
│   ├── ScriptableObjects/
│   │   ├── BoardConfig.asset
│   │   ├── Characters/      (9 CharacterData assets)
│   │   └── Levels/          (25 LevelData assets)
│   └── Scripts/
│       ├── Core/
│       │   ├── BoardManager.cs
│       │   ├── BoardCell.cs
│       │   ├── Tile.cs
│       │   ├── TileView.cs
│       │   ├── MatchDetector.cs
│       │   ├── SwapController.cs
│       │   ├── ResolutionPipeline.cs
│       │   ├── GravityResolver.cs
│       │   ├── RefillResolver.cs
│       │   └── ScoreManager.cs
│       ├── Specials/
│       │   ├── SpecialTile.cs
│       │   ├── SpecialFactory.cs
│       │   └── SpecialResolver.cs
│       ├── Blockers/
│       │   ├── BlockerBase.cs
│       │   ├── GlassBlocker.cs
│       │   ├── ChainBlocker.cs
│       │   └── StoneBlocker.cs
│       ├── Characters/
│       │   ├── CharacterData.cs
│       │   ├── CharacterManager.cs
│       │   └── UltimateSystem.cs
│       ├── Levels/
│       │   ├── LevelData.cs
│       │   ├── LevelLoader.cs
│       │   └── ObjectiveSystem.cs
│       ├── UI/
│       │   ├── HUDController.cs
│       │   ├── WinPanel.cs
│       │   ├── LosePanel.cs
│       │   └── RosterScreen.cs
│       └── Data/
│           ├── BoardConfig.cs
│           └── TileDatabase.cs
└── Packages/
```

---

## 11. What NOT To Do

These are the confirmed mistakes from the transcript:

| Mistake | Why it kills your project |
|---------|--------------------------|
| **One-shotting** "build me a match-3 game" | Claude invents mechanics, adds unwanted features, scope explodes |
| **Building art before core loop** | Looks pretty, ships nothing |
| **Multiple mechanics per prompt** | "Add blockers, boosters, leveling, particles, and enemies" = chaos |
| **Letting Claude rename/move files freely** | Always constrain: "do not rename scripts, do not move folders" |
| **Skipping testing** | Claude can write code that looks smart and faceplants in Play Mode |
| **Not committing after each feature** | One bad prompt nukes three good systems |
| **Ignoring context limits** | After `/compact`, Claude forgets scene, files, and rules |
| **Making Play Mode changes** | Changes made during Play Mode revert when you stop |
| **Designing all 9 characters fully at launch** | Implement 3, show 9 as cards, expand later |
| **Making paid characters objectively stronger** | Premium = cooler/specialized, not auto-win |

---

## 12. Solo Dev Survival Rules

These aren't motivational fluff. They're hard-earned rules from people who actually shipped games solo:

- **Rule 1: Ugly prototype first.** Your game will look terrible for weeks. That's correct. Gameplay feel matters more than visuals at the start. The art comes later and transforms everything overnight.
- **Rule 2: Playtest every single day.** Even if you only added one feature, play the game. Feel what's fun and what's broken. Your hands will tell you what your eyes miss.
- **Rule 3: One feature at a time.** Never work on blockers and characters and UI simultaneously. Finish one, commit, move on. Context-switching is the solo dev killer.
- **Rule 4: The GDD is your anchor.** When you feel lost or tempted to add features, re-read the GDD. It says "Ignore for now" on relics, events, story, PvP. Trust that. Build the MVP.
- **Rule 5: Ship ugly, then polish.** A working ugly game beats a beautiful broken one. Get all 25 levels playable first. Then make them pretty.
- **Rule 6: Dual-Tracking keeps you sane.** When coding frustrates you, switch to art generation. When prompts frustrate you, switch to code. Two tracks = no burnout dead ends.

---

## Quick Start Checklist

- [ ] Unity 2022 LTS installed (2D template)
- [ ] Claude Code installed and logged in
- [ ] Unity MCP (Coplay) connected
- [ ] Git repo initialized
- [ ] `/References` folder created with annotated screenshots
- [ ] Run **Prompt 0** — establish context
- [ ] Run **Prompt 1** — generate GDD
- [ ] Run **Prompt 2** — inspect project
- [ ] Run **Prompt 3** — board data model
- [ ] ...continue one prompt at a time...

---

> **The barrier to making real games has never been lower. All you need is a clear plan, the right AI tools, and the right workflow.**
> — `buildagame.mp4` transcript

[def]: #1-the-method
[def2]: #2-tool-stack
[def3]: #3-project-setup
[def4]: #11-what-not-to-do
[def5]: #10-folder-structure
[def6]: #9-bug-reporting-patterns
[def7]: #8-session-management-rules
[def8]: #7-milestone-commit-ladder
[def9]: #6-9-character-roster-spec
[def10]: #5-the-20-claude-prompts--in-order
