using UnityEngine;
using UnityEditor;
using TMPro;

public class CreateFreshTMPFont
{
    [MenuItem("Tools/Generate Fresh KoPubFont Asset")]
    public static void Generate()
    {
        string ttfPath = "Assets/Resources/StudyGame-Art/Fonts/KoPubWorld Batang Bold.ttf";
        Font font = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
        if (font == null) return;

        string fontAssetPath = "Assets/Resources/StudyGame-Art/Fonts/KoPubWorld Batang Bold.asset";
        
        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(font, 90, 9, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFF, 1024, 1024, AtlasPopulationMode.Dynamic);
        
        if (fontAsset != null)
        {
            AssetDatabase.CreateAsset(fontAsset, fontAssetPath);
            
            Shader tmpShader = Shader.Find("TextMeshPro/Distance Field");
            if (tmpShader != null)
            {
                Material mat = new Material(tmpShader);
                mat.name = fontAsset.name + " Material";
                mat.SetTexture(ShaderUtilities.ID_MainTex, fontAsset.atlasTexture);
                AssetDatabase.AddObjectToAsset(mat, fontAsset);
                fontAsset.material = mat;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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
