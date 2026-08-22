using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO;

[InitializeOnLoad]
public class TMPFontFixer
{
    static TMPFontFixer()
    {
        EditorApplication.delayCall += FixFontsAndScene;
    }

    [MenuItem("Tools/Fix Fonts and Scene")]
    public static void FixFontsAndScene()
    {
        string fontPath = "Assets/Resources/StudyGame-Art/Fonts/KoPubWorld Batang Bold.ttf";
        Font font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
        if (font == null) return;

        string assetPath = "Assets/Resources/StudyGame-Art/Fonts/KoPubWorld Batang Bold Dynamic.asset";
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);

        if (fontAsset == null)
        {
            fontAsset = TMP_FontAsset.CreateFontAsset(font);
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            AssetDatabase.CreateAsset(fontAsset, assetPath);
            AssetDatabase.SaveAssets();
        }
        else
        {
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            EditorUtility.SetDirty(fontAsset);
        }

        TextMeshProUGUI[] tmpTexts = Object.FindObjectsOfType<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI tmp in tmpTexts)
        {
            tmp.font = fontAsset;
            EditorUtility.SetDirty(tmp);
        }

        QuestionManager qm = Object.FindObjectOfType<QuestionManager>();
        if (qm != null)
        {
            qm.BuildLinkedText();
            EditorUtility.SetDirty(qm);
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
    }
}
