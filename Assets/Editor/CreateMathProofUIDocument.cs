using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

public class CreateMathProofUIDocument
{
    [MenuItem("Tools/UI Toolkit/Create Math Proof UI Document")]
    public static void CreateUIDocumentInScene()
    {
        GameObject go = new GameObject("MathProofUIDocument");
        UIDocument uiDoc = go.AddComponent<UIDocument>();

        VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI Toolkit/MathProofUI.uxml");
        PanelSettings settings = AssetDatabase.LoadAssetAtPath<PanelSettings>("Assets/UI Toolkit/PanelSettings.asset");

        if (settings != null)
        {
            uiDoc.panelSettings = settings;
        }

        if (uxml != null)
        {
            uiDoc.visualTreeAsset = uxml;
        }

        MathProofUIDocumentController controller = go.AddComponent<MathProofUIDocumentController>();
        SerializedObject so = new SerializedObject(controller);
        if (uxml != null)
        {
            so.FindProperty("uxmlDocument").objectReferenceValue = uxml;
        }
        if (settings != null)
        {
            so.FindProperty("panelSettings").objectReferenceValue = settings;
        }
        so.ApplyModifiedProperties();

        Selection.activeGameObject = go;
        Undo.RegisterCreatedObjectUndo(go, "Create Math Proof UI Document");
    }
}
