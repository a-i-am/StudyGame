using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder;

namespace StudyGame.Editor.Director
{
    public static class ProceduralLevelGenerator
    {
        public static GameObject GenerateLevel(string name, ProBuilderLevelSettings settings, Material mat)
        {
            GameObject root = new GameObject(name);

            if (settings.GeneratorType == LevelGeneratorType.BasicRoom)
            {
                var shape = ShapeGenerator.GenerateCube(PivotLocation.Center, settings.RoomSize);
                shape.gameObject.name = "Room_Mesh";
                shape.transform.SetParent(root.transform, false);

                if (settings.IsHollow)
                {
                    foreach (var face in shape.faces) face.Reverse();
                }

                ApplyMaterialAndFinalize(shape, mat);
            }
            else if (settings.GeneratorType == LevelGeneratorType.MultiStoryBuilding)
            {
                for (int i = 0; i < settings.FloorCount; i++)
                {
                    float yOffset = i * settings.FloorHeight;
                    GameObject floorRoot = new GameObject($"Floor_{i + 1}");
                    floorRoot.transform.SetParent(root.transform, false);
                    floorRoot.transform.localPosition = new Vector3(0, yOffset, 0);

                    // Floor Slab
                    var slab = ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(settings.BuildingWidth, settings.FloorThickness, settings.BuildingLength));
                    slab.gameObject.name = "Slab";
                    slab.transform.SetParent(floorRoot.transform, false);
                    slab.transform.localPosition = new Vector3(0, -settings.FloorThickness / 2f, 0);
                    ApplyMaterialAndFinalize(slab, mat);

                    // Pillars
                    if (settings.IncludePillars && i < settings.FloorCount - 1)
                    {
                        float pWidth = 1f;
                        float px = (settings.BuildingWidth / 2f) - (pWidth / 2f);
                        float pz = (settings.BuildingLength / 2f) - (pWidth / 2f);
                        Vector3[] positions = {
                            new Vector3(px, 0, pz),
                            new Vector3(-px, 0, pz),
                            new Vector3(px, 0, -pz),
                            new Vector3(-px, 0, -pz)
                        };

                        for (int p = 0; p < 4; p++)
                        {
                            var pillar = ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(pWidth, settings.FloorHeight, pWidth));
                            pillar.gameObject.name = $"Pillar_{p}";
                            pillar.transform.SetParent(floorRoot.transform, false);
                            pillar.transform.localPosition = positions[p] + new Vector3(0, settings.FloorHeight / 2f, 0);
                            ApplyMaterialAndFinalize(pillar, mat);
                        }
                    }

                    // Stairs (simple ramp for now, connecting to next floor)
                    if (settings.IncludeStairs && i < settings.FloorCount - 1)
                    {
                        var stair = ShapeGenerator.GenerateStair(PivotLocation.Center, new Vector3(3f, settings.FloorHeight, 6f), 10, true);
                        stair.gameObject.name = "Stairs";
                        stair.transform.SetParent(floorRoot.transform, false);
                        // Place stairs roughly in the center or side
                        stair.transform.localPosition = new Vector3(0, settings.FloorHeight / 2f, 0);
                        ApplyMaterialAndFinalize(stair, mat);
                    }
                }
            }

            return root;
        }

        private static void ApplyMaterialAndFinalize(ProBuilderMesh shape, Material mat)
        {
            if (mat != null)
            {
                shape.GetComponent<MeshRenderer>().sharedMaterial = mat;
                shape.SetMaterial(shape.faces, mat);
            }

            shape.ToMesh();
            shape.Refresh();

            if (shape.gameObject.GetComponent<MeshCollider>() == null)
            {
                shape.gameObject.AddComponent<MeshCollider>();
            }
        }
    }
}
