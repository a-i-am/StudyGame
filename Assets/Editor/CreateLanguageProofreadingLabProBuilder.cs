using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using System.Collections.Generic;

public class CreateLanguageProofreadingLabProBuilder : EditorWindow
{
    [MenuItem("Tools/ProBuilder/Create Language Proofreading Lab")]
    public static void CreateLab()
    {
        GameObject rootObj = new GameObject("Language_Proofreading_Lab");

        Material concreteMat = CreateMaterial("Mat_Lab_Concrete", new Color(0.78f, 0.77f, 0.75f), false);
        Material woodFloorMat = CreateMaterial("Mat_Lab_Wood_Floor", new Color(0.65f, 0.45f, 0.28f), false);
        Material darkWoodMat = CreateMaterial("Mat_Lab_Dark_Wood", new Color(0.38f, 0.22f, 0.12f), false);
        Material glassMat = CreateMaterial("Mat_Lab_Glass", new Color(0.5f, 0.75f, 0.85f, 0.45f), true);
        Material blackboardMat = CreateMaterial("Mat_Lab_Blackboard", new Color(0.15f, 0.22f, 0.18f), false);
        Material chalkSymbolMat = CreateEmissionMaterial("Mat_Lab_Chalk_Symbols", new Color(0.95f, 0.95f, 0.9f), 1.5f);
        Material paperMat = CreateMaterial("Mat_Lab_Paper", new Color(0.9f, 0.85f, 0.7f), false);
        Material lampGlowMat = CreateEmissionMaterial("Mat_Lab_Lamp_Glow", new Color(1.0f, 0.78f, 0.35f), 2.5f);
        Material runeCyanMat = CreateEmissionMaterial("Mat_Lab_Rune_Cyan", new Color(0.2f, 0.85f, 0.95f), 3.0f);
        Material runeGoldMat = CreateEmissionMaterial("Mat_Lab_Rune_Gold", new Color(1.0f, 0.75f, 0.2f), 3.0f);
        Material plantGreenMat = CreateMaterial("Mat_Lab_Plant_Green", new Color(0.25f, 0.55f, 0.25f), false);
        Material metalMat = CreateMaterial("Mat_Lab_Metal", new Color(0.25f, 0.25f, 0.28f), false);
        Material bookCoverRed = CreateMaterial("Mat_Lab_Book_Red", new Color(0.65f, 0.18f, 0.15f), false);
        Material bookCoverBlue = CreateMaterial("Mat_Lab_Book_Blue", new Color(0.18f, 0.3f, 0.6f), false);

        float floorW = 100f;
        float floorD = 80f;
        float wallH = 18f;

        ProBuilderMesh mainFloor = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW, 0.6f, floorD));
        mainFloor.gameObject.name = "Floor_1F_Base";
        mainFloor.transform.SetParent(rootObj.transform);
        mainFloor.transform.position = new Vector3(-floorW / 2f, -0.6f, -floorD / 2f);
        ApplyMaterial(mainFloor, woodFloorMat);

        ProBuilderMesh blueprintDecal = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(26f, 0.02f, 18f));
        blueprintDecal.gameObject.name = "Floor_Blueprint_Sketch_Map";
        blueprintDecal.transform.SetParent(rootObj.transform);
        blueprintDecal.transform.position = new Vector3(-5f, 0.01f, -28f);
        ApplyMaterial(blueprintDecal, paperMat);

        for (int b = 0; b < 12; b++)
        {
            ProBuilderMesh lineMesh = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3((b % 2 == 0) ? 24f : 0.15f, 0.03f, (b % 2 == 0) ? 0.15f : 16f));
            lineMesh.gameObject.name = "Blueprint_Grid_Line_" + b;
            lineMesh.transform.SetParent(blueprintDecal.transform);
            lineMesh.transform.position = new Vector3(-4f + (b % 2 == 0 ? 0f : (b / 2) * 4f), 0.02f, -26f + (b % 2 == 0 ? (b / 2) * 3f : 0f));
            ApplyMaterial(lineMesh, runeCyanMat);
        }

        ProBuilderMesh wallWest = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(1.0f, wallH, floorD));
        wallWest.gameObject.name = "Wall_West_Concrete";
        wallWest.transform.SetParent(rootObj.transform);
        wallWest.transform.position = new Vector3(-floorW / 2f - 1.0f, 0f, -floorD / 2f);
        ApplyMaterial(wallWest, concreteMat);

        ProBuilderMesh wallSouth = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW / 2f, wallH, 1.0f));
        wallSouth.gameObject.name = "Wall_South_Concrete";
        wallSouth.transform.SetParent(rootObj.transform);
        wallSouth.transform.position = new Vector3(-floorW / 2f, 0f, -floorD / 2f - 1.0f);
        ApplyMaterial(wallSouth, concreteMat);

        ProBuilderMesh innerDivider = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(1.0f, wallH, floorD / 2f));
        innerDivider.gameObject.name = "Wall_Inner_Divider_Concrete";
        innerDivider.transform.SetParent(rootObj.transform);
        innerDivider.transform.position = new Vector3(-5f, 0f, 0f);
        ApplyMaterial(innerDivider, concreteMat);

        ProBuilderMesh floor2FLeft = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(45f, 0.5f, 40f));
        floor2FLeft.gameObject.name = "Floor_2F_Left_Wing";
        floor2FLeft.transform.SetParent(rootObj.transform);
        floor2FLeft.transform.position = new Vector3(-floorW / 2f, 6.0f, 0f);
        ApplyMaterial(floor2FLeft, woodFloorMat);

        ProBuilderMesh floor2FRight = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(50f, 0.5f, 35f));
        floor2FRight.gameObject.name = "Floor_2F_Right_Atrium_Wing";
        floor2FRight.transform.SetParent(rootObj.transform);
        floor2FRight.transform.position = new Vector3(0f, 6.0f, 5f);
        ApplyMaterial(floor2FRight, woodFloorMat);

        ProBuilderMesh bridgeMesh = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(12f, 0.5f, 8f));
        bridgeMesh.gameObject.name = "Mezzanine_Curved_Bridge";
        bridgeMesh.transform.SetParent(rootObj.transform);
        bridgeMesh.transform.position = new Vector3(-10f, 6.0f, 0f);
        ApplyMaterial(bridgeMesh, darkWoodMat);

        ProBuilderMesh bridgeRailingL = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(12f, 1.1f, 0.15f));
        bridgeRailingL.gameObject.name = "Bridge_Glass_Railing_North";
        bridgeRailingL.transform.SetParent(bridgeMesh.transform);
        bridgeRailingL.transform.position = new Vector3(-10f, 6.5f, 7.9f);
        ApplyMaterial(bridgeRailingL, glassMat);

        ProBuilderMesh bridgeRailingR = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(12f, 1.1f, 0.15f));
        bridgeRailingR.gameObject.name = "Bridge_Glass_Railing_South";
        bridgeRailingR.transform.SetParent(bridgeMesh.transform);
        bridgeRailingR.transform.position = new Vector3(-10f, 6.5f, 0.05f);
        ApplyMaterial(bridgeRailingR, glassMat);

        int glassTrussCount = 8;
        float trussSpacing = 50f / glassTrussCount;
        for (int i = 0; i <= glassTrussCount; i++)
        {
            float xPos = 0f + (i * trussSpacing);

            ProBuilderMesh trussBeam = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(0.6f, 16f, 0.6f));
            trussBeam.gameObject.name = "Atrium_Glass_Truss_Beam_" + i;
            trussBeam.transform.SetParent(rootObj.transform);
            trussBeam.transform.position = new Vector3(xPos, 6.0f, 40f);
            trussBeam.transform.rotation = Quaternion.Euler(38f, 0f, 0f);
            ApplyMaterial(trussBeam, darkWoodMat);

            if (i < glassTrussCount)
            {
                ProBuilderMesh glassPanel = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(trussSpacing, 15.8f, 0.1f));
                glassPanel.gameObject.name = "Atrium_Sloped_Glass_Panel_" + i;
                glassPanel.transform.SetParent(rootObj.transform);
                glassPanel.transform.position = new Vector3(xPos, 6.1f, 40f);
                glassPanel.transform.rotation = Quaternion.Euler(38f, 0f, 0f);
                ApplyMaterial(glassPanel, glassMat);
            }
        }

        ProBuilderMesh mainConfTable = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(16f, 0.9f, 6.5f));
        mainConfTable.gameObject.name = "1F_Main_Conference_Table";
        mainConfTable.transform.SetParent(rootObj.transform);
        mainConfTable.transform.position = new Vector3(-18f, 0f, -12f);
        ApplyMaterial(mainConfTable, darkWoodMat);

        for (int c = 0; c < 10; c++)
        {
            float cx = -24f + (c % 5) * 3.5f;
            float cz = (c < 5) ? -7.8f : -13.8f;
            ProBuilderMesh chair = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(1.2f, 1.8f, 1.2f));
            chair.gameObject.name = "Chair_Conf_" + c;
            chair.transform.SetParent(mainConfTable.transform);
            chair.transform.position = new Vector3(cx, 0f, cz);
            ApplyMaterial(chair, metalMat);
        }

        ProBuilderMesh trackBoard1F = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(8f, 4.0f, 0.3f));
        trackBoard1F.gameObject.name = "1F_Manuscript_Tracking_Board";
        trackBoard1F.transform.SetParent(rootObj.transform);
        trackBoard1F.transform.position = new Vector3(-20f, 1.0f, -4.5f);
        ApplyMaterial(trackBoard1F, paperMat);

        ProBuilderMesh cabinWallGlass = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(0.2f, 5.0f, 25f));
        cabinWallGlass.gameObject.name = "Soundproof_Cabin_Glass_Wall";
        cabinWallGlass.transform.SetParent(rootObj.transform);
        cabinWallGlass.transform.position = new Vector3(-26f, 0f, -38f);
        ApplyMaterial(cabinWallGlass, glassMat);

        for (int deskIdx = 0; deskIdx < 3; deskIdx++)
        {
            float dz = -35f + deskIdx * 7.5f;
            ProBuilderMesh cabinDesk = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(6f, 0.85f, 2.8f));
            cabinDesk.gameObject.name = "Cabin_Proofreading_Desk_" + deskIdx;
            cabinDesk.transform.SetParent(rootObj.transform);
            cabinDesk.transform.position = new Vector3(-34f, 0f, dz);
            ApplyMaterial(cabinDesk, darkWoodMat);

            CreateDesktopProps(rootObj.transform, new Vector3(-34f, 0.85f, dz), paperMat, lampGlowMat, metalMat, bookCoverRed);
        }

        for (int s = 0; s < 6; s++)
        {
            float sx = 12f + (s % 3) * 12f;
            float sz = (s < 3) ? -32f : -18f;

            ProBuilderMesh bookcase = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(10f, 5.5f, 1.8f));
            bookcase.gameObject.name = "Archive_Bookcase_" + s;
            bookcase.transform.SetParent(rootObj.transform);
            bookcase.transform.position = new Vector3(sx, 0f, sz);
            ApplyMaterial(bookcase, darkWoodMat);

            for (int r = 0; r < 4; r++)
            {
                ProBuilderMesh shelfShelf = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(9.6f, 0.15f, 1.6f));
                shelfShelf.gameObject.name = "Shelf_Layer_" + s + "_" + r;
                shelfShelf.transform.SetParent(bookcase.transform);
                shelfShelf.transform.position = new Vector3(sx + 0.2f, 1.0f + r * 1.2f, sz + 0.1f);
                ApplyMaterial(shelfShelf, darkWoodMat);

                for (int bk = 0; bk < 8; bk++)
                {
                    GameObject book = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    book.name = "Archive_Book_" + s + "_" + r + "_" + bk;
                    book.transform.SetParent(bookcase.transform);
                    book.transform.position = new Vector3(sx + 0.8f + bk * 1.1f, 1.15f + r * 1.2f, sz + 0.4f);
                    book.transform.localScale = new Vector3(0.35f, 0.8f, 0.9f);
                    book.GetComponent<Renderer>().sharedMaterial = (bk % 2 == 0) ? bookCoverRed : bookCoverBlue;
                }
            }
        }

        ProBuilderMesh coffeeBar = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(6.5f, 1.1f, 3.0f));
        coffeeBar.gameObject.name = "Coffee_Refreshment_Bar";
        coffeeBar.transform.SetParent(rootObj.transform);
        coffeeBar.transform.position = new Vector3(38f, 0f, -36f);
        ApplyMaterial(coffeeBar, darkWoodMat);

        int stairSteps = 16;
        float stepW = 5.0f;
        float stepH = 6.0f / stairSteps;
        float stepD = 14.0f / stairSteps;
        for (int st = 0; st < stairSteps; st++)
        {
            ProBuilderMesh stairStep = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(stepW, stepH, stepD));
            stairStep.gameObject.name = "Stair_Step_1F_to_2F_" + st;
            stairStep.transform.SetParent(rootObj.transform);
            stairStep.transform.position = new Vector3(-10f, stepH * st, -5f + (st * stepD));
            ApplyMaterial(stairStep, darkWoodMat);
        }

        for (int d2 = 0; d2 < 4; d2++)
        {
            float dx = -40f + (d2 % 2) * 16f;
            float dz = 8f + (d2 / 2) * 14f;

            ProBuilderMesh desk2F = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(10f, 0.85f, 4f));
            desk2F.gameObject.name = "2F_Proofreading_Desk_" + d2;
            desk2F.transform.SetParent(rootObj.transform);
            desk2F.transform.position = new Vector3(dx, 6.5f, dz);
            ApplyMaterial(desk2F, darkWoodMat);

            CreateDesktopProps(rootObj.transform, new Vector3(dx + 1f, 7.35f, dz + 0.5f), paperMat, lampGlowMat, metalMat, bookCoverBlue);
            CreateDesktopProps(rootObj.transform, new Vector3(dx + 5f, 7.35f, dz + 0.5f), paperMat, lampGlowMat, metalMat, bookCoverRed);
        }

        ProBuilderMesh blackboardMain = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(14f, 6.0f, 0.2f));
        blackboardMain.gameObject.name = "2F_Vintage_Blackboard_Main";
        blackboardMain.transform.SetParent(rootObj.transform);
        blackboardMain.transform.position = new Vector3(-35f, 9.0f, 39.5f);
        ApplyMaterial(blackboardMain, blackboardMat);

        CreateChalkMagicCircle(blackboardMain.transform, new Vector3(-32f, 12.0f, 39.3f), chalkSymbolMat, runeGoldMat);
        CreateChalkMagicCircle(blackboardMain.transform, new Vector3(-24f, 11.5f, 39.3f), chalkSymbolMat, runeCyanMat);

        ProBuilderMesh blackboardSide = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(0.2f, 5.0f, 10f));
        blackboardSide.gameObject.name = "2F_Grammar_Rules_Blackboard_Side";
        blackboardSide.transform.SetParent(rootObj.transform);
        blackboardSide.transform.position = new Vector3(-49.5f, 9.0f, 15f);
        ApplyMaterial(blackboardSide, blackboardMat);

        CreateChalkGrammarLines(blackboardSide.transform, new Vector3(-49.3f, 11.0f, 15f), chalkSymbolMat);

        for (int ad = 0; ad < 3; ad++)
        {
            float ax = 8f + ad * 12f;

            ProBuilderMesh atriumDesk = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(9f, 0.85f, 3.5f));
            atriumDesk.gameObject.name = "2F_Atrium_Collaborative_Desk_" + ad;
            atriumDesk.transform.SetParent(rootObj.transform);
            atriumDesk.transform.position = new Vector3(ax, 6.5f, 18f);
            ApplyMaterial(atriumDesk, darkWoodMat);

            CreateDesktopProps(rootObj.transform, new Vector3(ax + 2f, 7.35f, 18.5f), paperMat, lampGlowMat, metalMat, bookCoverRed);
        }

        ProBuilderMesh trackingBoard2F = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(10f, 4.5f, 0.3f));
        trackingBoard2F.gameObject.name = "2F_Manuscript_Status_Tracking_Board";
        trackingBoard2F.transform.SetParent(rootObj.transform);
        trackingBoard2F.transform.position = new Vector3(25f, 7.5f, 32f);
        ApplyMaterial(trackingBoard2F, paperMat);

        for (int p = 0; p < 6; p++)
        {
            float px = -35f + (p % 3) * 30f;
            float pz = -30f + (p / 3) * 50f;
            float py = (p / 3 == 0) ? 0f : 6.5f;

            ProBuilderMesh pot = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(1.2f, 1.5f, 1.2f));
            pot.gameObject.name = "Plant_Pot_" + p;
            pot.transform.SetParent(rootObj.transform);
            pot.transform.position = new Vector3(px, py, pz);
            ApplyMaterial(pot, darkWoodMat);

            GameObject plantLeaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            plantLeaves.name = "Plant_Leaves_" + p;
            plantLeaves.transform.SetParent(pot.transform);
            plantLeaves.transform.position = new Vector3(px + 0.6f, py + 2.2f, pz + 0.6f);
            plantLeaves.transform.localScale = new Vector3(2.2f, 2.5f, 2.2f);
            plantLeaves.GetComponent<Renderer>().sharedMaterial = plantGreenMat;
        }

        for (int r = 0; r < 45; r++)
        {
            float rx = -40f + (r % 9) * 9.5f + Mathf.Sin(r) * 1.8f;
            float ry = 3.0f + (r % 6) * 2.5f + Mathf.Cos(r * 0.8f) * 1.2f;
            float rz = -30f + (r / 9) * 14f + Mathf.Sin(r * 2f) * 2.0f;

            GameObject runeParticle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            runeParticle.name = "Magic_Grammar_Rune_Particle_" + r;
            runeParticle.transform.SetParent(rootObj.transform);
            runeParticle.transform.position = new Vector3(rx, ry, rz);
            runeParticle.transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);
            runeParticle.transform.rotation = Quaternion.Euler(r * 22f, r * 37f, r * 15f);
            runeParticle.GetComponent<Renderer>().sharedMaterial = (r % 2 == 0) ? runeCyanMat : runeGoldMat;
        }

        Undo.RegisterCreatedObjectUndo(rootObj, "Create Language Proofreading Lab");
        Selection.activeGameObject = rootObj;
    }

    private static void CreateDesktopProps(Transform parent, Vector3 pos, Material paperMat, Material lampMat, Material metalMat, Material bookMat)
    {
        GameObject monitor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        monitor.name = "Desk_Monitor_Screen";
        monitor.transform.SetParent(parent);
        monitor.transform.position = pos + new Vector3(0f, 0.6f, 0f);
        monitor.transform.localScale = new Vector3(1.8f, 1.1f, 0.1f);
        monitor.GetComponent<Renderer>().sharedMaterial = metalMat;

        GameObject lampStand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        lampStand.name = "Vintage_Desk_Lamp";
        lampStand.transform.SetParent(parent);
        lampStand.transform.position = pos + new Vector3(-1.5f, 0.4f, 0.3f);
        lampStand.transform.localScale = new Vector3(0.2f, 0.4f, 0.2f);
        lampStand.GetComponent<Renderer>().sharedMaterial = lampMat;

        for (int p = 0; p < 4; p++)
        {
            GameObject paper = GameObject.CreatePrimitive(PrimitiveType.Cube);
            paper.name = "Manuscript_Paper_Stack_" + p;
            paper.transform.SetParent(parent);
            paper.transform.position = pos + new Vector3(1.2f, 0.03f * p, -0.2f + p * 0.05f);
            paper.transform.localScale = new Vector3(0.8f, 0.02f, 1.1f);
            paper.transform.rotation = Quaternion.Euler(0f, p * 12f, 0f);
            paper.GetComponent<Renderer>().sharedMaterial = paperMat;
        }

        GameObject notebook = GameObject.CreatePrimitive(PrimitiveType.Cube);
        notebook.name = "Old_Notebook";
        notebook.transform.SetParent(parent);
        notebook.transform.position = pos + new Vector3(-0.8f, 0.08f, -0.4f);
        notebook.transform.localScale = new Vector3(0.6f, 0.12f, 0.85f);
        notebook.GetComponent<Renderer>().sharedMaterial = bookMat;
    }

    private static void CreateChalkMagicCircle(Transform parent, Vector3 center, Material chalkMat, Material glowMat)
    {
        int circleSegments = 16;
        float radius = 2.2f;
        for (int i = 0; i < circleSegments; i++)
        {
            float angle = i * (360f / circleSegments) * Mathf.Deg2Rad;
            float px = center.x + Mathf.Cos(angle) * radius;
            float py = center.y + Mathf.Sin(angle) * radius;

            GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dot.name = "Chalk_Circle_Rune_Segment_" + i;
            dot.transform.SetParent(parent);
            dot.transform.position = new Vector3(px, py, center.z);
            dot.transform.localScale = new Vector3(0.25f, 0.25f, 0.05f);
            dot.GetComponent<Renderer>().sharedMaterial = (i % 3 == 0) ? glowMat : chalkMat;
        }

        GameObject centerRune = GameObject.CreatePrimitive(PrimitiveType.Cube);
        centerRune.name = "Chalk_Grammar_Center_Rune";
        centerRune.transform.SetParent(parent);
        centerRune.transform.position = center;
        centerRune.transform.localScale = new Vector3(1.2f, 1.2f, 0.05f);
        centerRune.transform.rotation = Quaternion.Euler(0f, 0f, 45f);
        centerRune.GetComponent<Renderer>().sharedMaterial = glowMat;
    }

    private static void CreateChalkGrammarLines(Transform parent, Vector3 startPos, Material chalkMat)
    {
        for (int l = 0; l < 6; l++)
        {
            GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.name = "Chalk_Grammar_Rule_Line_" + l;
            line.transform.SetParent(parent);
            line.transform.position = startPos + new Vector3(0f, -l * 0.7f, 0f);
            line.transform.localScale = new Vector3(0.05f, 0.12f, 4.5f);
            line.GetComponent<Renderer>().sharedMaterial = chalkMat;
        }
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

    private static Material CreateMaterial(string name, Color color, bool isTransparent)
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

        if (isTransparent)
        {
            if (mat.HasProperty("_Surface"))
            {
                mat.SetFloat("_Surface", 1.0f);
            }
            if (mat.HasProperty("_Blend"))
            {
                mat.SetFloat("_Blend", 0.0f);
            }
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        return mat;
    }

    private static Material CreateEmissionMaterial(string name, Color color, float intensity)
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
            mat.SetColor("_EmissionColor", color * intensity);
        }
        return mat;
    }
}
