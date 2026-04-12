using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using Matchmancer.Core;
using Matchmancer.View;
using Matchmancer.Enemy;
using Matchmancer.Character;
using Matchmancer.Combat;
using Matchmancer.Progression;

namespace Matchmancer.Editor
{
    /// <summary>
    /// One-click battle scene generator.
    /// Menu: Matchmancer ▸ Setup Battle Scene
    ///
    /// Creates every required GameObject, component, Canvas layout, prefab,
    /// and ScriptableObject asset, then wires all [SerializeField] references
    /// so you can press Play immediately.
    /// </summary>
    public static class MatchmancerSceneSetup
    {
        private const string DataPath   = "Assets/Data";
        private const string PrefabPath = "Assets/Prefabs";

        [MenuItem("Matchmancer/Setup Battle Scene", priority = 0)]
        public static void SetupBattleScene()
        {
            // --- Work in the current scene ---
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            EnsureFolder(DataPath);
            EnsureFolder(PrefabPath);

            // ── ScriptableObject Assets ─────────────────────────────────
            var combatConfig  = CreateOrLoad<CombatConfig>($"{DataPath}/CombatConfig.asset");
            var characterData = CreateOrLoad<CharacterData>($"{DataPath}/DefaultCharacter.asset");
            var enemyData     = CreateOrLoad<EnemyData>($"{DataPath}/Stage1_Enemy.asset");

            // Set reasonable defaults on character if fresh
            SetCharacterDefaults(characterData);
            SetEnemyDefaults(enemyData);
            EditorUtility.SetDirty(combatConfig);
            EditorUtility.SetDirty(characterData);
            EditorUtility.SetDirty(enemyData);

            // ── Placeholder Textures / Sprites ──────────────────────────
            var tileSprite  = GetOrCreatePlaceholderSprite("TileSprite",   Color.white, 64);
            var stoneSprite = GetOrCreatePlaceholderSprite("StoneSprite",  new Color(0.5f, 0.5f, 0.5f), 64);
            var starFilled  = GetOrCreatePlaceholderSprite("StarFilled",  Color.yellow, 32);
            var starEmpty   = GetOrCreatePlaceholderSprite("StarEmpty",   new Color(0.3f, 0.3f, 0.3f), 32);

            // ── Tile Prefab ─────────────────────────────────────────────
            var tilePrefab = CreateTilePrefab(tileSprite);

            // ── Stone Block Prefab ──────────────────────────────────────
            var stonePrefab = CreateStoneBlockPrefab(stoneSprite);

            // ── Camera ──────────────────────────────────────────────────
            var mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.transform.position = new Vector3(0f, 0f, -10f);
                mainCam.orthographic = true;
                mainCam.orthographicSize = 5f;
                mainCam.backgroundColor = new Color(0.08f, 0.06f, 0.12f); // dark purple
            }

            // ── Root GameObjects ────────────────────────────────────────
            var gameRoot    = new GameObject("GameManager");
            var boardGO     = new GameObject("BoardController");
            var boardViewGO = new GameObject("BoardView");
            var inputGO     = new GameObject("InputHandler");
            var enemyGO     = new GameObject("EnemyTurnController");
            var charGO      = new GameObject("CharacterBattleController");
            var gearGO      = new GameObject("GearInventoryController");
            var resultsGO   = new GameObject("ResultsController");

            // ── Add Components ──────────────────────────────────────────
            var gm          = gameRoot.AddComponent<GameManager>();
            var bc          = boardGO.AddComponent<BoardController>();
            var bv          = boardViewGO.AddComponent<BoardView>();
            var ih          = inputGO.AddComponent<InputHandler>();
            var etc         = enemyGO.AddComponent<EnemyTurnController>();
            var cbc         = charGO.AddComponent<CharacterBattleController>();
            var gic         = gearGO.AddComponent<GearInventoryController>();
            var rc          = resultsGO.AddComponent<ResultsController>();

            // ── Canvas + UI ─────────────────────────────────────────────
            var canvasGO = CreateBattleCanvas();
            var orchestrator = canvasGO.AddComponent<BattleUIOrchestrator>();

