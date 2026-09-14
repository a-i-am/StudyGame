using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MathData", menuName = "Math/MathData")]
public class MathData : ScriptableObject
{
    public string systemName;
    public string version;
    public int totalWorlds;
    public int totalEntities;
    public List<WorldData> worlds = new List<WorldData>();
}

[Serializable]
public class WorldData
{
    public string id;
    public string title;
    public string description;
    public List<BiomeData> biomes = new List<BiomeData>();
}

[Serializable]
public class BiomeData
{
    public string id;
    public string title;
    public List<LawData> laws = new List<LawData>();
    public List<EntityData> entities = new List<EntityData>();
}

[Serializable]
public class LawData
{
    public string id;
    public string law_type;
    public string name;
    public string formula_pattern;
    public InteractiveRulesData interactive_rules;
}

[Serializable]
public class InteractiveRulesData
{
    public string @operator;
    public string target_property;
    public string input_parameter;
    public string action_type;
}

[Serializable]
public class EntityData
{
    public string id;
    public string law_id;
    public string question_text;
    public string formula;
    public List<string> choices = new List<string>();
    public int points;
    public bool has_image;
    public string image_asset_id;
    public string render_type;
    public InteractionNodeData interaction_node;
}

[Serializable]
public class InteractionNodeData
{
    public string node_type;
    public string trigger_event;
    public string required_solution;
}
