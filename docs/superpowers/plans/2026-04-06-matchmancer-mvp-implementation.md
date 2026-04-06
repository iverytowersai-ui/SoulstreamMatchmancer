# Soulstream Matchmancer MVP Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Take the existing 26 C# scripts and turn them into a playable Unity project — testable core logic, working scene, placeholder art, and a complete Stage 1 (10 levels) experience.

**Architecture:** Core game logic (Board, Match, Sigils, Meter, Dice, StoneBlocks, Objectives) is framework-agnostic C#. View layer (BoardView, TileView, InputHandler, HUD) is Unity MonoBehaviours. Communication is event-driven via C# events on BoardController. All scripts already exist at `Assets/Scripts/`.

**Tech Stack:** Unity 2022.3 LTS (2D), C#, NUnit (Unity Test Framework), TextMeshPro for UI.

---

## Current State

All 26 C# scripts are written and committed. What's missing:

1. **Unity project scaffolding** — no `.unity` scene, no prefabs, no project settings
2. **Unit tests** — core logic is framework-agnostic and fully testable without Unity, but no tests exist
3. **Level 10 boss mechanic** — stone spreading every 3 turns (noted as deferred in spec assumption #7)
4. **Placeholder art** — no sprites, no materials, no fonts
5. **TextMeshPro upgrade** — HUDView uses legacy `UnityEngine.UI.Text`, should use TMP

---

## File Map

### Already Exists (no changes unless noted)
- `Assets/Scripts/Core/` — TileType, SigilType, MagickSchool, GridPosition, Tile, LevelConfig, Stage1Data
- `Assets/Scripts/Board/` — Board, SwapValidator, GravityHandler
- `Assets/Scripts/Match/` — MatchDetector, MatchInfo
- `Assets/Scripts/Sigils/` — SigilSystem
- `Assets/Scripts/Meter/` — MagickMeter
- `Assets/Scripts/Dice/` — DiceSystem
- `Assets/Scripts/StoneBlocks/` — StoneBlockSystem
- `Assets/Scripts/Objectives/` — MoveTracker, Scoring, ObjectiveChecker
- `Assets/Scripts/View/` — BoardView, TileView, StoneBlockView, InputHandler, HUDView
- `Assets/Scripts/Core/` — BoardController, GameManager

### To Create
- `Assets/Tests/EditMode/` — Unit tests for core logic (Tasks 1-7)
- `Assets/Tests/EditMode/EditModeTests.asmdef` — Assembly definition for edit-mode tests
- `Assets/Scripts/Core/BossMechanic.cs` — Level 10 stone spreading logic (Task 8)
- `Assets/Prefabs/TilePrefab.prefab` — Created in Unity Editor (Task 9)
- `Assets/Prefabs/StoneBlockPrefab.prefab` — Created in Unity Editor (Task 9)
- `Assets/Scenes/GameScene.unity` — Main gameplay scene (Task 9)
- `Assets/Materials/` — Placeholder tile materials (Task 9)

### To Modify
- `Assets/Scripts/Core/BoardController.cs` — Add boss mechanic hook (Task 8)
- `Assets/Scripts/View/HUDView.cs` — Upgrade to TextMeshPro (Task 10)

---

## Task 1: Unit Tests — Board & GridPosition

**Files:**
- Create: `Assets/Tests/EditMode/BoardTests.cs`
- Create: `Assets/Tests/EditMode/EditModeTests.asmdef`
- Read: `Assets/Scripts/Board/Board.cs`, `Assets/Scripts/Core/GridPosition.cs`

- [ ] **Step 1: Create the test assembly definition**

Create `Assets/Tests/EditMode/EditModeTests.asmdef`:

```json
{
    "name": "EditModeTests",
    "rootNamespace": "",
    "references": [],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": false,
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ],
    "versionDefines": [],
    "noEngineReferences": true
}
```

Note: `noEngineReferences: true` because core logic doesn't use UnityEngine.

- [ ] **Step 2: Write Board and GridPosition tests**

Create `Assets/Tests/EditMode/BoardTests.cs`:

```csharp
using NUnit.Framework;
using Matchmancer.Core;
using Matchmancer.Board;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class GridPositionTests
    {
        [Test]
        public void IsAdjacentTo_HorizontalNeighbor_ReturnsTrue()
        {
            var a = new GridPosition(3, 3);
            var b = new GridPosition(3, 4);
            Assert.IsTrue(a.IsAdjacentTo(b));
        }

        [Test]
        public void IsAdjacentTo_VerticalNeighbor_ReturnsTrue()
        {
            var a = new GridPosition(3, 3);
            var b = new GridPosition(4, 3);
            Assert.IsTrue(a.IsAdjacentTo(b));
        }

        [Test]
        public void IsAdjacentTo_Diagonal_ReturnsFalse()
        {
            var a = new GridPosition(3, 3);
            var b = new GridPosition(4, 4);
            Assert.IsFalse(a.IsAdjacentTo(b));
        }

        [Test]
        public void IsAdjacentTo_SamePosition_ReturnsFalse()
        {
            var a = new GridPosition(3, 3);
            Assert.IsFalse(a.IsAdjacentTo(a));
        }

        [Test]
        public void IsAdjacentTo_FarApart_ReturnsFalse()
        {
            var a = new GridPosition(0, 0);
            var b = new GridPosition(7, 7);
            Assert.IsFalse(a.IsAdjacentTo(b));
        }

        [Test]
        public void Equality_SameRowCol_AreEqual()
        {
            var a = new GridPosition(2, 5);
            var b = new GridPosition(2, 5);
            Assert.AreEqual(a, b);
            Assert.IsTrue(a == b);
        }

        [Test]
        public void Equality_DifferentRowCol_AreNotEqual()
        {
            var a = new GridPosition(2, 5);
            var b = new GridPosition(5, 2);
            Assert.AreNotEqual(a, b);
            Assert.IsTrue(a != b);
        }
    }

    [TestFixture]
    public class BoardTests
    {
        private Board.Board _board;
        private int _tileIndex;
        private readonly TileType[] _types = {
            TileType.PortRune, TileType.OzoneMark, TileType.CovenSeal,
            TileType.WitchbreedThorn, TileType.SoulstreamShard, TileType.PetshaCharm
        };

        [SetUp]
        public void SetUp()
        {
            _board = new Board.Board();
            _tileIndex = 0;
            _board.Initialize(() => _types[_tileIndex++ % _types.Length]);
        }

        [Test]
        public void Initialize_AllCellsPopulated()
        {
            for (int r = 0; r < Board.Board.Rows; r++)
                for (int c = 0; c < Board.Board.Cols; c++)
                    Assert.AreNotEqual(TileType.None, _board[r, c].Type);
        }

        [Test]
        public void IsInBounds_ValidPosition_ReturnsTrue()
        {
            Assert.IsTrue(_board.IsInBounds(0, 0));
            Assert.IsTrue(_board.IsInBounds(7, 7));
        }

        [Test]
        public void IsInBounds_OutOfBounds_ReturnsFalse()
        {
            Assert.IsFalse(_board.IsInBounds(-1, 0));
            Assert.IsFalse(_board.IsInBounds(8, 0));
            Assert.IsFalse(_board.IsInBounds(0, 8));
        }

        [Test]
        public void SwapTiles_SwapsTypesAndPositions()
        {
            var posA = new GridPosition(0, 0);
            var posB = new GridPosition(0, 1);
            var typeA = _board[posA].Type;
            var typeB = _board[posB].Type;

            _board.SwapTiles(posA, posB);

            Assert.AreEqual(typeB, _board[posA].Type);
            Assert.AreEqual(typeA, _board[posB].Type);
            Assert.AreEqual(posA, _board[posA].Position);
            Assert.AreEqual(posB, _board[posB].Position);
        }

        [Test]
        public void StoneBlock_SetAndQuery()
        {
            _board.SetStoneBlock(3, 3, true);
            Assert.IsTrue(_board.IsStoneBlock(3, 3));
            Assert.IsFalse(_board.IsPlayable(3, 3));
        }

        [Test]
        public void ClearTile_MakesTileEmpty()
        {
            var pos = new GridPosition(2, 2);
            Assert.IsFalse(_board[pos].IsEmpty);
            _board.ClearTile(pos);
            Assert.IsTrue(_board[pos].IsEmpty);
        }
    }
}
```

- [ ] **Step 3: Run tests in Unity**

Run: Open Unity > Window > General > Test Runner > EditMode > Run All
Expected: All 12 tests PASS (7 GridPosition + 5 Board)

- [ ] **Step 4: Commit**

```bash
git add Assets/Tests/
git commit -m "test: add unit tests for Board and GridPosition"
```

---

## Task 2: Unit Tests — MatchDetector

**Files:**
- Create: `Assets/Tests/EditMode/MatchDetectorTests.cs`
- Read: `Assets/Scripts/Match/MatchDetector.cs`, `Assets/Scripts/Match/MatchInfo.cs`

- [ ] **Step 1: Write MatchDetector tests**

Create `Assets/Tests/EditMode/MatchDetectorTests.cs`:

```csharp
using System.Linq;
using NUnit.Framework;
using Matchmancer.Core;
using Matchmancer.Board;
using Matchmancer.Match;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class MatchDetectorTests
    {
        private Board.Board _board;
        private MatchDetector _detector;

        [SetUp]
        public void SetUp()
        {
            _board = new Board.Board();
            // Initialize with all different types so no accidental matches
            int idx = 0;
            TileType[] types = {
                TileType.PortRune, TileType.OzoneMark, TileType.CovenSeal,
                TileType.WitchbreedThorn, TileType.SoulstreamShard, TileType.PetshaCharm
            };
            _board.Initialize(() => types[idx++ % types.Length]);
            _detector = new MatchDetector(_board);
        }

        private void SetRow(int row, int startCol, int count, TileType type)
        {
            for (int c = startCol; c < startCol + count; c++)
            {
                var pos = new GridPosition(row, c);
                _board.SetTile(pos, new Tile(type, pos));
            }
        }

        private void SetCol(int col, int startRow, int count, TileType type)
        {
            for (int r = startRow; r < startRow + count; r++)
            {
                var pos = new GridPosition(r, col);
                _board.SetTile(pos, new Tile(type, pos));
            }
        }

        [Test]
        public void FindAllMatches_NoMatches_ReturnsEmpty()
        {
            // Board initialized with rotating types — no 3-in-a-row
            var matches = _detector.FindAllMatches();
            Assert.AreEqual(0, matches.Count);
        }

        [Test]
        public void FindAllMatches_HorizontalThree_FindsMatch()
        {
            SetRow(0, 0, 3, TileType.PortRune);
            var matches = _detector.FindAllMatches();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(MatchPattern.ThreeInARow, matches[0].Pattern);
            Assert.AreEqual(3, matches[0].TileCount);
            Assert.AreEqual(TileType.PortRune, matches[0].TileType);
        }

        [Test]
        public void FindAllMatches_HorizontalFour_ClassifiesAsFour()
        {
            SetRow(0, 0, 4, TileType.OzoneMark);
            var matches = _detector.FindAllMatches();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(MatchPattern.FourInARow, matches[0].Pattern);
            Assert.AreEqual(4, matches[0].TileCount);
            Assert.IsNotNull(matches[0].SigilSpawnPosition);
        }

        [Test]
        public void FindAllMatches_HorizontalFive_ClassifiesAsFive()
        {
            SetRow(0, 0, 5, TileType.CovenSeal);
            var matches = _detector.FindAllMatches();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(MatchPattern.FiveInARow, matches[0].Pattern);
            Assert.AreEqual(5, matches[0].TileCount);
        }

        [Test]
        public void FindAllMatches_VerticalThree_FindsMatch()
        {
            SetCol(0, 0, 3, TileType.WitchbreedThorn);
            var matches = _detector.FindAllMatches();
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(MatchPattern.ThreeInARow, matches[0].Pattern);
        }

        [Test]
        public void FindAllMatches_LShape_ClassifiesAsLShape()
        {
            // L-shape: 3 horizontal + 3 vertical sharing a corner
            SetRow(0, 0, 3, TileType.SoulstreamShard);
            SetCol(0, 1, 3, TileType.SoulstreamShard); // col 0, rows 1-3
            // Intersection at (0,0)
            var matches = _detector.FindAllMatches();
            var lMatches = matches.Where(m =>
                m.Pattern == MatchPattern.LShape || m.Pattern == MatchPattern.TShape).ToList();
            Assert.GreaterOrEqual(lMatches.Count, 1);
        }

        [Test]
        public void FindAllMatches_StoneBlockBreaksRun()
        {
            // Place 5 of same type in a row but a stone block in the middle
            SetRow(0, 0, 5, TileType.PetshaCharm);
            _board.SetStoneBlock(0, 2, true);
            var matches = _detector.FindAllMatches();
            // Should find 0 matches: 2 on each side of stone, neither is 3+
            Assert.AreEqual(0, matches.Count);
        }

        [Test]
        public void MeterCharge_ThreeMatch_Returns1()
        {
            SetRow(0, 0, 3, TileType.PortRune);
            var matches = _detector.FindAllMatches();
            Assert.AreEqual(1, matches[0].MeterCharge);
        }

        [Test]
        public void MeterCharge_FourMatch_Returns2()
        {
            SetRow(0, 0, 4, TileType.PortRune);
            var matches = _detector.FindAllMatches();
            Assert.AreEqual(2, matches[0].MeterCharge);
        }

        [Test]
        public void MeterCharge_FiveMatch_Returns3()
        {
            SetRow(0, 0, 5, TileType.PortRune);
            var matches = _detector.FindAllMatches();
            Assert.AreEqual(3, matches[0].MeterCharge);
        }
    }
}
```

- [ ] **Step 2: Run tests**

Run: Unity Test Runner > EditMode > Run All
Expected: All 10 MatchDetector tests PASS

- [ ] **Step 3: Commit**

```bash
git add Assets/Tests/EditMode/MatchDetectorTests.cs
git commit -m "test: add unit tests for MatchDetector"
```

---

## Task 3: Unit Tests — MagickMeter

**Files:**
- Create: `Assets/Tests/EditMode/MagickMeterTests.cs`
- Read: `Assets/Scripts/Meter/MagickMeter.cs`

- [ ] **Step 1: Write MagickMeter tests**

Create `Assets/Tests/EditMode/MagickMeterTests.cs`:

```csharp
using NUnit.Framework;
using Matchmancer.Meter;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class MagickMeterTests
    {
        private MagickMeter _meter;

        [SetUp]
        public void SetUp()
        {
            _meter = new MagickMeter(10);
        }

        [Test]
        public void InitialState_ZeroCharge_NotFull()
        {
            Assert.AreEqual(0, _meter.CurrentCharge);
            Assert.AreEqual(10, _meter.MaxCapacity);
            Assert.IsFalse(_meter.IsFull);
        }

        [Test]
        public void AddCharge_IncreasesCharge()
        {
            _meter.AddCharge(3);
            Assert.AreEqual(3, _meter.CurrentCharge);
        }

        [Test]
        public void AddCharge_ClampsAtMax_NoOverflow()
        {
            _meter.AddCharge(7);
            _meter.AddCharge(5);
            Assert.AreEqual(10, _meter.CurrentCharge);
        }

        [Test]
        public void IsFull_AtCapacity_ReturnsTrue()
        {
            _meter.AddCharge(10);
            Assert.IsTrue(_meter.IsFull);
        }

        [Test]
        public void AddSigilActivationCharge_Adds3()
        {
            _meter.AddSigilActivationCharge();
            Assert.AreEqual(3, _meter.CurrentCharge);
        }

        [Test]
        public void Reset_SetsToZero()
        {
            _meter.AddCharge(8);
            _meter.Reset();
            Assert.AreEqual(0, _meter.CurrentCharge);
            Assert.IsFalse(_meter.IsFull);
        }

        [Test]
        public void FullCycle_ChargeToFull_Reset_ChargeAgain()
        {
            _meter.AddCharge(10);
            Assert.IsTrue(_meter.IsFull);
            _meter.Reset();
            Assert.AreEqual(0, _meter.CurrentCharge);
            _meter.AddCharge(5);
            Assert.AreEqual(5, _meter.CurrentCharge);
        }
    }
}
```

- [ ] **Step 2: Run tests**

Run: Unity Test Runner > EditMode > Run All
Expected: All 7 MagickMeter tests PASS

- [ ] **Step 3: Commit**

```bash
git add Assets/Tests/EditMode/MagickMeterTests.cs
git commit -m "test: add unit tests for MagickMeter"
```

---

## Task 4: Unit Tests — Scoring, MoveTracker, StoneBlockSystem

**Files:**
- Create: `Assets/Tests/EditMode/ScoringTests.cs`
- Create: `Assets/Tests/EditMode/MoveTrackerTests.cs`
- Create: `Assets/Tests/EditMode/StoneBlockTests.cs`
- Read: `Assets/Scripts/Objectives/Scoring.cs`, `Assets/Scripts/Objectives/MoveTracker.cs`, `Assets/Scripts/StoneBlocks/StoneBlockSystem.cs`

- [ ] **Step 1: Write Scoring tests**

Create `Assets/Tests/EditMode/ScoringTests.cs`:

```csharp
using NUnit.Framework;
using Matchmancer.Objectives;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class ScoringTests
    {
        private Scoring _scoring;

        [SetUp]
        public void SetUp()
        {
            _scoring = new Scoring(
                oneStar: 500, twoStar: 1000, threeStar: 2000,
                fourStar: 3500, fiveStar: 5000);
        }

        [Test]
        public void InitialScore_IsZero()
        {
            Assert.AreEqual(0, _scoring.Score);
        }

        [Test]
        public void AddMatchScore_3Tiles_Adds30Points()
        {
            _scoring.AddMatchScore(3); // 3 tiles * 10 pts = 30
            Assert.AreEqual(30, _scoring.Score);
        }

        [Test]
        public void CascadeBonus_IncreasesPerDepth()
        {
            _scoring.AddMatchScore(3); // depth 0: 3 * 10 = 30
            _scoring.IncrementCascade();
            _scoring.AddMatchScore(3); // depth 1: 3 * (10+5) = 45
            Assert.AreEqual(75, _scoring.Score);
        }

        [Test]
        public void ResetCascade_ResetsDepthToZero()
        {
            _scoring.IncrementCascade();
            _scoring.IncrementCascade();
            _scoring.ResetCascade();
            _scoring.AddMatchScore(3); // depth 0: 3 * 10 = 30
            Assert.AreEqual(30, _scoring.Score);
        }

        [Test]
        public void SigilActivation_Adds50Points()
        {
            _scoring.AddSigilActivationScore();
            Assert.AreEqual(50, _scoring.Score);
        }

        [Test]
        public void RemainingMovesBonus_Adds50PerMove()
        {
            _scoring.AddRemainingMovesBonus(5);
            Assert.AreEqual(250, _scoring.Score);
        }

        [Test]
        public void CalculateStars_0Stars_BelowOneStar()
        {
            Assert.AreEqual(0, _scoring.CalculateStars());
        }

        [Test]
        public void CalculateStars_3Stars_At2000()
        {
            // Add enough for 2000 points: 200 matches of 1 tile... let's use big match
            for (int i = 0; i < 200; i++) _scoring.AddMatchScore(1);
            Assert.AreEqual(3, _scoring.CalculateStars()); // 200 * 10 = 2000
        }

        [Test]
        public void CalculateStars_5Stars_At5000()
        {
            for (int i = 0; i < 100; i++) _scoring.AddSigilActivationScore(); // 100 * 50 = 5000
            Assert.AreEqual(5, _scoring.CalculateStars());
        }
    }
}
```

- [ ] **Step 2: Write MoveTracker tests**

Create `Assets/Tests/EditMode/MoveTrackerTests.cs`:

```csharp
using NUnit.Framework;
using Matchmancer.Objectives;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class MoveTrackerTests
    {
        [Test]
        public void InitialState_FullMoves()
        {
            var tracker = new MoveTracker(20);
            Assert.AreEqual(20, tracker.TotalMoves);
            Assert.AreEqual(20, tracker.MovesRemaining);
            Assert.AreEqual(0, tracker.MovesUsed);
            Assert.IsFalse(tracker.IsExhausted);
        }

        [Test]
        public void DeductMove_DecrementsBy1()
        {
            var tracker = new MoveTracker(20);
            tracker.DeductMove();
            Assert.AreEqual(19, tracker.MovesRemaining);
            Assert.AreEqual(1, tracker.MovesUsed);
        }

        [Test]
        public void DeductMove_AtZero_StaysAtZero()
        {
            var tracker = new MoveTracker(1);
            tracker.DeductMove();
            tracker.DeductMove(); // should not go negative
            Assert.AreEqual(0, tracker.MovesRemaining);
            Assert.IsTrue(tracker.IsExhausted);
        }

        [Test]
        public void IsExhausted_WhenAllMovesUsed()
        {
            var tracker = new MoveTracker(3);
            tracker.DeductMove();
            tracker.DeductMove();
            Assert.IsFalse(tracker.IsExhausted);
            tracker.DeductMove();
            Assert.IsTrue(tracker.IsExhausted);
        }
    }
}
```

- [ ] **Step 3: Write StoneBlockSystem tests**

Create `Assets/Tests/EditMode/StoneBlockTests.cs`:

```csharp
using System.Collections.Generic;
using NUnit.Framework;
using Matchmancer.Core;
using Matchmancer.StoneBlocks;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class StoneBlockTests
    {
        private Board.Board _board;
        private StoneBlockSystem _system;

        [SetUp]
        public void SetUp()
        {
            _board = new Board.Board();
            _board.Initialize(() => TileType.PortRune);
            _system = new StoneBlockSystem(_board);
        }

        [Test]
        public void PlaceStone_MarksAsStoneBlock()
        {
            _system.PlaceStone(3, 3, 2);
            Assert.IsTrue(_board.IsStoneBlock(3, 3));
            Assert.AreEqual(1, _system.RemainingStones);
        }

        [Test]
        public void DamageAdjacentStones_1HP_DestroysOnFirstHit()
        {
            _system.PlaceStone(3, 3, 1);
            var matchPositions = new List<GridPosition> { new GridPosition(3, 2) }; // adjacent
            var destroyed = _system.DamageAdjacentStones(matchPositions);
            Assert.AreEqual(1, destroyed.Count);
            Assert.AreEqual(0, _system.RemainingStones);
            Assert.IsFalse(_board.IsStoneBlock(3, 3));
        }

        [Test]
        public void DamageAdjacentStones_2HP_SurvivesFirstHit()
        {
            _system.PlaceStone(3, 3, 2);
            var matchPositions = new List<GridPosition> { new GridPosition(3, 2) };
            var destroyed = _system.DamageAdjacentStones(matchPositions);
            Assert.AreEqual(0, destroyed.Count);
            Assert.AreEqual(1, _system.RemainingStones);
            Assert.AreEqual(1, _system.Stones[new GridPosition(3, 3)].CurrentHP);
        }

        [Test]
        public void DamageAdjacentStones_OneDamagePerMatchEvent_NotPerTile()
        {
            _system.PlaceStone(3, 3, 3);
            // Multiple tiles adjacent to the stone in the same match event
            var matchPositions = new List<GridPosition>
            {
                new GridPosition(3, 2), // adjacent
                new GridPosition(2, 3), // adjacent
                new GridPosition(4, 3)  // adjacent
            };
            var destroyed = _system.DamageAdjacentStones(matchPositions);
            // Stone should only take 1 damage (per event), not 3
            Assert.AreEqual(2, _system.Stones[new GridPosition(3, 3)].CurrentHP);
        }

        [Test]
        public void DamageAdjacentStones_NonAdjacentMatch_NoDamage()
        {
            _system.PlaceStone(3, 3, 1);
            var matchPositions = new List<GridPosition> { new GridPosition(0, 0) }; // far away
            var destroyed = _system.DamageAdjacentStones(matchPositions);
            Assert.AreEqual(0, destroyed.Count);
            Assert.AreEqual(1, _system.RemainingStones);
        }
    }
}
```

- [ ] **Step 4: Run all tests**

Run: Unity Test Runner > EditMode > Run All
Expected: All Scoring (9), MoveTracker (4), StoneBlock (5) tests PASS

- [ ] **Step 5: Commit**

```bash
git add Assets/Tests/EditMode/ScoringTests.cs Assets/Tests/EditMode/MoveTrackerTests.cs Assets/Tests/EditMode/StoneBlockTests.cs
git commit -m "test: add unit tests for Scoring, MoveTracker, and StoneBlockSystem"
```

---

## Task 5: Unit Tests — SigilSystem

**Files:**
- Create: `Assets/Tests/EditMode/SigilTests.cs`
- Read: `Assets/Scripts/Sigils/SigilSystem.cs`

- [ ] **Step 1: Write SigilSystem tests**

Create `Assets/Tests/EditMode/SigilTests.cs`:

```csharp
using System.Collections.Generic;
using NUnit.Framework;
using Matchmancer.Core;
using Matchmancer.Match;
using Matchmancer.Sigils;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class SigilSystemTests
    {
        private Board.Board _board;
        private SigilSystem _sigilSystem;

        [SetUp]
        public void SetUp()
        {
            _board = new Board.Board();
            _board.Initialize(() => TileType.PortRune);
            _sigilSystem = new SigilSystem(_board);
        }

        [Test]
        public void CreateSigilFromMatch_FourInARow_CreatesLineSigil()
        {
            var positions = new List<GridPosition>
            {
                new(0, 0), new(0, 1), new(0, 2), new(0, 3)
            };
            var match = new MatchInfo(TileType.PortRune, MatchPattern.FourInARow, positions)
            {
                SigilSpawnPosition = new GridPosition(0, 2)
            };

            _sigilSystem.CreateSigilFromMatch(match);

            Assert.AreEqual(SigilType.Line, _board[0, 2].Sigil);
            Assert.AreEqual(TileType.PortRune, _board[0, 2].Type);
        }

        [Test]
        public void CreateSigilFromMatch_FiveInARow_CreatesStarSigil()
        {
            var positions = new List<GridPosition>
            {
                new(0, 0), new(0, 1), new(0, 2), new(0, 3), new(0, 4)
            };
            var match = new MatchInfo(TileType.CovenSeal, MatchPattern.FiveInARow, positions)
            {
                SigilSpawnPosition = new GridPosition(0, 2)
            };

            _sigilSystem.CreateSigilFromMatch(match);

            Assert.AreEqual(SigilType.Star, _board[0, 2].Sigil);
        }

        [Test]
        public void CreateSigilFromMatch_LShape_CreatesNovaSigil()
        {
            var positions = new List<GridPosition>
            {
                new(0, 0), new(0, 1), new(0, 2), new(1, 0), new(2, 0)
            };
            var match = new MatchInfo(TileType.OzoneMark, MatchPattern.LShape, positions)
            {
                SigilSpawnPosition = new GridPosition(0, 0)
            };

            _sigilSystem.CreateSigilFromMatch(match);

            Assert.AreEqual(SigilType.Nova, _board[0, 0].Sigil);
        }

        [Test]
        public void CreateSigilFromMatch_ThreeInARow_NoSigil()
        {
            var positions = new List<GridPosition> { new(0, 0), new(0, 1), new(0, 2) };
            var match = new MatchInfo(TileType.PortRune, MatchPattern.ThreeInARow, positions);

            _sigilSystem.CreateSigilFromMatch(match);

            Assert.AreEqual(SigilType.None, _board[0, 0].Sigil);
            Assert.AreEqual(SigilType.None, _board[0, 1].Sigil);
            Assert.AreEqual(SigilType.None, _board[0, 2].Sigil);
        }

        [Test]
        public void ActivateSigil_Line_ClearsEntireRow()
        {
            // Place a Line Sigil at (3, 3)
            _board[3, 3].Sigil = SigilType.Line;
            _board[3, 3].Type = TileType.PortRune;

            var cleared = _sigilSystem.ActivateSigil(new GridPosition(3, 3));

            // Should clear 7 positions (entire row minus the sigil itself)
            Assert.AreEqual(7, cleared.Count);
            foreach (var pos in cleared)
                Assert.AreEqual(3, pos.Row);
        }

        [Test]
        public void ActivateSigil_Star_ClearsAllOfType()
        {
            // Set a known board: half PortRune, half OzoneMark
            for (int r = 0; r < Board.Board.Rows; r++)
                for (int c = 0; c < Board.Board.Cols; c++)
                {
                    var pos = new GridPosition(r, c);
                    var type = (r + c) % 2 == 0 ? TileType.PortRune : TileType.OzoneMark;
                    _board.SetTile(pos, new Tile(type, pos));
                }

            _board[0, 0].Sigil = SigilType.Star;
            _board[0, 0].Type = TileType.PortRune;

            var cleared = _sigilSystem.ActivateSigil(new GridPosition(0, 0));

            // All PortRune tiles except the sigil itself
            foreach (var pos in cleared)
                Assert.AreEqual(TileType.PortRune, _board[pos].Type);
        }

        [Test]
        public void ActivateSigil_Nova_Clears3x3()
        {
            _board[3, 3].Sigil = SigilType.Nova;

            var cleared = _sigilSystem.ActivateSigil(new GridPosition(3, 3));

            // 3x3 = 9 minus center = 8
            Assert.AreEqual(8, cleared.Count);
            foreach (var pos in cleared)
            {
                Assert.GreaterOrEqual(pos.Row, 2);
                Assert.LessOrEqual(pos.Row, 4);
                Assert.GreaterOrEqual(pos.Col, 2);
                Assert.LessOrEqual(pos.Col, 4);
            }
        }

        [Test]
        public void ActivateSigil_Nova_AtCorner_ClearsPartial()
        {
            _board[0, 0].Sigil = SigilType.Nova;

            var cleared = _sigilSystem.ActivateSigil(new GridPosition(0, 0));

            // Corner: only 3 positions in the 3x3 are in bounds (excluding center)
            Assert.AreEqual(3, cleared.Count);
        }

        [Test]
        public void ActivateSigil_Line_SkipsStoneBlocks()
        {
            _board[3, 3].Sigil = SigilType.Line;
            _board.SetStoneBlock(3, 5, true);

            var cleared = _sigilSystem.ActivateSigil(new GridPosition(3, 3));

            // 7 in row minus 1 stone block = 6
            Assert.AreEqual(6, cleared.Count);
            Assert.IsFalse(cleared.Exists(p => p.Row == 3 && p.Col == 5));
        }
    }
}
```

- [ ] **Step 2: Run tests**

Run: Unity Test Runner > EditMode > Run All
Expected: All 9 SigilSystem tests PASS

- [ ] **Step 3: Commit**

```bash
git add Assets/Tests/EditMode/SigilTests.cs
git commit -m "test: add unit tests for SigilSystem"
```

---

## Task 6: Unit Tests — GravityHandler

**Files:**
- Create: `Assets/Tests/EditMode/GravityTests.cs`
- Read: `Assets/Scripts/Board/GravityHandler.cs`

- [ ] **Step 1: Write GravityHandler tests**

Create `Assets/Tests/EditMode/GravityTests.cs`:

```csharp
using NUnit.Framework;
using Matchmancer.Core;
using Matchmancer.Board;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class GravityHandlerTests
    {
        private Board.Board _board;
        private GravityHandler _gravity;
        private int _refillIndex;
        private readonly TileType[] _refillTypes = {
            TileType.PetshaCharm, TileType.CovenSeal, TileType.OzoneMark,
            TileType.PortRune, TileType.WitchbreedThorn, TileType.SoulstreamShard
        };

        [SetUp]
        public void SetUp()
        {
            _board = new Board.Board();
            _board.Initialize(() => TileType.PortRune);
            _refillIndex = 0;
            _gravity = new GravityHandler(_board, () => _refillTypes[_refillIndex++ % _refillTypes.Length]);
        }

        [Test]
        public void ApplyGravity_EmptyCell_TileDropsDown()
        {
            // Clear a cell at bottom, tile above should drop
            var bottomPos = new GridPosition(7, 0);
            var aboveType = _board[6, 0].Type;
            _board.ClearTile(bottomPos);

            var moves = _gravity.ApplyGravity();

            Assert.Greater(moves.Count, 0);
            Assert.AreEqual(aboveType, _board[7, 0].Type);
        }

        [Test]
        public void ApplyGravity_StoneBlock_TilesStopAboveStone()
        {
            _board.SetStoneBlock(5, 0, true);
            _board.ClearTile(new GridPosition(4, 0)); // above stone

            var moves = _gravity.ApplyGravity();

            // Tile at row 3 should drop to row 4 (above stone), not past it
            Assert.IsFalse(_board[4, 0].IsEmpty);
        }

        [Test]
        public void RefillBoard_FillsAllEmpties()
        {
            // Clear a column
            for (int r = 0; r < 3; r++)
                _board.ClearTile(new GridPosition(r, 0));

            var filled = _gravity.RefillBoard();

            Assert.AreEqual(3, filled.Count);
            for (int r = 0; r < 3; r++)
                Assert.IsFalse(_board[r, 0].IsEmpty);
        }

        [Test]
        public void RefillBoard_SkipsStoneBlocks()
        {
            _board.SetStoneBlock(2, 0, true);
            _board.ClearTile(new GridPosition(1, 0));

            var filled = _gravity.RefillBoard();

            Assert.IsTrue(_board.IsStoneBlock(2, 0)); // stone still there
            Assert.IsFalse(_board[1, 0].IsEmpty); // empty got filled
        }

        [Test]
        public void GravityThenRefill_FullCycle_NoCellsEmpty()
        {
            // Clear several cells
            _board.ClearTile(new GridPosition(7, 3));
            _board.ClearTile(new GridPosition(6, 3));
            _board.ClearTile(new GridPosition(5, 3));

            _gravity.ApplyGravity();
            _gravity.RefillBoard();

            for (int r = 0; r < Board.Board.Rows; r++)
                Assert.IsFalse(_board[r, 3].IsEmpty, $"Cell ({r},3) is still empty after gravity+refill");
        }
    }
}
```

- [ ] **Step 2: Run tests**

Run: Unity Test Runner > EditMode > Run All
Expected: All 5 GravityHandler tests PASS

- [ ] **Step 3: Commit**

```bash
git add Assets/Tests/EditMode/GravityTests.cs
git commit -m "test: add unit tests for GravityHandler"
```

---

## Task 7: Unit Tests — DiceSystem & ObjectiveChecker

**Files:**
- Create: `Assets/Tests/EditMode/DiceSystemTests.cs`
- Create: `Assets/Tests/EditMode/ObjectiveCheckerTests.cs`
- Read: `Assets/Scripts/Dice/DiceSystem.cs`, `Assets/Scripts/Objectives/ObjectiveChecker.cs`

- [ ] **Step 1: Write DiceSystem tests**

Create `Assets/Tests/EditMode/DiceSystemTests.cs`:

```csharp
using NUnit.Framework;
using Matchmancer.Core;
using Matchmancer.Dice;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class DiceSystemTests
    {
        private Board.Board _board;
        private DiceSystem _dice;

        [SetUp]
        public void SetUp()
        {
            _board = new Board.Board();
            _board.Initialize(() => TileType.PortRune);
            _dice = new DiceSystem(_board, new System.Random(42)); // seeded for reproducibility
        }

        [Test]
        public void Roll_ReturnsValidSchool()
        {
            var result = _dice.Roll();
            Assert.IsTrue(System.Enum.IsDefined(typeof(MagickSchool), result.School));
        }

        [Test]
        public void Roll_ReturnsNonEmptyAffectedPositions()
        {
            var result = _dice.Roll();
            Assert.Greater(result.AffectedPositions.Count, 0);
        }

        [Test]
        public void Roll_AllPositionsAreInBounds()
        {
            for (int i = 0; i < 20; i++) // test multiple rolls
            {
                var result = _dice.Roll();
                foreach (var pos in result.AffectedPositions)
                {
                    Assert.IsTrue(_board.IsInBounds(pos),
                        $"Position {pos} is out of bounds on roll {i}");
                }
            }
        }

        [Test]
        public void Roll_HasEffectDescription()
        {
            var result = _dice.Roll();
            Assert.IsFalse(string.IsNullOrEmpty(result.EffectDescription));
        }

        [Test]
        public void Roll_AvoidsStoneBlocks()
        {
            _board.SetStoneBlock(3, 3, true);
            _board.SetStoneBlock(3, 4, true);
            _board.SetStoneBlock(4, 3, true);
            _board.SetStoneBlock(4, 4, true);

            for (int i = 0; i < 50; i++)
            {
                var result = _dice.Roll();
                foreach (var pos in result.AffectedPositions)
                {
                    Assert.IsTrue(_board.IsPlayable(pos.Row, pos.Col),
                        $"Dice affected stone block at {pos}");
                }
            }
        }
    }
}
```

- [ ] **Step 2: Write ObjectiveChecker tests**

Create `Assets/Tests/EditMode/ObjectiveCheckerTests.cs`:

```csharp
using NUnit.Framework;
using Matchmancer.Core;
using Matchmancer.Objectives;
using Matchmancer.StoneBlocks;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class ObjectiveCheckerTests
    {
        [Test]
        public void ReachScore_ScoreMet_Victory()
        {
            var scoring = new Scoring(100, 200, 300, 400, 500);
            var moveTracker = new MoveTracker(10);
            var board = new Board.Board();
            board.Initialize(() => TileType.PortRune);
            var stones = new StoneBlockSystem(board);
            var config = new ObjectiveConfig { Type = ObjectiveType.ReachScore, TargetScore = 100 };
            var checker = new ObjectiveChecker(config, scoring, moveTracker, stones);

            scoring.AddMatchScore(10); // 10 * 10 = 100

            Assert.AreEqual(LevelResult.Victory, checker.Evaluate());
        }

        [Test]
        public void ReachScore_MovesExhausted_Defeat()
        {
            var scoring = new Scoring(100, 200, 300, 400, 500);
            var moveTracker = new MoveTracker(1);
            var board = new Board.Board();
            board.Initialize(() => TileType.PortRune);
            var stones = new StoneBlockSystem(board);
            var config = new ObjectiveConfig { Type = ObjectiveType.ReachScore, TargetScore = 100 };
            var checker = new ObjectiveChecker(config, scoring, moveTracker, stones);

            moveTracker.DeductMove();
            Assert.AreEqual(LevelResult.Defeat, checker.Evaluate());
        }

        [Test]
        public void ClearAllStones_AllDestroyed_Victory()
        {
            var scoring = new Scoring(100, 200, 300, 400, 500);
            var moveTracker = new MoveTracker(10);
            var board = new Board.Board();
            board.Initialize(() => TileType.PortRune);
            var stones = new StoneBlockSystem(board);
            var config = new ObjectiveConfig { Type = ObjectiveType.ClearAllStones };
            var checker = new ObjectiveChecker(config, scoring, moveTracker, stones);

            // No stones placed — 0 remaining = victory
            Assert.AreEqual(LevelResult.Victory, checker.Evaluate());
        }

        [Test]
        public void ClearAllStones_StonesRemain_MovesLeft_InProgress()
        {
            var scoring = new Scoring(100, 200, 300, 400, 500);
            var moveTracker = new MoveTracker(10);
            var board = new Board.Board();
            board.Initialize(() => TileType.PortRune);
            var stones = new StoneBlockSystem(board);
            stones.PlaceStone(3, 3, 2);
            var config = new ObjectiveConfig { Type = ObjectiveType.ClearAllStones };
            var checker = new ObjectiveChecker(config, scoring, moveTracker, stones);

            Assert.AreEqual(LevelResult.InProgress, checker.Evaluate());
        }

        [Test]
        public void Survive_TurnsReached_Victory()
        {
            var scoring = new Scoring(100, 200, 300, 400, 500);
            var moveTracker = new MoveTracker(20);
            var board = new Board.Board();
            board.Initialize(() => TileType.PortRune);
            var stones = new StoneBlockSystem(board);
            var config = new ObjectiveConfig { Type = ObjectiveType.Survive, SurviveTurns = 3 };
            var checker = new ObjectiveChecker(config, scoring, moveTracker, stones);

            checker.IncrementTurn();
            checker.IncrementTurn();
            Assert.AreEqual(LevelResult.InProgress, checker.Evaluate());
            checker.IncrementTurn();
            Assert.AreEqual(LevelResult.Victory, checker.Evaluate());
        }
    }
}
```

- [ ] **Step 3: Run all tests**

Run: Unity Test Runner > EditMode > Run All
Expected: All DiceSystem (5) + ObjectiveChecker (5) tests PASS

- [ ] **Step 4: Commit**

```bash
git add Assets/Tests/EditMode/DiceSystemTests.cs Assets/Tests/EditMode/ObjectiveCheckerTests.cs
git commit -m "test: add unit tests for DiceSystem and ObjectiveChecker"
```

---

## Task 8: Implement Level 10 Boss Mechanic (Stone Spreading)

**Files:**
- Create: `Assets/Scripts/Core/BossMechanic.cs`
- Modify: `Assets/Scripts/Core/BoardController.cs` — add boss hook after turn resolution
- Create: `Assets/Tests/EditMode/BossMechanicTests.cs`

- [ ] **Step 1: Write the failing test**

Create `Assets/Tests/EditMode/BossMechanicTests.cs`:

```csharp
using System.Collections.Generic;
using NUnit.Framework;
using Matchmancer.Core;
using Matchmancer.StoneBlocks;

namespace Matchmancer.Tests
{
    [TestFixture]
    public class BossMechanicTests
    {
        private Board.Board _board;
        private StoneBlockSystem _stoneSystem;
        private BossMechanic _boss;

        [SetUp]
        public void SetUp()
        {
            _board = new Board.Board();
            _board.Initialize(() => TileType.PortRune);
            _stoneSystem = new StoneBlockSystem(_board);
            _stoneSystem.PlaceStone(3, 3, 2);
            _stoneSystem.PlaceStone(3, 4, 2);
            _boss = new BossMechanic(_board, _stoneSystem, spreadInterval: 3, spreadHP: 1);
        }

        [Test]
        public void SpreadStones_NotTriggered_BeforeInterval()
        {
            int stonesBefore = _stoneSystem.RemainingStones;
            _boss.OnTurnEnd(1); // turn 1
            Assert.AreEqual(stonesBefore, _stoneSystem.RemainingStones);
        }

        [Test]
        public void SpreadStones_Triggered_AtInterval()
        {
            int stonesBefore = _stoneSystem.RemainingStones;
            _boss.OnTurnEnd(3); // turn 3 = interval
            Assert.Greater(_stoneSystem.RemainingStones, stonesBefore);
        }

        [Test]
        public void SpreadStones_AddsAdjacentToExisting()
        {
            _boss.OnTurnEnd(3);
            // New stones should be adjacent to existing stones at (3,3) and (3,4)
            // Check that at least one new stone is adjacent
            bool foundAdjacent = false;
            foreach (var kvp in _stoneSystem.Stones)
            {
                var pos = kvp.Key;
                if (pos != new GridPosition(3, 3) && pos != new GridPosition(3, 4))
                {
                    bool isAdj = pos.IsAdjacentTo(new GridPosition(3, 3)) ||
                                 pos.IsAdjacentTo(new GridPosition(3, 4));
                    if (isAdj) foundAdjacent = true;
                }
            }
            Assert.IsTrue(foundAdjacent);
        }

        [Test]
        public void SpreadStones_TriggeredAgain_AtDoubleInterval()
        {
            _boss.OnTurnEnd(3);
            int afterFirst = _stoneSystem.RemainingStones;
            _boss.OnTurnEnd(6);
            Assert.Greater(_stoneSystem.RemainingStones, afterFirst);
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: Unity Test Runner > EditMode > BossMechanicTests
Expected: FAIL — `BossMechanic` class does not exist

- [ ] **Step 3: Implement BossMechanic**

Create `Assets/Scripts/Core/BossMechanic.cs`:

```csharp
using System.Collections.Generic;
using System.Linq;
using Matchmancer.StoneBlocks;

namespace Matchmancer.Core
{
    /// <summary>
    /// Level 10 boss mechanic: every N turns, new 1-HP stone blocks spread
    /// adjacent to existing stones.
    /// </summary>
    public class BossMechanic
    {
        private readonly Board.Board _board;
        private readonly StoneBlockSystem _stoneSystem;
        private readonly int _spreadInterval;
        private readonly int _spreadHP;

        public BossMechanic(Board.Board board, StoneBlockSystem stoneSystem, int spreadInterval = 3, int spreadHP = 1)
        {
            _board = board;
            _stoneSystem = stoneSystem;
            _spreadInterval = spreadInterval;
            _spreadHP = spreadHP;
        }

        /// <summary>
        /// Call at end of each turn with the current turn number (1-based).
        /// Returns positions of newly spawned stones (for animation).
        /// </summary>
        public List<GridPosition> OnTurnEnd(int turnNumber)
        {
            if (turnNumber % _spreadInterval != 0)
                return new List<GridPosition>();

            return SpreadStones();
        }

        private List<GridPosition> SpreadStones()
        {
            var candidates = new HashSet<GridPosition>();
            int[] dr = { -1, 1, 0, 0 };
            int[] dc = { 0, 0, -1, 1 };

            // Collect all empty positions adjacent to existing stones
            foreach (var kvp in _stoneSystem.Stones.ToList())
            {
                var stonePos = kvp.Key;
                for (int i = 0; i < 4; i++)
                {
                    int r = stonePos.Row + dr[i];
                    int c = stonePos.Col + dc[i];
                    if (!_board.IsInBounds(r, c)) continue;
                    if (_board.IsStoneBlock(r, c)) continue;

                    candidates.Add(new GridPosition(r, c));
                }
            }

            // Spawn 1-2 new stones from candidates (fewer = more survivable)
            var spawned = new List<GridPosition>();
            int toSpawn = System.Math.Min(2, candidates.Count);
            var candidateList = candidates.ToList();

            for (int i = 0; i < toSpawn && candidateList.Count > 0; i++)
            {
                var pos = candidateList[i]; // deterministic for testability
                _stoneSystem.PlaceStone(pos.Row, pos.Col, _spreadHP);
                spawned.Add(pos);
            }

            return spawned;
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: Unity Test Runner > EditMode > BossMechanicTests
Expected: All 4 tests PASS

- [ ] **Step 5: Add boss hook to BoardController**

Modify `Assets/Scripts/Core/BoardController.cs`. Add after the `_objectiveChecker` field:

```csharp
private BossMechanic _bossMechanic; // null for non-boss levels
private int _turnCount;
```

Add event:
```csharp
public event Action<List<GridPosition>> OnBossStonesSpawned;
```

In `InitializeLevel()`, after stone block placement, add:

```csharp
_turnCount = 0;

// Boss mechanic for Level 10 (Survive objective with stones)
if (config.Objective.Type == Objectives.ObjectiveType.Survive && config.StoneBlocks.Count > 0)
{
    _bossMechanic = new BossMechanic(_board, _stoneBlockSystem, spreadInterval: 3, spreadHP: 1);
}
else
{
    _bossMechanic = null;
}
```

In `ExecuteTurn()`, after `_objectiveChecker.IncrementTurn()` and before `var result = _objectiveChecker.Evaluate()`, add:

```csharp
_turnCount++;

// Boss mechanic: spread stones every N turns
if (_bossMechanic != null)
{
    var newStones = _bossMechanic.OnTurnEnd(_turnCount);
    if (newStones.Count > 0)
    {
        OnBossStonesSpawned?.Invoke(newStones);
        yield return new WaitForSeconds(0.3f);
    }
}
```

- [ ] **Step 6: Run all tests**

Run: Unity Test Runner > EditMode > Run All
Expected: All tests PASS (including BossMechanic)

- [ ] **Step 7: Commit**

```bash
git add Assets/Scripts/Core/BossMechanic.cs Assets/Scripts/Core/BoardController.cs Assets/Tests/EditMode/BossMechanicTests.cs
git commit -m "feat: add Level 10 boss mechanic — stones spread every 3 turns"
```

---

## Task 9: Unity Scene Setup & Placeholder Art

**Files:**
- Create: `Assets/Scenes/GameScene.unity` (via Unity Editor)
- Create: `Assets/Prefabs/TilePrefab.prefab` (via Unity Editor)
- Create: `Assets/Prefabs/StoneBlockPrefab.prefab` (via Unity Editor)
- Create: `Assets/Materials/TileMaterial.mat` (via Unity Editor)

This task is done entirely in the Unity Editor. No code changes.

- [ ] **Step 1: Create a new Unity 2D project (if not already done)**

Open Unity Hub > New Project > 2D (URP or Built-in) > name: SoulstreamMatchmancer > location: `/Users/marknp/Documents/`

If the project already exists, copy `Assets/Scripts/` and `Assets/Tests/` into it.

- [ ] **Step 2: Create TilePrefab**

1. In Project window: right-click Assets > Create > Folder > name "Prefabs"
2. In Hierarchy: Create > 2D Object > Sprites > Square
3. Rename to "Tile"
4. Add Component: `TileView` (from `Assets/Scripts/View/TileView.cs`)
5. Set SpriteRenderer scale to (0.6, 0.6, 1) — slightly smaller than cell size for gaps
6. Create a child object "SigilOverlay": 2D > Sprites > Square, scale (0.3, 0.3, 1), assign to `_sigilOverlay` field
7. Create a child object "SelectionHighlight": 2D > Sprites > Square, color yellow, alpha 0.3, assign to `_selectionHighlight` field, set inactive
8. Drag "Tile" from Hierarchy into `Assets/Prefabs/` to create prefab
9. Delete the Hierarchy instance

- [ ] **Step 3: Create StoneBlockPrefab**

1. In Hierarchy: Create > 2D Object > Sprites > Square
2. Rename to "StoneBlock"
3. Set SpriteRenderer color to dark gray (0.3, 0.3, 0.3, 1)
4. Set scale to (0.65, 0.65, 1)
5. Add Component: `StoneBlockView`
6. Drag into `Assets/Prefabs/`
7. Delete the Hierarchy instance

- [ ] **Step 4: Build the GameScene**

1. File > New Scene > Save as `Assets/Scenes/GameScene.unity`
2. Set Camera:
   - Position: (0, 0, -10)
   - Size: 5 (orthographic)
   - Background: dark color (#1A1A2E)
3. Create empty GameObject "GameRoot":
   - Add `GameManager` component
   - Add `BoardController` component
4. Create child "Board" under GameRoot:
   - Add `BoardView` component
   - Add `InputHandler` component
   - Assign TilePrefab and StoneBlockPrefab to BoardView's serialized fields
5. Create Canvas (Screen Space - Overlay):
   - Add `HUDView` component
   - Create child Text elements: "ScoreText", "MovesText", "MeterText", "ObjectiveText"
   - Position them at top of screen
   - Assign to HUDView's serialized fields
6. Wire up GameManager: drag BoardController, BoardView, InputHandler, HUDView into serialized fields
7. Set Level to Load = 1

- [ ] **Step 5: Test — hit Play**

Expected behavior:
- 8x8 grid of colored squares appears
- Clicking two adjacent tiles swaps them
- Valid matches clear, tiles fall, new tiles fill
- Score/Moves/Meter text updates
- After 10 charges, dice roll triggers (logged to console)

- [ ] **Step 6: Commit**

```bash
git add Assets/Scenes/ Assets/Prefabs/ Assets/Materials/
git commit -m "feat: add Unity scene, prefabs, and placeholder art for playable MVP"
```

---

## Task 10: Upgrade HUDView to TextMeshPro

**Files:**
- Modify: `Assets/Scripts/View/HUDView.cs`

- [ ] **Step 1: Import TextMeshPro**

In Unity: Window > Package Manager > TextMeshPro > Install
When prompted, import TMP Essential Resources.

- [ ] **Step 2: Update HUDView to use TMP**

Replace the using and field declarations in `Assets/Scripts/View/HUDView.cs`:

Replace:
```csharp
using UnityEngine.UI;
```
With:
```csharp
using TMPro;
```

Replace all four field declarations:
```csharp
[SerializeField] private Text _scoreText;
[SerializeField] private Text _movesText;
[SerializeField] private Image _meterFill;
[SerializeField] private Text _meterText;
[SerializeField] private Text _objectiveText;
```
With:
```csharp
[SerializeField] private TextMeshProUGUI _scoreText;
[SerializeField] private TextMeshProUGUI _movesText;
[SerializeField] private UnityEngine.UI.Image _meterFill;
[SerializeField] private TextMeshProUGUI _meterText;
[SerializeField] private TextMeshProUGUI _objectiveText;
```

The `.text` property access is identical on both `Text` and `TextMeshProUGUI`, so no other changes needed.

- [ ] **Step 3: Update scene — replace Text objects with TMP**

In GameScene:
1. Delete old Text child objects under Canvas
2. Create > UI > Text - TextMeshPro for each: ScoreText, MovesText, MeterText, ObjectiveText
3. Re-assign to HUDView fields
4. Style with Cinzel or Rajdhani font (import from Google Fonts if desired)

- [ ] **Step 4: Test in Play mode**

Expected: HUD text displays correctly with TMP rendering

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/View/HUDView.cs Assets/Scenes/GameScene.unity
git commit -m "feat: upgrade HUD to TextMeshPro"
```

---

## Summary

| Task | What | Tests |
|---|---|---|
| 1 | Board & GridPosition tests | 12 |
| 2 | MatchDetector tests | 10 |
| 3 | MagickMeter tests | 7 |
| 4 | Scoring + MoveTracker + StoneBlock tests | 18 |
| 5 | SigilSystem tests | 9 |
| 6 | GravityHandler tests | 5 |
| 7 | DiceSystem + ObjectiveChecker tests | 10 |
| 8 | Boss mechanic (code + tests) | 4 |
| 9 | Unity scene setup (Editor) | Manual |
| 10 | TextMeshPro upgrade | Manual |
| **Total** | | **75 unit tests** |

After all 10 tasks: the MVP is a playable Unity project with 26 scripts, 75 unit tests, 10 levels, and a complete core game loop.
