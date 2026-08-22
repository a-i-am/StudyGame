using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO;

public class RebuildKoPubFont
{
    [MenuItem("Tools/Rebuild KoPubWorld Font Asset")]
    public static void Rebuild()
    {
        string ttfPath = "Assets/Resources/StudyGame-Art/Fonts/KoPubWorld Batang Bold.ttf";
        Font font = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
        if (font == null) return;

        string txtPath = "Assets/Resources/StudyGame-Art/Fonts/상용한글 2350자 + 영문 + 특수문자.txt";
        TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(txtPath);
        string charSequence = textAsset != null ? textAsset.text : "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        string fontAssetPath = "Assets/Resources/StudyGame-Art/Fonts/KoPubWorld Batang Bold.asset";
        TMP_FontAsset existingAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontAssetPath);

        TMP_FontAsset newAsset = TMP_FontAsset.CreateFontAsset(font, 90, 9, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic);
        
        if (existingAsset != null)
        {
            EditorUtility.CopySerialized(newAsset, existingAsset);
            existingAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            EditorUtility.SetDirty(existingAsset);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
