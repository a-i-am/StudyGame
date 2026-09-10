using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using System.Collections.Generic;

public class CreateSyntaxCorridorProBuilder : EditorWindow
{
    [MenuItem("Tools/ProBuilder/Create Syntax Cloister")]
    public static void CreateCorridor()
    {
        GameObject rootObj = new GameObject("Syntax_Cloister_Corridor");

        Material woodFloorMat = CreateMaterial("Mat_Syn_Wood_Deck", new Color(0.75f, 0.55f, 0.35f));
        Material timberFrameMat = CreateMaterial("Mat_Syn_Timber_Beam", new Color(0.45f, 0.32f, 0.22f));
        Material glassMat = CreateTransparentMaterial("Mat_Syn_Sunlight_Glass", new Color(1.0f, 0.95f, 0.8f, 0.38f));
        Material benchMat = CreateMaterial("Mat_Syn_Wood_Bench", new Color(0.58f, 0.38f, 0.24f));
        Material potMat = CreateMaterial("Mat_Syn_Terracotta_Pot", new Color(0.82f, 0.48f, 0.35f));
        Material leafMat = CreateMaterial("Mat_Syn_Lush_Green", new Color(0.28f, 0.65f, 0.28f));
        Material runeGlowCyan = CreateEmissionMaterial("Mat_Syn_Concept_Cyan", new Color(0.25f, 0.88f, 0.95f), 2.8f);
        Material runeGlowGold = CreateEmissionMaterial("Mat_Syn_Concept_Gold", new Color(1.0f, 0.82f, 0.25f), 2.8f);

        float corridorLength = 100f;
        float corridorWidth = 35f;
        float corridorH = 22f;

        ProBuilderMesh floor1F = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(corridorLength, 0.6f, corridorWidth));
        floor1F.gameObject.name = "Corridor_Floor_1F_Deck";
        floor1F.transform.SetParent(rootObj.transform);
        floor1F.transform.position = new Vector3(-corridorLength / 2f, -0.6f, -corridorWidth / 2f);
        ApplyMaterial(floor1F, woodFloorMat);

