using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;

namespace StudyGame.Editor.Director
{
    public class EpisodeGraphView : GraphView
    {
        public EpisodeGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            
            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                {
                    compatiblePorts.Add(port);
                }
            });
            
            return compatiblePorts;
        }

        public EpisodeNode CreateNode(string title, Vector2 position, string[] badges)
        {
            var node = new EpisodeNode(title);
            node.SetPosition(new Rect(position, Vector2.zero));
            node.SetBadges(badges);
            AddElement(node);
            return node;
        }
    }
}
