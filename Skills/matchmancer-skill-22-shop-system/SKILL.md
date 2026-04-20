---
name: "Matchmancer Skill 22: Shop System"
description: "Build the economy: ShopItemData ScriptableObject, ShopController for listing and purchasing boosters/gear/consumables, currency deduction through SaveManager, and a daily-rotation stock system. Load this skill when the user asks about the shop, Magickal Bazaar, buying items, currency, gold, purchase flow, or item listings. Requires Skills 15, 16, 20."
---

# Skill 22: Shop System

## Objective
Build the Magickal Bazaar. A page of items for sale. Tapping buys → deducts gold → grants the item to `InventoryManager` or `GearSystem`. Stock rotates daily using real-world date as a seed.

After this skill:
- `ShopController` enumerates today's stock.
- `TryPurchase(itemId)` checks gold, debits, grants, returns a result enum.
- Bought-out items grey out until next rotation.
- Designers add items by creating `ShopItemData` assets.

## Prerequisites
- Skill 15 — GearData, GearRoller, GearInstance
- Skill 16 — InventoryManager, BoosterData
- Skill 20 — SaveManager.Data.gold

---

## Step 1: ShopItemData SO

**File:** `Assets/Scripts/Shop/ShopItemData.cs`

```csharp
using Matchmancer.Character;
using UnityEngine;

namespace Matchmancer.Shop
{
    public enum ShopItemKind { Booster, Gear, Gold, Consumable }

    [CreateAssetMenu(fileName = "Shop_", menuName = "Matchmancer/Shop Item Data")]
    public class ShopItemData : ScriptableObject
    {
        public string       id;
        public ShopItemKind kind;
        public string       displayName;
        public Sprite       icon;
        public int          priceGold;
        public int          quantityPerPurchase = 1;

        [Header("Kind-specific payload")]
        public string       boosterId;        // for kind == Booster
        public GearData     gearArchetype;    // for kind == Gear
        public int          goldPayout;       // for kind == Gold (IAP-style, dev-testing only)
    }
}
```

---

## Step 2: PurchaseResult enum

**File:** `Assets/Scripts/Shop/PurchaseResult.cs`

```csharp
namespace Matchmancer.Shop
{
    public enum PurchaseResult { Success, InsufficientGold, OutOfStock, UnknownItem, InvalidPayload }
}
```

---

## Step 3: ShopController

**File:** `Assets/Scripts/Shop/ShopController.cs`

```csharp
using System;
using System.Collections.Generic;
using Matchmancer.Character;
using Matchmancer.Inventory;
using Matchmancer.Save;
using UnityEngine;

namespace Matchmancer.Shop
{
    public class ShopController : MonoBehaviour
    {
        public static ShopController Instance { get; private set; }

        [SerializeField] private List<ShopItemData> catalog;
        [SerializeField] private int                dailyStockSize = 6;

        private List<ShopItemData>    todaysStock = new List<ShopItemData>();
        private readonly HashSet<string> soldOut   = new HashSet<string>();

        public IReadOnlyList<ShopItemData> TodaysStock => todaysStock;
        public event Action                OnStockChanged;
        public event Action<ShopItemData, PurchaseResult> OnPurchaseAttempt;

        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            RotateStockIfNeeded();
        }

        public bool IsSoldOut(string id) => soldOut.Contains(id);

        private void RotateStockIfNeeded()
        {
            var d = SaveManager.Instance.Data;
            string today = System.DateTime.UtcNow.ToString("yyyy-MM-dd");
            var lastDayKey = PlayerPrefs.GetString("shop.rotation.day", "");
            if (lastDayKey == today && todaysStock.Count > 0) return;

            // Seed RNG by today's date — deterministic per day
            int seed = today.GetHashCode();
            var rng  = new System.Random(seed);

            todaysStock.Clear();
            soldOut.Clear();
            var pool = new List<ShopItemData>(catalog);
            for (int i = 0; i < dailyStockSize && pool.Count > 0; i++)
            {
                int pick = rng.Next(pool.Count);
                todaysStock.Add(pool[pick]);
                pool.RemoveAt(pick);
            }

            PlayerPrefs.SetString("shop.rotation.day", today);
            PlayerPrefs.Save();
            OnStockChanged?.Invoke();
        }

        public PurchaseResult TryPurchase(string id)
        {
            var item = todaysStock.Find(s => s != null && s.id == id);
            if (item == null)          { var r = PurchaseResult.UnknownItem;     OnPurchaseAttempt?.Invoke(item, r); return r; }
            if (soldOut.Contains(id))  { var r = PurchaseResult.OutOfStock;      OnPurchaseAttempt?.Invoke(item, r); return r; }

            var save = SaveManager.Instance.Data;
            if (save.gold < item.priceGold)
            { var r = PurchaseResult.InsufficientGold; OnPurchaseAttempt?.Invoke(item, r); return r; }

            save.gold -= item.priceGold;

            switch (item.kind)
            {
                case ShopItemKind.Booster:
                    if (string.IsNullOrEmpty(item.boosterId))
                    { save.gold += item.priceGold; var r = PurchaseResult.InvalidPayload; OnPurchaseAttempt?.Invoke(item, r); return r; }
                    InventoryManager.Instance.AddBooster(item.boosterId, item.quantityPerPurchase);
                    break;

                case ShopItemKind.Gear:
                    if (item.gearArchetype == null)
                    { save.gold += item.priceGold; var r = PurchaseResult.InvalidPayload; OnPurchaseAttempt?.Invoke(item, r); return r; }
                    var rolled = GearRoller.Roll(item.gearArchetype);
                    InventoryManager.Instance.AddGear(rolled);
                    break;

                case ShopItemKind.Gold:
                    save.gold += item.goldPayout;  // dev/testing
                    break;

                case ShopItemKind.Consumable:
                    InventoryManager.Instance.AddBooster(item.id, item.quantityPerPurchase);
                    break;
            }

            soldOut.Add(id);
            SaveManager.Instance.Save();
            OnPurchaseAttempt?.Invoke(item, PurchaseResult.Success);
            OnStockChanged?.Invoke();
            return PurchaseResult.Success;
        }
    }
}
```

