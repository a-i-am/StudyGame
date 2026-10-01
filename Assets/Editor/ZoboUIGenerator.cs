using UnityEditor;
using UnityEngine;
using ZoboUI.Core;

public static class ZoboUIGenerator
{
    [MenuItem("Tools/Generate ZoboUI Styles")]
    public static void GenerateStyles()
    {
        if (!AssetDatabase.IsValidFolder("Assets/UI"))
        {
            AssetDatabase.CreateFolder("Assets", "UI");
        }
        if (!AssetDatabase.IsValidFolder("Assets/UI/Styles"))
        {
            AssetDatabase.CreateFolder("Assets/UI", "Styles");
        }

        string configPath = "Assets/UI/Styles/ZoboThemeConfig.asset";
        var manager = AssetDatabase.LoadAssetAtPath<ThemeConfigManager>(configPath);
        if (manager == null)
        {
            manager = ScriptableObject.CreateInstance<ThemeConfigManager>();
            manager.GeneratedUssFilePath = "Assets/UI/Styles/zoboui_generated.uss";
            manager.PurgedUssFilePath = "Assets/UI/Styles/zoboui_purged.uss";
            manager.LoadDefaultThemeConfig();
            AssetDatabase.CreateAsset(manager, configPath);
        }

        manager.GenerateUSSFile();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("ZoboUI styles successfully generated at Assets/UI/Styles/zoboui_generated.uss");
    }
}
