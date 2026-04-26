using System.Collections.Generic;
using Core.Background;
using Core.Shop;
using UnityEditor;
using UnityEngine;

public class BackgroundShopEditorWindow : EditorWindow
{
    private BackgroundShopDatabase _shopDatabase;
    private SerializedObject _serializedShopDatabase;
    private Vector2 _scrollPos;

    [MenuItem("Tools/SuikaGame/Background Shop Editor")]
    private static void OpenWindow()
    {
        GetWindow<BackgroundShopEditorWindow>("Background Shop");
    }

    private void OnEnable()
    {
        TryAutoAssignDatabase();
    }

    private void OnGUI()
    {
        DrawDatabaseSelector();
        EditorGUILayout.Space();

        if (_shopDatabase == null)
        {
            EditorGUILayout.HelpBox(
                "Assign or create a BackgroundShopDatabase to edit shop data.",
                MessageType.Info);
            return;
        }

        EnsureSerializedObject();
        _serializedShopDatabase.Update();

        var backgroundDatabaseProperty = _serializedShopDatabase.FindProperty("backgroundDatabase");
        EditorGUILayout.PropertyField(backgroundDatabaseProperty, new GUIContent("Background Database"));
        DrawAutoAssignBackgroundDatabaseButton(backgroundDatabaseProperty);

        EditorGUILayout.Space(6f);

        var itemsProperty = _serializedShopDatabase.FindProperty("items");
        DrawItemsHeader(itemsProperty);

        int removeIndex = -1;
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        for (int i = 0; i < itemsProperty.arraySize; i++)
        {
            var itemProperty = itemsProperty.GetArrayElementAtIndex(i);
            if (DrawItem(itemProperty, i, _shopDatabase.backgroundDatabase))
            {
                removeIndex = i;
            }
        }
        EditorGUILayout.EndScrollView();

        if (removeIndex >= 0)
        {
            itemsProperty.DeleteArrayElementAtIndex(removeIndex);
        }

        if (_serializedShopDatabase.ApplyModifiedProperties())
        {
            EditorUtility.SetDirty(_shopDatabase);
        }

        DrawValidation(itemsProperty, _shopDatabase.backgroundDatabase);
    }