            // Create all UI panels
            var hudGO       = CreateUIPanel(canvasGO.transform, "HUD",              typeof(HUDView));
            var enemyPanel  = CreateUIPanel(canvasGO.transform, "EnemyPanel",       typeof(EnemyDisplayView));
            var charPanel   = CreateUIPanel(canvasGO.transform, "CharacterPanel",   typeof(CharacterDisplayView));
            var ultBtn      = CreateUIPanel(canvasGO.transform, "UltimateButton",   typeof(UltimateButtonView));
            var comboBanner = CreateUIPanel(canvasGO.transform, "ComboBanner",      typeof(ComboBannerView));
            var popupCont   = CreateUIPanel(canvasGO.transform, "PopupContainer",   null);
            var resultsOvr  = CreateUIPanel(canvasGO.transform, "ResultsOverlay",   typeof(BattleResultsView));

            // Create popup prefab
            var popupPrefab = CreatePopupPrefab();

            // Popup anchors (empty transforms near enemy / character)
            var enemyAnchor = new GameObject("EnemyPopupAnchor");
            enemyAnchor.transform.position = new Vector3(0f, 3f, 0f);
            var charAnchor  = new GameObject("CharacterPopupAnchor");
            charAnchor.transform.position = new Vector3(-2f, -3f, 0f);

            // ── Wire ALL SerializeField references ──────────────────────

            // GameManager
            SetField(gm, "_boardController", bc);
            SetField(gm, "_boardView",       bv);
            SetField(gm, "_inputHandler",    ih);
            SetField(gm, "_hudView",         hudGO.GetComponent<HUDView>());
            SetField(gm, "_enemyTurnController",      etc);
            SetField(gm, "_characterBattleController", cbc);
            SetField(gm, "_gearInventoryController",   gic);
            SetField(gm, "_resultsController",         rc);
            SetField(gm, "_battleUIOrchestrator",      orchestrator);

            // BoardController
            SetField(bc, "_combatConfig", combatConfig);

            // BoardView
            SetField(bv, "_tilePrefab",       tilePrefab);
            SetField(bv, "_stoneBlockPrefab", stonePrefab);

            // EnemyTurnController
            SetField(etc, "boardController", bc);
            SetField(etc, "enemyData",       enemyData);
            SetField(etc, "combatConfig",    combatConfig);

            // CharacterBattleController
            SetField(cbc, "boardController",      bc);
            SetField(cbc, "enemyTurnController",  etc);
            SetField(cbc, "characterData",        characterData);

            // GearInventoryController
            SetField(gic, "characterBattleController", cbc);

            // ResultsController
            // (no LevelProgressionManager in scene yet — fine for MVP)

            // BattleUIOrchestrator
            SetField(orchestrator, "boardController",          bc);
            SetField(orchestrator, "enemyTurnController",      etc);
            SetField(orchestrator, "characterBattleController", cbc);
            SetField(orchestrator, "resultsController",        rc);
            SetField(orchestrator, "enemyDisplay",     enemyPanel.GetComponent<EnemyDisplayView>());
            SetField(orchestrator, "characterDisplay", charPanel.GetComponent<CharacterDisplayView>());
            SetField(orchestrator, "ultimateButton",   ultBtn.GetComponent<UltimateButtonView>());
            SetField(orchestrator, "comboBanner",      comboBanner.GetComponent<ComboBannerView>());
            SetField(orchestrator, "resultsView",      resultsOvr.GetComponent<BattleResultsView>());
            SetField(orchestrator, "_popupPrefab",     popupPrefab.GetComponent<DamagePopupView>());
            SetField(orchestrator, "_popupContainer",  popupCont.GetComponent<RectTransform>());
            SetField(orchestrator, "_enemyPopupAnchor",    enemyAnchor.transform);
            SetField(orchestrator, "_characterPopupAnchor", charAnchor.transform);

            // BattleResultsView — wire star sprites + continue button
            var brv = resultsOvr.GetComponent<BattleResultsView>();
            if (brv != null)
            {
                SetField(brv, "_starFilled", starFilled);
                SetField(brv, "_starEmpty",  starEmpty);
            }

            // ── Mark everything dirty and save ──────────────────────────
            EditorUtility.SetDirty(gameRoot);
            EditorUtility.SetDirty(boardGO);
            EditorUtility.SetDirty(boardViewGO);
            EditorUtility.SetDirty(inputGO);
            EditorUtility.SetDirty(enemyGO);
            EditorUtility.SetDirty(charGO);
            EditorUtility.SetDirty(gearGO);
            EditorUtility.SetDirty(resultsGO);
            EditorUtility.SetDirty(canvasGO);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Save the scene
            EnsureFolder("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/BattleScene.unity");

            Debug.Log("<color=#00FF88>[Matchmancer]</color> Battle scene created! " +
                      "Press Play to test Level 1.");
        }

        // =================================================================
        // Helpers
        // =================================================================

