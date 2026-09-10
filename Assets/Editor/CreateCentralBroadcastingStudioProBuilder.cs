using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using System.Collections.Generic;

public class CreateCentralBroadcastingStudioProBuilder : EditorWindow
{
    [MenuItem("Tools/ProBuilder/Create Central Broadcasting Studio")]
    public static void CreateStudio()
    {
        GameObject rootObj = new GameObject("Central_Broadcasting_Studio");

        Material studioWoodMat = CreateMaterial("Mat_Studio_Wood_Panel", new Color(0.48f, 0.32f, 0.22f));
        Material darkOakMat = CreateMaterial("Mat_Studio_Dark_Oak", new Color(0.32f, 0.2f, 0.14f));
        Material carpetMat = CreateMaterial("Mat_Studio_Acoustic_Carpet", new Color(0.42f, 0.28f, 0.22f));
        Material metalGearMat = CreateMaterial("Mat_Studio_Metal_Equipment", new Color(0.28f, 0.28f, 0.32f));
        Material lpVinylBlack = CreateMaterial("Mat_Studio_LP_Vinyl", new Color(0.12f, 0.12f, 0.14f));
        Material lampWarmGlow = CreateEmissionMaterial("Mat_Studio_Warm_Lamp_Glow", new Color(1.0f, 0.78f, 0.35f), 3.0f);
        Material glassViewMat = CreateTransparentMaterial("Mat_Studio_City_View_Glass", new Color(0.85f, 0.9f, 1.0f, 0.35f));
        Material sofaMat = CreateMaterial("Mat_Studio_Cozy_Sofa", new Color(0.25f, 0.25f, 0.28f));

        float floorW = 100f;
        float floorD = 80f;
        float wallH = 20f;

        ProBuilderMesh floor = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW, 0.6f, floorD));
        floor.gameObject.name = "Floor_1F_Base_Acoustic";
        floor.transform.SetParent(rootObj.transform);
        floor.transform.position = new Vector3(-floorW / 2f, -0.6f, -floorD / 2f);
        ApplyMaterial(floor, studioWoodMat);

        ProBuilderMesh mainCarpet = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(70f, 0.05f, 50f));
        mainCarpet.gameObject.name = "Acoustic_Studio_Carpet";
        mainCarpet.transform.SetParent(rootObj.transform);
        mainCarpet.transform.position = new Vector3(-35f, 0.01f, -25f);
        ApplyMaterial(mainCarpet, carpetMat);

        ProBuilderMesh wallBack = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW, wallH, 1.0f));
        wallBack.gameObject.name = "Wall_Back_Acoustic_Panel";
        wallBack.transform.SetParent(rootObj.transform);
        wallBack.transform.position = new Vector3(-floorW / 2f, 0f, floorD / 2f);
        ApplyMaterial(wallBack, studioWoodMat);

        ProBuilderMesh wallWest = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(1.0f, wallH, floorD));
        wallWest.gameObject.name = "Wall_West_Acoustic_Panel";
        wallWest.transform.SetParent(rootObj.transform);
        wallWest.transform.position = new Vector3(-floorW / 2f - 1.0f, 0f, -floorD / 2f);
        ApplyMaterial(wallWest, studioWoodMat);

        ProBuilderMesh cityViewFrame = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(1.0f, 14f, 45f));
        cityViewFrame.gameObject.name = "City_Skyline_View_Window_Frame";
        cityViewFrame.transform.SetParent(rootObj.transform);
        cityViewFrame.transform.position = new Vector3(floorW / 2f - 0.5f, 2f, -22.5f);
        ApplyMaterial(cityViewFrame, darkOakMat);

        ProBuilderMesh cityViewGlass = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(0.2f, 13.5f, 44.5f));
        cityViewGlass.gameObject.name = "City_Skyline_View_Window_Glass";
        cityViewGlass.transform.SetParent(rootObj.transform);
        cityViewGlass.transform.position = new Vector3(floorW / 2f - 0.1f, 2.2f, -22.25f);
        ApplyMaterial(cityViewGlass, glassViewMat);

        ProBuilderMesh mainBroadcastDesk = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(22f, 0.9f, 8.5f));
        mainBroadcastDesk.gameObject.name = "Main_4Person_Broadcast_Desk";
        mainBroadcastDesk.transform.SetParent(rootObj.transform);
        mainBroadcastDesk.transform.position = new Vector3(-11f, 0f, -5f);
        ApplyMaterial(mainBroadcastDesk, darkOakMat);

        for (int m = 0; m < 4; m++)
        {
            float mx = -8f + (m % 2) * 12f;
            float mz = (m < 2) ? -2f : -8f;

            GameObject micStand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            micStand.name = "Studio_Broadcast_Mic_Stand_" + m;
            micStand.transform.SetParent(mainBroadcastDesk.transform);
            micStand.transform.position = new Vector3(mx, 0.9f, mz);
            micStand.transform.localScale = new Vector3(0.25f, 0.8f, 0.25f);
            micStand.GetComponent<Renderer>().sharedMaterial = metalGearMat;

            GameObject micHead = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            micHead.name = "Studio_Broadcast_Mic_Capsule_" + m;
            micHead.transform.SetParent(micStand.transform);
            micHead.transform.position = new Vector3(mx, 1.8f, mz);
            micHead.transform.localScale = Vector3.one * 0.45f;
            micHead.GetComponent<Renderer>().sharedMaterial = metalGearMat;
        }

        ProBuilderMesh lpStorageWall = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(45f, 14.0f, 3.5f));
        lpStorageWall.gameObject.name = "Massive_LP_Vinyl_Record_Storage_Wall";
        lpStorageWall.transform.SetParent(rootObj.transform);
        lpStorageWall.transform.position = new Vector3(-22.5f, 0f, floorD / 2f - 4.0f);
        ApplyMaterial(lpStorageWall, darkOakMat);

        for (int lp = 0; lp < 30; lp++)
        {
            float lpx = -20f + (lp % 10) * 4.2f;
            float lpy = 1.5f + (lp / 10) * 4.0f;

            GameObject lpDisc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lpDisc.name = "Old_Analog_LP_Vinyl_Record_" + lp;
            lpDisc.transform.SetParent(lpStorageWall.transform);
            lpDisc.transform.position = new Vector3(lpx, lpy, floorD / 2f - 2.0f);
            lpDisc.transform.localScale = new Vector3(1.6f, 0.05f, 1.6f);
            lpDisc.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            lpDisc.GetComponent<Renderer>().sharedMaterial = lpVinylBlack;
        }

        ProBuilderMesh turntableDesk = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(12f, 0.85f, 4.5f));
        turntableDesk.gameObject.name = "Analog_LP_Mixing_Turntable_Desk";
        turntableDesk.transform.SetParent(rootObj.transform);
        turntableDesk.transform.position = new Vector3(-35f, 0f, 15f);
        ApplyMaterial(turntableDesk, darkOakMat);

        for (int tt = 0; tt < 2; tt++)
        {
            GameObject turntable = GameObject.CreatePrimitive(PrimitiveType.Cube);
            turntable.name = "Turntable_Player_" + tt;
            turntable.transform.SetParent(turntableDesk.transform);
            turntable.transform.position = new Vector3(-33f + tt * 6f, 0.9f, 17f);
            turntable.transform.localScale = new Vector3(2.5f, 0.2f, 2.2f);
            turntable.GetComponent<Renderer>().sharedMaterial = metalGearMat;

            GameObject record = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            record.name = "Turntable_Spinning_LP_" + tt;
            record.transform.SetParent(turntable.transform);
            record.transform.position = new Vector3(-33f + tt * 6f, 1.02f, 17f);
            record.transform.localScale = new Vector3(1.8f, 0.04f, 1.8f);
            record.GetComponent<Renderer>().sharedMaterial = lpVinylBlack;
        }

        for (int s = 0; s < 3; s++)
        {
            float sx = -38f + s * 14f;
            ProBuilderMesh sofa = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(8f, 1.2f, 3.5f));
            sofa.gameObject.name = "Studio_Cozy_Lounge_Sofa_" + s;
            sofa.transform.SetParent(rootObj.transform);
            sofa.transform.position = new Vector3(sx, 0f, -28f);
            ApplyMaterial(sofa, sofaMat);
        }

        for (int lamp = 0; lamp < 12; lamp++)
        {
            float lx = -42f + (lamp % 6) * 16f;
            float lz = (lamp < 6) ? 22f : -32f;

            GameObject lampPole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lampPole.name = "Standing_Floor_Lamp_Pole_" + lamp;
            lampPole.transform.SetParent(rootObj.transform);
            lampPole.transform.position = new Vector3(lx, 0f, lz);
            lampPole.transform.localScale = new Vector3(0.35f, 2.2f, 0.35f);
            lampPole.GetComponent<Renderer>().sharedMaterial = metalGearMat;

            GameObject lampShade = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lampShade.name = "Standing_Floor_Lamp_Glow_Shade_" + lamp;
            lampShade.transform.SetParent(lampPole.transform);
            lampShade.transform.position = new Vector3(lx, 4.2f, lz);
            lampShade.transform.localScale = new Vector3(1.4f, 0.6f, 1.4f);
            lampShade.GetComponent<Renderer>().sharedMaterial = lampWarmGlow;
        }

        Undo.RegisterCreatedObjectUndo(rootObj, "Create Central Broadcasting Studio");
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
