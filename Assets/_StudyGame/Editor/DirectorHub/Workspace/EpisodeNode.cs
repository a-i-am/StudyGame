using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using StudyGame.Data;

namespace StudyGame.Editor.Director
{
    public class EpisodeNode : Node
    {
        public string NodeId { get; set; }
        public WorkspaceNodeData NodeData;

        public EpisodeNodeSaveData SaveData { get; private set; }

        public EpisodeNode(string title)
        {
            NodeId = System.Guid.NewGuid().ToString();
            this.title = title;

            SaveData = new EpisodeNodeSaveData { NodeGuid = NodeId };

            // Apply style class from USS
            mainContainer.AddToClassList("diary-node-main");

            // Create Ports
            var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            inputPort.portName = "In";
            inputContainer.Add(inputPort);

            var outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            outputPort.portName = "Out";
            outputContainer.Add(outputPort);

            RefreshExpandedState();
            RefreshPorts();
        }

        public void SetBadges(string[] badges)
        {
            // Clear existing
            var container = extensionContainer.Q<VisualElement>("badge-container");
            if (container != null)
                extensionContainer.Remove(container);

            container = new VisualElement();
            container.name = "badge-container";
            container.AddToClassList("badge-container");

            foreach (var badge in badges)
            {
                var label = new Label(badge);
                label.AddToClassList("badge");

                // TODO: 시스템 속성 뱃지 스타일 적용 (USS 클래스 추가 필요)
                if (badge.StartsWith("Type:")) label.AddToClassList("badge-system-type");
                if (badge.StartsWith("Lock:")) label.AddToClassList("badge-system-lock");

                if (badge.Contains("도영")) label.AddToClassList("badge-doyoung");
                if (badge.Contains("지민")) label.AddToClassList("badge-jimin");
                if (badge.Contains("플레이어")) label.AddToClassList("badge-player");
                if (badge.Contains("퍼즐")) label.AddToClassList("badge-puzzle");

                container.Add(label);
            }

            extensionContainer.Add(container);
            RefreshExpandedState();
        }
    }
}