        ProBuilderMesh floor2F = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(corridorLength, 0.6f, corridorWidth));
        floor2F.gameObject.name = "Greenhouse_Bridge_Floor_2F_Deck";
        floor2F.transform.SetParent(rootObj.transform);
        floor2F.transform.position = new Vector3(-corridorLength / 2f, 8.0f, -corridorWidth / 2f);
        ApplyMaterial(floor2F, woodFloorMat);

        int pillarCount = 12;
        float pillarSpacing = corridorLength / (pillarCount - 1);
        for (int p = 0; p < pillarCount; p++)
        {
            float px = -corridorLength / 2f + p * pillarSpacing;

            ProBuilderMesh pillarFront = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(1.2f, corridorH, 1.2f));
            pillarFront.gameObject.name = "Grand_Timber_Pillar_Front_" + p;
            pillarFront.transform.SetParent(rootObj.transform);
            pillarFront.transform.position = new Vector3(px, 0f, corridorWidth / 2f - 1.2f);
            ApplyMaterial(pillarFront, timberFrameMat);

            ProBuilderMesh pillarBack = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(1.2f, corridorH, 1.2f));
            pillarBack.gameObject.name = "Grand_Timber_Pillar_Back_" + p;
            pillarBack.transform.SetParent(rootObj.transform);
            pillarBack.transform.position = new Vector3(px, 0f, -corridorWidth / 2f);
            ApplyMaterial(pillarBack, timberFrameMat);
        }

        ProBuilderMesh glassWallFront2F = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(corridorLength, 14f, 0.2f));
        glassWallFront2F.gameObject.name = "Greenhouse_Glass_Curtain_Wall_Front";
        glassWallFront2F.transform.SetParent(rootObj.transform);
        glassWallFront2F.transform.position = new Vector3(-corridorLength / 2f, 8.5f, corridorWidth / 2f - 0.2f);
        ApplyMaterial(glassWallFront2F, glassMat);

        ProBuilderMesh glassWallBack2F = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(corridorLength, 14f, 0.2f));
        glassWallBack2F.gameObject.name = "Greenhouse_Glass_Curtain_Wall_Back";
        glassWallBack2F.transform.SetParent(rootObj.transform);
        glassWallBack2F.transform.position = new Vector3(-corridorLength / 2f, 8.5f, -corridorWidth / 2f);
        ApplyMaterial(glassWallBack2F, glassMat);

        ProBuilderMesh glassRoof = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(corridorLength, 0.2f, corridorWidth));
        glassRoof.gameObject.name = "Greenhouse_Glass_Roof";
        glassRoof.transform.SetParent(rootObj.transform);
        glassRoof.transform.position = new Vector3(-corridorLength / 2f, corridorH, -corridorWidth / 2f);
        ApplyMaterial(glassRoof, glassMat);

        for (int b = 0; b < 10; b++)
        {
            float bx = -42f + b * 9f;

            ProBuilderMesh bench1F = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(5.5f, 0.9f, 1.8f));
            bench1F.gameObject.name = "Corridor_Lounge_Bench_1F_" + b;
            bench1F.transform.SetParent(rootObj.transform);
            bench1F.transform.position = new Vector3(bx, 0f, (b % 2 == 0) ? 10f : -12f);
            ApplyMaterial(bench1F, benchMat);

            ProBuilderMesh bench2F = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(5.5f, 0.9f, 1.8f));
            bench2F.gameObject.name = "Corridor_Lounge_Bench_2F_" + b;
            bench2F.transform.SetParent(rootObj.transform);
            bench2F.transform.position = new Vector3(bx, 8.5f, (b % 2 == 0) ? -12f : 10f);
            ApplyMaterial(bench2F, benchMat);
        }

        for (int potIdx = 0; potIdx < 36; potIdx++)
        {
            float px = -46f + (potIdx % 18) * 5.2f;
            float py = (potIdx < 18) ? 0f : 8.5f;
            float pz = (potIdx % 2 == 0) ? (corridorWidth / 2f - 3.5f) : (-corridorWidth / 2f + 2.0f);

            GameObject pot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pot.name = "Greenhouse_Plant_Pot_Grand_" + potIdx;
            pot.transform.SetParent(rootObj.transform);
            pot.transform.position = new Vector3(px, py, pz);
            pot.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            pot.GetComponent<Renderer>().sharedMaterial = potMat;

            GameObject plant = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            plant.name = "Greenhouse_Lush_Tree_" + potIdx;
            plant.transform.SetParent(pot.transform);
            plant.transform.position = pot.transform.position + new Vector3(0f, 2.0f, 0f);
            plant.transform.localScale = new Vector3(2.5f, 3.2f, 2.5f);
            plant.GetComponent<Renderer>().sharedMaterial = leafMat;
        }

        string[] syntaxConcepts = new string[] { "구문", "논리", "흐름", "구조" };
        for (int c = 0; c < 16; c++)
        {
            float cx = -42f + c * 5.5f;
            float cy = 11.5f + Mathf.Sin(c * 0.7f) * 2.2f;
            float cz = (c % 2 == 0) ? 8.0f : -8.0f;

            GameObject conceptCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            conceptCube.name = "Grand_Illuminated_Syntax_Concept_Cube_" + syntaxConcepts[c % 4] + "_" + c;
            conceptCube.transform.SetParent(rootObj.transform);
            conceptCube.transform.position = new Vector3(cx, cy, cz);
            conceptCube.transform.localScale = Vector3.one * 1.6f;
            conceptCube.transform.rotation = Quaternion.Euler(20f * c, 30f * c, 15f);
            conceptCube.GetComponent<Renderer>().sharedMaterial = (c % 2 == 0) ? runeGlowCyan : runeGlowGold;
        }

        Undo.RegisterCreatedObjectUndo(rootObj, "Create Syntax Cloister");
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