        private static void SetField(object target, string fieldName, object value)
        {
            var so = new SerializedObject(target as Object);
            var prop = so.FindProperty(fieldName);
            if (prop != null && value is Object unityObj)
            {
                prop.objectReferenceValue = unityObj;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                Debug.LogWarning($"[SceneSetup] Could not set '{fieldName}' on {target.GetType().Name}");
            }
        }

        private static T CreateOrLoad<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;

            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parts = path.Split('/');
            var current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static void SetCharacterDefaults(CharacterData cd)
        {
            if (cd.displayName == "The Arcanist" || string.IsNullOrEmpty(cd.displayName))
            {
                cd.displayName     = "Blue";
                cd.baseMaxHp       = 150;
                cd.baseAttack      = 10f;
                cd.baseDefense     = 5f;
                cd.baseLuck        = 0f;
                cd.hpPerLevel      = 12;
                cd.attackPerLevel  = 1.2f;
                cd.defensePerLevel = 0.5f;
                cd.luckPerLevel    = 0.2f;
                cd.ultimateName    = "Soul Surge";
                cd.maxEnergy       = 100f;
                cd.ultimateDamageMultiplier = 3f;
                cd.xpPerLevel = new int[] { 100, 150, 220, 310, 420, 550, 700, 870, 1060 };
            }
        }

        private static void SetEnemyDefaults(EnemyData ed)
        {
            if (ed.displayName == "Unknown Foe" || string.IsNullOrEmpty(ed.displayName))
            {
                ed.displayName   = "Shadow Wisp";
                ed.maxHp         = 120;
                ed.defense       = 0f;
                ed.maxArmor      = 0;
                ed.baseAttackPower = 12f;
                ed.attacksPerTurn = 1;
            }
        }

        // ── Sprite generation ───────────────────────────────────────────

        private static Sprite GetOrCreatePlaceholderSprite(string name, Color color, int size)
        {
            string path = $"{PrefabPath}/{name}.png";
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null) return existing;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();

