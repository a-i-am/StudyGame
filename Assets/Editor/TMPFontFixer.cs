using UnityEngine;
using UnityEditor;
using TMPro;

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
        string fontAssetPath = "Assets/Resources/StudyGame-Art/Fonts/KoPubWorld Batang Bold.asset";
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontAssetPath);
        if (fontAsset == null) return;

        fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
        EditorUtility.SetDirty(fontAsset);

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
