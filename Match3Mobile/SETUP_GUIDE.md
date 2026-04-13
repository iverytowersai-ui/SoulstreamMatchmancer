# Matchmancer: ScriptableObject Assets & Scene Wiring Guide

## Step 1: Create Folder Structure

In the Unity Project window, create these folders:

```
Assets/
├── ScriptableObjects/
│   ├── TileTypes/
│   ├── Characters/
│   └── Enemies/
├── Scripts/
│   ├── Combat/      ← already created
│   ├── Character/   ← already created
│   └── Enemy/       ← already created
```

---

## Step 2: Create TileTypeData Assets (×6)

Right-click in `Assets/ScriptableObjects/TileTypes/` → **Create → Matchmancer → Tile Type Data**

Create 6 assets with these settings:

### TileTypeData_Damage.asset
| Field | Value |
|-------|-------|
| Tile Type | Damage |
| Display Name | Soulstream Shard |
| Tile Color | Red (#FF3344) |
| Base Damage Value | 10 |
| Lore | "Raw soulstream energy, channeled through shattered crystal." |

### TileTypeData_Energy.asset
| Field | Value |
|-------|-------|
| Tile Type | Energy |
| Display Name | Port Rune |
| Tile Color | Blue (#3399FF) |
| Base Energy Value | 15 |
| Lore | "Arcane conduits that fuel the ultimate surge." |

### TileTypeData_Defense.asset
| Field | Value |
|-------|-------|
| Tile Type | Defense |
| Display Name | Coven Seal |
| Tile Color | Green (#33CC66) |
| Base Defense Value | 8 |
| Lore | "Protective wards drawn from ancient coven pacts." |

### TileTypeData_Debuff.asset
| Field | Value |
|-------|-------|
| Tile Type | Debuff |
| Display Name | OZONE Mark |
| Tile Color | Purple (#9933FF) |
| Lore | "Toxic sigils that corrode and weaken the enemy." |

### TileTypeData_Break.asset
| Field | Value |
|-------|-------|
| Tile Type | Break |
| Display Name | Witchbreed Thorn |
| Tile Color | Orange (#FF9933) |
| Lore | "Piercing spines that shatter armor and barriers." |

### TileTypeData_Luck.asset
| Field | Value |
|-------|-------|
| Tile Type | Luck |
| Display Name | Petsha Charm |
| Tile Color | Yellow (#FFCC33) |
| Lore | "Fortune favors those who align the stars." |

**IMPORTANT:** Assign placeholder sprites to each asset's `Icon` field. You can use colored squares for now — the colors above will tint the sprites via `Tile Color`.

---

## Step 3: Create CombatConfig Asset (×1)

Right-click in `Assets/ScriptableObjects/` → **Create → Matchmancer → Combat Config**

Save as `CombatConfig.asset`. Default values are already tuned in the script — just create it.

---

## Step 4: Create CharacterData Asset (×1)

Right-click in `Assets/ScriptableObjects/Characters/` → **Create → Matchmancer → Character Data**

Save as `CharacterData_Arcanist.asset`:

| Field | Value |
|-------|-------|
| Character Name | The Arcanist |
| Base Max HP | 150 |
| Base Attack | 10 |
| Base Defense | 5 |
| Base Crit Chance | 0.05 |
| Base Luck | 0 |
| HP Per Level | 10 |
| Attack Per Level | 1 |
| Defense Per Level | 0.5 |
| Luck Per Level | 0.2 |
| Ultimate Name | Soul Surge |
| Ultimate Energy Cost | 100 |
| Ultimate Damage Multiplier | 3.0 |
| XP Per Level | 100, 150, 220, 310, 420, 550, 700, 870, 1060 |

---

## Step 5: Create EnemyData Asset (×1 starter)

Right-click in `Assets/ScriptableObjects/Enemies/` → **Create → Matchmancer → Enemy Data**

Save as `EnemyData_ShadowThrall.asset`:

| Field | Value |
|-------|-------|
| Enemy Name | Shadow Thrall |
| Max HP | 100 |
| Defense | 0 |
| Armor | 0 |
| XP Reward | 50 |
| Gold Reward | 25 |
| HP Scale Per Stage | 1.15 |
| Damage Scale Per Stage | 1.10 |

For Action Pattern, add 3 entries:
1. Attack, Value: 15, Intent: "sword", Telegraph: "The Shadow Thrall raises its claw..."
2. Attack, Value: 20, Intent: "sword", Telegraph: "The Shadow Thrall lunges!"
3. Defend, Value: 10, Intent: "shield", Telegraph: "The Shadow Thrall braces itself."

---

## Step 6: Wire the Scene Hierarchy

Open your gameplay scene. Create empty GameObjects and attach scripts:

```
Hierarchy:
├── --- BOARD ---          (existing)
│   ├── GameBoard          (existing — UPDATE Inspector fields)
│   ├── MatchFinder        (existing)
│   ├── MatchResolver      (existing — UPDATE Inspector fields)
│   ├── BoardFiller        (existing)
│   ├── SwapHandler        (existing)
│   └── TouchInput         (existing)
│
├── --- COMBAT ---         (NEW empty GameObject)
│   ├── CombatStats.cs     (add component)
│   ├── CombatResolver.cs  (add component)
│   └── BattleBridge.cs    (add component — temporary debug logger)
│
├── --- CHARACTER ---      (NEW empty GameObject)
│   ├── CharacterRuntime.cs (add component)
│   └── UltimateSystem.cs   (add component)
│
├── --- ENEMY ---          (NEW empty GameObject)
│   ├── EnemyController.cs     (add component)
│   └── EnemyTurnController.cs (add component)
│
├── --- MANAGERS ---       (existing)
│   ├── GameManager
│   ├── ScoreManager
│   ├── LevelManager
│   └── AudioManager
│
├── --- UI ---             (existing)
│   ├── HUDController
│   ├── MenuController
│   └── PopupController
│
└── --- VFX ---            (existing)
    ├── TileVFX
    ├── ScreenShake
    └── ScorePopup
```

---

## Step 7: Inspector Wiring (Critical!)

### GameBoard (updated)
| Field | Assign |
|-------|--------|
| Tile Type Data Assets [0] | TileTypeData_Damage.asset |
| Tile Type Data Assets [1] | TileTypeData_Energy.asset |
| Tile Type Data Assets [2] | TileTypeData_Defense.asset |
| Tile Type Data Assets [3] | TileTypeData_Debuff.asset |
| Tile Type Data Assets [4] | TileTypeData_Break.asset |
| Tile Type Data Assets [5] | TileTypeData_Luck.asset |

**NOTE:** The old `Tile Sprites` array is removed. All visuals now come from `TileTypeData`.

### MatchResolver (updated)
| Field | Assign |
|-------|--------|
| Board | GameBoard |
| Match Finder | MatchFinder |
| Board Filler | BoardFiller |
| Tile VFX | TileVFX (if exists) |
| **Combat Resolver** | **CombatResolver (on COMBAT object)** |

### CombatResolver
| Field | Assign |
|-------|--------|
| Combat Config | CombatConfig.asset |
| Character Runtime | CharacterRuntime (on CHARACTER object) |

### BattleBridge (temporary)
| Field | Assign |
|-------|--------|
| Combat Resolver | CombatResolver (on COMBAT object) |

### CharacterRuntime
| Field | Assign |
|-------|--------|
| Combat Resolver | CombatResolver (on COMBAT object) |
| Enemy Turn Controller | EnemyTurnController (on ENEMY object) |

### UltimateSystem
| Field | Assign |
|-------|--------|
| Character Runtime | CharacterRuntime (on CHARACTER object) |
| Combat Resolver | CombatResolver (on COMBAT object) |

### EnemyController
| Field | Assign |
|-------|--------|
| Combat Resolver | CombatResolver (on COMBAT object) |

### EnemyTurnController
| Field | Assign |
|-------|--------|
| Enemy Controller | EnemyController (on ENEMY object) |
| Match Resolver | MatchResolver (on BOARD object) |

---

## Step 8: Battle Initialization

You need a script to kick off the battle. Add this to GameManager or create a new `BattleInitializer.cs`:

```csharp
using UnityEngine;

public class BattleInitializer : MonoBehaviour
{
    [SerializeField] private CharacterRuntime characterRuntime;
    [SerializeField] private EnemyController  enemyController;
    [SerializeField] private EnemyTurnController enemyTurnController;
    [SerializeField] private CharacterData    characterData;
    [SerializeField] private EnemyData        enemyData;
    [SerializeField] private int              playerLevel = 1;
    [SerializeField] private int              stageNumber = 1;

    private void Start()
    {
        characterRuntime.InitialiseForBattle(characterData, playerLevel);
        enemyController.Initialise(enemyData, stageNumber);
        enemyTurnController.StartBattle(stageNumber);
        CombatStats.Instance?.ResetForNewBattle();
    }
}
```

Attach to a new "BattleInitializer" GameObject and wire:
- Character Data → CharacterData_Arcanist.asset
- Enemy Data → EnemyData_ShadowThrall.asset
- All runtime references → the corresponding scene objects

---

## Step 9: Verify

Hit Play and make matches. You should see in the Console:

```
[Combat] DAMAGE 30.0 (match 3, combo 1)
[Combat] ENERGY +45.0
[Combat] SHIELD +24.0
[Combat] ARMOR DMG 36.0
[Combat] DEBUFF — poison:True vuln:False dur:3
[Combat] LUCK — crit+0.5% combo+5.0%
[Enemy Turn] Shadow Thrall attacks for 15.0
```

If you see these logs, the full combat pipeline is working.

---

## What's Next

Once verified, you can move to:
- **Skill 15:** Gear System (equipment that modifies stats)
- **Skill 16:** Boosters & Inventory
- **Skill 17:** Level/Stage Data (100-level structure)
- **Skill 19:** Achievements & Titles
- **Skill 20:** Full progression UI
