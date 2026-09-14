using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

public class CreateBattleAndPhoneUIDocuments
{
    [MenuItem("Tools/UI Toolkit/Create Phone UI Document")]
    public static void CreatePhoneUIDocumentInScene()
    {
        GameObject go = new GameObject("PhoneUIDocument");
        UIDocument uiDoc = go.AddComponent<UIDocument>();

        VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI Toolkit/PhoneUIDocument.uxml");
        PanelSettings settings = AssetDatabase.LoadAssetAtPath<PanelSettings>("Assets/UI Toolkit/PanelSettings.asset");

        if (settings != null)
        {
            uiDoc.panelSettings = settings;
        }

        if (uxml != null)
        {
            uiDoc.visualTreeAsset = uxml;
        }

        go.AddComponent<PhoneUIDocumentController>();

        Selection.activeGameObject = go;
        Undo.RegisterCreatedObjectUndo(go, "Create Phone UI Document");
    }
}
