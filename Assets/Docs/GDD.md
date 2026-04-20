# Soulstream Matchmancer - Game Design Document

## 1. Overview
Soulstream Matchmancer is a portrait-orientation mobile match-3 puzzle RPG. The game combines deep dark fantasy narrative with satisfying, tactical match-3 combat.

## 2. Core Constraints
- Portrait mobile orientation
- 9x9 board size
- Adjacent tile swap only (no diagonal)
- Match 3+ horizontally or vertically
- Moves-based gameplay (out of moves = lose condition)

## 3. Tile System
### Base Tiles (6 Types)
1. Port Rune (Conjuration)
2. OZONE Mark (Technomancy)
3. Coven Seal (Abjuration)
4. Witchbreed Thorn (Biomancy)
5. Soulstream Shard (Universal)
6. Petsha Charm (Fate)

### Special Tiles (Sigils)
- Line Sigil (Match-4 in a line) - Clears a row or column
- Star Sigil (Match-5 in a line) - Clears all tiles of a chosen color
- Nova Sigil (L/T Shape Match) - 3x3 explosion area clear

### Blocker Tiles
- Glass Blocker (1-2 hits): Sits on top of tiles, cleared by adjacent matches.
- Chain Blocker (1 hit): Locks a tile from being swapped.
- Stone/Armor Blocker (2-3 hits): Solid block that occupies a cell.

## 4. Character System
The game features a roster of 9 characters (6 unlockable, 3 starters).
Each character has:
- A unique Ultimate Ability (charged by matching tiles).
- A unique Passive Ability (changes board behavior).
- A specific tile affinity.

## 5. Narrative / World
- Setting: Dark fantasy / tech-arcana hybrid aesthetic.
- Chapter 1 Theme: Modern urban storm / OZONE conspiracy.
