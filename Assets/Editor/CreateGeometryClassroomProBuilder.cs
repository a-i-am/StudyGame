using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using System.Collections.Generic;

public class CreateGeometryClassroomProBuilder : EditorWindow
{
    [MenuItem("Tools/ProBuilder/Create Geometry Precision Classroom")]
    public static void CreateClassroom()
    {
        GameObject rootObj = new GameObject("Geometry_Precision_Classroom");

        Material floorMat = CreateMaterial("Mat_Geo_Wood_Floor", new Color(0.85f, 0.62f, 0.38f));
        Material wallMat = CreateMaterial("Mat_Geo_Warm_Wall", new Color(0.95f, 0.92f, 0.82f));
        Material darkWoodMat = CreateMaterial("Mat_Geo_Dark_Wood", new Color(0.48f, 0.32f, 0.18f));
        Material deskWoodMat = CreateMaterial("Mat_Geo_Desk_Wood", new Color(0.88f, 0.72f, 0.52f));
        Material glassMat = CreateTransparentMaterial("Mat_Geo_Window_Glass", new Color(1.0f, 0.95f, 0.8f, 0.35f));
        Material frameMat = CreateMaterial("Mat_Geo_Window_Frame", new Color(0.92f, 0.82f, 0.65f));
        Material plantPotMat = CreateMaterial("Mat_Geo_Plant_Pot", new Color(0.9f, 0.88f, 0.85f));
        Material leafMat = CreateMaterial("Mat_Geo_Leaf_Green", new Color(0.4f, 0.68f, 0.38f));
        Material posterMat = CreateMaterial("Mat_Geo_Poster_Paper", new Color(0.95f, 0.95f, 0.95f));

        Material shapePink = CreateMaterial("Mat_Geo_Shape_Pink", new Color(0.95f, 0.55f, 0.65f));
        Material shapeCyan = CreateMaterial("Mat_Geo_Shape_Cyan", new Color(0.45f, 0.82f, 0.92f));
        Material shapeYellow = CreateMaterial("Mat_Geo_Shape_Yellow", new Color(0.98f, 0.85f, 0.35f));
        Material shapePurple = CreateMaterial("Mat_Geo_Shape_Purple", new Color(0.72f, 0.55f, 0.88f));
        Material shapeGreen = CreateMaterial("Mat_Geo_Shape_Green", new Color(0.48f, 0.82f, 0.58f));
        Material shapeOrange = CreateMaterial("Mat_Geo_Shape_Orange", new Color(0.95f, 0.62f, 0.35f));
        Material graphRainbow = CreateEmissionMaterial("Mat_Geo_3D_Graph_Rainbow", new Color(0.4f, 0.9f, 0.95f), 2.0f);

        float floorW = 100f;
        float floorD = 80f;
        float wallH = 20f;

        ProBuilderMesh floor = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW, 0.6f, floorD));
        floor.gameObject.name = "Floor_1F_Base";
        floor.transform.SetParent(rootObj.transform);
        floor.transform.position = new Vector3(-floorW / 2f, -0.6f, -floorD / 2f);
        ApplyMaterial(floor, floorMat);

        ProBuilderMesh wallWest = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(1.0f, wallH, floorD));
        wallWest.gameObject.name = "Wall_West_Concrete";
        wallWest.transform.SetParent(rootObj.transform);
        wallWest.transform.position = new Vector3(-floorW / 2f - 1.0f, 0f, -floorD / 2f);
        ApplyMaterial(wallWest, wallMat);

        ProBuilderMesh wallSouth = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW, wallH, 1.0f));
        wallSouth.gameObject.name = "Wall_South_Concrete";
        wallSouth.transform.SetParent(rootObj.transform);
        wallSouth.transform.position = new Vector3(-floorW / 2f, 0f, -floorD / 2f - 1.0f);
        ApplyMaterial(wallSouth, wallMat);

        ProBuilderMesh atriumWindowFrame = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(1.0f, 15f, 50f));
        atriumWindowFrame.gameObject.name = "Grand_Sunlight_Atrium_Window_Frame";
        atriumWindowFrame.transform.SetParent(rootObj.transform);
        atriumWindowFrame.transform.position = new Vector3(floorW / 2f - 0.5f, 2f, -25f);
        ApplyMaterial(atriumWindowFrame, frameMat);

        ProBuilderMesh atriumWindowGlass = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(0.2f, 14.5f, 49.5f));
        atriumWindowGlass.gameObject.name = "Grand_Sunlight_Atrium_Window_Glass";
        atriumWindowGlass.transform.SetParent(rootObj.transform);
        atriumWindowGlass.transform.position = new Vector3(floorW / 2f - 0.1f, 2.2f, -24.75f);
        ApplyMaterial(atriumWindowGlass, glassMat);

        ProBuilderMesh gallery2F = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(80f, 0.5f, 12f));
        gallery2F.gameObject.name = "Gallery_2F_Walkway";
        gallery2F.transform.SetParent(rootObj.transform);
        gallery2F.transform.position = new Vector3(-40f, 6.5f, 28f);
        ApplyMaterial(gallery2F, darkWoodMat);

        ProBuilderMesh gallery3F = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(80f, 0.5f, 10f));
        gallery3F.gameObject.name = "Gallery_3F_Walkway";
        gallery3F.transform.SetParent(rootObj.transform);
        gallery3F.transform.position = new Vector3(-40f, 13.0f, 30f);
        ApplyMaterial(gallery3F, darkWoodMat);

        for (int p = 0; p < 6; p++)
        {
            ProBuilderMesh poster = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(8f, 10f, 0.1f));
            poster.gameObject.name = "Grand_Geometry_Poster_" + p;
            poster.transform.SetParent(rootObj.transform);
            poster.transform.position = new Vector3(-42f + p * 15f, 5f, floorD / 2f - 0.3f);
            ApplyMaterial(poster, posterMat);
        }

        Material[] shapeMats = new Material[] { shapePink, shapeCyan, shapeYellow, shapePurple, shapeGreen, shapeOrange };
        for (int m = 0; m < 15; m++)
        {
            float mx = -30f + (m % 5) * 15f;
            float mz = -20f + (m / 5) * 18f;

            PrimitiveType pType = (PrimitiveType)(m % 5);
            GameObject monument = GameObject.CreatePrimitive(pType);
            monument.name = "Grand_Geometric_Monument_" + m;
            monument.transform.SetParent(rootObj.transform);
            monument.transform.position = new Vector3(mx, 1.8f + (m % 3) * 0.8f, mz);
            monument.transform.localScale = Vector3.one * (2.5f + (m % 4) * 0.8f);
            monument.transform.rotation = Quaternion.Euler(m * 20f, m * 35f, m * 15f);
            monument.GetComponent<Renderer>().sharedMaterial = shapeMats[m % shapeMats.Length];
        }

        for (int g = 0; g < 4; g++)
        {
            float gx = -35f + g * 22f;
            ProBuilderMesh graphMesh = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(12f, 2.5f, 8f));
            graphMesh.gameObject.name = "Grand_3D_Surface_Graph_Mesh_" + g;
            graphMesh.transform.SetParent(rootObj.transform);
            graphMesh.transform.position = new Vector3(gx, 0f, -32f);
            graphMesh.transform.rotation = Quaternion.Euler(10f, g * 25f, -5f);
            ApplyMaterial(graphMesh, graphRainbow);
        }

        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 5; col++)
            {
                float dx = -38f + col * 16f;
                float dz = 15f - row * 12f;

                ProBuilderMesh desk = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(10f, 0.85f, 4.5f));
                desk.gameObject.name = "Student_Study_Desk_Station_" + row + "_" + col;
                desk.transform.SetParent(rootObj.transform);
                desk.transform.position = new Vector3(dx, 0f, dz);
                ApplyMaterial(desk, deskWoodMat);

                GameObject laptop = GameObject.CreatePrimitive(PrimitiveType.Cube);
                laptop.name = "Laptop_Station_" + row + "_" + col;
                laptop.transform.SetParent(desk.transform);
                laptop.transform.position = new Vector3(dx + 2f, 0.9f, dz + 2f);
                laptop.transform.localScale = new Vector3(1.4f, 0.08f, 1.0f);
                laptop.GetComponent<Renderer>().sharedMaterial = frameMat;

                for (int b = 0; b < 4; b++)
                {
                    ProBuilderMesh bar = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(0.4f, 0.4f + b * 0.3f, 0.4f));
                    bar.gameObject.name = "Bar_Chart_Block_" + row + "_" + col + "_" + b;
                    bar.transform.SetParent(desk.transform);
                    bar.transform.position = new Vector3(dx + 5f + b * 0.55f, 0.85f, dz + 1.5f);
                    ApplyMaterial(bar, shapeMats[(b + col) % shapeMats.Length]);
                }
            }
        }

        for (int potIdx = 0; potIdx < 12; potIdx++)
        {
            float px = -42f + potIdx * 7.5f;
            GameObject pot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pot.name = "Succulent_Pot_Grand_" + potIdx;
            pot.transform.SetParent(rootObj.transform);
            pot.transform.position = new Vector3(px, 6.5f, 32.5f);
            pot.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            pot.GetComponent<Renderer>().sharedMaterial = plantPotMat;

            GameObject plant = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            plant.name = "Succulent_Plant_Grand_" + potIdx;
            plant.transform.SetParent(pot.transform);
            plant.transform.position = pot.transform.position + new Vector3(0f, 1.0f, 0f);
            plant.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            plant.GetComponent<Renderer>().sharedMaterial = leafMat;
        }

        Undo.RegisterCreatedObjectUndo(rootObj, "Create Geometry Precision Classroom");
        Selection.activeGameObject = rootObj;
    }

    private static void ApplyMaterial(ProBuilderMesh pbMesh, Material mat)
    {
        if (pbMesh == null || mat == null) return;
        MeshRenderer renderer = pbMesh.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = mat;
        }
        pbMesh.ToMesh();
        pbMesh.Refresh();
    }

    private static Material CreateMaterial(string name, Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");
        Material mat = new Material(shader);
        mat.name = name;
        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", color);
        }
        if (mat.HasProperty("_Color"))
        {
            mat.SetColor("_Color", color);
        }
        return mat;
    }

    private static Material CreateTransparentMaterial(string name, Color color)
    {
        Material mat = CreateMaterial(name, color);
        if (mat.HasProperty("_Surface"))
        {
            mat.SetFloat("_Surface", 1.0f);
        }
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        return mat;
    }

    private static Material CreateEmissionMaterial(string name, Color color, float intensity)
    {
        Material mat = CreateMaterial(name, color);
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * intensity);
        }
        return mat;
    }
}
