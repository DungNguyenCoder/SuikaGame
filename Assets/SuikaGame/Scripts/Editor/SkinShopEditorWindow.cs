using System.Collections.Generic;
using Core.Shop;
using Core.Skin;
using UnityEditor;
using UnityEngine;

public class SkinShopEditorWindow : EditorWindow
{
    private SkinShopDatabase _shopDatabase;
    private SerializedObject _serializedShopDatabase;
    private Vector2 _scrollPos;

    [MenuItem("Tools/SuikaGame/Skin Shop Editor")]
    private static void OpenWindow()
    {
        GetWindow<SkinShopEditorWindow>("Skin Shop");
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
                "Assign or create a SkinShopDatabase to edit shop data.",
                MessageType.Info);
            return;
        }

        EnsureSerializedObject();
        _serializedShopDatabase.Update();

        var skinDatabaseProperty = _serializedShopDatabase.FindProperty("skinDatabase");
        EditorGUILayout.PropertyField(skinDatabaseProperty, new GUIContent("Skin Database"));
        DrawAutoAssignSkinDatabaseButton(skinDatabaseProperty);

        EditorGUILayout.Space(6f);

        var itemsProperty = _serializedShopDatabase.FindProperty("items");
        DrawItemsHeader(itemsProperty);

        int removeIndex = -1;
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        for (int i = 0; i < itemsProperty.arraySize; i++)
        {
            var itemProperty = itemsProperty.GetArrayElementAtIndex(i);
            if (DrawItem(itemProperty, i, _shopDatabase.skinDatabase))
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

        DrawValidation(itemsProperty, _shopDatabase.skinDatabase);
    }

    private void DrawDatabaseSelector()
    {
        EditorGUILayout.LabelField("Shop Database", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        var selected = (SkinShopDatabase)EditorGUILayout.ObjectField(
            "Asset",
            _shopDatabase,
            typeof(SkinShopDatabase),
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

    private void DrawAutoAssignSkinDatabaseButton(SerializedProperty skinDatabaseProperty)
    {
        if (skinDatabaseProperty.objectReferenceValue != null)
        {
            return;
        }

        EditorGUILayout.HelpBox("Skin Database is required to pick valid skin series.", MessageType.Warning);
        if (GUILayout.Button("Auto Assign First SkinDatabase"))
        {
            var skinDatabase = FindFirstAsset<SkinDatabase>();
            if (skinDatabase != null)
            {
                skinDatabaseProperty.objectReferenceValue = skinDatabase;
            }
            else
            {
                Debug.LogWarning("No SkinDatabase asset found.");
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
            ResetItem(itemProperty, ResolveSuggestedSeriesID(_shopDatabase.skinDatabase, itemsProperty));
        }

        EditorGUILayout.EndHorizontal();
    }

    private bool DrawItem(SerializedProperty itemProperty, int index, SkinDatabase skinDatabase)
    {
        var bannerProperty = itemProperty.FindPropertyRelative("Banner");
        var priceProperty = itemProperty.FindPropertyRelative("Price");
        var skinSeriesIDProperty = itemProperty.FindPropertyRelative("SkinSeriesID");

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

        DrawSkinSeriesSelector(skinSeriesIDProperty, skinDatabase);

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

    private static void DrawSkinSeriesSelector(SerializedProperty skinSeriesIDProperty, SkinDatabase skinDatabase)
    {
        if (skinDatabase == null || skinDatabase.skinSeries == null || skinDatabase.skinSeries.Count == 0)
        {
            skinSeriesIDProperty.intValue = EditorGUILayout.IntField("Skin Series ID", skinSeriesIDProperty.intValue);
            return;
        }

        var availableSeries = new List<SkinSeries>();
        foreach (var series in skinDatabase.skinSeries)
        {
            if (series != null)
            {
                availableSeries.Add(series);
            }
        }

        if (availableSeries.Count == 0)
        {
            skinSeriesIDProperty.intValue = EditorGUILayout.IntField("Skin Series ID", skinSeriesIDProperty.intValue);
            return;
        }

        int selectedIndex = availableSeries.FindIndex(s => s.ID == skinSeriesIDProperty.intValue);
        var options = new string[availableSeries.Count];
        for (int i = 0; i < availableSeries.Count; i++)
        {
            int skinCount = availableSeries[i].skinDatas != null ? availableSeries[i].skinDatas.Count : 0;
            options[i] = $"ID {availableSeries[i].ID} ({skinCount} sprites)";
        }

        EditorGUI.BeginChangeCheck();
        int nextIndex = EditorGUILayout.Popup("Skin Series", Mathf.Max(0, selectedIndex), options);
        if (EditorGUI.EndChangeCheck())
        {
            skinSeriesIDProperty.intValue = availableSeries[nextIndex].ID;
        }

        if (selectedIndex < 0)
        {
            EditorGUILayout.HelpBox(
                $"Series ID {skinSeriesIDProperty.intValue} does not exist in SkinDatabase.",
                MessageType.Warning);
            skinSeriesIDProperty.intValue = EditorGUILayout.IntField("Skin Series ID", skinSeriesIDProperty.intValue);
        }
    }

    private static void DrawValidation(SerializedProperty itemsProperty, SkinDatabase skinDatabase)
    {
        var seenSeries = new HashSet<int>();
        var duplicateSeries = new List<int>();
        var missingSeries = new List<int>();

        for (int i = 0; i < itemsProperty.arraySize; i++)
        {
            var itemProperty = itemsProperty.GetArrayElementAtIndex(i);
            int seriesID = itemProperty.FindPropertyRelative("SkinSeriesID").intValue;

            if (!seenSeries.Add(seriesID) && !duplicateSeries.Contains(seriesID))
            {
                duplicateSeries.Add(seriesID);
            }

            if (skinDatabase != null && !HasSeriesID(skinDatabase, seriesID) && !missingSeries.Contains(seriesID))
            {
                missingSeries.Add(seriesID);
            }
        }

        if (duplicateSeries.Count > 0)
        {
            EditorGUILayout.HelpBox(
                $"Duplicate SkinSeriesID in shop items: {string.Join(", ", duplicateSeries)}",
                MessageType.Warning);
        }

        if (missingSeries.Count > 0)
        {
            EditorGUILayout.HelpBox(
                $"SkinSeriesID not found in SkinDatabase: {string.Join(", ", missingSeries)}",
                MessageType.Warning);
        }
    }

    private void TryAutoAssignDatabase()
    {
        if (_shopDatabase != null)
        {
            return;
        }

        AssignDatabase(FindFirstAsset<SkinShopDatabase>());
    }

    private void AssignDatabase(SkinShopDatabase database)
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
            "Create Skin Shop Database",
            "SkinShopDatabase",
            "asset",
            "Choose where to save SkinShopDatabase.");
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        var database = CreateInstance<SkinShopDatabase>();
        database.skinDatabase = FindFirstAsset<SkinDatabase>();

        AssetDatabase.CreateAsset(database, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        AssignDatabase(database);
        Selection.activeObject = database;
        EditorGUIUtility.PingObject(database);
    }

    private static T FindFirstAsset<T>() where T : UnityEngine.Object
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        if (guids == null || guids.Length == 0)
        {
            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }

    private static bool HasSeriesID(SkinDatabase skinDatabase, int seriesID)
    {
        if (skinDatabase == null || skinDatabase.skinSeries == null)
        {
            return false;
        }

        foreach (var series in skinDatabase.skinSeries)
        {
            if (series != null && series.ID == seriesID)
            {
                return true;
            }
        }

        return false;
    }

    private static void ResetItem(SerializedProperty itemProperty, int suggestedSeriesID)
    {
        itemProperty.FindPropertyRelative("SkinSeriesID").intValue = suggestedSeriesID;
        itemProperty.FindPropertyRelative("Banner").objectReferenceValue = null;
        itemProperty.FindPropertyRelative("Price").intValue = 0;
    }

    private static int ResolveSuggestedSeriesID(SkinDatabase skinDatabase, SerializedProperty itemsProperty)
    {
        if (skinDatabase == null || skinDatabase.skinSeries == null || skinDatabase.skinSeries.Count == 0)
        {
            return 1;
        }

        var usedSeries = new HashSet<int>();
        for (int i = 0; i < itemsProperty.arraySize; i++)
        {
            var item = itemsProperty.GetArrayElementAtIndex(i);
            usedSeries.Add(item.FindPropertyRelative("SkinSeriesID").intValue);
        }

        foreach (var series in skinDatabase.skinSeries)
        {
            if (series != null && !usedSeries.Contains(series.ID))
            {
                return series.ID;
            }
        }

        return skinDatabase.skinSeries[0] != null ? skinDatabase.skinSeries[0].ID : 1;
    }
}
