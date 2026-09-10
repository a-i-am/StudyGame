using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using System.Collections.Generic;

public class CreateMootCourtDebateRoomProBuilder : EditorWindow
{
    [MenuItem("Tools/ProBuilder/Create Moot Court Debate Room")]
    public static void CreateDebateRoom()
    {
        GameObject rootObj = new GameObject("Moot_Court_Debate_Room");

        Material floorWoodMat = CreateMaterial("Mat_Debate_Floor_Wood", new Color(0.68f, 0.45f, 0.28f));
        Material wallWoodMat = CreateMaterial("Mat_Debate_Wall_Panel", new Color(0.52f, 0.35f, 0.22f));
        Material darkWoodMat = CreateMaterial("Mat_Debate_Dark_Oak", new Color(0.35f, 0.22f, 0.14f));
        Material blackboardMat = CreateMaterial("Mat_Debate_Blackboard", new Color(0.18f, 0.25f, 0.2f));
        Material chalkTextMat = CreateEmissionMaterial("Mat_Debate_Chalk_Text", new Color(0.95f, 0.95f, 0.9f), 1.8f);
        Material brassScaleMat = CreateEmissionMaterial("Mat_Debate_Brass_Scale", new Color(0.92f, 0.78f, 0.32f), 1.8f);

        Material armchairGreen = CreateMaterial("Mat_Debate_Armchair_Green", new Color(0.25f, 0.48f, 0.32f));
        Material armchairBurgundy = CreateMaterial("Mat_Debate_Armchair_Burgundy", new Color(0.58f, 0.22f, 0.22f));
        Material armchairYellow = CreateMaterial("Mat_Debate_Armchair_Yellow", new Color(0.88f, 0.68f, 0.28f));
        Material armchairNavy = CreateMaterial("Mat_Debate_Armchair_Navy", new Color(0.22f, 0.32f, 0.52f));

        Material cubeJustice = CreateEmissionMaterial("Mat_Debate_Cube_Justice", new Color(0.3f, 0.75f, 0.95f), 2.5f);
        Material cubeLogic = CreateEmissionMaterial("Mat_Debate_Cube_Logic", new Color(0.95f, 0.75f, 0.25f), 2.5f);
        Material leafMat = CreateMaterial("Mat_Debate_Leaf_Green", new Color(0.25f, 0.55f, 0.25f));

        float floorW = 100f;
        float floorD = 80f;
        float wallH = 20f;

        ProBuilderMesh floor = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW, 0.6f, floorD));
        floor.gameObject.name = "Floor_1F_Base_Wood";
        floor.transform.SetParent(rootObj.transform);
        floor.transform.position = new Vector3(-floorW / 2f, -0.6f, -floorD / 2f);
        ApplyMaterial(floor, floorWoodMat);

        ProBuilderMesh wallBack = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW, wallH, 1.0f));
        wallBack.gameObject.name = "Wall_Back_Wood_Panel";
        wallBack.transform.SetParent(rootObj.transform);
        wallBack.transform.position = new Vector3(-floorW / 2f, 0f, floorD / 2f);
        ApplyMaterial(wallBack, wallWoodMat);

        ProBuilderMesh wallWest = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(1.0f, wallH, floorD));
        wallWest.gameObject.name = "Wall_West_Wood_Panel";
        wallWest.transform.SetParent(rootObj.transform);
        wallWest.transform.position = new Vector3(-floorW / 2f - 1.0f, 0f, -floorD / 2f);
        ApplyMaterial(wallWest, wallWoodMat);

        ProBuilderMesh gallery2F = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(80f, 0.5f, 15f));
        gallery2F.gameObject.name = "Gallery_2F_Jury_Balcony";
        gallery2F.transform.SetParent(rootObj.transform);
        gallery2F.transform.position = new Vector3(-40f, 6.5f, 22f);
        ApplyMaterial(gallery2F, darkWoodMat);

        ProBuilderMesh stagePlatform = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(28f, 1.2f, 12f));
        stagePlatform.gameObject.name = "Grand_Judge_Moderator_Stage_Platform";
        stagePlatform.transform.SetParent(rootObj.transform);
        stagePlatform.transform.position = new Vector3(-14f, 0f, 22f);
        ApplyMaterial(stagePlatform, darkWoodMat);

        ProBuilderMesh judgePodium = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(14f, 1.8f, 4.5f));
        judgePodium.gameObject.name = "Judge_Podium_Desk";
        judgePodium.transform.SetParent(rootObj.transform);
        judgePodium.transform.position = new Vector3(-7f, 1.2f, 25f);
        ApplyMaterial(judgePodium, darkWoodMat);

        CreateScaleOfJusticeProp(judgePodium.transform, new Vector3(0f, 3.0f, 26f), brassScaleMat);

        ProBuilderMesh blackboard = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(22f, 8.0f, 0.3f));
        blackboard.gameObject.name = "Logic_Workshop_Blackboard";
        blackboard.transform.SetParent(rootObj.transform);
        blackboard.transform.position = new Vector3(-11f, 3.8f, floorD / 2f - 0.5f);
        ApplyMaterial(blackboard, blackboardMat);

        ProBuilderMesh chalkTitle = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(14f, 1.5f, 0.1f));
        chalkTitle.gameObject.name = "Blackboard_Title_AI_Ethics";
        chalkTitle.transform.SetParent(blackboard.transform);
        chalkTitle.transform.position = new Vector3(-7f, 9.5f, floorD / 2f - 0.6f);
        ApplyMaterial(chalkTitle, chalkTextMat);

        int roundTableSegments = 36;
        float radius = 12.0f;
        for (int i = 0; i < roundTableSegments; i++)
        {
            float angle = i * (360f / roundTableSegments) * Mathf.Deg2Rad;
            float px = Mathf.Cos(angle) * radius;
            float pz = -10.0f + Mathf.Sin(angle) * radius;

            ProBuilderMesh seg = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(2.5f, 0.9f, 2.5f));
            seg.gameObject.name = "Grand_Round_Table_Segment_" + i;
            seg.transform.SetParent(rootObj.transform);
            seg.transform.position = new Vector3(px - 1.25f, 0f, pz - 1.25f);
            ApplyMaterial(seg, darkWoodMat);
        }

        Material[] chairMats = new Material[] { armchairGreen, armchairBurgundy, armchairYellow, armchairNavy };
        int chairCount = 20;
        for (int c = 0; c < chairCount; c++)
        {
            float angle = c * (360f / chairCount) * Mathf.Deg2Rad;
            float cx = Mathf.Cos(angle) * 16.0f;
            float cz = -10.0f + Mathf.Sin(angle) * 16.0f;

            ProBuilderMesh chair = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(2.5f, 2.8f, 2.5f));
            chair.gameObject.name = "Grand_Casual_Debate_Armchair_" + c;
            chair.transform.SetParent(rootObj.transform);
            chair.transform.position = new Vector3(cx - 1.25f, 0f, cz - 1.25f);
            chair.transform.rotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg + 90f, 0f);
            ApplyMaterial(chair, chairMats[c % chairMats.Length]);
        }

        for (int b = 0; b < 6; b++)
        {
            float bx = (b < 3) ? -42f : 35f;
            float bz = -25f + (b % 3) * 18f;
            ProBuilderMesh bookcase = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(5.0f, 9.0f, 2.5f));
            bookcase.gameObject.name = "Grand_Law_Bookcase_" + b;
            bookcase.transform.SetParent(rootObj.transform);
            bookcase.transform.position = new Vector3(bx, 0f, bz);
            ApplyMaterial(bookcase, darkWoodMat);
        }

        for (int p = 0; p < 8; p++)
        {
            float px = -35f + p * 10.0f;
            ProBuilderMesh pot = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(2.0f, 2.2f, 2.0f));
            pot.gameObject.name = "Debate_Room_Plant_Pot_" + p;
            pot.transform.SetParent(rootObj.transform);
            pot.transform.position = new Vector3(px, 0f, -34f);
            ApplyMaterial(pot, darkWoodMat);

            GameObject plant = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            plant.name = "Plant_Leaves_" + p;
            plant.transform.SetParent(pot.transform);
            plant.transform.position = pot.transform.position + new Vector3(1.0f, 3.2f, 1.0f);
            plant.transform.localScale = new Vector3(3.5f, 4.0f, 3.5f);
            plant.GetComponent<Renderer>().sharedMaterial = leafMat;
        }

        string[] logicConcepts = new string[] { "정의", "논쟁", "화합", "논리" };
        for (int lc = 0; lc < 8; lc++)
        {
            float lx = 28f + (lc % 2) * 8f;
            float lz = -25f + (lc / 2) * 15f;

            GameObject logicCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            logicCube.name = "Grand_Logic_Concept_Cube_" + logicConcepts[lc % 4] + "_" + lc;
            logicCube.transform.SetParent(rootObj.transform);
            logicCube.transform.position = new Vector3(lx, 3.0f + lc * 0.8f, lz);
            logicCube.transform.localScale = Vector3.one * 1.8f;
            logicCube.transform.rotation = Quaternion.Euler(20f, lc * 25f, 10f);
            logicCube.GetComponent<Renderer>().sharedMaterial = (lc % 2 == 0) ? cubeJustice : cubeLogic;
        }

        Undo.RegisterCreatedObjectUndo(rootObj, "Create Moot Court Debate Room");
        Selection.activeGameObject = rootObj;
    }

    private static void CreateScaleOfJusticeProp(Transform parent, Vector3 pos, Material brassMat)
    {
        GameObject stand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stand.name = "Scale_Central_Pillar";
        stand.transform.SetParent(parent);
        stand.transform.position = pos;
        stand.transform.localScale = new Vector3(0.35f, 1.8f, 0.35f);
        stand.GetComponent<Renderer>().sharedMaterial = brassMat;

        GameObject crossBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crossBar.name = "Scale_Cross_Bar";
        crossBar.transform.SetParent(stand.transform);
        crossBar.transform.position = pos + new Vector3(0f, 1.6f, 0f);
        crossBar.transform.localScale = new Vector3(3.8f, 0.18f, 0.25f);
        crossBar.GetComponent<Renderer>().sharedMaterial = brassMat;

        for (int p = 0; p < 2; p++)
        {
            float px = (p == 0) ? -1.8f : 1.8f;
            GameObject pan = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pan.name = "Scale_Pan_" + p;
            pan.transform.SetParent(stand.transform);
            pan.transform.position = pos + new Vector3(px, 1.0f, 0f);
            pan.transform.localScale = new Vector3(1.1f, 0.08f, 1.1f);
            pan.GetComponent<Renderer>().sharedMaterial = brassMat;
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
