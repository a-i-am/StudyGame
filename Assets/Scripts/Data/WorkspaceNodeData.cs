using System;
using System.Collections.Generic;
using UnityEngine;

namespace StudyGame.Data
{
    [CreateAssetMenu(fileName = "NewWorkspaceNodeData", menuName = "StudyGame/Workspace Node Data")]
    public class WorkspaceNodeData : ScriptableObject
    {
        public string NodeId;
        public string NodeTitle = "Untitled Node";
        public string TemplateType = "일반"; 
        
        public string ThemeColorHex = "#ffffff";
        public string ThemeIcon = "📝";

        public List<DynamicProperty> Properties = new List<DynamicProperty>();
    }
}
