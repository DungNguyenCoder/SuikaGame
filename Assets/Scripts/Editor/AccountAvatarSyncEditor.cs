using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class AccountAvatarSyncEditor
{
    private const string AccountPanelPrefabPath = "Assets/Panels/AccountPanel.prefab";
    private const string AvatarContentPath = "Background/AvatarBlock/Scroll View/Viewport/Content";
    private const string SkinSpritesFolderPath = "Assets/Sprites/Skin";

    [MenuItem("Tools/SuikaGame/Account Panel/Sync Avatars From Skin Folder")]
    private static void SyncAvatarsFromSkinFolder()
    {
        if (!AssetDatabase.IsValidFolder(SkinSpritesFolderPath))
        {
            Debug.LogError($"Folder not found: {SkinSpritesFolderPath}");
            return;
        }

        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(AccountPanelPrefabPath);
        try
        {
            Transform content = prefabRoot.transform.Find(AvatarContentPath);
            if (content == null)
            {
                Debug.LogError($"Cannot find content path: {AvatarContentPath}");
                return;
            }

            List<Sprite> sprites = LoadSpritesFromFolder(SkinSpritesFolderPath);
            SyncChildCount(content, sprites.Count);

            for (int i = 0; i < sprites.Count; i++)
            {
                Transform child = content.GetChild(i);
                if (!child.TryGetComponent(out Image image))
                {
                    image = child.gameObject.AddComponent<Image>();
                }

                image.sprite = sprites[i];
                image.preserveAspect = true;
                child.name = $"Image ({i + 1})";
                EditorUtility.SetDirty(image);
            }

            EditorUtility.SetDirty(prefabRoot);
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, AccountPanelPrefabPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"Synced {sprites.Count} sprites into {AccountPanelPrefabPath}");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }

    [MenuItem("Tools/SuikaGame/Account Panel/Sync Avatars From Skin Folder", true)]
    private static bool ValidateSyncAvatarsFromSkinFolder()
    {
        return AssetDatabase.LoadAssetAtPath<GameObject>(AccountPanelPrefabPath) != null;
    }

    private static List<Sprite> LoadSpritesFromFolder(string folderPath)
    {
        string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });
        var entries = new List<SpriteEntry>(spriteGuids.Length);

        foreach (string guid in spriteGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Object[] assetsAtPath = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            if (assetsAtPath == null || assetsAtPath.Length == 0)
            {
                continue;
            }

            foreach (Object asset in assetsAtPath)
            {
                if (!(asset is Sprite sprite))
                {
                    continue;
                }

                entries.Add(new SpriteEntry(assetPath, sprite.name, sprite));
            }
        }

        entries.Sort((a, b) =>
        {
            int pathCompare = string.CompareOrdinal(a.AssetPath, b.AssetPath);
            return pathCompare != 0 ? pathCompare : string.CompareOrdinal(a.SpriteName, b.SpriteName);
        });

        var sortedSprites = new List<Sprite>(entries.Count);
        foreach (SpriteEntry entry in entries)
        {
            sortedSprites.Add(entry.Sprite);
        }

        return sortedSprites;
    }

    private static void SyncChildCount(Transform content, int targetCount)
    {
        while (content.childCount < targetCount)
        {
            CreateImageChild(content);
        }

        for (int i = content.childCount - 1; i >= targetCount; i--)
        {
            Object.DestroyImmediate(content.GetChild(i).gameObject);
        }
    }

    private static void CreateImageChild(Transform content)
    {
        var gameObject = new GameObject("Image", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.SetParent(content, false);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
    }

    private readonly struct SpriteEntry
    {
        public SpriteEntry(string assetPath, string spriteName, Sprite sprite)
        {
            AssetPath = assetPath;
            SpriteName = spriteName;
            Sprite = sprite;
        }

        public string AssetPath { get; }
        public string SpriteName { get; }
        public Sprite Sprite { get; }
    }
}
