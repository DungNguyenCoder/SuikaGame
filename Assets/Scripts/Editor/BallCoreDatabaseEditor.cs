using System.Collections.Generic;
using Core;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BallCoreDatabase))]
public class BallCoreDatabaseEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
        {
            if (GUILayout.Button("Apply Pixels Per Unit To All Skins"))
            {
                var database = (BallCoreDatabase)target;
                ApplyPixelsPerUnitToAllSkins(database);
            }
        }
    }

    private static void ApplyPixelsPerUnitToAllSkins(BallCoreDatabase ballCoreDatabase)
    {
        if (ballCoreDatabase == null) return;

        var ppuByBallId = new Dictionary<int, float>();
        foreach (var ballData in ballCoreDatabase.ballDatas)
        {
            if (ballData == null) continue;
            if (ballData.ID <= 0) continue;
            if (ballData.PixelsPerUnit <= 0f) continue;

            ppuByBallId[ballData.ID] = ballData.PixelsPerUnit;
        }

        if (ppuByBallId.Count == 0)
        {
            Debug.LogWarning("No valid BallData ID/PPU entries found.");
            return;
        }

        string[] skinDbGuids = AssetDatabase.FindAssets("t:SkinDatabase");
        if (skinDbGuids == null || skinDbGuids.Length == 0)
        {
            Debug.LogWarning("No SkinDatabase asset found.");
            return;
        }

        var ppuByTexturePath = new Dictionary<string, float>();
        var conflictedTexturePaths = new HashSet<string>();

        foreach (var guid in skinDbGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var skinDatabase = AssetDatabase.LoadAssetAtPath<SkinDatabase>(path);
            if (skinDatabase == null || skinDatabase.skinSeries == null) continue;

            foreach (var series in skinDatabase.skinSeries)
            {
                if (series == null || series.skinDatas == null) continue;

                foreach (var skinData in series.skinDatas)
                {
                    if (skinData == null || skinData.Sprite == null) continue;
                    if (!ppuByBallId.TryGetValue(skinData.BallID, out float targetPPU)) continue;

                    string spritePath = AssetDatabase.GetAssetPath(skinData.Sprite);
                    if (string.IsNullOrEmpty(spritePath)) continue;

                    if (ppuByTexturePath.TryGetValue(spritePath, out float existingPPU) && !Mathf.Approximately(existingPPU, targetPPU))
                    {
                        conflictedTexturePaths.Add(spritePath);
                        continue;
                    }

                    ppuByTexturePath[spritePath] = targetPPU;
                }
            }
        }

        int changedCount = 0;
        foreach (var entry in ppuByTexturePath)
        {
            if (conflictedTexturePaths.Contains(entry.Key)) continue;

            var importer = AssetImporter.GetAtPath(entry.Key) as TextureImporter;
            if (importer == null) continue;

            if (Mathf.Abs(importer.spritePixelsPerUnit - entry.Value) < 0.01f) continue;

            importer.spritePixelsPerUnit = entry.Value;
            importer.SaveAndReimport();
            changedCount++;
        }

        foreach (var conflictedPath in conflictedTexturePaths)
        {
            Debug.LogWarning($"Skipped PPU apply for conflicted texture: {conflictedPath}");
        }

        Debug.Log($"Apply Pixels Per Unit complete. Updated textures: {changedCount}, conflicts: {conflictedTexturePaths.Count}.");
    }
}
