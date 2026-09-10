using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using System.Collections.Generic;

public class CreateLargeArenaProBuilder : EditorWindow
{
    [MenuItem("Tools/ProBuilder/Create Large Multi-Level Arena")]
    public static void CreateArena()
    {
        GameObject rootObj = new GameObject("Large_MultiLevel_Arena");

        Material gridMat = CreateGridMaterial("Mat_Arena_Grid", new Color(0.85f, 0.87f, 0.9f));
        Material pillarMat = CreateGridMaterial("Mat_Arena_Pillar", new Color(0.75f, 0.78f, 0.82f));
        Material orangeMat = CreateSimpleMaterial("Mat_Capsule_Orange", new Color(0.95f, 0.5f, 0.1f));
        Material redMat = CreateSimpleMaterial("Mat_Capsule_Red", new Color(0.85f, 0.15f, 0.15f));

        ProBuilderMesh ground = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(40f, 0.5f, 40f));
        ground.gameObject.name = "Arena_Ground_Floor";
        ground.transform.SetParent(rootObj.transform);
        ground.transform.position = new Vector3(-20f, -0.5f, -20f);
        ApplyMaterial(ground, gridMat);

        float wallHeight = 10f;
        float wallThickness = 0.5f;

        ProBuilderMesh wallNorth = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(40f, wallHeight, wallThickness));
        wallNorth.gameObject.name = "Wall_North";
        wallNorth.transform.SetParent(rootObj.transform);
        wallNorth.transform.position = new Vector3(-20f, 0f, 20f);
        ApplyMaterial(wallNorth, gridMat);

        ProBuilderMesh wallSouth = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(40f, wallHeight, wallThickness));
        wallSouth.gameObject.name = "Wall_South";
        wallSouth.transform.SetParent(rootObj.transform);
        wallSouth.transform.position = new Vector3(-20f, 0f, -20.5f);
        ApplyMaterial(wallSouth, gridMat);

        ProBuilderMesh wallEast = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(wallThickness, wallHeight, 40f));
        wallEast.gameObject.name = "Wall_East";
        wallEast.transform.SetParent(rootObj.transform);
        wallEast.transform.position = new Vector3(20f, 0f, -20f);
        ApplyMaterial(wallEast, gridMat);

        ProBuilderMesh wallWest = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(wallThickness, wallHeight, 40f));
        wallWest.gameObject.name = "Wall_West";
        wallWest.transform.SetParent(rootObj.transform);
        wallWest.transform.position = new Vector3(-20.5f, 0f, -20f);
        ApplyMaterial(wallWest, gridMat);

        ProBuilderMesh walkwayNorth = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(40f, 0.4f, 4f));
        walkwayNorth.gameObject.name = "Walkway_2F_North";
        walkwayNorth.transform.SetParent(rootObj.transform);
        walkwayNorth.transform.position = new Vector3(-20f, 4f, 16f);
        ApplyMaterial(walkwayNorth, gridMat);

        ProBuilderMesh walkwaySouth = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(40f, 0.4f, 4f));
        walkwaySouth.gameObject.name = "Walkway_2F_South";
        walkwaySouth.transform.SetParent(rootObj.transform);
        walkwaySouth.transform.position = new Vector3(-20f, 4f, -20f);
        ApplyMaterial(walkwaySouth, gridMat);

        ProBuilderMesh walkwayEast = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(4f, 0.4f, 32f));
        walkwayEast.gameObject.name = "Walkway_2F_East";
        walkwayEast.transform.SetParent(rootObj.transform);
        walkwayEast.transform.position = new Vector3(16f, 4f, -16f);
        ApplyMaterial(walkwayEast, gridMat);

        ProBuilderMesh walkwayWest = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(4f, 0.4f, 32f));
        walkwayWest.gameObject.name = "Walkway_2F_West";
        walkwayWest.transform.SetParent(rootObj.transform);
        walkwayWest.transform.position = new Vector3(-20f, 4f, -16f);
        ApplyMaterial(walkwayWest, gridMat);

        ProBuilderMesh centerBridge = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(32f, 0.4f, 4f));
        centerBridge.gameObject.name = "Central_Skybridge";
        centerBridge.transform.SetParent(rootObj.transform);
        centerBridge.transform.position = new Vector3(-16f, 4f, -2f);
        ApplyMaterial(centerBridge, gridMat);

        ProBuilderMesh galleryNorth = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(40f, 0.4f, 3f));
        galleryNorth.gameObject.name = "Gallery_3F_North";
        galleryNorth.transform.SetParent(rootObj.transform);
        galleryNorth.transform.position = new Vector3(-20f, 7.5f, 17f);
        ApplyMaterial(galleryNorth, gridMat);

        ProBuilderMesh gallerySouth = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(40f, 0.4f, 3f));
        gallerySouth.gameObject.name = "Gallery_3F_South";
        gallerySouth.transform.SetParent(rootObj.transform);
        gallerySouth.transform.position = new Vector3(-20f, 7.5f, -20f);
        ApplyMaterial(gallerySouth, gridMat);

        ProBuilderMesh galleryEast = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(3f, 0.4f, 34f));
        galleryEast.gameObject.name = "Gallery_3F_East";
        galleryEast.transform.SetParent(rootObj.transform);
        galleryEast.transform.position = new Vector3(17f, 7.5f, -17f);
        ApplyMaterial(galleryEast, gridMat);

        ProBuilderMesh galleryWest = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(3f, 0.4f, 34f));
        galleryWest.gameObject.name = "Gallery_3F_West";
        galleryWest.transform.SetParent(rootObj.transform);
        galleryWest.transform.position = new Vector3(-20f, 7.5f, -17f);
        ApplyMaterial(galleryWest, gridMat);

        float[] pillarXPositions = new float[] { -15f, -5f, 5f, 15f };
        for (int i = 0; i < pillarXPositions.Length; i++)
        {
            ProBuilderMesh p1 = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(0.8f, 4f, 0.8f));
            p1.gameObject.name = "Pillar_Ground_North_" + i;
            p1.transform.SetParent(rootObj.transform);
            p1.transform.position = new Vector3(pillarXPositions[i], 0f, 16f);
            ApplyMaterial(p1, pillarMat);

            ProBuilderMesh p2 = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(0.8f, 4f, 0.8f));
            p2.gameObject.name = "Pillar_Ground_South_" + i;
            p2.transform.SetParent(rootObj.transform);
            p2.transform.position = new Vector3(pillarXPositions[i], 0f, -16f);
            ApplyMaterial(p2, pillarMat);
        }

        ProBuilderMesh bridgePillar1 = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(1.2f, 4f, 1.2f));
        bridgePillar1.gameObject.name = "Bridge_Support_Pillar_1";
        bridgePillar1.transform.SetParent(rootObj.transform);
        bridgePillar1.transform.position = new Vector3(-8f, 0f, 0f);
        ApplyMaterial(bridgePillar1, pillarMat);

        ProBuilderMesh bridgePillar2 = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(1.2f, 4f, 1.2f));
        bridgePillar2.gameObject.name = "Bridge_Support_Pillar_2";
        bridgePillar2.transform.SetParent(rootObj.transform);
        bridgePillar2.transform.position = new Vector3(8f, 0f, 0f);
        ApplyMaterial(bridgePillar2, pillarMat);

        for (int i = 0; i < 8; i++)
        {
            float x = -17.5f + i * 5f;
            ProBuilderMesh col = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(0.6f, 3.1f, 0.6f));
            col.gameObject.name = "Gallery_Col_North_" + i;
            col.transform.SetParent(rootObj.transform);
            col.transform.position = new Vector3(x, 4.4f, 17f);
            ApplyMaterial(col, pillarMat);
        }

        int stairSteps = 10;
        float stairW = 2.5f;
        float totalH = 4.0f;
        float totalD = 6.0f;
        float stepH = totalH / stairSteps;
        float stepD = totalD / stairSteps;

        for (int i = 0; i < stairSteps; i++)
        {
            ProBuilderMesh step = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(stairW, stepH, stepD));
            step.gameObject.name = "Main_Stair_Step_" + (i + 1);
            step.transform.SetParent(rootObj.transform);
            step.transform.position = new Vector3(-18.5f, stepH * i, -16f + (i * stepD));
            ApplyMaterial(step, gridMat);
        }

        Vector3[] capsulePositionsOrange = new Vector3[]
        {
            new Vector3(-5f, 4.4f, 0f),
            new Vector3(10f, 4.4f, 18f),
            new Vector3(-12f, 0f, -8f),
            new Vector3(8f, 7.9f, -18.5f)
        };

        foreach (Vector3 pos in capsulePositionsOrange)
        {
            GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            cap.name = "Dummy_Target_Orange";
            cap.transform.SetParent(rootObj.transform);
            cap.transform.position = pos + new Vector3(0f, 1f, 0f);
            cap.GetComponent<Renderer>().sharedMaterial = orangeMat;
        }

        Vector3[] capsulePositionsRed = new Vector3[]
        {
            new Vector3(5f, 0f, -5f),
            new Vector3(18f, 4.4f, -2f)
        };

        foreach (Vector3 pos in capsulePositionsRed)
        {
            GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            cap.name = "Dummy_Target_Red";
            cap.transform.SetParent(rootObj.transform);
            cap.transform.position = pos + new Vector3(0f, 1f, 0f);
            cap.GetComponent<Renderer>().sharedMaterial = redMat;
        }

        Undo.RegisterCreatedObjectUndo(rootObj, "Create Large Multi-Level Arena");
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

    private static Material CreateGridMaterial(string name, Color color)
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

    private static Material CreateSimpleMaterial(string name, Color color)
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
}
