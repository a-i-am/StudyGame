using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class DisableUITemp
{
    [MenuItem("StudyGame/Temp/Disable Synth UI")]
    public static void Fix()
    {
        GameObject go = GameObject.Find("SynthesizerTest");
        if (go != null) go.SetActive(false);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
    }
}
