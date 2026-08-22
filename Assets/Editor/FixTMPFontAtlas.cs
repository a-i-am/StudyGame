using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO;

public class FixTMPFontAtlas
{
    [MenuItem("Tools/Fix TMP Font Atlas")]
    public static void FixAtlas()
    {
        string fontPath = "Assets/Resources/StudyGame-Art/Fonts/KoPubWorld Batang Bold.ttf";
        Font font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
        if (font == null) return;

        string txtPath = "Assets/Resources/StudyGame-Art/Fonts/상용한글 2350자 + 영문 + 특수문자.txt";
        TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(txtPath);
        string charSequence = textAsset != null ? textAsset.text : "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+-=[]{}|;:'\",.<>/?~ 폰트적용테스트점수";

        string fontAssetPath = "Assets/Resources/StudyGame-Art/Fonts/KoPubWorld Batang Bold.asset";
        
        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(font, 90, 9, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic);
        
        if (fontAsset != null)
        {
            fontAsset.TryAddCharacters(charSequence);
            
            Shader tmpShader = Shader.Find("TextMeshPro/Distance Field");
            if (tmpShader != null)
            {
                Material mat = new Material(tmpShader);
                mat.name = fontAsset.name + " Material";
                mat.SetTexture(ShaderUtilities.ID_MainTex, fontAsset.atlasTexture);
                AssetDatabase.AddObjectToAsset(mat, fontAsset);
                fontAsset.material = mat;
            }

            AssetDatabase.CreateAsset(fontAsset, fontAssetPath);
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
