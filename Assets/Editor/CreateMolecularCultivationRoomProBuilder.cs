using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using System.Collections.Generic;

public class CreateMolecularCultivationRoomProBuilder : EditorWindow
{
    [MenuItem("Tools/ProBuilder/Create Molecular Cultivation Room")]
    public static void CreateRoom()
    {
        GameObject rootObj = new GameObject("Molecular_Cultivation_Room");

        Material woodDeckMat = CreateMaterial("Mat_Mol_Wood_Deck", new Color(0.72f, 0.55f, 0.35f));
        Material steelFrameMat = CreateMaterial("Mat_Mol_Steel_Frame", new Color(0.45f, 0.48f, 0.52f));
        Material glassDomeMat = CreateTransparentMaterial("Mat_Mol_Reinforced_Glass", new Color(0.85f, 0.95f, 0.92f, 0.38f));
        Material leafGreenMat = CreateMaterial("Mat_Mol_Cultivation_Plant", new Color(0.25f, 0.65f, 0.28f));
        Material potMat = CreateMaterial("Mat_Mol_Cultivation_Pot", new Color(0.38f, 0.38f, 0.4f));

        Material dnaCyanMat = CreateEmissionMaterial("Mat_Mol_Hologram_DNA_Cyan", new Color(0.2f, 0.9f, 0.95f), 3.5f);
        Material dnaGoldMat = CreateEmissionMaterial("Mat_Mol_Hologram_DNA_Gold", new Color(1.0f, 0.8f, 0.25f), 3.0f);
        Material screenGlowMat = CreateEmissionMaterial("Mat_Mol_Gene_Dashboard", new Color(0.25f, 0.85f, 0.65f), 2.5f);

        float floorW = 100f;
        float floorD = 80f;
        float domeH = 22f;

        ProBuilderMesh floor = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW, 0.6f, floorD));
        floor.gameObject.name = "Floor_1F_Base_Deck";
        floor.transform.SetParent(rootObj.transform);
        floor.transform.position = new Vector3(-floorW / 2f, -0.6f, -floorD / 2f);
        ApplyMaterial(floor, woodDeckMat);

        ProBuilderMesh glassDomeCeiling = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW, 0.3f, floorD));
        glassDomeCeiling.gameObject.name = "Glass_Dome_Greenhouse_Roof";
        glassDomeCeiling.transform.SetParent(rootObj.transform);
        glassDomeCeiling.transform.position = new Vector3(-floorW / 2f, domeH, -floorD / 2f);
        ApplyMaterial(glassDomeCeiling, glassDomeMat);

        int frameTrusses = 10;
        float trussSpacing = floorW / (frameTrusses - 1);
        for (int t = 0; t < frameTrusses; t++)
        {
            float tx = -floorW / 2f + t * trussSpacing;

            ProBuilderMesh trussBeam = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(1.2f, domeH, 1.2f));
            trussBeam.gameObject.name = "Steel_Frame_Truss_Beam_" + t;
            trussBeam.transform.SetParent(rootObj.transform);
            trussBeam.transform.position = new Vector3(tx, 0f, floorD / 2f - 1.2f);
            ApplyMaterial(trussBeam, steelFrameMat);

            ProBuilderMesh trussBeamBack = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(1.2f, domeH, 1.2f));
            trussBeamBack.gameObject.name = "Steel_Frame_Truss_Beam_Back_" + t;
            trussBeamBack.transform.SetParent(rootObj.transform);
            trussBeamBack.transform.position = new Vector3(tx, 0f, -floorD / 2f);
            ApplyMaterial(trussBeamBack, steelFrameMat);
        }

        int dnaNodeCount = 36;
        float dnaRadius = 3.5f;
        float dnaHeightStep = 14.0f / dnaNodeCount;
        for (int i = 0; i < dnaNodeCount; i++)
        {
            float angle = i * (720f / dnaNodeCount) * Mathf.Deg2Rad;
            float x1 = Mathf.Cos(angle) * dnaRadius;
            float z1 = Mathf.Sin(angle) * dnaRadius;
            float y = 1.0f + i * dnaHeightStep;

            float x2 = Mathf.Cos(angle + Mathf.PI) * dnaRadius;
            float z2 = Mathf.Sin(angle + Mathf.PI) * dnaRadius;

            GameObject node1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            node1.name = "Hologram_DNA_Node_A_" + i;
            node1.transform.SetParent(rootObj.transform);
            node1.transform.position = new Vector3(x1, y, z1);
            node1.transform.localScale = Vector3.one * 0.75f;
            node1.GetComponent<Renderer>().sharedMaterial = dnaCyanMat;

            GameObject node2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            node2.name = "Hologram_DNA_Node_B_" + i;
            node2.transform.SetParent(rootObj.transform);
            node2.transform.position = new Vector3(x2, y, z2);
            node2.transform.localScale = Vector3.one * 0.75f;
            node2.GetComponent<Renderer>().sharedMaterial = dnaGoldMat;

            if (i % 2 == 0)
            {
                ProBuilderMesh strandBond = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(dnaRadius * 2f, 0.15f, 0.15f));
                strandBond.gameObject.name = "Hologram_DNA_Strand_Bond_" + i;
                strandBond.transform.SetParent(rootObj.transform);
                strandBond.transform.position = new Vector3(-dnaRadius, y, 0f);
                strandBond.transform.rotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg, 0f);
                ApplyMaterial(strandBond, dnaCyanMat);
            }
        }

        ProBuilderMesh controlConsole = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(14f, 1.1f, 14f));
        controlConsole.gameObject.name = "Circular_Hologram_DNA_Control_Console";
        controlConsole.transform.SetParent(rootObj.transform);
        controlConsole.transform.position = new Vector3(-7f, 0f, -7f);
        ApplyMaterial(controlConsole, steelFrameMat);

        ProBuilderMesh balcony2F = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(80f, 0.5f, 15f));
        balcony2F.gameObject.name = "Cultivation_Balcony_Walkway_2F";
        balcony2F.transform.SetParent(rootObj.transform);
        balcony2F.transform.position = new Vector3(-40f, 7.0f, 20f);
        ApplyMaterial(balcony2F, woodDeckMat);

        for (int r = 0; r < 6; r++)
        {
            float rx = -42f + r * 15f;

            ProBuilderMesh rack = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(12f, 6.0f, 3.2f));
            rack.gameObject.name = "MultiTier_Plant_Cultivation_Rack_" + r;
            rack.transform.SetParent(rootObj.transform);
            rack.transform.position = new Vector3(rx, 0f, -30f);
            ApplyMaterial(rack, steelFrameMat);

            for (int shelf = 0; shelf < 3; shelf++)
            {
                for (int p = 0; p < 5; p++)
                {
                    GameObject pot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    pot.name = "Bio_Plant_Pot_" + r + "_" + shelf + "_" + p;
                    pot.transform.SetParent(rack.transform);
                    pot.transform.position = new Vector3(rx + 1.2f + p * 2.2f, 1.0f + shelf * 1.8f, -28.5f);
                    pot.transform.localScale = new Vector3(0.6f, 0.5f, 0.6f);
                    pot.GetComponent<Renderer>().sharedMaterial = potMat;

                    GameObject plant = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    plant.name = "Bio_Cultivation_Plant_" + r + "_" + shelf + "_" + p;
                    plant.transform.SetParent(pot.transform);
                    plant.transform.position = pot.transform.position + new Vector3(0f, 0.7f, 0f);
                    plant.transform.localScale = new Vector3(1.1f, 1.3f, 1.1f);
                    plant.GetComponent<Renderer>().sharedMaterial = leafGreenMat;
                }
            }
        }

        for (int w = 0; w < 4; w++)
        {
            float wx = -35f + w * 20f;
            ProBuilderMesh desk = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(12f, 0.85f, 5.0f));
            desk.gameObject.name = "Bio_Analysis_Microscope_Desk_" + w;
            desk.transform.SetParent(rootObj.transform);
            desk.transform.position = new Vector3(wx, 0f, 5f);
            ApplyMaterial(desk, steelFrameMat);

            GameObject monitor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            monitor.name = "Gene_Data_Dashboard_Screen_" + w;
            monitor.transform.SetParent(desk.transform);
            monitor.transform.position = new Vector3(wx + 3f, 1.15f, 9.2f);
            monitor.transform.localScale = new Vector3(3.2f, 1.8f, 0.12f);
            monitor.GetComponent<Renderer>().sharedMaterial = screenGlowMat;

            GameObject roboticArm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            roboticArm.name = "Automated_Plant_Care_Robotic_Arm_" + w;
            roboticArm.transform.SetParent(desk.transform);
            roboticArm.transform.position = new Vector3(wx + 9.5f, 1.4f, 7.5f);
            roboticArm.transform.localScale = new Vector3(0.35f, 1.2f, 0.35f);
            roboticArm.GetComponent<Renderer>().sharedMaterial = steelFrameMat;
        }

        Undo.RegisterCreatedObjectUndo(rootObj, "Create Molecular Cultivation Room");
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