            byte[] png = tex.EncodeToPNG();
            File.WriteAllBytes(path, png);
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);

            // Configure as sprite
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = size;
                importer.filterMode = FilterMode.Point;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        // ── Prefab creation ─────────────────────────────────────────────

        private static GameObject CreateTilePrefab(Sprite sprite)
        {
            string path = $"{PrefabPath}/Tile.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject("Tile");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 1;

            var tv = go.AddComponent<TileView>();
            SetField(tv, "_spriteRenderer", sr);

            // Selection highlight child
            var highlight = new GameObject("SelectionHighlight");
            highlight.transform.SetParent(go.transform);
            highlight.transform.localPosition = Vector3.zero;
            var hlSr = highlight.AddComponent<SpriteRenderer>();
            hlSr.sprite = sprite;
            hlSr.color = new Color(1f, 1f, 0f, 0.4f);
            hlSr.sortingOrder = 2;
            highlight.SetActive(false);
            SetField(tv, "_selectionHighlight", highlight);

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateStoneBlockPrefab(Sprite sprite)
        {
            string path = $"{PrefabPath}/StoneBlock.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject("StoneBlock");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(0.5f, 0.5f, 0.5f);
            sr.sortingOrder = 3;

            var sbv = go.AddComponent<StoneBlockView>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreatePopupPrefab()
        {
            string path = $"{PrefabPath}/DamagePopup.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject("DamagePopup");
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(120, 40);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 28;
            tmp.fontStyle = FontStyles.Bold;
            tmp.text = "0";

            var dpv = go.AddComponent<DamagePopupView>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        // ── Canvas creation ─────────────────────────────────────────────

        private static GameObject CreateBattleCanvas()
        {
            var canvasGO = new GameObject("BattleCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // EventSystem
            #pragma warning disable CS0618 // FindObjectOfType is deprecated in newer Unity
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            #pragma warning restore CS0618
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            return canvasGO;
        }

        private static GameObject CreateUIPanel(Transform parent, string name, System.Type viewType)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            // Add CanvasGroup so views can fade
            go.AddComponent<CanvasGroup>();

            if (viewType != null)
                go.AddComponent(viewType);

            // Add TMP text children for views that need them
            if (viewType == typeof(HUDView))
            {
                SetupHUDChildren(go);
            }
            else if (viewType == typeof(EnemyDisplayView))
            {
                SetupEnemyDisplayChildren(go);
            }
            else if (viewType == typeof(CharacterDisplayView))
            {
                SetupCharacterDisplayChildren(go);
            }
            else if (viewType == typeof(ComboBannerView))
            {
                SetupComboBannerChildren(go);
            }
            else if (viewType == typeof(BattleResultsView))
            {
                SetupResultsChildren(go);
            }
            else if (viewType == typeof(UltimateButtonView))
            {
                SetupUltimateButtonChildren(go);
            }

            return go;
        }

        // ── UI child creation helpers ───────────────────────────────────

        private static TextMeshProUGUI AddTMPChild(GameObject parent, string name,
            TextAlignmentOptions align = TextAlignmentOptions.Center, int fontSize = 20)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent.transform, false);
            var rt = child.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            var tmp = child.AddComponent<TextMeshProUGUI>();
            tmp.alignment = align;
            tmp.fontSize = fontSize;
            tmp.text = name;
            return tmp;
        }

        private static Image AddImageChild(GameObject parent, string name, Color color)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent.transform, false);
            var rt = child.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            var img = child.AddComponent<Image>();
            img.color = color;
            return img;
        }

        private static void SetupHUDChildren(GameObject go)
        {
            var view = go.GetComponent<HUDView>();
            var score = AddTMPChild(go, "ScoreText", TextAlignmentOptions.TopLeft, 24);
            var moves = AddTMPChild(go, "MovesText", TextAlignmentOptions.TopRight, 24);
            var meter = AddImageChild(go, "MeterFill", new Color(0.3f, 0.7f, 1f));
            var meterTxt = AddTMPChild(go, "MeterText", TextAlignmentOptions.Center, 18);
            var obj = AddTMPChild(go, "ObjectiveText", TextAlignmentOptions.Top, 18);

            SetField(view, "_scoreText",     score);
            SetField(view, "_movesText",     moves);
            SetField(view, "_meterFill",     meter);
            SetField(view, "_meterText",     meterTxt);
            SetField(view, "_objectiveText", obj);
        }

        private static void SetupEnemyDisplayChildren(GameObject go)
        {
            var view = go.GetComponent<EnemyDisplayView>();
            var name_   = AddTMPChild(go, "NameText", TextAlignmentOptions.Center, 22);
            var hpFill  = AddImageChild(go, "HpFill", Color.red);
            var hpText  = AddTMPChild(go, "HpText", TextAlignmentOptions.Center, 18);

            // Armor group (hidden by default)
            var armorGroupGO = new GameObject("ArmorGroup");
            armorGroupGO.transform.SetParent(go.transform, false);
            armorGroupGO.AddComponent<RectTransform>();
            armorGroupGO.SetActive(false);
            var armorFill = AddImageChild(armorGroupGO, "ArmorFill", Color.gray);
            var armorText = AddTMPChild(armorGroupGO, "ArmorText", TextAlignmentOptions.Center, 16);

            // Status icons (hidden by default)
            var poisonIcon = new GameObject("PoisonIcon");
            poisonIcon.transform.SetParent(go.transform, false);
            poisonIcon.AddComponent<RectTransform>();
            poisonIcon.SetActive(false);
            var poisonStacks = AddTMPChild(poisonIcon, "PoisonStacks", TextAlignmentOptions.Center, 14);

            var vulnIcon = new GameObject("VulnerableIcon");
            vulnIcon.transform.SetParent(go.transform, false);
            vulnIcon.AddComponent<RectTransform>();
            vulnIcon.SetActive(false);

            SetField(view, "_nameText",         name_);
            SetField(view, "_hpFill",           hpFill);
            SetField(view, "_hpText",           hpText);
            SetField(view, "_armorFill",        armorFill);
            SetField(view, "_armorText",        armorText);
            SetField(view, "_armorGroup",       armorGroupGO);
            SetField(view, "_poisonIcon",       poisonIcon);
            SetField(view, "_poisonStacksText", poisonStacks);
            SetField(view, "_vulnerableIcon",   vulnIcon);
        }

        private static void SetupCharacterDisplayChildren(GameObject go)
        {
            var view = go.GetComponent<CharacterDisplayView>();
            var name_   = AddTMPChild(go, "NameText", TextAlignmentOptions.Center, 20);
            var lvl     = AddTMPChild(go, "LevelText", TextAlignmentOptions.Center, 16);
            var hpFill  = AddImageChild(go, "HpFill", Color.green);
            var hpText  = AddTMPChild(go, "HpText", TextAlignmentOptions.Center, 16);

            // Shield group (hidden by default)
            var shieldGroup = new GameObject("ShieldGroup");
            shieldGroup.transform.SetParent(go.transform, false);
            shieldGroup.AddComponent<RectTransform>();
            shieldGroup.SetActive(false);
            var shieldFill  = AddImageChild(shieldGroup, "ShieldFill", new Color(0.3f, 0.6f, 1f));
            var shieldText  = AddTMPChild(shieldGroup, "ShieldText", TextAlignmentOptions.Center, 14);

            var energyFill  = AddImageChild(go, "EnergyFill", Color.yellow);
            var energyText  = AddTMPChild(go, "EnergyText", TextAlignmentOptions.Center, 14);

            SetField(view, "_nameText",    name_);
            SetField(view, "_levelText",   lvl);
            SetField(view, "_hpFill",      hpFill);
            SetField(view, "_hpText",      hpText);
            SetField(view, "_shieldFill",  shieldFill);
            SetField(view, "_shieldText",  shieldText);
            SetField(view, "_shieldGroup", shieldGroup);
            SetField(view, "_energyFill",  energyFill);
            SetField(view, "_energyText",  energyText);
        }

        private static void SetupComboBannerChildren(GameObject go)
        {
            var view = go.GetComponent<ComboBannerView>();
            var cg = go.GetComponent<CanvasGroup>();
            var txt = AddTMPChild(go, "ComboText", TextAlignmentOptions.Center, 36);

            SetField(view, "_comboText",   txt);
            SetField(view, "_canvasGroup", cg);
        }

        private static void SetupResultsChildren(GameObject go)
        {
            var view = go.GetComponent<BattleResultsView>();

            var panel = new GameObject("Panel");
            panel.transform.SetParent(go.transform, false);
            panel.AddComponent<RectTransform>();
            var panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0f, 0f, 0f, 0.85f);
            panel.SetActive(false);

            var outcome = AddTMPChild(panel, "OutcomeText", TextAlignmentOptions.Center, 48);
            var score   = AddTMPChild(panel, "ScoreText", TextAlignmentOptions.Center, 24);
            var combo   = AddTMPChild(panel, "ComboText", TextAlignmentOptions.Center, 20);
            var hp      = AddTMPChild(panel, "HpText", TextAlignmentOptions.Center, 20);

            // Continue button
            var btnGO = new GameObject("ContinueButton");
            btnGO.transform.SetParent(panel.transform, false);
            var btnRt = btnGO.AddComponent<RectTransform>();
            btnRt.sizeDelta = new Vector2(250, 60);
            btnRt.anchoredPosition = new Vector2(0, -200);
            var btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.6f, 0.3f);
            var btn = btnGO.AddComponent<Button>();
            var btnTxt = AddTMPChild(btnGO, "Label", TextAlignmentOptions.Center, 22);
            btnTxt.text = "Continue";

            // Star images (5)
            var starContainer = new GameObject("Stars");
            starContainer.transform.SetParent(panel.transform, false);
            var starRt = starContainer.AddComponent<RectTransform>();
            starRt.sizeDelta = new Vector2(300, 50);
            var starLayout = starContainer.AddComponent<HorizontalLayoutGroup>();
            starLayout.spacing = 10f;
            starLayout.childAlignment = TextAnchor.MiddleCenter;

            var starImages = new Image[5];
            for (int i = 0; i < 5; i++)
            {
                var starGO = new GameObject($"Star{i + 1}");
                starGO.transform.SetParent(starContainer.transform, false);
                var srt = starGO.AddComponent<RectTransform>();
                srt.sizeDelta = new Vector2(40, 40);
                var sImg = starGO.AddComponent<Image>();
                sImg.color = Color.gray;
                starImages[i] = sImg;
            }

            SetField(view, "_panel",         panel);
            SetField(view, "_outcomeText",   outcome);
            SetField(view, "_scoreText",     score);
            SetField(view, "_comboText",     combo);
            SetField(view, "_hpText",        hp);
            SetField(view, "_continueButton", btn);

            // Star images are serialized as an array — use SerializedObject directly
            var so = new SerializedObject(view);
            var starProp = so.FindProperty("_starImages");
            if (starProp != null)
            {
                starProp.arraySize = 5;
                for (int i = 0; i < 5; i++)
                    starProp.GetArrayElementAtIndex(i).objectReferenceValue = starImages[i];
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetupUltimateButtonChildren(GameObject go)
        {
            var view = go.GetComponent<UltimateButtonView>();
            var nameTxt = AddTMPChild(go, "UltNameText", TextAlignmentOptions.Center, 16);

            SetField(view, "_nameText", nameTxt);
        }
    }
}