    private void DrawDatabaseSelector()
    {
        EditorGUILayout.LabelField("Shop Database", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        var selected = (BackgroundShopDatabase)EditorGUILayout.ObjectField(
            "Asset",
            _shopDatabase,
            typeof(BackgroundShopDatabase),
            false);
        if (EditorGUI.EndChangeCheck())
        {
            AssignDatabase(selected);
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create New Database"))
        {
            CreateDatabaseAsset();
        }

        using (new EditorGUI.DisabledScope(_shopDatabase == null))
        {
            if (GUILayout.Button("Select Asset"))
            {
                Selection.activeObject = _shopDatabase;
                EditorGUIUtility.PingObject(_shopDatabase);
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawAutoAssignBackgroundDatabaseButton(SerializedProperty backgroundDatabaseProperty)
    {
        if (backgroundDatabaseProperty.objectReferenceValue != null)
        {
            return;
        }

        EditorGUILayout.HelpBox("Background Database is required to pick valid backgrounds.", MessageType.Warning);
        if (GUILayout.Button("Auto Assign First BackgroundDatabase"))
        {
            var backgroundDatabase = FindFirstAsset<BackgroundDatabase>();
            if (backgroundDatabase != null)
            {
                backgroundDatabaseProperty.objectReferenceValue = backgroundDatabase;
            }
            else
            {
                Debug.LogWarning("No BackgroundDatabase asset found.");
            }
        }
    }

    private void DrawItemsHeader(SerializedProperty itemsProperty)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Shop Items", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Add Item", GUILayout.Width(90f)))
        {
            int newIndex = itemsProperty.arraySize;
            itemsProperty.InsertArrayElementAtIndex(newIndex);
            var itemProperty = itemsProperty.GetArrayElementAtIndex(newIndex);
            ResetItem(itemProperty, ResolveSuggestedBackgroundID(_shopDatabase.backgroundDatabase, itemsProperty));
        }

        EditorGUILayout.EndHorizontal();
    }

    private bool DrawItem(SerializedProperty itemProperty, int index, BackgroundDatabase backgroundDatabase)
    {
        var bannerProperty = itemProperty.FindPropertyRelative("Banner");
        var priceProperty = itemProperty.FindPropertyRelative("Price");
        var backgroundIDProperty = itemProperty.FindPropertyRelative("BackgroundID");

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Item {index + 1}", EditorStyles.boldLabel);
        bool shouldDelete = GUILayout.Button("Delete", GUILayout.Width(70f));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(bannerProperty, new GUIContent("Banner Sprite"));
        DrawSpritePreview(bannerProperty.objectReferenceValue as Sprite);

        int currentPrice = Mathf.Max(0, priceProperty.intValue);
        priceProperty.intValue = EditorGUILayout.IntField("Price", currentPrice);
        priceProperty.intValue = Mathf.Max(0, priceProperty.intValue);

        DrawBackgroundSelector(backgroundIDProperty, backgroundDatabase);

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(3f);
        return shouldDelete;
    }

    private static void DrawSpritePreview(Sprite sprite)
    {
        if (sprite == null)
        {
            return;
        }

        Rect previewRect = GUILayoutUtility.GetRect(96f, 96f, GUILayout.ExpandWidth(false));
        Texture2D preview = AssetPreview.GetAssetPreview(sprite);
        if (preview == null)
        {
            preview = sprite.texture;
        }

        if (preview != null)
        {
            GUI.DrawTexture(previewRect, preview, ScaleMode.ScaleToFit);
        }
    }

    private static void DrawBackgroundSelector(SerializedProperty backgroundIDProperty, BackgroundDatabase backgroundDatabase)
    {
        if (backgroundDatabase == null || backgroundDatabase.backgroundPictures == null || backgroundDatabase.backgroundPictures.Count == 0)
        {
            backgroundIDProperty.intValue = EditorGUILayout.IntField("Background ID", backgroundIDProperty.intValue);
            return;
        }

        var availableBackgrounds = new List<BackgroundData>();
        foreach (var background in backgroundDatabase.backgroundPictures)
        {
            if (background != null)
            {
                availableBackgrounds.Add(background);
            }
        }

        if (availableBackgrounds.Count == 0)
        {
            backgroundIDProperty.intValue = EditorGUILayout.IntField("Background ID", backgroundIDProperty.intValue);
            return;
        }

        int selectedIndex = availableBackgrounds.FindIndex(b => b.ID == backgroundIDProperty.intValue);
        var options = new string[availableBackgrounds.Count];
        for (int i = 0; i < availableBackgrounds.Count; i++)
        {
            string name = availableBackgrounds[i].Sprite != null ? availableBackgrounds[i].Sprite.name : "No Sprite";
            options[i] = $"ID {availableBackgrounds[i].ID} ({name})";
        }

        EditorGUI.BeginChangeCheck();
        int nextIndex = EditorGUILayout.Popup("Background", Mathf.Max(0, selectedIndex), options);
        if (EditorGUI.EndChangeCheck())
        {
            backgroundIDProperty.intValue = availableBackgrounds[nextIndex].ID;
        }

        if (selectedIndex < 0)
        {
            EditorGUILayout.HelpBox(
                $"Background ID {backgroundIDProperty.intValue} does not exist in BackgroundDatabase.",
                MessageType.Warning);
            backgroundIDProperty.intValue = EditorGUILayout.IntField("Background ID", backgroundIDProperty.intValue);
        }
    }

    private static void DrawValidation(SerializedProperty itemsProperty, BackgroundDatabase backgroundDatabase)
    {
        var seenBackgrounds = new HashSet<int>();
        var duplicateBackgrounds = new List<int>();
        var missingBackgrounds = new List<int>();

        for (int i = 0; i < itemsProperty.arraySize; i++)
        {
            var itemProperty = itemsProperty.GetArrayElementAtIndex(i);
            int backgroundID = itemProperty.FindPropertyRelative("BackgroundID").intValue;

            if (!seenBackgrounds.Add(backgroundID) && !duplicateBackgrounds.Contains(backgroundID))
            {
                duplicateBackgrounds.Add(backgroundID);
            }

            if (backgroundDatabase != null &&
                !HasBackgroundID(backgroundDatabase, backgroundID) &&
                !missingBackgrounds.Contains(backgroundID))
            {
                missingBackgrounds.Add(backgroundID);
            }
        }

        if (duplicateBackgrounds.Count > 0)
        {
            EditorGUILayout.HelpBox(
                $"Duplicate BackgroundID in shop items: {string.Join(", ", duplicateBackgrounds)}",
                MessageType.Warning);
        }

        if (missingBackgrounds.Count > 0)
        {
            EditorGUILayout.HelpBox(
                $"BackgroundID not found in BackgroundDatabase: {string.Join(", ", missingBackgrounds)}",
                MessageType.Warning);
        }
    }

    private void TryAutoAssignDatabase()
    {
        if (_shopDatabase != null)
        {
            return;
        }

        AssignDatabase(FindFirstAsset<BackgroundShopDatabase>());
    }

    private void AssignDatabase(BackgroundShopDatabase database)
    {
        _shopDatabase = database;
        EnsureSerializedObject();
        Repaint();
    }

    private void EnsureSerializedObject()
    {
        if (_shopDatabase == null)
        {
            _serializedShopDatabase = null;
            return;
        }

        if (_serializedShopDatabase == null || _serializedShopDatabase.targetObject != _shopDatabase)
        {
            _serializedShopDatabase = new SerializedObject(_shopDatabase);
        }
    }

    private void CreateDatabaseAsset()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create Background Shop Database",
            "BackgroundShopDatabase",
            "asset",
            "Choose where to save BackgroundShopDatabase.");
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        var database = CreateInstance<BackgroundShopDatabase>();
        database.backgroundDatabase = FindFirstAsset<BackgroundDatabase>();

        AssetDatabase.CreateAsset(database, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        AssignDatabase(database);
        Selection.activeObject = database;
        EditorGUIUtility.PingObject(database);
    }

    private static T FindFirstAsset<T>() where T : Object
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        if (guids == null || guids.Length == 0)
        {
            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }

    private static bool HasBackgroundID(BackgroundDatabase backgroundDatabase, int backgroundID)
    {
        if (backgroundDatabase == null || backgroundDatabase.backgroundPictures == null)
        {
            return false;
        }

        foreach (var background in backgroundDatabase.backgroundPictures)
        {
            if (background != null && background.ID == backgroundID)
            {
                return true;
            }
        }

        return false;
    }

    private static void ResetItem(SerializedProperty itemProperty, int suggestedBackgroundID)
    {
        itemProperty.FindPropertyRelative("BackgroundID").intValue = suggestedBackgroundID;
        itemProperty.FindPropertyRelative("Banner").objectReferenceValue = null;
        itemProperty.FindPropertyRelative("Price").intValue = 0;
    }

    private static int ResolveSuggestedBackgroundID(BackgroundDatabase backgroundDatabase, SerializedProperty itemsProperty)
    {
        if (backgroundDatabase == null || backgroundDatabase.backgroundPictures == null || backgroundDatabase.backgroundPictures.Count == 0)
        {
            return 1;
        }

        var usedBackgrounds = new HashSet<int>();
        for (int i = 0; i < itemsProperty.arraySize; i++)
        {
            var item = itemsProperty.GetArrayElementAtIndex(i);
            usedBackgrounds.Add(item.FindPropertyRelative("BackgroundID").intValue);
        }

        foreach (var background in backgroundDatabase.backgroundPictures)
        {
            if (background != null && !usedBackgrounds.Contains(background.ID))
            {
                return background.ID;
            }
        }

        return backgroundDatabase.backgroundPictures[0] != null ? backgroundDatabase.backgroundPictures[0].ID : 1;
    }
}
