using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using System.Collections.Generic;

public class CreateGrandLibraryAcademyProBuilder : EditorWindow
{
    [MenuItem("Tools/ProBuilder/Create Grand Gothic Academy Library")]
    public static void CreateGrandLibrary()
    {
        GameObject rootObj = new GameObject("Grand_Gothic_Academy_Library");

        Material stoneMat = CreateMaterial("Mat_Gothic_Stone", new Color(0.88f, 0.85f, 0.78f));
        Material darkStoneMat = CreateMaterial("Mat_Gothic_DarkStone", new Color(0.72f, 0.69f, 0.64f));
        Material woodMat = CreateMaterial("Mat_Academy_Wood", new Color(0.55f, 0.35f, 0.2f));
        Material darkWoodMat = CreateMaterial("Mat_Bookcase_Wood", new Color(0.4f, 0.24f, 0.14f));
        Material windowGlassMat = CreateMaterial("Mat_Gothic_WindowGlass", new Color(0.4f, 0.75f, 0.85f, 0.6f));
        Material bookCoverMat1 = CreateMaterial("Mat_MagicBook_Red", new Color(0.75f, 0.2f, 0.2f));
        Material bookCoverMat2 = CreateMaterial("Mat_MagicBook_Blue", new Color(0.2f, 0.35f, 0.75f));
        Material bookCoverMat3 = CreateMaterial("Mat_MagicBook_Gold", new Color(0.85f, 0.65f, 0.2f));
        Material magicGlowCyan = CreateEmissionMaterial("Mat_Magic_Glow_Cyan", new Color(0.2f, 0.9f, 0.95f));
        Material magicGlowGold = CreateEmissionMaterial("Mat_Magic_Glow_Gold", new Color(1.0f, 0.8f, 0.2f));

        float floorW = 100f;
        float floorD = 80f;

        ProBuilderMesh mainFloor = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW, 0.6f, floorD));
        mainFloor.gameObject.name = "Floor_Grand_Hall";
        mainFloor.transform.SetParent(rootObj.transform);
        mainFloor.transform.position = new Vector3(-floorW / 2f, -0.6f, -floorD / 2f);
        ApplyMaterial(mainFloor, stoneMat);

        ProBuilderMesh carpet = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(30f, 0.05f, 50f));
        carpet.gameObject.name = "Central_Carpet";
        carpet.transform.SetParent(rootObj.transform);
        carpet.transform.position = new Vector3(-15f, 0.01f, -25f);
        ApplyMaterial(carpet, bookCoverMat1);

        float wallH = 18f;
        float wallThick = 1.0f;

        ProBuilderMesh wallNorth = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW, wallH, wallThick));
        wallNorth.gameObject.name = "Wall_North_Gothic";
        wallNorth.transform.SetParent(rootObj.transform);
        wallNorth.transform.position = new Vector3(-floorW / 2f, 0f, floorD / 2f);
        ApplyMaterial(wallNorth, stoneMat);

        ProBuilderMesh wallSouth = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW, wallH, wallThick));
        wallSouth.gameObject.name = "Wall_South_Gothic";
        wallSouth.transform.SetParent(rootObj.transform);
        wallSouth.transform.position = new Vector3(-floorW / 2f, 0f, -floorD / 2f - wallThick);
        ApplyMaterial(wallSouth, stoneMat);

        ProBuilderMesh wallEast = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(wallThick, wallH, floorD));
        wallEast.gameObject.name = "Wall_East_Gothic";
        wallEast.transform.SetParent(rootObj.transform);
        wallEast.transform.position = new Vector3(floorW / 2f, 0f, -floorD / 2f);
        ApplyMaterial(wallEast, stoneMat);

        ProBuilderMesh wallWest = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(wallThick, wallH, floorD));
        wallWest.gameObject.name = "Wall_West_Gothic";
        wallWest.transform.SetParent(rootObj.transform);
        wallWest.transform.position = new Vector3(-floorW / 2f - wallThick, 0f, -floorD / 2f);
        ApplyMaterial(wallWest, stoneMat);

        int windowCount = 5;
        float windowSpacing = floorW / (windowCount + 1);
        for (int i = 1; i <= windowCount; i++)
        {
            float xPos = -floorW / 2f + (i * windowSpacing);

            ProBuilderMesh windowFrame = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(6f, 10f, 0.4f));
            windowFrame.gameObject.name = "Gothic_Window_Frame_" + i;
            windowFrame.transform.SetParent(rootObj.transform);
            windowFrame.transform.position = new Vector3(xPos - 3f, 4f, floorD / 2f - 0.2f);
            ApplyMaterial(windowFrame, darkStoneMat);

            ProBuilderMesh windowGlass = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(5.2f, 9.2f, 0.1f));
            windowGlass.gameObject.name = "Gothic_Window_Glass_" + i;
            windowGlass.transform.SetParent(rootObj.transform);
            windowGlass.transform.position = new Vector3(xPos - 2.6f, 4.4f, floorD / 2f - 0.1f);
            ApplyMaterial(windowGlass, windowGlassMat);

            for (int r = 0; r < 4; r++)
            {
                ProBuilderMesh mullionV = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(0.2f, 9.2f, 0.2f));
                mullionV.gameObject.name = "Mullion_V_" + i + "_" + r;
                mullionV.transform.SetParent(rootObj.transform);
                mullionV.transform.position = new Vector3(xPos - 2.0f + (r * 1.3f), 4.4f, floorD / 2f - 0.25f);
                ApplyMaterial(mullionV, darkStoneMat);
            }

            for (int r = 0; r < 5; r++)
            {
                ProBuilderMesh mullionH = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(5.2f, 0.2f, 0.2f));
                mullionH.gameObject.name = "Mullion_H_" + i + "_" + r;
                mullionH.transform.SetParent(rootObj.transform);
                mullionH.transform.position = new Vector3(xPos - 2.6f, 5.0f + (r * 1.8f), floorD / 2f - 0.25f);
                ApplyMaterial(mullionH, darkStoneMat);
            }
        }

        ProBuilderMesh walkwayNorth2F = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW, 0.5f, 8f));
        walkwayNorth2F.gameObject.name = "Terrace_2F_North";
        walkwayNorth2F.transform.SetParent(rootObj.transform);
        walkwayNorth2F.transform.position = new Vector3(-floorW / 2f, 5f, floorD / 2f - 8f);
        ApplyMaterial(walkwayNorth2F, woodMat);

        ProBuilderMesh walkwaySouth2F = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW, 0.5f, 8f));
        walkwaySouth2F.gameObject.name = "Terrace_2F_South";
        walkwaySouth2F.transform.SetParent(rootObj.transform);
        walkwaySouth2F.transform.position = new Vector3(-floorW / 2f, 5f, -floorD / 2f);
        ApplyMaterial(walkwaySouth2F, woodMat);

        ProBuilderMesh walkwayEast2F = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(8f, 0.5f, floorD - 16f));
        walkwayEast2F.gameObject.name = "Terrace_2F_East";
        walkwayEast2F.transform.SetParent(rootObj.transform);
        walkwayEast2F.transform.position = new Vector3(floorW / 2f - 8f, 5f, -floorD / 2f + 8f);
        ApplyMaterial(walkwayEast2F, woodMat);

        ProBuilderMesh walkwayWest2F = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(8f, 0.5f, floorD - 16f));
        walkwayWest2F.gameObject.name = "Terrace_2F_West";
        walkwayWest2F.transform.SetParent(rootObj.transform);
        walkwayWest2F.transform.position = new Vector3(-floorW / 2f, 5f, -floorD / 2f + 8f);
        ApplyMaterial(walkwayWest2F, woodMat);

        ProBuilderMesh walkwayNorth3F = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW, 0.5f, 6f));
        walkwayNorth3F.gameObject.name = "Gallery_3F_North";
        walkwayNorth3F.transform.SetParent(rootObj.transform);
        walkwayNorth3F.transform.position = new Vector3(-floorW / 2f, 10.5f, floorD / 2f - 6f);
        ApplyMaterial(walkwayNorth3F, stoneMat);

        ProBuilderMesh walkwaySouth3F = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW, 0.5f, 6f));
        walkwaySouth3F.gameObject.name = "Gallery_3F_South";
        walkwaySouth3F.transform.SetParent(rootObj.transform);
        walkwaySouth3F.transform.position = new Vector3(-floorW / 2f, 10.5f, -floorD / 2f);
        ApplyMaterial(walkwaySouth3F, stoneMat);

        ProBuilderMesh walkwayEast3F = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(6f, 0.5f, floorD - 12f));
        walkwayEast3F.gameObject.name = "Gallery_3F_East";
        walkwayEast3F.transform.SetParent(rootObj.transform);
        walkwayEast3F.transform.position = new Vector3(floorW / 2f - 6f, 10.5f, -floorD / 2f + 6f);
        ApplyMaterial(walkwayEast3F, stoneMat);

        ProBuilderMesh walkwayWest3F = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(6f, 0.5f, floorD - 12f));
        walkwayWest3F.gameObject.name = "Gallery_3F_West";
        walkwayWest3F.transform.SetParent(rootObj.transform);
        walkwayWest3F.transform.position = new Vector3(-floorW / 2f, 10.5f, -floorD / 2f + 6f);
        ApplyMaterial(walkwayWest3F, stoneMat);

        ProBuilderMesh skybridgeMain = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW - 16f, 0.5f, 6f));
        skybridgeMain.gameObject.name = "Central_Grand_Skybridge";
        skybridgeMain.transform.SetParent(rootObj.transform);
        skybridgeMain.transform.position = new Vector3(-floorW / 2f + 8f, 5f, -3f);
        ApplyMaterial(skybridgeMain, woodMat);

        ProBuilderMesh skybridgeCross = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(6f, 0.5f, floorD - 16f));
        skybridgeCross.gameObject.name = "Central_Cross_Skybridge";
        skybridgeCross.transform.SetParent(rootObj.transform);
        skybridgeCross.transform.position = new Vector3(-3f, 5f, -floorD / 2f + 8f);
        ApplyMaterial(skybridgeCross, woodMat);

        float[] pillarXList = new float[] { -35f, -20f, -5f, 5f, 20f, 35f };
        foreach (float px in pillarXList)
        {
            ProBuilderMesh pilN = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(1.2f, 18.0f, 1.2f));
            pilN.gameObject.name = "Gothic_Pillar_North_" + px;
            pilN.transform.SetParent(rootObj.transform);
            pilN.transform.position = new Vector3(px - 0.6f, 0f, floorD / 2f - 8.2f);
            ApplyMaterial(pilN, darkStoneMat);

            ProBuilderMesh pilS = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(1.2f, 18.0f, 1.2f));
            pilS.gameObject.name = "Gothic_Pillar_South_" + px;
            pilS.transform.SetParent(rootObj.transform);
            pilS.transform.position = new Vector3(px - 0.6f, 0f, -floorD / 2f + 7f);
            ApplyMaterial(pilS, darkStoneMat);
        }

        int stepsCount = 14;
        float stairW = 3.5f;
        float stairH = 5.0f;
        float stairD = 8.0f;
        float stepH = stairH / stepsCount;
        float stepD = stairD / stepsCount;

        for (int i = 0; i < stepsCount; i++)
        {
            ProBuilderMesh step1 = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(stairW, stepH, stepD));
            step1.gameObject.name = "Grand_Stair_East_" + (i + 1);
            step1.transform.SetParent(rootObj.transform);
            step1.transform.position = new Vector3(floorW / 2f - 8f - stairW, stepH * i, -15f + (i * stepD));
            ApplyMaterial(step1, woodMat);

            ProBuilderMesh step2 = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(stairW, stepH, stepD));
            step2.gameObject.name = "Grand_Stair_West_" + (i + 1);
            step2.transform.SetParent(rootObj.transform);
            step2.transform.position = new Vector3(-floorW / 2f + 8f, stepH * i, -15f + (i * stepD));
            ApplyMaterial(step2, woodMat);
        }

        for (int b = 0; b < 10; b++)
        {
            float bx = -35f + (b % 5) * 16f;
            float bz = (b < 5) ? 22f : -22f;

            ProBuilderMesh bookcase = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(8f, 4.5f, 1.2f));
            bookcase.gameObject.name = "Bookcase_Wall_" + b;
            bookcase.transform.SetParent(rootObj.transform);
            bookcase.transform.position = new Vector3(bx, 0f, bz);
            ApplyMaterial(bookcase, darkWoodMat);
        }

        for (int b = 0; b < 6; b++)
        {
            float bx = -32f + (b % 3) * 22f;
            float bz = (b < 3) ? 28f : -28f;

            ProBuilderMesh bookcase2F = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(6f, 3.8f, 1.0f));
            bookcase2F.gameObject.name = "Bookcase_2F_" + b;
            bookcase2F.transform.SetParent(rootObj.transform);
            bookcase2F.transform.position = new Vector3(bx, 5.5f, bz);
            ApplyMaterial(bookcase2F, darkWoodMat);
        }

        ProBuilderMesh studyTable = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(12f, 1.0f, 6f));
        studyTable.gameObject.name = "Grand_Study_Table";
        studyTable.transform.SetParent(rootObj.transform);
        studyTable.transform.position = new Vector3(-6f, 0f, -3f);
        ApplyMaterial(studyTable, woodMat);

        for (int leg = 0; leg < 4; leg++)
        {
            float lx = (leg < 2) ? -5.5f : 5.0f;
            float lz = (leg % 2 == 0) ? -2.5f : 2.5f;
            ProBuilderMesh legMesh = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(0.6f, 1.0f, 0.6f));
            legMesh.gameObject.name = "Table_Leg_" + leg;
            legMesh.transform.SetParent(studyTable.transform);
            legMesh.transform.position = new Vector3(lx, -1.0f, lz);
            ApplyMaterial(legMesh, darkWoodMat);
        }

        Material[] bookMaterials = new Material[] { bookCoverMat1, bookCoverMat2, bookCoverMat3 };
        for (int i = 0; i < 24; i++)
        {
            float angle = i * (360f / 24f) * Mathf.Deg2Rad;
            float radius = 8f + (i % 3) * 3f;
            float floatY = 2.5f + Mathf.Sin(i * 0.7f) * 1.5f + (i % 4) * 0.8f;
            float fx = Mathf.Cos(angle) * radius;
            float fz = Mathf.Sin(angle) * radius;

            GameObject bookObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bookObj.name = "Magic_Floating_Book_" + i;
            bookObj.transform.SetParent(rootObj.transform);
            bookObj.transform.position = new Vector3(fx, floatY, fz);
            bookObj.transform.localScale = new Vector3(0.35f, 0.45f, 0.08f);
            bookObj.transform.rotation = Quaternion.Euler(15f * (i % 3), i * 25f, 10f * (i % 5));
            bookObj.GetComponent<Renderer>().sharedMaterial = bookMaterials[i % 3];
        }

        for (int p = 0; p < 40; p++)
        {
            float px = -20f + (p % 8) * 5.5f + Mathf.Sin(p) * 2f;
            float py = 1.5f + (p % 5) * 2.2f + Mathf.Cos(p) * 1.0f;
            float pz = -18f + (p / 8) * 8f + Mathf.Sin(p * 2f) * 2f;

            GameObject particleObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            particleObj.name = "Magic_Light_Particle_" + p;
            particleObj.transform.SetParent(rootObj.transform);
            particleObj.transform.position = new Vector3(px, py, pz);
            particleObj.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
            particleObj.transform.rotation = Quaternion.Euler(p * 18f, p * 33f, p * 45f);
            particleObj.GetComponent<Renderer>().sharedMaterial = (p % 2 == 0) ? magicGlowCyan : magicGlowGold;
        }

        Undo.RegisterCreatedObjectUndo(rootObj, "Create Grand Gothic Academy Library");
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

    private static Material CreateEmissionMaterial(string name, Color color)
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
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 2.5f);
        }
        return mat;
    }
}
