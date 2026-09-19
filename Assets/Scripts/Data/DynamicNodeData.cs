using System;
using System.Collections.Generic;
using UnityEngine;

namespace StudyGame.Data
{
    public enum PropertyType
    {
        Text,
        Number,
        Color,
        Asset,
        Dropdown
    }

    [Serializable]
    public class DynamicProperty
    {
        public string PropertyName = "New Property";
        public PropertyType Type = PropertyType.Text;
        
        [TextArea(1, 4)]
        public string StringValue = "";
        public float FloatValue = 0f;
        public Color ColorValue = Color.white;
        public UnityEngine.Object AssetValue;
        
        public bool ShowAsBadge = false;
    }

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
