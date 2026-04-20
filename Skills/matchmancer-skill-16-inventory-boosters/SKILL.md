---
name: "Matchmancer Skill 16: Inventory & Booster System"
description: "Build the inventory + booster layer: BoosterData ScriptableObject, BoosterEffect runtime (hammer/shuffle/extra-moves/instant-damage), InventoryManager singleton for consumable quantities and gear storage, and the in-battle booster activation flow. Load this skill when the user asks about inventory, boosters, consumables, hammer tile breaker, shuffle, extra moves, or pre-battle loadout. Requires Skills 11–15."
---

# Skill 16: Inventory & Booster System

## Objective
Give the player a bag. Store gear (unequipped), consumables (potions, keys), and boosters (hammer, shuffle, +5 moves, instant damage). Make boosters usable in-battle via a single `InventoryManager.UseBooster(id)` call. All quantities persist through `SaveManager`.

After this skill:
- `InventoryManager` singleton holds gear list, consumable dictionary, booster counts.
- Boosters fire effects through a polymorphic `BoosterEffect` base class.
- `UseBooster` decrements quantity, plays effect, emits telemetry event.
- Save/load round-trips the full inventory.

## Prerequisites
- Skill 14 — CharacterRuntime (for heal/damage boosters)
- Skill 13 — EnemyController (for direct-damage booster)
- Skill 15 — GearInstance (gear storage)

---

## Step 1: BoosterData SO

**File:** `Assets/Scripts/Inventory/BoosterData.cs`

```csharp
using UnityEngine;

namespace Matchmancer.Inventory
{
    public enum BoosterType
    {
        Hammer,          // destroy one tile
        Shuffle,         // reshuffle board
        ExtraMoves,      // +N moves this battle
        InstantDamage,   // direct damage to enemy
        Heal             // player heal
    }

    [CreateAssetMenu(fileName = "Booster_", menuName = "Matchmancer/Booster Data")]
    public class BoosterData : ScriptableObject
    {
        public string     id = "hammer";
        public string     displayName = "Hammer";
        public Sprite     icon;
        public BoosterType type = BoosterType.Hammer;
        public float      value = 1f;       // meaning depends on type (damage, moves, etc.)
        public int        costGold = 50;
        [TextArea] public string description;
    }
}
```

---

## Step 2: BoosterEffect — polymorphic runtime

**File:** `Assets/Scripts/Inventory/BoosterEffect.cs`

```csharp
using Matchmancer.Character;
using Matchmancer.Combat;
using UnityEngine;

namespace Matchmancer.Inventory
{
    public static class BoosterEffect
    {
        public static void Apply(BoosterData data,
                                 CharacterRuntime character,
                                 EnemyController enemy,
                                 IBoardBoosterHost board)
        {
            switch (data.type)
            {
                case BoosterType.Hammer:
                    board?.RequestSingleTileBreak();
                    break;

                case BoosterType.Shuffle:
                    board?.RequestShuffle();
                    break;

                case BoosterType.ExtraMoves:
                    board?.AddMoves(Mathf.RoundToInt(data.value));
                    break;

                case BoosterType.InstantDamage:
                    enemy?.ApplyExternalDamage(data.value);
                    break;

                case BoosterType.Heal:
                    character?.Heal(data.value);
                    break;
            }
            Debug.Log($"[Booster] Applied {data.displayName} (value {data.value})");
        }
    }

    /// <summary>Implemented by the scene's board/game controller so boosters can drive board actions.</summary>
    public interface IBoardBoosterHost
    {
        void RequestSingleTileBreak();
        void RequestShuffle();
        void AddMoves(int amount);
    }
}
```

> `EnemyController.ApplyExternalDamage(float)` and `CharacterRuntime.Heal(float)` are small public wrappers — add them if they don't already exist.

---

## Step 3: InventoryManager — singleton

**File:** `Assets/Scripts/Inventory/InventoryManager.cs`

