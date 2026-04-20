**SOULSTREAM**

**MATCHMANCER**

GAME DESIGN DOCUMENT

Mobile Game  |  Android & iOS

Version 1.0  |  April 2026

Developer: Ivery Towers LLC

Engine: Unity 2022 LTS (C#)

**CONFIDENTIAL**

# **TABLE OF CONTENTS**

	1. Executive Summary

	2. Game Overview

	3. Target Platform & Technical Specifications

	4. Core Gameplay Mechanics

	5. Tile System

	6. Special Tiles (Sigils)

	7. Blocker System

	8. Character System

	9. Character Roster

	10. Progression & Level Design

	11. Scoring & Combo System

	12. UI/UX Design

	13. Visual Art Direction

	14. Color Palette & Visual Language

	15. Audio Design

	16. Lore & World Building

	17. Factions of the Soulstream Universe

	18. Monetization Strategy

	19. Art Asset Pipeline & AI Tools

	20. Technical Architecture

	21. Development Roadmap

	22. MVP Scope & Priorities

	23. Future Content Roadmap

	24. Appendix: Asset Checklist

# **1. EXECUTIVE SUMMARY**

Soulstream: Matchmancer is a dark-fantasy match-3 puzzle adventure game designed for mobile platforms (Android and iOS). Set in the Soulstream universe created by author Geo Ivery, the game blends strategic tile-matching gameplay with character-driven abilities, progressive blocker mechanics, and deep supernatural lore.

Players command powerful Supernatural heroes, each with unique Ultimate abilities and passive skills, as they navigate through a 25-level chapter of increasingly challenging puzzles. The game targets the massive casual-puzzle mobile audience while differentiating itself through its dark, prestige-drama aesthetic and rich character world.

| **Attribute** | **Detail** |
| --- | --- |
| Game Title | Soulstream: Matchmancer |
| Genre | Match-3 Puzzle Adventure |
| Platform | Android (Google Play) & iOS (App Store) |
| Engine | Unity 2022 LTS (C#) |
| Orientation | Portrait |
| Target Audience | Casual to mid-core gamers, ages 16+ |
| Art Style | Dark fantasy, tech-arcana fusion, HBO prestige tone |
| Monetization | Free-to-play (monetization deferred post-MVP) |
| Developer | Ivery Towers LLC |
| Estimated Dev Time | 8-12 weeks (solo dev with AI assistance) |

# **2. GAME OVERVIEW**

## **2.1 High Concept**

Match tiles. Charge heroes. Unleash Magick. Matchmancer combines the proven addictiveness of match-3 puzzles with the character depth of a gacha RPG, all wrapped in a dark supernatural universe where gods walk among mortals and secret societies hunt the gifted.

## **2.2 Elevator Pitch**

*Imagine Candy Crush meets a dark-fantasy anime. Every match charges your hero**'**s power. Swap tiles to create devastating combos, break through magical blockers, and activate each character**'**s unique Ultimate ability to clear the board in spectacular fashion. Nine heroes drawn from the Soulstream novel universe, each with distinct playstyles and lore-rich backstories.*

## **2.3 Core Pillars**

- Strategic Depth: Every swap matters. Tile placement, combo planning, and Ultimate timing create meaningful decisions within a simple-to-learn framework.

- Character Fantasy: Players bond with heroes who have distinct personalities, visual identities, and mechanical playstyles that express their Soulstream lore.

- Dark Elegance: A prestige visual identity (obsidian, gold kintsugi, contained glow) that stands out from bright, cartoonish competitors in the match-3 space.

- Accessible Mastery: Easy for anyone to pick up, with enough depth to reward skilled play across hundreds of levels.

## **2.4 Unique Selling Points**

- Lore-integrated tile set where every tile represents a real Soulstream universe concept (Port Runes, Coven Seals, Soulstream Shards, etc.)

- Character Ultimate system that transforms match-3 from passive to active: charge your hero, then tap to unleash area clears, type conversions, or cross-pattern destruction.

- Dark fantasy art direction using AI-generated assets with a strict visual bible, creating a cohesive HBO-quality aesthetic rarely seen in casual mobile games.

- Balanced character design where premium characters are cooler and more specialized, never objectively stronger than free characters.

# **3. TARGET PLATFORM ****&**** TECHNICAL SPECIFICATIONS**

## **3.1 Supported Platforms**

| **Platform** | **Min OS** | **Distribution** | **Notes** |
| --- | --- | --- | --- |
| Android | Android 7.0 (API 24)+ | Google Play Store | ARM64 primary, ARMv7 fallback |
| iOS | iOS 14.0+ | Apple App Store | iPhone 8 and later |

## **3.2 Technical Specifications**

| **Specification** | **Detail** |
| --- | --- |
| Engine | Unity 2022 LTS |
| Language | C# |
| Rendering | URP (Universal Render Pipeline) |
| Target FPS | 60 FPS |
| Screen Orientation | Portrait only |
| Resolution | 1080 x 1920 (16:9 base, adaptive scaling) |
| Min RAM | 2 GB |
| Install Size Target | < 150 MB |
| Aspect Ratio Support | 16:9, 18:9, 19.5:9, 20:9 (safe area handling) |
| Input Method | Touch (tap + drag/swipe) |
| Network | Offline-first (no internet required for core gameplay) |
| Save System | PlayerPrefs-based local persistence |
| Audio | Background music + 11 SFX types with combo pitch scaling |

## **3.3 Performance Targets**

- Consistent 60 FPS during match cascades and VFX

- Sprite Atlas for draw call reduction (< 20 draw calls during gameplay)

- Object pooling for tiles, particles, and score popups

- Memory budget: < 300 MB RAM during peak gameplay

- Battery-conscious: minimize GPU overdraw with dark UI backgrounds

# **4. CORE GAMEPLAY MECHANICS**

## **4.1 Board Configuration**

| **Parameter** | **Value** |
| --- | --- |
| Grid Size | 9 x 9 (81 cells) |
| Orientation | Portrait, centered on screen |
| Tile Count | 6 base tile types |
| Swap Rules | Adjacent tiles only (no diagonal) |
| Cascade Direction | Top-to-bottom gravity |
| Refill Direction | New tiles spawn from top edge |

## **4.2 Core Loop**

The fundamental gameplay loop follows this sequence:

- Player swaps two adjacent tiles.

- System validates swap: does it create a match of 3 or more?

- If valid: matched tiles are cleared, score awarded, and charge meter increases (+1 per match group).

- Gravity pulls remaining tiles downward to fill gaps.

- New tiles spawn from the top to fill empty cells.

- System checks for cascade matches (chain reactions from falling tiles).

- Cascade matches repeat steps 3-6 with combo multiplier applied.

- When no more cascades exist, player's move count decrements by 1.

- Player either achieves all objectives (WIN) or runs out of moves (LOSE).

If a swap does not create a match, the tiles animate back to their original positions (invalid swap bounce).

## **4.3 Match Rules**

- Minimum match: 3 tiles of the same type in a horizontal or vertical line.

- Extended matches (4+ tiles) generate Special Tiles (Sigils).

- L-shaped and T-shaped matches generate Nova Sigils.

- No diagonal matching.

- Board generates with zero pre-existing matches at level start.

## **4.4 Deadlock Detection ****&**** Resolution**

If no valid moves exist on the board (deadlock), the system automatically shuffles all tiles while preserving blockers and special tiles. A visual shuffle animation plays to communicate this to the player.

## **4.5 Win ****&**** Lose Conditions**

| **Condition** | **Trigger** | **Result** |
| --- | --- | --- |
| Win | All level objectives completed before moves expire | Star rating (1-3), score display, Next Level unlocked |
| Lose | Moves reach 0 with objectives incomplete | Retry or Quit options displayed |

# **5. TILE SYSTEM**

## **5.1 Base Tile Set (6 Types)**

Every tile in Matchmancer is lore-accurate to the Soulstream universe. Each represents a real supernatural concept, faction, or artifact. All tiles are icon-based and designed for clarity at 60x60 pixels on mobile screens.

| **Tile Name** | **Color** | **Hex Code** | **Lore Meaning** |
| --- | --- | --- | --- |
| Port Rune | Cyan / Teal | #00D4FF | Portal/teleportation rune; Gemini's travel power |
| OZONE Mark | Electric Gold | #FFD100 | Secret society surveillance emblem; tech dominance |
| Coven Seal | Deep Violet | #7B2CBF | Witchbreed coven unity symbol; 9 covens united |
| Witchbreed Thorn | Crimson Red | #C41E3A | Living thorned identity mark; organic supernatural |
| Soulstream Shard | White / Silver | #C0C0C0 | Crystal soul fragment; divine cosmic energy |
| Petsha Charm | Warm Amber | #D4A017 | Gypsy coins/amulets; probability & protection magic |

## **5.2 Tile Technical Specifications**

| **Property** | **Value** |
| --- | --- |
| Generation Size | 512 x 512 pixels |
| Display Size | ~60 x 60 pixels on device |
| Format | PNG with transparent background |
| Style | Icon-based; cracked obsidian, forged metal, contained glow |
| Negative Constraints | No text, no watermarks, no cartoon, no bloom |
| Readability Rule | Must be identifiable at 32px for colorblind testing |

# **6. SPECIAL TILES (SIGILS)**

Special tiles are created when players make extended matches (4+ tiles). They provide powerful board-clearing effects and are central to high-score strategies.

| **Sigil** | **Creation Condition** | **Activation Effect** | **Visual** |
| --- | --- | --- | --- |
| Line Sigil | Match 4 tiles in a straight line | Clears entire row OR column (direction based on match) | Horizontal/vertical energy beam overlay |
| Star Sigil | Match 5 tiles in a straight line | Clears ALL tiles of one chosen type from the board | Radiant five-pointed star with pulsing glow |
| Nova Sigil | L-shaped or T-shaped match | 3x3 explosion centered on the sigil position | Explosive burst icon with shockwave rings |

## **6.1 Sigil Combination Effects (Future Feature)**

When two Sigils are swapped into each other, they produce enhanced combination effects. This system is designed but deferred to post-MVP:

- Line + Line: Clears both a full row AND a full column simultaneously.

- Line + Nova: Clears 3 rows AND 3 columns in a wide cross pattern.

- Star + Any: Converts all tiles of the Star's type into the other Sigil type, then activates them all.

- Star + Star: Clears the entire board (ultimate combo).

- Nova + Nova: 5x5 mega explosion.

# **7. BLOCKER SYSTEM**

Blockers add strategic depth to levels by restricting tile movement and requiring focused clearing strategies. Three blocker types appear in the launch chapter.

| **Blocker** | **Durability** | **Behavior** | **Visual** |
| --- | --- | --- | --- |
| Glass | 1-2 layers | Transparent overlay on tile; cracks with adjacent match damage; tile beneath cannot be swapped until glass is broken | Ice-blue translucent overlay with progressive cracks |
| Chain / Lock | 1 hit | Prevents tile from being swapped; removed when an adjacent tile matches or clears nearby | Dark iron chains in X-pattern; breaks with metallic shatter |
| Stone / Armor | 2-3 hits | Cannot be swapped or matched; requires multiple adjacent match hits; shows progressive damage states | Cracked obsidian shield; shows fracture lines per hit |

## **7.1 Blocker Interaction Rules**

- Blockers are placed on the board during level initialization via LevelData ScriptableObjects.

- Match-adjacent clearing damages blockers: each match that includes a tile adjacent to a blocker reduces blocker durability by 1.

- Special tile activations (Line, Star, Nova sigils) also damage blockers they pass through.

- Character ultimates interact with blockers based on the character's specific ability design.

- Kaery's passive (Sealbreaker) deals +1 extra hit to Stone/Armor blockers when specials activate, up to 3 times per level.

## **7.2 Blocker Introduction Schedule**

| **Level Range** | **New Blocker** | **Teaching Approach** |
| --- | --- | --- |
| Levels 1-10 | None | Pure matching and Sigil creation mastery |
| Levels 11-15 | Glass | Simple 1-layer glass; teaches adjacency damage |
| Levels 16-20 | Chain / Lock | Introduces movement restriction concept |
| Levels 21-25 | Stone / Armor + Mixed | Multi-hit durability; all blockers in combination |

# **8. CHARACTER SYSTEM**

## **8.1 Overview**

Characters are the emotional core of Matchmancer. Each hero has a unique Ultimate ability (activated skill) and a Passive ability (always-on bonus). Players select one character before entering a level, and that character's abilities shape the player's strategy throughout the puzzle.

## **8.2 Charge System**

| **Mechanic** | **Detail** |
| --- | --- |
| Charge Source | Every match group (including cascades) grants +1 Charge |
| Charge Cost | Character-specific (10 for Blue/Crux, 12 for Kaery) |
| Activation | Tap the character portrait when charge is full |
| Targeting | Some Ultimates require tapping a board position; others are automatic |
| Reset | Charge resets to 0 after Ultimate use; begins recharging immediately |
| Visual | Charge meter fills with character's signature color glow |

## **8.3 Balance Philosophy**

All characters are balanced around a consistent power budget of approximately 16-18 tiles of value per Ultimate activation. No character provides an auto-win advantage. Premium characters are designed to be cooler, more specialized, and more satisfying to play, but never objectively stronger than free characters. This approach ensures that free players never feel disadvantaged, while premium characters deliver aspiration and fantasy fulfillment.

# **9. CHARACTER ROSTER**

## **9.1 Blue  -  Soulstream Pulse Caster**

| **Attribute** | **Detail** |
| --- | --- |
| Tier | Free (Starter / Protagonist) |
| Faction | Mancers (Immortal gods with storm/lightning powers) |
| Age | 19 years old |
| Color Identity | Deep Ocean Blue (#003366) with white-gold Soulstream glow |
| Appearance | Olive skin, dark tousled hair, bright blue eyes, bad-boy energy, black leather jacket, no shirt, fingerless gloves |
| Ultimate (Cost: 10) | Soulstream Pulse: Clear a 4x4 area at the tapped location (max 16 tiles) |
| Passive | First Spark: First match each level grants +1 bonus charge (triggers once) |
| Playstyle | Direct damage; straightforward power; ideal for beginners |

## **9.2 Crux  -  Port Cross Navigator**

| **Attribute** | **Detail** |
| --- | --- |
| Tier | Earnable early (soft unlock through progression) |
| Faction | Petsha (Ancient Gypsy family; probability/density shifters) |
| Age | 14 years old |
| Color Identity | Warm Amber-Gold (#D4A017) with probability shimmer energy |
| Appearance | Warm brown skin, Native American/Romani features, dark hair, genuine smile, carries coins/pebbles, lean agile build |
| Ultimate (Cost: 10) | Port Cross: Clear 1 full row + 1 full column in a cross at the tapped position (max ~17 tiles) |
| Passive | Pathfinder: Once per level, first Match-4 has increased chance to become a Line Sigil |
| Playstyle | Precision cross-clear; more skill-based targeting than Blue |

## **9.3 Kaery  -  Oracle Enchantress**

| **Attribute** | **Detail** |
| --- | --- |
| Tier | Earnable (completes the starter trio) |
| Faction | Witchbreed (Coven of Unity leader; truth-teller; Banshee) |
| Age | 20 years old |
| Color Identity | Deep Violet (#7B2CBF) with warm gold eye glow |
| Appearance | Light brown hair with soft violet bangs, sun-kissed skin, golden amber eyes, confident/honest energy, dark fitted top with rune embroidery |
| Ultimate (Cost: 12) | Glamour Shift: Choose tile type A, convert up to 8 scattered copies into tile type B |
| Passive | Sealbreaker: Special tiles deal +1 extra hit to Stone/Armor blockers, up to 3 times per level |
| Playstyle | Setup/conversion strategy; no direct clear; rewards planning and board reading |

## **9.4 Future Characters (Locked at Launch)**

Six additional characters are designed and shown as locked silhouettes with preview cards at launch. No purchase flow is implemented in MVP.

| **Character** | **Title** | **Rarity** | **Faction** | **Specialty** |
| --- | --- | --- | --- | --- |
| Kimmy | Feral Shift Spiraar | Rare | Spiraar (Shapeshifters) | Tempo/combo specialist; child shapeshifter |
| Surge-Overload | Technomancer | Epic | OZONE (Secret Society) | Board rewrite abilities; elite hacker |
| Mr. Violet | Gaia Guardian | Rare | Ancient Creature | Support/blocker damage specialist |
| Guardian | OZONE Assassin | Epic | OZONE (Secret Society) | Objective blocker killer; military specialist |
| Yanoro | Shadow Ambusher | Legendary | Vampyl (Vampires) | Trap/delayed burst; darkness user |
| Chanel | Nightmare Empress | Legendary | Witchbreed (Dark) | Corruption engine; blood-red nightmare power |

# **10. PROGRESSION ****&**** LEVEL DESIGN**

## **10.1 Chapter Structure**

The game launches with Chapter 1: Stormbreak, featuring 25 levels with a modern urban rooftop and OZONE conspiracy theme. The map screen presents a linear path of 25 nodes, each unlocked sequentially by beating the previous level.

## **10.2 Difficulty Curve (Chapter 1)**

| **Level Range** | **Focus** | **New Mechanics Introduced** | **Difficulty** |
| --- | --- | --- | --- |
| 1-5 | Basic Matching | Core swap + match; Line Sigil introduction | Tutorial / Easy |
| 6-10 | Sigil Mastery | Star Sigil + Nova Sigil teaching | Easy / Medium |
| 11-15 | Glass Blockers | 1-2 layer glass; adjacency damage concept | Medium |
| 16-20 | Chain Blockers | Movement restriction; multi-threat boards | Medium / Hard |
| 21-25 | Full Challenge | Stone/Armor + all blockers mixed together | Hard |

## **10.3 Level Objective Types**

- Collect X Tiles: Gather a specific number of a tile type (e.g., 'Collect 20 Port Runes').

- Break X Blockers: Destroy a set number of Glass, Chain, or Stone blockers.

- Reach Target Score: Achieve a minimum score within the move limit.

- Combined Objectives: Later levels use multiple objectives simultaneously.

## **10.4 Star Rating System**

Each level awards 1-3 stars based on the player's final score. Star thresholds are defined per level in LevelData ScriptableObjects. Stars serve as a progression metric and future gating mechanism for content unlocks.

## **10.5 Level Data Architecture**

Each level is defined as a Unity ScriptableObject containing: level number, board dimensions (default 9x9), allowed tile types (subset of 6), blocker placements with positions and durability, move limit, objective list, and star score thresholds for 1-star, 2-star, and 3-star ratings.

# **11. SCORING ****&**** COMBO SYSTEM**

## **11.1 Base Scoring**

| **Match Type** | **Base Points** |
| --- | --- |
| 3-tile match | 50 points |
| 4-tile match (creates Line Sigil) | 100 points |
| 5-tile match (creates Star Sigil) | 200 points |
| L/T match (creates Nova Sigil) | 150 points |
| Sigil activation bonus | +100 points per Sigil triggered |
| Blocker destroyed | +75 points per blocker |

## **11.2 Combo Multiplier**

Cascading chain reactions apply an escalating combo multiplier to all points earned during the cascade sequence. The multiplier resets when the player makes their next manual swap.

| **Cascade Depth** | **Multiplier** |
| --- | --- |
| 1st cascade (initial match) | 1.0x |
| 2nd cascade | 1.5x |
| 3rd cascade | 2.0x |
| 4th cascade | 2.5x |
| 5th+ cascade | 3.0x (cap) |

# **12. UI/UX DESIGN**

## **12.1 Screen Flow**

- Boot Screen: Splash logo and loading animation.

- Main Menu / Map Screen: Chapter progression with 25 node path. Character selection accessible from here.

- Roster Screen: 9-character grid showing unlocked heroes and locked silhouettes.

- Gameplay Screen: The core puzzle board with HUD overlay.

- Win Panel: Star rating (1-3), final score, Next Level and Replay buttons.

- Lose Panel: Retry and Quit buttons with moves-used display.

- Pause Menu: Volume sliders, resume, and quit options.

## **12.2 Gameplay HUD Layout (Portrait)**

| **Position** | **Element** | **Function** |
| --- | --- | --- |
| Top-Left | Move Counter | Shows remaining moves; decrements per valid swap |
| Top-Right | Pause Button | Opens pause overlay with settings |
| Top-Center | Objective Icons | Shows current level objectives with progress counts |
| Below Top Bar | Score Display | Real-time score with animated counter |
| Center | 9x9 Game Board | The puzzle grid (primary interaction area) |
| Bottom-Left | Character Portrait | Selected hero with expression changes |
| Bottom-Center | Charge Meter | 0-10/12 fill bar showing Ultimate readiness |
| Bottom-Right | Booster Slots (3) | Bomb, Line, Swap icons (optional, may be post-MVP) |
| Overlay | Combo Banner | Displays during cascades: 1x, 1.5x, 2x, etc. |

## **12.3 UI Technical Requirements**

- Canvas scaling: Scale With Screen Size (1080 x 1920 reference resolution).

- Text rendering: TextMeshPro for all UI text.

- Safe area handling for notched displays (iPhone X+, modern Android).

- Animated score counter with punch scaling on increment.

- Star progress bar with punch animation on threshold crossing.

# **13. VISUAL ART DIRECTION**

## **13.1 Global Aesthetic**

Matchmancer's visual identity is dark fantasy with tech-arcana fusion, evoking the tone of an HBO prestige drama rather than a typical bright, cartoony mobile game. The world feels ancient yet modern, where supernatural power manifests through materials like cracked obsidian, forged metals, ritually carved stone, and gold kintsugi veins that trace through darkness.

## **13.2 Core Visual Rules**

- Magick is organic: Flowing, warm-edged light with soft glow (Witchbreed, Mancers). Never harsh or digital.

- Technology is angular: Cold, clinical, and sharp-edged (OZONE). Surveillance drones, satellite arrays, digital interference.

- Glow means contained power: Light from supernatural sources (runes, Magick energy, candlelight). Minimal bloom. Never decorative.

- Readability first: Icons and tiles must be identifiable at 32-60px on phone screens. Clarity over detail.

- Darkness is the canvas: Void black and deep gray backgrounds let supernatural light command attention.

- Imperfection is authenticity: Weathering, patina, cracks, flyaway hairs. Nothing should look pristine or 'AI-generated.'

## **13.3 Material Palette**

- Cracked obsidian: Primary surface material for UI frames, board backgrounds, blocker bases.

- Forged metal: Iron, bronze, and steel for chains, locks, weapon elements.

- Ritually carved stone: Ancient rune-inscribed surfaces for tile backgrounds.

- Gold kintsugi: Molten gold veins tracing through cracked surfaces, symbolizing beauty in imperfection.

- Bone ivory: Carved bone accents for decorative UI borders and character accessories.

# **14. COLOR PALETTE ****&**** VISUAL LANGUAGE**

## **14.1 Primary Color Palette**

| **Color Name** | **Hex Code** | **Usage** |
| --- | --- | --- |
| Void Black | #000000 to #1A1A1A | Backgrounds, UI frames, negative space |
| Neon Cyan | #00D4FF | Soulstream/portal energy, UI accents, Blue's identity |
| Deep Violet | #7B2CBF | Witchbreed/coven Magick, Kaery's identity |
| Crimson Red | #C41E3A | Witchbreed thorns, danger, blood magic |
| Electric Gold | #FFD100 | OZONE tech, UI highlights, rarity indicators |
| Warm Amber | #D4A017 | Petsha magic, Crux's identity, patina gold |
| Silver White | #C0C0C0 / #D9D9D9 | Divine light, Soulstream shards, mystical glow |
| Bone Ivory | #E8E8E0 | Carved bone accents, text on dark backgrounds |

## **14.2 Global Negative Prompt (for AI Art Generation)**

Applied to every asset generation to maintain visual consistency:

*watermark, text, cartoon, anime, chibi, cute, bright neon, sparkles, glitter, chrome, plastic, smooth gradients, soft pastel, flat colors, clipart, stock photo, bloom, lens flare, vignette, noise, grain*

# **15. AUDIO DESIGN**

## **15.1 Audio System Architecture**

A singleton AudioManager handles all audio through an event-driven system. Background music fades in/out between scenes, and SFX are wired to game actions for responsive feedback.

## **15.2 Sound Effect Types (11 SFX)**

| **SFX** | **Trigger** | **Description** |
| --- | --- | --- |
| Tile Select | Player taps a tile | Subtle crystal chime |
| Valid Swap | Tiles successfully swap positions | Satisfying whoosh |
| Invalid Swap | Swap attempt creates no match | Soft thud/buzz |
| Match Clear | 3+ tiles are matched and cleared | Harmonic pop with energy release |
| Cascade | Chain reaction match occurs | Escalating harmonic sequence |
| Sigil Create | Special tile (Line/Star/Nova) forms | Power charge crystallization sound |
| Sigil Activate | Special tile effect triggers | Explosive energy burst |
| Blocker Crack | Blocker takes damage | Stone/glass cracking |
| Blocker Break | Blocker is destroyed | Shattering with energy release |
| Ultimate Charge | Charge meter reaches full | Power-ready harmonic swell |
| Ultimate Activate | Player triggers Ultimate ability | Character-specific power surge |

## **15.3 Combo Pitch Scaling**

During cascade chains, each successive match plays its SFX at an increasingly higher pitch, creating an ascending musical sequence that communicates combo momentum to the player. The pitch resets on the next manual swap.

## **15.4 Volume Persistence**

Music and SFX volume levels are saved via PlayerPrefs and persist between sessions. The pause menu provides separate sliders for music and sound effects.

# **16. LORE ****&**** WORLD BUILDING**

## **16.1 The Soulstream Universe**

Matchmancer is set in the Soulstream universe created by author Geo Ivery. In this world, Supernaturals (beings with extraordinary abilities) live hidden among humanity. The cosmic force known as the Soulstream connects all living souls, and those who can tap into it wield immense power. Five major factions of Supernaturals exist, each with distinct cultures, abilities, and agendas.

## **16.2 The Soulstream**

The Soulstream is the river of all souls, a cosmic force that permeates reality. Individual souls are crystallized fragments of this force (represented in-game as Soulstream Shards). Mancers, the most powerful Supernaturals, can manipulate the Soulstream directly, giving them godlike abilities over elements, energy, and reality itself.

## **16.3 Chapter 1 Setting: Stormbreak**

The first chapter takes place against a modern urban backdrop, specifically rooftops and hidden meeting places where OZONE's conspiracy against Supernaturals comes to light. The visual environment blends modern architecture with supernatural energy, creating a world where ancient magic pulses beneath neon city lights.

# **17. FACTIONS OF THE SOULSTREAM UNIVERSE**

| **Faction** | **Nature** | **Abilities** | **Key Character** |
| --- | --- | --- | --- |
| Mancers | Immortal gods; most powerful Supernaturals | Storm, lightning, Soulstream manipulation, elemental mastery | Blue (Soulstreamancer) |
| Witchbreed (Wicasht) | Magick users organized in covens | Organic spellcasting, truth-sensing (Banshee), living thorns | Kaery (Coven of Unity leader) |
| Spiraar | Shapeshifters; animal-human hybrids | Animal transformation, feral instincts, nature communion | Kimmy (child shapeshifter) |
| Vampyl | Blood drinkers; seductive predators | Shadow manipulation, enhanced speed, blood magic | Yanoro (Shandor family) |
| Jreamers | Supernatural hunters; warrior class | Military precision, enhanced combat, tactical strategy | Guardian (OZONE operative) |

## **17.1 Key Organizations**

### **OZONE**

The most powerful secret society in the Soulstream universe. OZONE hunts and assassinates Supernaturals using cutting-edge surveillance technology and military operations. Their aesthetic is cold, clinical, and angular, representing technology's opposition to organic Magick. In-game, the OZONE Mark tile represents their surveillance dominance.

### **Petsha Family**

An ancient Gypsy family descended from Romani travelers, the Petsha are masters of stealth, probability manipulation, and density shifting. Crux carries coins and pebbles as weapons and tools, using probability magic to bend outcomes in his favor. The Petsha Charm tile represents their protective amulet tradition.

### **Coven of Unity**

Founded by Kaery, the Coven of Unity brings together 9 diverse Witchbreed covens that previously operated independently. This unprecedented alliance represents a new era for the Witchbreed, uniting different magical traditions under a shared purpose. The Coven Seal tile embodies this unity.

# **18. MONETIZATION STRATEGY**

## **18.1 Launch Approach (MVP)**

No monetization is implemented in the MVP release. The focus is entirely on delivering a polished, complete gameplay experience. Blue is free forever as the game's protagonist and face. Crux and Kaery are earnable through normal progression. Six additional characters are shown as locked preview cards with no purchase flow.

## **18.2 Post-Launch Monetization Philosophy**

The guiding principle is: Premium equals cooler and more specialized, never objectively stronger.

| **Revenue Stream** | **Description** | **Priority** |
| --- | --- | --- |
| Character Unlocks | Premium characters with unique visual polish and specialized playstyles | High |
| Cosmetic Skins | Alternate visual themes for characters and tiles | Medium |
| Battle Pass | Seasonal progression track with rewards | Medium |
| Booster Packs | Consumable items (extra moves, specific sigils) | Low |
| Ad-Supported Rewards | Optional rewarded video ads for free currency/boosters | Low |

## **18.3 Anti-Pay-to-Win Commitment**

- No character clears more than ~16-18 tiles of power per Ultimate, regardless of rarity.

- Free characters (Blue, Crux, Kaery) can complete 100% of game content.

- Premium characters offer different strategic options, not superior ones.

- No energy system or play-gating in MVP.

# **19. ART ASSET PIPELINE ****&**** AI TOOLS**

## **19.1 AI Art Generation Tools**

| **Tool** | **Best For** | **Key Settings** |
| --- | --- | --- |
| Leonardo AI (SDXL) | Tiles, blockers, special tiles, VFX sprites | Guidance 7-9, 50 steps, 512x512 output |
| Midjourney V7 / Niji 7 | Character portraits, splash art | --ar 2:3 --style raw --v 7 or --niji 7 --s 400 |
| DALL-E 3 | UI backgrounds, environmental scenes | Natural language prompts, 1080x1920 |
| Stable Diffusion 2.1 | Alternative/backup generation, variety | Checkpoint-based, custom models available |

## **19.2 Asset Specifications**

| **Asset Type** | **Resolution** | **Format** | **Count (MVP)** |
| --- | --- | --- | --- |
| Base Tile Icons | 512 x 512 | PNG (transparent bg) | 6 |
| Special Tile Icons | 512 x 512 | PNG (transparent bg) | 3 |
| Blocker Overlays | 512 x 512 | PNG (transparent bg) | 6 (2 glass + 1 chain + 3 stone states) |
| Character Portraits | 1024 x 1536 | PNG | 3 (+ 6 silhouettes) |
| Ultimate Button Icons | 256 x 256 | PNG (transparent bg) | 3 |
| Passive Ability Icons | 128 x 128 | PNG (transparent bg) | 3 |
| Booster Icons | 256 x 256 | PNG (transparent bg) | 3 |
| Background Screens | 1080 x 1920 | PNG/JPG | 5 |
| App Icon | 1024 x 1024 | PNG | 1 |
| Loading Screen | 1080 x 1920 | PNG | 1 |
| Match VFX Sprites | 256 x 256 | PNG (transparent bg) | 6 |
| Ultimate VFX Sprites | 512 x 512 | PNG (transparent bg) | 3 |

**Total Unique Assets for MVP: ****~42 assets**

## **19.3 Prompt Engineering Principles**

- Global style lock applied to every prompt (dark fantasy, obsidian/forged metal, contained glow).

- One variable per iteration: change only lighting OR pose OR material per generation.

- 4 variations per prompt, select the best lighting match.

- Readability over detail: tile icons are visual communication, not paintings.

- Imperfection injection for portraits: flyaway hairs, skin pores, lens artifacts eliminate the AI look.

# **20. TECHNICAL ARCHITECTURE**

## **20.1 Unity Project Folder Structure**

Assets/

- Docs/ - GDD, TechPlan, Milestones

- References/ - Annotated screenshots, design notes

- Scripts/Core/ - BoardManager, Tile, MatchDetector, ResolutionPipeline, ScoreManager

- Scripts/Specials/ - SpecialTile, SpecialFactory

- Scripts/Blockers/ - BlockerBase, Glass, Chain, Stone

- Scripts/Characters/ - CharacterData, CharacterManager, UltimateSystem

- Scripts/Levels/ - LevelData, LevelLoader, ObjectiveSystem

- Scripts/UI/ - HUDController, WinPanel, LosePanel, RosterScreen

- Scripts/Data/ - BoardConfig, TileDatabase

- ScriptableObjects/ - BoardConfig, Character assets, Level assets

- Prefabs/ - Tiles, Blockers, Characters, UI

- Scenes/ - Boot, Gameplay, Map, Roster

- Art/ - Tiles, Characters, Backgrounds, UI

## **20.2 Core Systems Build Order**

| **#** | **System** | **Description** | **Dependencies** |
| --- | --- | --- | --- |
| 1 | Data Model | TileType enum, GridPosition, BoardCell, BoardConfig | None |
| 2 | Board Generation | 9x9 grid with no starting matches | Data Model |
| 3 | Board Visuals | Tile prefab instantiation and positioning | Board Generation |
| 4 | Input & Swapping | Tap/select and adjacent-only swap logic | Board Visuals |
| 5 | Match Detection | Horizontal + vertical 3+ detection with overlap | Swap System |
| 6 | Resolution Loop | Clear > Gravity > Refill > Cascade repeat | Match Detection |
| 7 | Animation System | Swap, pop, fall, fade-in animations | Resolution Loop |
| 8 | Score System | Base points + combo multiplier | Resolution Loop |
| 9 | HUD | Moves, objectives, charge, portrait, buttons | Score System |
| 10 | Objective System | Collect/break/score tracking per level | HUD |
| 11 | Win/Lose Flow | Condition checks, panels, star rating | Objective System |
| 12 | Special Tiles | Line/Star/Nova sigil creation and activation | Resolution Loop |
| 13 | Blockers | Glass/Chain/Stone with health tracking | Special Tiles |
| 14 | Character Powers | Charge meter, Ultimate activation, 3 heroes | Blockers |
| 15 | Level Data | ScriptableObject levels + LevelLoader | Character Powers |
| 16 | Map Screen | 25-node linear progression path | Level Data |
| 17 | Roster Screen | 9-character grid with lock/unlock states | Map Screen |
| 18 | Save System | PlayerPrefs-based persistence | All Systems |

## **20.3 Key Design Patterns**

- Singleton pattern: GameManager, ScoreManager, LevelManager, AudioManager.

- Object pooling: Tiles, particles, score popups (mobile performance critical).

- Event-driven architecture: Match events trigger VFX, audio, score, and UI updates.

- ScriptableObject data: All level, character, and tile definitions stored as assets.

- Coroutine-based animation: Swap, gravity, refill steps as atomic coroutine sequences.

# **21. DEVELOPMENT ROADMAP**

## **21.1 Dual-Track Production (8-12 Weeks)**

Development follows a dual-track approach: coding in the morning, art generation in the evening. This prevents burnout and ensures parallel progress on both technical and visual fronts.

| **Phase** | **Weeks** | **Track A: Code** | **Track B: Art** | **Milestone** |
| --- | --- | --- | --- | --- |
| Foundation | 1-2 | Unity basics, C# fundamentals, project setup | 6 tile icons, color palette finalization | Playable board with swapping |
| Core Gameplay | 3-4 | Match-3 engine, cascades, special tiles | 3 character portraits, 3 special tile icons | Full match-cascade loop working |
| Advanced Mechanics | 5-6 | Blockers, character ultimates, objectives | 3 blocker sets, UI backgrounds | All mechanics functional |
| Content & Polish | 7-8 | 25 levels, map screen, win/lose flow | Map screen art, VFX sprites, app icon | Complete MVP ready for testing |
| Testing & Build | 9-10 | Bug fixes, performance optimization | Final asset polish, loading screen | Android APK + iOS build |
| Submission | 11-12 | Store listing, screenshots, compliance | Store graphics, promotional assets | Published on Google Play + App Store |

## **21.2 Solo Dev Survival Rules**

- Ugly prototype first: Gameplay feel matters more than visuals in early phases.

- Playtest every day: Play the game after each feature addition.

- One feature at a time: Never work on blockers + characters + UI simultaneously.

- GDD is your anchor: Refer back when tempted to add scope creep.

- Ship ugly, then polish: Get 25 levels playable before the final art pass.

- Dual-track prevents burnout: When coding frustrates, switch to art and vice versa.

- Commit every working feature: Git version control after each system works.

# **22. MVP SCOPE ****&**** PRIORITIES**

## **22.1 Must Have (MVP Launch)**

- Complete match-3 board engine (swap, match, cascade, gravity, refill)

- 6 base tile types with lore-accurate icons

- 3 special tile types (Line, Star, Nova sigils)

- 3 blocker types (Glass, Chain, Stone) with progressive damage

- 3 playable characters (Blue, Crux, Kaery) with Ultimates and Passives

- 25 levels with varied objectives and difficulty curve

- Map screen with linear node progression

- Roster screen showing all 9 characters (3 unlocked, 6 locked preview)

- Win/Lose panels with star rating

- Score and combo multiplier system

- Pause menu with volume controls

- Save system (PlayerPrefs) for level progress, stars, and high scores

- 60 FPS targeting and screen lock configuration

- Android APK and iOS build

## **22.2 Nice to Have (Post-MVP)**

- Sigil combination effects (Line+Line, Star+Star, etc.)

- Boosters system (Bomb, Line, Swap consumables)

- Full sound design with music and all 11 SFX

- Tutorial system with guided first-time experience

- Daily rewards (7-day cycle)

- Achievement/task system

## **22.3 Deferred to Future Updates**

- 6 additional playable characters with unique mechanics

- Monetization implementation (character unlocks, cosmetics, battle pass)

- Additional chapters (Chapter 2+) with new themes and blockers

- Relic/Artifact system (equippable power modifiers)

- Live events and seasonal content

- Deep story UI with narrative scenes between levels

- PvP or online competitive modes

- Lucky Wheel spin mechanic

- Ad integration for optional rewarded videos

# **23. FUTURE CONTENT ROADMAP**

| **Update** | **Target** | **Content** |
| --- | --- | --- |
| v1.1 | Month 2 | Sound design, tutorial system, bug fixes, boosters |
| v1.2 | Month 3 | Kimmy + Mr. Violet characters, Chapter 2 (25 levels) |
| v1.3 | Month 4 | Battle Pass Season 1, daily rewards, achievements |
| v1.5 | Month 6 | Surge-Overload + Guardian characters, Chapter 3, new blocker type |
| v2.0 | Month 9 | Yanoro + Chanel (legendary characters), Relic system, seasonal events |
| v2.5 | Month 12 | PvP mode, cosmetic skins, expanded lore scenes |

# **24. APPENDIX: COMPLETE ASSET CHECKLIST**

## **24.1 Tile Assets**

| **Asset** | **Size** | **Status** |
| --- | --- | --- |
| Port Rune icon | 512x512 | Art Dept |
| OZONE Mark icon | 512x512 | Art Dept |
| Coven Seal icon | 512x512 | Art Dept |
| Witchbreed Thorn icon | 512x512 | Art Dept |
| Soulstream Shard icon | 512x512 | Art Dept |
| Petsha Charm icon | 512x512 | Art Dept |
| Line Sigil icon | 512x512 | TODO |
| Star Sigil icon | 512x512 | TODO |
| Nova Sigil icon | 512x512 | TODO |

## **24.2 Blocker Assets**

| **Asset** | **Size** | **Status** |
| --- | --- | --- |
| Glass Layer 1 overlay | 512x512 | TODO |
| Glass Layer 2 overlay (cracked) | 512x512 | TODO |
| Chain/Lock overlay | 512x512 | TODO |
| Stone/Armor state 1 (full) | 512x512 | TODO |
| Stone/Armor state 2 (cracked) | 512x512 | TODO |
| Stone/Armor state 3 (breaking) | 512x512 | TODO |

## **24.3 Character Assets**

| **Asset** | **Size** | **Status** |
| --- | --- | --- |
| Blue portrait | 1024x1536 | Art Dept |
| Crux portrait | 1024x1536 | Art Dept |
| Kaery portrait | 1024x1536 | Art Dept |
| 6 locked character silhouettes | 1024x1536 | TODO |
| Blue Ultimate icon | 256x256 | TODO |
| Crux Ultimate icon | 256x256 | TODO |
| Kaery Ultimate icon | 256x256 | TODO |
| Blue Passive icon | 128x128 | TODO |
| Crux Passive icon | 128x128 | TODO |
| Kaery Passive icon | 128x128 | TODO |

## **24.4 UI ****&**** Environment Assets**

| **Asset** | **Size** | **Status** |
| --- | --- | --- |
| Gameplay board background | 1080x1920 | TODO |
| Map screen background | 1080x1920 | TODO |
| Win panel background | 1080x1920 | TODO |
| Lose panel background | 1080x1920 | TODO |
| Character select background | 1080x1920 | TODO |
| Bomb booster icon | 256x256 | TODO |
| Line booster icon | 256x256 | TODO |
| Swap booster icon | 256x256 | TODO |
| App icon | 1024x1024 | TODO |
| Loading screen | 1080x1920 | TODO |
| 6 match VFX particle sprites | 256x256 | TODO |
| 3 Ultimate VFX sprites | 512x512 | TODO |

**END OF DOCUMENT**

Soulstream: Matchmancer  |  Game Design Document v1.0  |  April 2026

Ivery Towers LLC  |  CONFIDENTIAL