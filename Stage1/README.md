# Stage 1 — "The Threshold" (Real Levels)

This drop populates **real Stage 1 content** for Matchmancer: 4 enemies, 10 LevelData assets, and 1 StageData wrapper — all built from the Skill 17 difficulty ladder.

## What's included

```
Scripts/
├── Combat/
│   ├── TileType.cs                 Enum (stub, drop if Skill 11 exists)
│   └── EnemyData.cs                Minimal EnemyData (replace when Skill 13 lands)
├── Character/
│   └── GearData.cs                 Minimal GearData stub (replace when Skill 15 lands)
└── Progression/
    ├── LevelData.cs                Full LevelData SO (Skill 17)
    ├── StageData.cs                Full StageData SO (Skill 17)
    └── LevelProgressionManager.cs  Singleton, unlock gating, star tracking

Editor/
└── Stage1Bootstrap.cs              One-click content generator — runs Matchmancer → Bootstrap Stage 1
```

## Install

1. Drop `Scripts/` into `Assets/Scripts/` in your Unity project
2. Drop `Editor/` into `Assets/Editor/`
3. Unity will compile. If you already have `TileType.cs`, `EnemyData.cs`, or `GearData.cs` from Skills 11/13/15, delete the stubs in this drop — the real ones take precedence
4. Ensure namespaces in the bootstrap match your existing code (this drop uses `Matchmancer.Combat`, `Matchmancer.Character`, `Matchmancer.Progression`)

## Run the bootstrap

In the Unity Editor:

**Matchmancer → Bootstrap Stage 1**

This creates:

```
Assets/ScriptableObjects/
├── Enemies/Stage1/
│   ├── ShadeWraith.asset        HP 80   | atk 8  | cadence 3
│   ├── WraithSentinel.asset     HP 140  | atk 12 | cadence 3
│   ├── ArmoredShade.asset       HP 180  | armor 30 | atk 14 | elite
│   └── HollowJudge.asset        HP 300  | atk 18 | cadence 1 | BOSS
├── Levels/Stage1/
│   ├── Level_S1_L01.asset       The First Spark
│   ├── Level_S1_L02.asset       Echoes in the Grey
│   ├── Level_S1_L03.asset       Threshold's Edge
│   ├── Level_S1_L04.asset       The Sentinels Wake
│   ├── Level_S1_L05.asset       Silent Watch
│   ├── Level_S1_L06.asset       Under Cold Lanterns
│   ├── Level_S1_L07.asset       The Grey Procession
│   ├── Level_S1_L08.asset       Forged in Coldfire
│   ├── Level_S1_L09.asset       Plated Hunger
│   └── Level_S1_L10.asset       The Hollow Judge (boss)
└── Stages/
    └── Stage_01.asset           Stage 1: The Threshold (all 10 levels pre-assigned)
```

## Stage 1 design at a glance

| Levels | Enemy           | Board | Moves | Tile types        | Role                               |
|--------|-----------------|-------|-------|-------------------|------------------------------------|
| 1–3    | Shade Wraith    | 8×8   | 25    | Damage/Energy/Def/Luck | Tutorial — teach core loop         |
| 4–7    | Wraith Sentinel | 8×8   | 22    | All 6             | Standard combat — introduce Debuff/Break |
| 8–9    | Armored Shade   | 8×8   | 20    | All 6             | Armor layer — Break tiles essential |
| 10     | Hollow Judge    | 8×8   | 20    | All 6             | Stage boss — 300 HP, attacks every turn |

Difficulty ramps via: fewer moves, higher HP, tighter star thresholds, and new mechanics per bucket (tile variety → combo pressure → armor → boss cadence).

## Hook it into progression

Drop a `LevelProgressionManager` MonoBehaviour into your Managers scene object and assign `Stage_01.asset` to slot 0 of the `Stages` array. As you build Stages 2–10, populate the rest.

```csharp
// From battle scene entry
LevelData level = LevelProgressionManager.Instance.CurrentLevel;
gameBoard.InitialiseFromLevelData(level);
gameBoard.CreateBoard();
enemyController.Initialise(level.enemy);
```

## What to tune

All values in `Stage1Bootstrap.cs` are designer-editable constants. Re-run the menu after editing to regenerate assets, or tweak the created `.asset` files directly in the Inspector.

Likely first tuning pass after playtesting:
- Shade Wraith HP 80 → raise if L1 feels trivial even after 25 moves
- L4 move limit 22 → may need 24 since it's the first 6-tile level
- L10 Hollow Judge attack cadence 1 → lower attack power if cadence 1 is too punishing

## Next steps

- **Skill 18** — Stars & Results System: hook the star thresholds in each LevelData into a post-battle results screen
- **Skill 20** — Progression save/load: persist `LevelProgressionManager` state to disk
- **Skill 24** — Battle UI: render the HP bars, enemy portrait, intent, and move counter

Ready when you are.
