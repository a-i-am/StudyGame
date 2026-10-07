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
        Dropdown,
        Table
    }

    [Serializable]
    public class TableRowData
    {
        public List<string> Cells = new List<string>();
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
        
        // For Table Type
        public List<string> TableColumns = new List<string> { "화자", "대사" };
        public List<TableRowData> TableRows = new List<TableRowData>();
        
        public bool ShowAsBadge = false;
    }

}
