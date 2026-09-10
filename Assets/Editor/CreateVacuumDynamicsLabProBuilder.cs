using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using System.Collections.Generic;

public class CreateVacuumDynamicsLabProBuilder : EditorWindow
{
    [MenuItem("Tools/ProBuilder/Create Vacuum Dynamics Lab")]
    public static void CreateLab()
    {
        GameObject rootObj = new GameObject("Vacuum_Dynamics_Lab");

        Material titaniumMat = CreateMaterial("Mat_Vac_Titanium_Steel", new Color(0.68f, 0.7f, 0.72f));
        Material wallMat = CreateMaterial("Mat_Vac_Lab_Wall", new Color(0.82f, 0.82f, 0.85f));
        Material glassMat = CreateTransparentMaterial("Mat_Vac_Reinforced_Glass", new Color(0.5f, 0.85f, 0.92f, 0.45f));
        Material pipeMetalMat = CreateMaterial("Mat_Vac_Energy_Pipe", new Color(0.4f, 0.42f, 0.45f));
        Material darkMetalMat = CreateMaterial("Mat_Vac_Dark_Metal", new Color(0.28f, 0.28f, 0.32f));

        Material liquidAmber = CreateEmissionMaterial("Mat_Vac_Liquid_Amber", new Color(1.0f, 0.65f, 0.2f), 3.0f);
        Material liquidPurple = CreateEmissionMaterial("Mat_Vac_Liquid_Purple", new Color(0.75f, 0.35f, 0.95f), 3.0f);
        Material liquidCyan = CreateEmissionMaterial("Mat_Vac_Liquid_Cyan", new Color(0.25f, 0.88f, 0.95f), 3.0f);
        Material screenGlow = CreateEmissionMaterial("Mat_Vac_Screen_Dashboard", new Color(0.2f, 0.85f, 0.95f), 2.5f);

        float floorW = 100f;
        float floorD = 80f;
        float wallH = 22f;

        ProBuilderMesh floor = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW, 0.6f, floorD));
        floor.gameObject.name = "Floor_1F_Base_Steel";
        floor.transform.SetParent(rootObj.transform);
        floor.transform.position = new Vector3(-floorW / 2f, -0.6f, -floorD / 2f);
        ApplyMaterial(floor, titaniumMat);

        ProBuilderMesh wallWest = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(1.0f, wallH, floorD));
        wallWest.gameObject.name = "Wall_West_Titanium";
        wallWest.transform.SetParent(rootObj.transform);
        wallWest.transform.position = new Vector3(-floorW / 2f - 1.0f, 0f, -floorD / 2f);
        ApplyMaterial(wallWest, wallMat);

        ProBuilderMesh wallSouth = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW, wallH, 1.0f));
        wallSouth.gameObject.name = "Wall_South_Titanium";
        wallSouth.transform.SetParent(rootObj.transform);
        wallSouth.transform.position = new Vector3(-floorW / 2f, 0f, -floorD / 2f - 1.0f);
        ApplyMaterial(wallSouth, wallMat);

        ProBuilderMesh centralTower = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(18f, 18f, 18f));
        centralTower.gameObject.name = "Central_Vacuum_Chamber_Tower";
        centralTower.transform.SetParent(rootObj.transform);
        centralTower.transform.position = new Vector3(-9f, 0f, -9f);
        ApplyMaterial(centralTower, titaniumMat);

        ProBuilderMesh towerGlassWindow = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(14f, 8f, 0.3f));
        towerGlassWindow.gameObject.name = "Vacuum_Chamber_Observation_Window";
        towerGlassWindow.transform.SetParent(centralTower.transform);
        towerGlassWindow.transform.position = new Vector3(-7f, 5f, 9.1f);
        ApplyMaterial(towerGlassWindow, glassMat);

        ProBuilderMesh towerGlassCore = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(6f, 6f, 6f));
        towerGlassCore.gameObject.name = "Vacuum_Core_Plasma_Reactor";
        towerGlassCore.transform.SetParent(centralTower.transform);
        towerGlassCore.transform.position = new Vector3(-3f, 6f, -3f);
        ApplyMaterial(towerGlassCore, liquidCyan);

        ProBuilderMesh balcony2F = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(36f, 0.5f, 36f));
        balcony2F.gameObject.name = "Central_Chamber_Balcony_2F";
        balcony2F.transform.SetParent(rootObj.transform);
        balcony2F.transform.position = new Vector3(-18f, 7.5f, -18f);
        ApplyMaterial(balcony2F, darkMetalMat);

        int stairSteps = 24;
        float radius = 16f;
        for (int s = 0; s < stairSteps; s++)
        {
            float angle = s * (180f / stairSteps) * Mathf.Deg2Rad;
            float sx = Mathf.Cos(angle) * radius;
            float sz = Mathf.Sin(angle) * radius;
            float sy = (7.5f / stairSteps) * s;

            ProBuilderMesh step = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(2.5f, 0.35f, 1.8f));
            step.gameObject.name = "Spiral_Stair_Step_" + s;
            step.transform.SetParent(rootObj.transform);
            step.transform.position = new Vector3(sx - 1.25f, sy, sz - 0.9f);
            ApplyMaterial(step, darkMetalMat);
        }

        for (int p = 0; p < 8; p++)
        {
            float px = -40f + p * 11.5f;

            ProBuilderMesh pipeH = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(0.8f, 0.8f, floorD - 4f));
            pipeH.gameObject.name = "Energy_Vacuum_Overhead_Pipe_" + p;
            pipeH.transform.SetParent(rootObj.transform);
            pipeH.transform.position = new Vector3(px, 19f, -floorD / 2f + 2f);
            ApplyMaterial(pipeH, pipeMetalMat);
        }

        for (int ws = 0; ws < 6; ws++)
        {
            float wx = -38f + (ws % 3) * 16f;
            float wz = (ws < 3) ? 22f : -28f;

            ProBuilderMesh desk = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(10f, 0.85f, 4.5f));
            desk.gameObject.name = "Control_Workstation_Desk_" + ws;
            desk.transform.SetParent(rootObj.transform);
            desk.transform.position = new Vector3(wx, 0f, wz);
            ApplyMaterial(desk, titaniumMat);

            GameObject monitor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            monitor.name = "Workstation_Dashboard_Monitor_" + ws;
            monitor.transform.SetParent(desk.transform);
            monitor.transform.position = new Vector3(wx + 2f, 1.15f, wz + 3.8f);
            monitor.transform.localScale = new Vector3(2.8f, 1.6f, 0.12f);
            monitor.GetComponent<Renderer>().sharedMaterial = screenGlow;

            CreateBeakerCluster(desk.transform, new Vector3(wx + 6.5f, 0.85f, wz + 2.0f), liquidAmber, liquidPurple, liquidCyan);

            GameObject gearPump = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            gearPump.name = "Vacuum_Pump_Gear_Device_" + ws;
            gearPump.transform.SetParent(desk.transform);
            gearPump.transform.position = new Vector3(wx + 8.5f, 1.2f, wz + 1.2f);
            gearPump.transform.localScale = new Vector3(1.2f, 0.6f, 1.2f);
            gearPump.GetComponent<Renderer>().sharedMaterial = pipeMetalMat;
        }

        Undo.RegisterCreatedObjectUndo(rootObj, "Create Vacuum Dynamics Lab");
        Selection.activeGameObject = rootObj;
    }

    private static void CreateBeakerCluster(Transform parent, Vector3 pos, Material amber, Material purple, Material cyan)
    {
        Material[] liquids = new Material[] { amber, purple, cyan };
        for (int b = 0; b < 4; b++)
        {
            float bx = pos.x + (b % 2) * 0.7f;
            float bz = pos.z + (b / 2) * 0.7f;

            GameObject beaker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beaker.name = "Liquid_Beaker_" + b;
            beaker.transform.SetParent(parent);
            beaker.transform.position = new Vector3(bx, pos.y + 0.3f, bz);
            beaker.transform.localScale = new Vector3(0.35f, 0.6f, 0.35f);
            beaker.GetComponent<Renderer>().sharedMaterial = liquids[b % liquids.Length];
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
