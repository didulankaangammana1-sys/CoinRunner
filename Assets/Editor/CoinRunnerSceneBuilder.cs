using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

/// <summary>Builds the lab level from editable Unity objects and assets.</summary>
public static class CoinRunnerSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/CoinRunner.unity";
    private const string PrefabFolder = "Assets/Prefabs";
    private const string TileFolder = "Assets/Tiles";
    private const string SpriteFolder = "Assets/Sprites";
    private const string TmpSettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";

    [MenuItem("Coin Runner/Rebuild Lab Scene")]
    public static void Build()
    {
        EnsureFolders();
        EnsureTagsAndGroundLayer();
        if (!EnsureTextMeshProResources())
            return;

        Sprite groundSprite = CreateSprite("GroundSprite", SpriteKind.Square, new Color32(139, 91, 61, 255));
        Sprite playerSprite = CreateSprite("PlayerSprite", SpriteKind.Capsule, new Color32(55, 190, 95, 255));
        Sprite coinSprite = CreateSprite("CoinSprite", SpriteKind.Circle, new Color32(255, 211, 55, 255));
        Sprite spikeSprite = CreateSprite("SpikeSprite", SpriteKind.Triangle, new Color32(220, 55, 60, 255));
        Sprite goalSprite = CreateSprite("GoalSprite", SpriteKind.Flag, new Color32(70, 205, 105, 255));
        Tile groundTile = CreateGroundTile(groundSprite);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "CoinRunner";

        Tilemap ground = CreateLevel(groundTile);
        GameObject player = CreatePlayer(playerSprite, ground.gameObject.layer);
        Camera camera = CreateCamera(player.transform);
        GameManager manager = CreateUI(out Button restartButton);
        UnityEventTools.AddPersistentListener(restartButton.onClick, manager.RestartGame);
        EditorUtility.SetDirty(restartButton);

        CreateCoins(coinSprite);
        CreateHazards(spikeSprite);
        CreateGoal(goalSprite);

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        Selection.activeObject = player;
        Debug.Log($"Coin Runner scene created at {ScenePath}. Camera: {camera.name}; ground tile count: {ground.GetUsedTilesCount()}.");
    }

    private static void EnsureFolders()
    {
        foreach (string path in new[] { "Assets/Scripts", "Assets/Editor", "Assets/Palettes", PrefabFolder, TileFolder, SpriteFolder, "Assets/Scenes" })
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path).Replace('\\', '/');
                string folder = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }
    }

    private static void EnsureTagsAndGroundLayer()
    {
        Object asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0];
        SerializedObject settings = new SerializedObject(asset);
        SerializedProperty tags = settings.FindProperty("tags");
        foreach (string tag in new[] { "Coin", "Hazard", "Goal" })
        {
            bool found = false;
            for (int i = 0; i < tags.arraySize; i++)
                found |= tags.GetArrayElementAtIndex(i).stringValue == tag;

            if (!found)
            {
                tags.InsertArrayElementAtIndex(tags.arraySize);
                tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
            }
        }

        SerializedProperty layers = settings.FindProperty("layers");
        int groundLayer = -1;
        for (int i = 0; i < layers.arraySize; i++)
        {
            string value = layers.GetArrayElementAtIndex(i).stringValue;
            if (value == "Ground")
                groundLayer = i;
        }

        if (groundLayer < 0)
        {
            for (int i = 8; i < layers.arraySize; i++)
            {
                if (string.IsNullOrEmpty(layers.GetArrayElementAtIndex(i).stringValue))
                {
                    layers.GetArrayElementAtIndex(i).stringValue = "Ground";
                    groundLayer = i;
                    break;
                }
            }
        }

        if (groundLayer < 0)
            throw new System.InvalidOperationException("No free user layer is available for Ground.");

        settings.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();
    }

    private static bool EnsureTextMeshProResources()
    {
        if (AssetDatabase.LoadAssetAtPath<TMP_Settings>(TmpSettingsPath) != null)
            return true;

        const string packagePath = "Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage";
        if (!File.Exists(packagePath))
            throw new FileNotFoundException("TextMeshPro essential resources package was not found.", packagePath);

        AssetDatabase.importPackageCompleted -= OnTmpResourcesImported;
        AssetDatabase.importPackageFailed -= OnTmpResourcesImportFailed;
        AssetDatabase.importPackageCompleted += OnTmpResourcesImported;
        AssetDatabase.importPackageFailed += OnTmpResourcesImportFailed;
        AssetDatabase.ImportPackage(packagePath, false);
        Debug.Log("Importing TextMeshPro essential resources before building the Coin Runner scene.");
        return false;
    }

    private static void OnTmpResourcesImported(string packageName)
    {
        AssetDatabase.importPackageCompleted -= OnTmpResourcesImported;
        AssetDatabase.importPackageFailed -= OnTmpResourcesImportFailed;
        Build();
    }

    private static void OnTmpResourcesImportFailed(string packageName, string errorMessage)
    {
        AssetDatabase.importPackageCompleted -= OnTmpResourcesImported;
        AssetDatabase.importPackageFailed -= OnTmpResourcesImportFailed;
        Debug.LogError($"Could not import TextMeshPro essential resources: {errorMessage}");
    }

    private static Tilemap CreateLevel(Tile tile)
    {
        GameObject gridObject = new GameObject("Grid", typeof(Grid));
        GameObject groundObject = new GameObject("Ground", typeof(Tilemap), typeof(TilemapRenderer), typeof(TilemapCollider2D));
        groundObject.transform.SetParent(gridObject.transform, false);
        groundObject.layer = LayerMask.NameToLayer("Ground");
        Tilemap map = groundObject.GetComponent<Tilemap>();
        map.color = Color.white;
        groundObject.GetComponent<TilemapRenderer>().sortingOrder = -2;

        PaintRange(map, tile, -10, 6, -4, -5);
        PaintRange(map, tile, 9, 22, -4, -5);
        PaintRange(map, tile, 2, 5, -1, -1);
        PaintRange(map, tile, 12, 15, 1, 1);
        PaintRange(map, tile, -10, -10, -3, 2);
        return map;
    }

    private static void PaintRange(Tilemap map, Tile tile, int xStart, int xEnd, int yStart, int yEnd)
    {
        for (int x = xStart; x <= xEnd; x++)
            for (int y = yEnd; y <= yStart; y++)
                map.SetTile(new Vector3Int(x, y, 0), tile);
    }

    private static GameObject CreatePlayer(Sprite sprite, int groundLayer)
    {
        GameObject player = new GameObject("Player", typeof(SpriteRenderer), typeof(CapsuleCollider2D), typeof(Rigidbody2D), typeof(PlayerMovement), typeof(PlayerInteractions));
        player.transform.position = new Vector3(-6f, -2f, 0f);
        player.GetComponent<SpriteRenderer>().sprite = sprite;
        player.GetComponent<SpriteRenderer>().sortingOrder = 2;

        CapsuleCollider2D capsule = player.GetComponent<CapsuleCollider2D>();
        capsule.size = new Vector2(0.85f, 1.55f);
        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;

        GameObject feet = new GameObject("GroundCheck");
        feet.transform.SetParent(player.transform, false);
        feet.transform.localPosition = new Vector3(0f, -1f, 0f);

        SerializedObject movement = new SerializedObject(player.GetComponent<PlayerMovement>());
        movement.FindProperty("groundCheck").objectReferenceValue = feet.transform;
        movement.FindProperty("groundLayer").intValue = 1 << groundLayer;
        movement.ApplyModifiedPropertiesWithoutUndo();
        return player;
    }

    private static Camera CreateCamera(Transform target)
    {
        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(CameraFollow));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(-6f, -1f, -10f);
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 6f;
        camera.backgroundColor = new Color(0.48f, 0.75f, 0.91f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        SerializedObject follow = new SerializedObject(cameraObject.GetComponent<CameraFollow>());
        follow.FindProperty("target").objectReferenceValue = target;
        follow.ApplyModifiedPropertiesWithoutUndo();
        return camera;
    }

    private static GameManager CreateUI(out Button restartButton)
    {
        GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        TMP_Text coinText = MakeText("CoinText", canvasObject.transform, "Coins: 0", 50, TextAlignmentOptions.TopLeft);
        RectTransform coinRect = coinText.rectTransform;
        coinRect.anchorMin = new Vector2(0f, 1f);
        coinRect.anchorMax = new Vector2(0f, 1f);
        coinRect.pivot = new Vector2(0f, 1f);
        coinRect.anchoredPosition = new Vector2(30f, -20f);
        coinRect.sizeDelta = new Vector2(500f, 80f);

        GameObject managerObject = new GameObject("GameManager", typeof(GameManager));
        GameManager manager = managerObject.GetComponent<GameManager>();

        GameObject panel = new GameObject("WinPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvasObject.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0.03f, 0.05f, 0.08f, 0.82f);

        TMP_Text winText = MakeText("WinText", panel.transform, "LEVEL COMPLETE!", 110, TextAlignmentOptions.Center);
        winText.rectTransform.anchorMin = new Vector2(0.08f, 0.58f);
        winText.rectTransform.anchorMax = new Vector2(0.92f, 0.9f);
        winText.rectTransform.offsetMin = Vector2.zero;
        winText.rectTransform.offsetMax = Vector2.zero;

        GameObject buttonObject = new GameObject("RestartButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(panel.transform, false);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.36f, 0.25f);
        buttonRect.anchorMax = new Vector2(0.64f, 0.42f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        buttonObject.GetComponent<Image>().color = new Color(0.96f, 0.65f, 0.16f, 1f);
        restartButton = buttonObject.GetComponent<Button>();
        ColorBlock colors = restartButton.colors;
        colors.highlightedColor = new Color(1f, 0.78f, 0.32f, 1f);
        colors.pressedColor = new Color(0.82f, 0.46f, 0.09f, 1f);
        restartButton.colors = colors;
        TMP_Text buttonText = MakeText("ButtonText", buttonObject.transform, "Play Again", 48, TextAlignmentOptions.Center);
        buttonText.rectTransform.anchorMin = Vector2.zero;
        buttonText.rectTransform.anchorMax = Vector2.one;
        buttonText.rectTransform.offsetMin = Vector2.zero;
        buttonText.rectTransform.offsetMax = Vector2.zero;

        SerializedObject managerSerialized = new SerializedObject(manager);
        managerSerialized.FindProperty("coinText").objectReferenceValue = coinText;
        managerSerialized.FindProperty("winPanel").objectReferenceValue = panel;
        managerSerialized.ApplyModifiedPropertiesWithoutUndo();

        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        return manager;
    }

    private static TMP_Text MakeText(string name, Transform parent, string value, int fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.font = TMP_Settings.defaultFontAsset;
        return text;
    }

    private static void CreateCoins(Sprite sprite)
    {
        GameObject coin = new GameObject("Coin", typeof(SpriteRenderer), typeof(CircleCollider2D));
        coin.tag = "Coin";
        coin.transform.localScale = Vector3.one * 0.5f;
        coin.GetComponent<SpriteRenderer>().sprite = sprite;
        coin.GetComponent<SpriteRenderer>().sortingOrder = 3;
        coin.GetComponent<CircleCollider2D>().isTrigger = true;
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(coin, $"{PrefabFolder}/Coin.prefab");
        Object.DestroyImmediate(coin);

        foreach (Vector2 position in new[] { new Vector2(0f, -2f), new Vector2(4f, 0f), new Vector2(10f, -2f), new Vector2(13f, 3f), new Vector2(16f, -2f), new Vector2(19f, -2f) })
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = position;
        }
    }

    private static void CreateHazards(Sprite sprite)
    {
        GameObject spike = new GameObject("Spike", typeof(SpriteRenderer), typeof(PolygonCollider2D));
        spike.tag = "Hazard";
        spike.GetComponent<SpriteRenderer>().sprite = sprite;
        spike.GetComponent<SpriteRenderer>().sortingOrder = 2;
        PolygonCollider2D polygon = spike.GetComponent<PolygonCollider2D>();
        polygon.isTrigger = false;
        polygon.SetPath(0, new[] { new Vector2(-0.5f, -0.5f), new Vector2(0.5f, -0.5f), new Vector2(0f, 0.5f) });
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(spike, $"{PrefabFolder}/Spike.prefab");
        Object.DestroyImmediate(spike);

        foreach (Vector2 position in new[] { new Vector2(11f, -3f), new Vector2(17f, -3f) })
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = position;
        }
    }

    private static void CreateGoal(Sprite sprite)
    {
        GameObject goal = new GameObject("Goal", typeof(SpriteRenderer), typeof(BoxCollider2D));
        goal.tag = "Goal";
        goal.transform.position = new Vector3(21f, -2f, 0f);
        goal.transform.localScale = new Vector3(0.3f, 2f, 1f);
        goal.GetComponent<SpriteRenderer>().sprite = sprite;
        goal.GetComponent<SpriteRenderer>().sortingOrder = 4;
        goal.GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private static Tile CreateGroundTile(Sprite sprite)
    {
        const string path = TileFolder + "/GroundTile.asset";
        Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
        if (tile == null)
        {
            tile = ScriptableObject.CreateInstance<Tile>();
            AssetDatabase.CreateAsset(tile, path);
        }

        tile.sprite = sprite;
        tile.colliderType = Tile.ColliderType.Sprite;
        EditorUtility.SetDirty(tile);
        return tile;
    }

    private enum SpriteKind { Square, Capsule, Circle, Triangle, Flag }

    private static Sprite CreateSprite(string name, SpriteKind kind, Color32 color)
    {
        string path = $"{SpriteFolder}/{name}.png";
        if (!File.Exists(path))
        {
            const int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            Color32 clear = new Color32(0, 0, 0, 0);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool fill = kind switch
                    {
                        SpriteKind.Square => true,
                        SpriteKind.Capsule => InCapsule(x, y),
                        SpriteKind.Circle => (x - 31.5f) * (x - 31.5f) + (y - 31.5f) * (y - 31.5f) <= 28f * 28f,
                        SpriteKind.Triangle => y < 58 && y > 5 && x >= 5 + (y - 5) * 0.45f && x <= 59 - (y - 5) * 0.45f,
                        SpriteKind.Flag => (x >= 8 && x <= 14) || (y >= 43 && y <= 56 && x >= 14 && x <= 55 - (y - 43)),
                        _ => false
                    };
                    texture.SetPixel(x, y, fill ? color : clear);
                }
            }
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
        }

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 64f;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static bool InCapsule(int x, int y)
    {
        const float center = 31.5f;
        float dx = x - center;
        float dy = y - center;
        if (y >= 20 && y <= 43)
            return Mathf.Abs(dx) <= 21f;

        float capY = y < 20 ? 20f : 43f;
        return dx * dx + (y - capY) * (y - capY) <= 21f * 21f;
    }
}
