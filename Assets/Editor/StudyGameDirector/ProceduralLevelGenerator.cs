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
                        float pWidth = settings.PillarThickness;
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

                    // Outer Walls
                    if (settings.IncludeOuterWalls)
                    {
                        float t = settings.WallThickness;
                        float w = settings.BuildingWidth;
                        float l = settings.BuildingLength;
                        float h = settings.FloorHeight;
                        
                        // North Wall
                        var nWall = ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(w, h, t));
                        nWall.gameObject.name = "Wall_North";
                        nWall.transform.SetParent(floorRoot.transform, false);
                        nWall.transform.localPosition = new Vector3(0, h/2f, l/2f - t/2f);
                        ApplyMaterialAndFinalize(nWall, mat);
                        
                        // South Wall
                        var sWall = ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(w, h, t));
                        sWall.gameObject.name = "Wall_South";
                        sWall.transform.SetParent(floorRoot.transform, false);
                        sWall.transform.localPosition = new Vector3(0, h/2f, -l/2f + t/2f);
                        ApplyMaterialAndFinalize(sWall, mat);
                        
                        // East Wall
                        var eWall = ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(t, h, l - t*2));
                        eWall.gameObject.name = "Wall_East";
                        eWall.transform.SetParent(floorRoot.transform, false);
                        eWall.transform.localPosition = new Vector3(w/2f - t/2f, h/2f, 0);
                        ApplyMaterialAndFinalize(eWall, mat);
                        
                        // West Wall
                        var wWall = ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(t, h, l - t*2));
                        wWall.gameObject.name = "Wall_West";
                        wWall.transform.SetParent(floorRoot.transform, false);
                        wWall.transform.localPosition = new Vector3(-w/2f + t/2f, h/2f, 0);
                        ApplyMaterialAndFinalize(wWall, mat);
                    }

                    // Stairs (simple ramp for now, connecting to next floor)
                    if (settings.IncludeStairs && i < settings.FloorCount - 1)
                    {
                        float stairLength = settings.StairTreadDepth * settings.StairSteps;
                        var stair = ShapeGenerator.GenerateStair(PivotLocation.Center, new Vector3(settings.StairWidth, settings.FloorHeight, stairLength), settings.StairSteps, true);
                        stair.gameObject.name = "Stairs";
                        stair.transform.SetParent(floorRoot.transform, false);
                        stair.transform.localPosition = new Vector3(settings.StairOffsetX, settings.FloorHeight / 2f, settings.StairOffsetZ);
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
