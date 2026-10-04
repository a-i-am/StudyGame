using UnityEditor;
using UnityEditor.SceneManagement;

public static class PopulateGalleryTool
{
    [MenuItem("Tools/Populate Gallery Now")]
    public static void Run()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/UIGalleryScene.unity", OpenSceneMode.Single);
        var directors = UnityEngine.Object.FindObjectsOfType<UIGalleryDirector>();
        foreach (var dir in directors)
        {
            dir.PopulateAllUxml();
            EditorUtility.SetDirty(dir);
        }
        EditorSceneManager.SaveScene(scene);
        UnityEngine.Debug.Log("Gallery Populated Successfully!");
    }
}