```csharp
using System;
using System.Collections.Generic;
using Matchmancer.Character;
using Matchmancer.Combat;
using UnityEngine;

namespace Matchmancer.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [SerializeField] private BoosterData[] boosterCatalog;

        // Runtime state
        private readonly Dictionary<string, int> boosterCounts = new Dictionary<string, int>();
        private readonly List<GearInstance>      storedGear    = new List<GearInstance>();

        public IReadOnlyDictionary<string, int> BoosterCounts => boosterCounts;
        public IReadOnlyList<GearInstance>      StoredGear    => storedGear;

        public event Action<string, int> OnBoosterCountChanged;   // (id, newCount)
        public event Action<GearInstance> OnGearAcquired;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ---------- Boosters ----------

        public int GetCount(string id) => boosterCounts.TryGetValue(id, out var n) ? n : 0;

        public void AddBooster(string id, int amount)
        {
            if (amount <= 0) return;
            boosterCounts[id] = GetCount(id) + amount;
            OnBoosterCountChanged?.Invoke(id, boosterCounts[id]);
        }

        public bool UseBooster(string id, CharacterRuntime character, EnemyController enemy, IBoardBoosterHost board)
        {
            int count = GetCount(id);
            if (count <= 0) return false;

            var data = FindBooster(id);
            if (data == null) { Debug.LogWarning($"[Inventory] Booster {id} not in catalog"); return false; }

            BoosterEffect.Apply(data, character, enemy, board);
            boosterCounts[id] = count - 1;
            OnBoosterCountChanged?.Invoke(id, boosterCounts[id]);
            return true;
        }

        private BoosterData FindBooster(string id)
        {
            foreach (var b in boosterCatalog)
                if (b != null && b.id == id) return b;
            return null;
        }

        // ---------- Gear storage ----------

        public void AddGear(GearInstance gear)
        {
            if (gear == null) return;
            storedGear.Add(gear);
            OnGearAcquired?.Invoke(gear);
        }

        public void RemoveGear(GearInstance gear) => storedGear.Remove(gear);

        // ---------- Save/load snapshot ----------

        [Serializable]
        public class Snapshot
        {
            public List<string>    boosterIds    = new List<string>();
            public List<int>       boosterCounts = new List<int>();
            public List<GearInstance> gear       = new List<GearInstance>();
        }

        public Snapshot CaptureSnapshot()
        {
            var s = new Snapshot();
            foreach (var kv in boosterCounts)
            {
                s.boosterIds.Add(kv.Key);
                s.boosterCounts.Add(kv.Value);
            }
            s.gear.AddRange(storedGear);
            return s;
        }

        public void RestoreSnapshot(Snapshot s)
        {
            boosterCounts.Clear();
            storedGear.Clear();
            if (s == null) return;
            for (int i = 0; i < s.boosterIds.Count; i++)
                boosterCounts[s.boosterIds[i]] = s.boosterCounts[i];
            storedGear.AddRange(s.gear);
        }
    }
}
```

---

## Step 4: Hook SaveManager

In `MatchmancerSaveData.cs` (Skill 20), add a `InventoryManager.Snapshot inventory` field. `SaveManager.Save` calls `InventoryManager.Instance.CaptureSnapshot()`, `Load` calls `RestoreSnapshot`.

---

## Step 5: Battle UI integration (stub for Skill 24)

Skill 24 wires booster buttons to:
```csharp
InventoryManager.Instance.UseBooster("hammer", character, enemy, boardHost);
```
The button should grey out when count == 0 — subscribe to `OnBoosterCountChanged`.

---

## Validation checklist

- [ ] Scripts compile
- [ ] `InventoryManager.Instance` persists across scene loads
- [ ] `AddBooster("hammer", 2)` then `UseBooster("hammer", …)` leaves count = 1
- [ ] `UseBooster` of an empty id returns false, no crash
- [ ] `CaptureSnapshot` / `RestoreSnapshot` round-trip
- [ ] Hammer booster actually calls `board.RequestSingleTileBreak()`
- [ ] Heal booster increases `CharacterRuntime.CurrentHP` and clamps to `EffectiveMaxHP`

---

## Report

**Built:** BoosterData SO, BoosterEffect switch, IBoardBoosterHost contract, InventoryManager singleton with booster counts + gear list + save snapshot.
**Files:** `BoosterData.cs`, `BoosterEffect.cs`, `InventoryManager.cs` under `Assets/Scripts/Inventory/`. Catalog assets under `Assets/Data/Boosters/`.
**Modified:** `EnemyController.ApplyExternalDamage`, `CharacterRuntime.Heal` added. Board controller implements `IBoardBoosterHost`.
**Assumptions:** One active InventoryManager in scene. Booster catalog assigned in Inspector. Gold cost is debited by ShopController in Skill 22, not here.
**Next:** Skill 17 — level/stage data flows level rewards into `AddBooster` / `AddGear`.
