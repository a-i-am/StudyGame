using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using System.Collections.Generic;

public class CreateCartographyObservatoryProBuilder : EditorWindow
{
    [MenuItem("Tools/ProBuilder/Create Cartography Observatory")]
    public static void CreateObservatory()
    {
        GameObject rootObj = new GameObject("Cartography_Observatory");

        Material floorWoodMat = CreateMaterial("Mat_Cart_Floor_Wood", new Color(0.55f, 0.38f, 0.22f));
        Material darkOakMat = CreateMaterial("Mat_Cart_Dark_Oak", new Color(0.35f, 0.22f, 0.14f));
        Material wallMat = CreateMaterial("Mat_Cart_Observatory_Wall", new Color(0.38f, 0.32f, 0.38f));
        Material domeStarMat = CreateEmissionMaterial("Mat_Cart_Constellation_Dome", new Color(0.15f, 0.25f, 0.55f), 2.5f);
        Material globeGlowMat = CreateEmissionMaterial("Mat_Cart_Hologram_Globe", new Color(0.25f, 0.85f, 0.95f), 3.5f);
        Material brassMat = CreateEmissionMaterial("Mat_Cart_Ancient_Brass", new Color(0.9f, 0.75f, 0.32f), 1.5f);
        Material paperMapMat = CreateMaterial("Mat_Cart_Paper_Map", new Color(0.92f, 0.85f, 0.68f));

        Material cubeExplore = CreateEmissionMaterial("Mat_Cart_Cube_Explore", new Color(0.2f, 0.7f, 0.95f), 2.5f);
        Material cubeNav = CreateEmissionMaterial("Mat_Cart_Cube_Nav", new Color(0.95f, 0.75f, 0.25f), 2.5f);
        Material cubeDiscover = CreateEmissionMaterial("Mat_Cart_Cube_Discover", new Color(0.75f, 0.35f, 0.85f), 2.5f);
        Material cubeKnowledge = CreateEmissionMaterial("Mat_Cart_Cube_Knowledge", new Color(0.35f, 0.85f, 0.45f), 2.5f);

        float floorW = 100f;
        float floorD = 80f;
        float wallH = 22f;

        ProBuilderMesh floor = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW, 0.6f, floorD));
        floor.gameObject.name = "Floor_1F_Base_Wood";
        floor.transform.SetParent(rootObj.transform);
        floor.transform.position = new Vector3(-floorW / 2f, -0.6f, -floorD / 2f);
        ApplyMaterial(floor, floorWoodMat);

        ProBuilderMesh wallWest = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(1.0f, wallH, floorD));
        wallWest.gameObject.name = "Wall_West_Observatory";
        wallWest.transform.SetParent(rootObj.transform);
        wallWest.transform.position = new Vector3(-floorW / 2f - 1.0f, 0f, -floorD / 2f);
        ApplyMaterial(wallWest, wallMat);

        ProBuilderMesh wallSouth = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW, wallH, 1.0f));
        wallSouth.gameObject.name = "Wall_South_Observatory";
        wallSouth.transform.SetParent(rootObj.transform);
        wallSouth.transform.position = new Vector3(-floorW / 2f, 0f, -floorD / 2f - 1.0f);
        ApplyMaterial(wallSouth, wallMat);

        ProBuilderMesh domeCeiling = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW, 0.3f, floorD));
        domeCeiling.gameObject.name = "Constellation_Starry_Dome_Ceiling";
        domeCeiling.transform.SetParent(rootObj.transform);
        domeCeiling.transform.position = new Vector3(-floorW / 2f, wallH, -floorD / 2f);
        ApplyMaterial(domeCeiling, domeStarMat);

        ProBuilderMesh gallery2F = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(80f, 0.5f, 15f));
        gallery2F.gameObject.name = "Gallery_2F_Star_Observation_Balcony";
        gallery2F.transform.SetParent(rootObj.transform);
        gallery2F.transform.position = new Vector3(-40f, 7.5f, 20f);
        ApplyMaterial(gallery2F, darkOakMat);

        GameObject mainHologramGlobe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mainHologramGlobe.name = "Giant_Suspended_Hologram_Globe";
        mainHologramGlobe.transform.SetParent(rootObj.transform);
        mainHologramGlobe.transform.position = new Vector3(0f, 13f, 0f);
        mainHologramGlobe.transform.localScale = Vector3.one * 14.0f;
        mainHologramGlobe.GetComponent<Renderer>().sharedMaterial = globeGlowMat;

        ProBuilderMesh pedestal = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(12f, 1.2f, 12f));
        pedestal.gameObject.name = "Hologram_Globe_Projection_Pedestal";
        pedestal.transform.SetParent(rootObj.transform);
        pedestal.transform.position = new Vector3(-6f, 0f, -6f);
        ApplyMaterial(pedestal, darkOakMat);

        GameObject projectorLens = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        projectorLens.name = "Globe_Projector_Lens";
        projectorLens.transform.SetParent(pedestal.transform);
        projectorLens.transform.position = new Vector3(0f, 1.3f, 0f);
        projectorLens.transform.localScale = new Vector3(6f, 0.4f, 6f);
        projectorLens.GetComponent<Renderer>().sharedMaterial = brassMat;

        for (int t = 0; t < 6; t++)
        {
            float angle = t * (360f / 6f) * Mathf.Deg2Rad;
            float tx = Mathf.Cos(angle) * 25f;
            float tz = Mathf.Sin(angle) * 22f;

            ProBuilderMesh mapTable = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(8f, 0.9f, 8f));
            mapTable.gameObject.name = "Circular_Cartography_Map_Table_" + t;
            mapTable.transform.SetParent(rootObj.transform);
            mapTable.transform.position = new Vector3(tx - 4f, 0f, tz - 4f);
            ApplyMaterial(mapTable, darkOakMat);

            ProBuilderMesh mapSheet = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(6.5f, 0.04f, 5.5f));
            mapSheet.gameObject.name = "Ancient_Star_Chart_Map_" + t;
            mapSheet.transform.SetParent(mapTable.transform);
            mapSheet.transform.position = new Vector3(tx - 3.25f, 0.92f, tz - 2.75f);
            ApplyMaterial(mapSheet, paperMapMat);

            GameObject compass = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            compass.name = "Ancient_Astrolabe_Compass_" + t;
            compass.transform.SetParent(mapTable.transform);
            compass.transform.position = new Vector3(tx + 2f, 1.1f, tz + 2f);
            compass.transform.localScale = new Vector3(1.2f, 0.25f, 1.2f);
            compass.GetComponent<Renderer>().sharedMaterial = brassMat;
        }

        for (int s = 0; s < 6; s++)
        {
            float sx = -42f + s * 16f;
            ProBuilderMesh bookcase = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(12f, 8.5f, 2.8f));
            bookcase.gameObject.name = "Constellation_Bookcase_Archive_" + s;
            bookcase.transform.SetParent(rootObj.transform);
            bookcase.transform.position = new Vector3(sx, 0f, floorD / 2f - 3.5f);
            ApplyMaterial(bookcase, darkOakMat);
        }

        for (int st = 0; st < 80; st++)
        {
            float sx = -45f + (st % 10) * 10f + Mathf.Sin(st) * 2f;
            float sy = 18.5f + (st % 3) * 0.8f;
            float sz = -35f + (st / 10) * 9f + Mathf.Cos(st) * 2f;

            GameObject starLight = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            starLight.name = "Dome_Constellation_Star_Light_" + st;
            starLight.transform.SetParent(rootObj.transform);
            starLight.transform.position = new Vector3(sx, sy, sz);
            starLight.transform.localScale = Vector3.one * (0.35f + (st % 3) * 0.15f);
            starLight.GetComponent<Renderer>().sharedMaterial = (st % 2 == 0) ? globeGlowMat : brassMat;
        }

        string[] concepts = new string[] { "탐험", "항해", "발견", "지식" };
        Material[] cubeMats = new Material[] { cubeExplore, cubeNav, cubeDiscover, cubeKnowledge };
        for (int c = 0; c < 8; c++)
        {
            float cx = -30f + c * 8.5f;
            float cy = 3.5f + (c % 3) * 0.8f;
            float cz = -28f + (c % 2) * 12f;

            GameObject conceptCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            conceptCube.name = "Observatory_Concept_Cube_" + concepts[c % 4] + "_" + c;
            conceptCube.transform.SetParent(rootObj.transform);
            conceptCube.transform.position = new Vector3(cx, cy, cz);
            conceptCube.transform.localScale = Vector3.one * 1.8f;
            conceptCube.transform.rotation = Quaternion.Euler(20f, c * 30f, 15f);
            conceptCube.GetComponent<Renderer>().sharedMaterial = cubeMats[c % 4];
        }

        Undo.RegisterCreatedObjectUndo(rootObj, "Create Cartography Observatory");
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
