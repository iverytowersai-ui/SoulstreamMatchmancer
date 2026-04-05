# Soulstream Matchmancer — Core Game Loop Design Spec

**Date:** 2026-04-06
**Status:** Approved
**Platform:** Unity (C#)
**Scope:** MVP — Core match-3 engine + Stage 1 (10 levels)

---

## 1. Overview

Soulstream Matchmancer is a high-stakes puzzle RPG combining free-swap match-3 gameplay with a charge-based Magick Dice system. Players match lore-themed tiles on an 8x8 grid, charge a Magick Meter through successful matches, and trigger randomized School of Magick abilities via dice rolls.

### Claude API Role

The Claude API is used as a **development tool** — not embedded in the game. It assists with level generation, balancing parameters, asset prompt creation, and dialogue scripting during the build process.

---

## 2. Architecture

**Hybrid pattern:** Unity C# for all game logic and rendering. Clean separation between core logic (framework-agnostic C# classes) and Unity-specific presentation (MonoBehaviours).

```
BoardController (MonoBehaviour — orchestrator)
├── Board              (8x8 grid state)
├── SwapValidator      (adjacency + match check + sigil override)
├── MatchDetector      (find runs, classify L/T/line patterns)
├── SigilSystem        (create from match + activate effects)
├── GravityHandler     (drop tiles + refill empties)
├── MagickMeter        (charge tracking, cap at 10, reset)
├── DiceSystem         (random school roll, 13 effect types)
├── StoneBlockSystem   (HP tracking, damage per match event)
├── MoveTracker        (deduct 1 per swap, exhaustion check)
├── Scoring            (points + cascade multiplier + 5-star calc)
└── ObjectiveChecker   (win/lose for score/stones/survive)

View Layer (MonoBehaviours — presentation only)
├── BoardView          (grid rendering, event subscriptions)
├── TileView           (sprite + color + animations)
├── StoneBlockView     (cracked states, damage/destroy effects)
├── InputHandler       (click/touch → select → swap)
├── HUDView            (score, moves, meter, objective)
└── GameManager        (scene bootstrap, level loading)
```

---

## 3. Core Game Loop (Authoritative)

### Player Turn Flow

1. Player swaps two adjacent tiles.
2. Board validates the swap.
   - If no match is created, the swap reverses and no move is spent.
   - If a match is created, resolve the turn in this exact order:
     a. Remove matched tiles
     b. Create Sigils for qualifying match patterns:
        - Match 4 → LINE Sigil
        - Match 5 → STAR Sigil
        - L/T shape → NOVA Sigil
     c. Apply match effects
     d. Add score
     e. Charge Magick Meter
     f. Damage adjacent Stone Blocks
     g. Apply gravity so tiles fall into empty spaces
     h. Refill empty spaces with new tiles
     i. Check for cascades and repeat from step (a) until the board is stable
     j. Deduct exactly 1 move for the original successful swap only
3. After the board is fully stable, check the Magick Meter.
   - If the meter is full, trigger exactly one Dice Roll phase for that turn.
   - Dice Roll phase order:
     a. Randomly select one School of Magick effect
     b. Show/apply the effect to the board
     c. Resolve any resulting cascades using the same board resolution rules
     d. Reset the Magick Meter to 0
4. Check win/lose conditions.
   - If objective is met, trigger victory and star calculation
   - If moves are 0 and objective is not met, trigger defeat
   - Otherwise continue to the next turn

### Authoritative Rules

- Cascades are free and never cost extra moves
- Only the original successful swap costs 1 move
- Invalid swaps cost 0 moves
- Magick Meter charge: match-3 = +1, match-4 = +2, match-5+ = +3, Sigil activation = +3 flat
- Magick Meter capacity = 10 (MVP), clamped at max, no overflow
- Meter triggers at most once per turn, then resets to 0
- Sigils activate by swapping with any adjacent tile
- Stone Blocks take 1 damage per adjacent match event, not per tile matched

---

## 4. Tile System

### 4.1 Base Tiles (6 Types)

| Tile Name | Color | Hex Code | Lore Meaning |
|---|---|---|---|
| Port Rune | Cyan/Teal | #00D4FF | Portal/teleportation rune |
| OZONE Mark | Electric Gold | #FFD100 | Secret society surveillance emblem |
| Coven Seal | Deep Violet | #7B2CBF | Witchbreed coven unity symbol |
| Witchbreed Thorn | Crimson Red | #C41E3A | Living thorned identity mark |
| Soulstream Shard | White/Silver | #C0C0C0 | Crystal soul fragment |
| Petsha Charm | Warm Amber | #D4A017 | Gypsy coins/amulets |

### 4.2 Special Tiles (Sigils)

| Sigil | Creation | Activation Effect |
|---|---|---|
| Line | Match 4 in a row | Clears entire row |
| Star | Match 5 in a row | Clears all tiles of the Sigil's type |
| Nova | L/T-shaped match | 3x3 explosion centered on Sigil |

Sigil combinations (Line+Line, Star+Star, etc.) are deferred to post-MVP.

---

## 5. Magick Dice System

Charge-based: matches fill a Magick Meter. When full (10 charges), a die is rolled selecting one of 13 Schools of Magick. Each school has a distinct board effect:

| School | Effect Type |
|---|---|
| Evocation | 3x3 explosion at random position |
| Necromancy | Clear all tiles of one random type |
| Transmutation | Convert up to 6 random tiles |
| Divination | Clear a full row |
| Abjuration | Clear a full column |
| Illusion | Clear 5 random tiles |
| Conjuration | 3x3 burst |
| Enchantment | Clear all tiles of one type |
| Universal | Cross pattern at board center |
| Biomancy | Clear 5 random tiles |
| Technomancy | Clear a full row |
| Alchemy | Convert up to 6 random tiles |
| Fate | Clear 5 random tiles |

After the dice effect resolves (including cascades), the meter resets to 0.

---

## 6. Stone Block System

Stage 1's unique mechanic. Stone Blocks are immovable obstacles with HP.

- Occupy grid cells — tiles cannot occupy the same space
- Take 1 damage per adjacent match event
- Multi-hit stones show visual cracking as HP decreases
- When destroyed, the space opens for gravity fill

---

## 7. Stage 1 Level Design (10 Levels)

| Level | Moves | Stones | Objective | Difficulty |
|---|---|---|---|---|
| 1 | 25 | None | Score 300 | Tutorial — learn matching |
| 2 | 22 | None | Score 400 | Introduce Sigils naturally |
| 3 | 20 | None | Score 500 | Tighter move budget |
| 4 | 22 | 2x 1HP (center) | Clear all stones | Introduce stones |
| 5 | 22 | 4x 1HP (corners) | Clear all stones | Spatial awareness |
| 6 | 20 | 6x 1HP (edges) | Clear all stones | Stone formations |
| 7 | 25 | 4x 2HP (center block) | Clear all stones | Multi-hit stones |
| 8 | 25 | 6x mixed (1-3HP) | Clear all stones | HP variety |
| 9 | 28 | 8x mixed (2-3HP) | Clear all stones | Dense stone field |
| 10 | 30 | 4x 2HP (center) + spreading | Survive 15 turns | Boss — stones spread |

### Star Thresholds

- 3-star system based on score thresholds
- 5-star system rewards efficiency (remaining moves grant bonus points)

---

## 8. Scoring

- Base: 10 points per tile matched
- Cascade bonus: +5 points per tile per cascade depth
- Sigil activation: +50 points flat
- Remaining moves bonus (on victory): +50 points per unused move
- Star calculation based on 5 score thresholds per level

---

## 9. Objective Types (MVP)

| Type | Win Condition | Lose Condition |
|---|---|---|
| ReachScore | Score >= target | Moves exhausted |
| ClearAllStones | All stones destroyed | Moves exhausted |
| Survive | Survived N turns | Moves exhausted |

---

## 10. Project Structure

```
Assets/Scripts/
├── Core/           BoardController, GameManager, Stage1Data, LevelConfig, enums, Tile, GridPosition
├── Board/          Board (grid state), SwapValidator, GravityHandler
├── Match/          MatchDetector, MatchInfo
├── Sigils/         SigilSystem
├── Meter/          MagickMeter
├── Dice/           DiceSystem
├── StoneBlocks/    StoneBlockSystem
├── Objectives/     MoveTracker, Scoring, ObjectiveChecker
└── View/           BoardView, TileView, StoneBlockView, InputHandler, HUDView
```

26 C# files total. Core logic is framework-agnostic; View layer is Unity MonoBehaviours.

---

## 11. Assumptions

1. Gravity: Row 0 = top, Row 7 = bottom. Tiles fall downward.
2. Line Sigil clears the entire row for MVP (direction tracking deferred).
3. Star Sigil auto-targets the Sigil's stored tile type (no player choice for MVP).
4. Sigil chains: activating one Sigil that clears another chains recursively.
5. Board initializes match-free so the player starts clean.
6. Animation timing uses placeholder WaitForSeconds values — presentation layer overrides.
7. Level 10 boss "stone spreading" mechanic needs a BoardController extension (hook exists, logic deferred).

---

## 12. Not In Scope (MVP)

- Sigil combination effects (Line+Line, Star+Star, etc.)
- Meter overflow / multi-trigger per turn
- In-game Claude API integration
- UI screens beyond HUD (shop, lore, gear, achievements)
- Audio/music
- Particle effects beyond placeholder animations
- Stages 2-10 (50+ additional level mechanics)
- Save/load system
- Online features
