using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using StudyGame.Data;

namespace StudyGame.Editor.Director
{
    public class ScenarioSequenceNode : Node
    {
        public string Guid { get; private set; }
        public WorkspaceNodeData Data { get; private set; }
        public Port InputPort { get; private set; }
        public Port OutputPort { get; private set; }

        public ScenarioSequenceNode(WorkspaceNodeData data, string guid = null)
        {
            Data = data;
            Guid = string.IsNullOrEmpty(guid) ? System.Guid.NewGuid().ToString() : guid;
            
            title = data != null ? data.NodeTitle : "Empty Scenario";

            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            InputPort.portName = "In";
            inputContainer.Add(InputPort);

            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            OutputPort.portName = "Out";
            outputContainer.Add(OutputPort);

            RefreshExpandedState();
            RefreshPorts();
        }
    }
}
