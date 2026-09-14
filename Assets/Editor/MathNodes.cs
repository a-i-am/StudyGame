#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

public abstract class MathBaseNode : Node
{
    public string NodeGUID;
    public Port InputPort;
    public Port OutputPort;

    protected Port GeneratePort(Direction direction, Port.Capacity capacity = Port.Capacity.Single)
    {
        return InstantiatePort(Orientation.Horizontal, direction, capacity, typeof(float));
    }
}

public class MathInputNode : MathBaseNode
{
    public TextField ExpressionField;

    public MathInputNode()
    {
        title = "입력 노드 (조건식)";
        NodeGUID = System.Guid.NewGuid().ToString();

        OutputPort = GeneratePort(Direction.Output, Port.Capacity.Multi);
        OutputPort.portName = "Output";
        outputContainer.Add(OutputPort);

        ExpressionField = new TextField("조건식 (Formula)") { value = "x > 0" };
        mainContainer.Add(ExpressionField);

        RefreshExpandedState();
        RefreshPorts();
    }
}

public class MathOperationNode : MathBaseNode
{
    public PopupField<string> OperationDropdown;

    public MathOperationNode()
    {
        title = "연산 노드 (수학 법칙)";
        NodeGUID = System.Guid.NewGuid().ToString();

        InputPort = GeneratePort(Direction.Input, Port.Capacity.Multi);
        InputPort.portName = "Input";
        inputContainer.Add(InputPort);

        OutputPort = GeneratePort(Direction.Output, Port.Capacity.Multi);
        OutputPort.portName = "Output";
        outputContainer.Add(OutputPort);

        var operations = new List<string> { "Logarithm (로그)", "Derivative (미분)", "Limit (극한)", "Integral (적분)" };
        OperationDropdown = new PopupField<string>("연산 타입", operations, 0);
        mainContainer.Add(OperationDropdown);

        RefreshExpandedState();
        RefreshPorts();
    }
}

public class MathOutputNode : MathBaseNode
{
    public PopupField<string> EffectDropdown;

    public MathOutputNode()
    {
        title = "출력 노드 (물리/기믹 효과)";
        NodeGUID = System.Guid.NewGuid().ToString();

        InputPort = GeneratePort(Direction.Input, Port.Capacity.Multi);
        InputPort.portName = "Input";
        inputContainer.Add(InputPort);

        var effects = new List<string> { "ScaleTransform (압축/확대)", "DampVelocity (둔화)", "Oscillate (주기 파동)", "RedirectVector (경사 전환)" };
        EffectDropdown = new PopupField<string>("인게임 효과", effects, 0);
        mainContainer.Add(EffectDropdown);

        RefreshExpandedState();
        RefreshPorts();
    }
}
#endif
