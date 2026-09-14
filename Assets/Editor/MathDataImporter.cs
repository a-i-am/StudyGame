#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

public class MathDataImporter
{
    [MenuItem("Tools/Import Math Data from JSON")]
    public static void ImportMathData()
    {
        string path = EditorUtility.OpenFilePanel("Select math_world_hierarchy.json", "", "json");
        if (string.IsNullOrEmpty(path)) return;

        string jsonText = File.ReadAllText(path);
        MathDataDto dto = JsonConvert.DeserializeObject<MathDataDto>(jsonText);

        MathData asset = ScriptableObject.CreateInstance<MathData>();
        asset.systemName = dto.system_name;
        asset.version = dto.version;
        asset.totalWorlds = dto.total_worlds;
        asset.totalEntities = dto.total_entities;

        foreach (var w in dto.worlds)
        {
            WorldData worldData = new WorldData
            {
                id = w.id,
                title = w.title,
                description = w.description
            };

            foreach (var b in w.biomes)
            {
                BiomeData biomeData = new BiomeData
                {
                    id = b.id,
                    title = b.title
                };

                foreach (var l in b.laws)
                {
                    biomeData.laws.Add(new LawData
                    {
                        id = l.id,
                        law_type = l.law_type,
                        name = l.name,
                        formula_pattern = l.formula_pattern,
                        interactive_rules = l.interactive_rules != null ? new InteractiveRulesData
                        {
                            @operator = l.interactive_rules.@operator,
                            target_property = l.interactive_rules.target_property,
                            input_parameter = l.interactive_rules.input_parameter,
                            action_type = l.interactive_rules.action_type
                        } : null
                    });
                }

                foreach (var e in b.entities)
                {
                    biomeData.entities.Add(new EntityData
                    {
                        id = e.id,
                        law_id = e.law_id,
                        question_text = e.question_text,
                        formula = e.formula,
                        choices = e.choices != null ? new List<string>(e.choices) : new List<string>(),
                        points = e.points,
                        has_image = e.has_image,
                        image_asset_id = e.image_asset_id,
                        render_type = e.render_type,
                        interaction_node = e.interaction_node != null ? new InteractionNodeData
                        {
                            node_type = e.interaction_node.node_type,
                            trigger_event = e.interaction_node.trigger_event,
                            required_solution = e.interaction_node.required_solution
                        } : null
                    });
                }

                worldData.biomes.Add(biomeData);
            }

            asset.worlds.Add(worldData);
        }

        string savePath = "Assets/Resources/MathData.asset";
        string dir = Path.GetDirectoryName(savePath);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        AssetDatabase.CreateAsset(asset, savePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Import Success", "MathData.asset created at " + savePath, "OK");
    }
}

public class MathDataDto
{
    public string system_name;
    public string version;
    public int total_worlds;
    public int total_entities;
    public List<WorldDto> worlds;
}

public class WorldDto
{
    public string id;
    public string title;
    public string description;
    public List<BiomeDto> biomes;
}

public class BiomeDto
{
    public string id;
    public string title;
    public List<LawDto> laws;
    public List<EntityDto> entities;
}

public class LawDto
{
    public string id;
    public string law_type;
    public string name;
    public string formula_pattern;
    public InteractiveRulesDto interactive_rules;
}

public class InteractiveRulesDto
{
    public string @operator;
    public string target_property;
    public string input_parameter;
    public string action_type;
}

public class EntityDto
{
    public string id;
    public string law_id;
    public string question_text;
    public string formula;
    public List<string> choices;
    public int points;
    public bool has_image;
    public string image_asset_id;
    public string render_type;
    public InteractionNodeDto interaction_node;
}

public class InteractionNodeDto
{
    public string node_type;
    public string trigger_event;
    public string required_solution;
}
#endif
