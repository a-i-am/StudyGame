using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

public static class UIGallerySceneBuilder
{
    [MenuItem("Tools/Build UI Gallery Scene")]
    public static void BuildScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Main Camera");
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.12f, 0.14f);
        camGo.tag = "MainCamera";

        var lightGo = new GameObject("Directional Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        var galleryGo = new GameObject("UIGalleryManager");
        var uiDoc = galleryGo.AddComponent<UIDocument>();

        var panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>("Assets/UI Toolkit/PanelSettings.asset");
        if (panelSettings != null)
        {
            uiDoc.panelSettings = panelSettings;
        }

        var galleryUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UIGallery/UIGallery.uxml");
        if (galleryUxml != null)
        {
            uiDoc.visualTreeAsset = galleryUxml;
        }

        var director = galleryGo.AddComponent<UIGalleryDirector>();
        director.PopulateAllUxml();

        string scenePath = "Assets/Scenes/UIGalleryScene.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.Refresh();
        Debug.Log("UIGalleryScene created and saved at " + scenePath);
    }
}
