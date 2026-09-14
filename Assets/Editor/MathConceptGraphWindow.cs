#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;

public class MathConceptGraphWindow : EditorWindow
{
    private MathGraphView _graphView;

    [MenuItem("Tools/Math Concept Graph")]
    public static void OpenWindow()
    {
        var window = GetWindow<MathConceptGraphWindow>();
        window.titleContent = new GUIContent("Math Concept Graph");
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
    }

    private void OnDisable()
    {
        if (_graphView != null)
        {
            rootVisualElement.Remove(_graphView);
        }
    }

    private void ConstructGraphView()
    {
        _graphView = new MathGraphView
        {
            name = "Math Concept Graph"
        };
        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);
    }

    private void GenerateToolbar()
    {
        var toolbar = new Toolbar();

        var addInputBtn = new Button(() => _graphView.CreateInputNode(new Vector2(100, 100))) { text = "입력 노드 추가" };
        var addOpBtn = new Button(() => _graphView.CreateOperationNode(new Vector2(380, 100))) { text = "연산 노드 추가" };
        var addOutBtn = new Button(() => _graphView.CreateOutputNode(new Vector2(660, 100))) { text = "출력 노드 추가" };
        var saveProfileBtn = new Button(SaveProfile) { text = "프로파일 저장 (.asset)" };

        toolbar.Add(addInputBtn);
        toolbar.Add(addOpBtn);
        toolbar.Add(addOutBtn);
        toolbar.Add(saveProfileBtn);

        rootVisualElement.Add(toolbar);
    }

    private void SaveProfile()
    {
        string expr = "x > 0";
        string opType = "Logarithm";
        string effType = "ScaleTransform";

        _graphView.nodes.ForEach(node =>
        {
            if (node is MathInputNode inputNode)
            {
                expr = inputNode.ExpressionField.value;
            }
            else if (node is MathOperationNode opNode)
            {
                opType = opNode.OperationDropdown.value;
            }
            else if (node is MathOutputNode outNode)
            {
                effType = outNode.EffectDropdown.value;
            }
        });

        string savePath = EditorUtility.SaveFilePanelInProject("Save Math Gimmick Profile", "NewMathGimmickProfile", "asset", "Select save location");
        if (string.IsNullOrEmpty(savePath)) return;

        MathGimmickProfile profile = ScriptableObject.CreateInstance<MathGimmickProfile>();
        profile.profileName = Path.GetFileNameWithoutExtension(savePath);
        profile.inputExpression = expr;
        profile.operationType = opType;
        profile.effectType = effType;

        AssetDatabase.CreateAsset(profile, savePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Save Complete", "MathGimmickProfile asset saved successfully at:\n" + savePath, "OK");
    }
}
#endif