---

## Step 4: ShopScreen UI

**File:** `Assets/Scripts/UI/ShopScreenController.cs`

```csharp
using Matchmancer.Save;
using Matchmancer.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Matchmancer.UI
{
    public class ShopScreenController : MonoBehaviour
    {
        [SerializeField] private Transform  gridRoot;
        [SerializeField] private GameObject itemCardPrefab;
        [SerializeField] private TMP_Text   goldLabel;
        [SerializeField] private Button     backButton;

        private void OnEnable()
        {
            ShopController.Instance.OnStockChanged += Rebuild;
            Rebuild();
            backButton.onClick.AddListener(() => ScreenRouter.Instance.Go(ScreenId.MainHub, false));
        }

        private void OnDisable()
        {
            ShopController.Instance.OnStockChanged -= Rebuild;
            backButton.onClick.RemoveAllListeners();
        }

        private void Rebuild()
        {
            goldLabel.text = $"{SaveManager.Instance.Data.gold} Gold";
            for (int i = gridRoot.childCount - 1; i >= 0; i--)
                Destroy(gridRoot.GetChild(i).gameObject);

            foreach (var item in ShopController.Instance.TodaysStock)
            {
                var go = Instantiate(itemCardPrefab, gridRoot);
                var card = go.GetComponent<ShopItemCard>();
                card.Bind(item, () => ShopController.Instance.TryPurchase(item.id));
            }
        }
    }

    public class ShopItemCard : MonoBehaviour
    {
        [SerializeField] private Image    iconImg;
        [SerializeField] private TMP_Text nameLbl, priceLbl;
        [SerializeField] private Button   buyBtn;
        [SerializeField] private GameObject soldOutOverlay;

        public void Bind(ShopItemData item, System.Action onBuy)
        {
            iconImg.sprite = item.icon;
            nameLbl.text   = item.displayName;
            priceLbl.text  = $"{item.priceGold}";
            buyBtn.onClick.RemoveAllListeners();
            buyBtn.onClick.AddListener(() => onBuy?.Invoke());
            bool sold = ShopController.Instance.IsSoldOut(item.id);
            if (soldOutOverlay) soldOutOverlay.SetActive(sold);
            buyBtn.interactable = !sold;
        }
    }
}
```

---

## Validation checklist

- [ ] Shop screen opens and populates with `dailyStockSize` items
- [ ] Same day → same items (deterministic)
- [ ] Insufficient gold → `InsufficientGold`, no debit, no grant
- [ ] Successful purchase deducts gold and grants to inventory/gear
- [ ] Item greys out after buying
- [ ] Stock re-rolls on next real-world day

---

## Report

**Built:** ShopItemData SO, PurchaseResult enum, ShopController with deterministic daily rotation, ShopScreenController + ShopItemCard.
**Files:** `ShopItemData.cs`, `PurchaseResult.cs`, `ShopController.cs` under `Assets/Scripts/Shop/`; `ShopScreenController.cs` under `UI/`.
**Assumptions:** SaveManager.Data.gold exists (Skill 20). Gear purchases use `GearRoller.Roll`. Rotation seed uses UTC date; swap to server time later for multiplayer-safe rotation.
**Next:** Skill 23 — Lore & Character Gallery reuses the same card-grid pattern.
