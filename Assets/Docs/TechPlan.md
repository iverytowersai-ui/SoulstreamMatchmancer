# Soulstream Matchmancer - Technical Plan

## Architecture Overview
The game follows a modular architecture separating data, presentation, and logic.
- **Core Logic:** Standalone C# classes and mono-behaviours decoupled from specific scenes where possible.
- **Data Model:** Extensively driven by `ScriptableObjects` for level configs, tile types, and character stats.
- **View Layer:** Mono-behaviours handling animations and UI updates without containing business logic.

## Key Subsystems
1. **Board Manager:** Handles tile generation, grid state, and cascade logic.
2. **Match Detector:** Evaluates the board matrix for valid horizontal, vertical, and shape-based matches.
3. **Character Framework:** Manages ultimate points accumulation and ability execution.
4. **Progression Manager:** Tracks completed stages, earned stars, and unlocks.

## Directory Structure
- `Scripts/Core`: Business logic and grid resolution.
- `Scripts/Specials`: Logic for match-4, match-5, and shape abilities.
- `Scripts/Blockers`: Overlay and obstacle resolution.
- `Scripts/Levels`: Handling level and stage data configurations.
- `Scripts/Characters`: Character specific data, attacks, and modifiers.
- `Scripts/UI`: User interface controllers.
