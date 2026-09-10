using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using System.Collections.Generic;

public class CreateLanguageLabProBuilder : EditorWindow
{
    [MenuItem("Tools/ProBuilder/Create Language Lab")]
    public static void CreateLab()
    {
        GameObject rootObj = new GameObject("Language_Multimedia_Lab");

        Material floorMat = CreateMaterial("Mat_Lang_Wood_Floor", new Color(0.82f, 0.65f, 0.45f));
        Material wallMat = CreateMaterial("Mat_Lang_Warm_Wall", new Color(0.95f, 0.92f, 0.82f));
        Material windowFrameMat = CreateMaterial("Mat_Lang_Window_Frame", new Color(0.88f, 0.75f, 0.58f));
        Material windowGlassMat = CreateTransparentMaterial("Mat_Lang_Glass", new Color(1.0f, 0.95f, 0.8f, 0.35f));

        Material boothPastelPink = CreateMaterial("Mat_Lang_Booth_Pink", new Color(0.95f, 0.72f, 0.78f));
        Material boothPastelMint = CreateMaterial("Mat_Lang_Booth_Mint", new Color(0.68f, 0.88f, 0.78f));
        Material boothPastelYellow = CreateMaterial("Mat_Lang_Booth_Yellow", new Color(0.96f, 0.88f, 0.62f));
        Material boothPastelBlue = CreateMaterial("Mat_Lang_Booth_Blue", new Color(0.68f, 0.82f, 0.92f));

        Material deskWoodMat = CreateMaterial("Mat_Lang_Desk_Wood", new Color(0.9f, 0.85f, 0.75f));
        Material metalGearMat = CreateMaterial("Mat_Lang_Gear_Metal", new Color(0.3f, 0.32f, 0.35f));
        Material waveformScreenMat = CreateEmissionMaterial("Mat_Lang_Waveform_Screen", new Color(0.2f, 0.85f, 0.65f), 2.5f);
        Material headphonePastel = CreateMaterial("Mat_Lang_Headphone_Color", new Color(0.9f, 0.55f, 0.65f));
        Material micGlowMat = CreateEmissionMaterial("Mat_Lang_Mic_Glow", new Color(0.95f, 0.35f, 0.35f), 2.0f);

        Material cubeComm = CreateEmissionMaterial("Mat_Lang_Cube_Comm", new Color(0.35f, 0.75f, 0.95f), 2.5f);
        Material cubeExpress = CreateEmissionMaterial("Mat_Lang_Cube_Express", new Color(0.95f, 0.65f, 0.35f), 2.5f);

        float floorW = 100f;
        float floorD = 80f;
        float wallH = 20f;

        ProBuilderMesh floor = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW, 0.6f, floorD));
        floor.gameObject.name = "Floor_1F_Base_Wood";
        floor.transform.SetParent(rootObj.transform);
        floor.transform.position = new Vector3(-floorW / 2f, -0.6f, -floorD / 2f);
        ApplyMaterial(floor, floorMat);

        ProBuilderMesh wallBack = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW, wallH, 1.0f));
        wallBack.gameObject.name = "Wall_Back_Warm";
        wallBack.transform.SetParent(rootObj.transform);
        wallBack.transform.position = new Vector3(-floorW / 2f, 0f, floorD / 2f);
        ApplyMaterial(wallBack, wallMat);

        ProBuilderMesh wallWest = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(1.0f, wallH, floorD));
        wallWest.gameObject.name = "Wall_West_Warm";
        wallWest.transform.SetParent(rootObj.transform);
        wallWest.transform.position = new Vector3(-floorW / 2f - 1.0f, 0f, -floorD / 2f);
        ApplyMaterial(wallWest, wallMat);

        ProBuilderMesh gridWindowFrame = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(1.0f, 15f, 50f));
        gridWindowFrame.gameObject.name = "Grand_Grid_Window_Frame";
        gridWindowFrame.transform.SetParent(rootObj.transform);
        gridWindowFrame.transform.position = new Vector3(floorW / 2f - 0.5f, 2f, -25f);
        ApplyMaterial(gridWindowFrame, windowFrameMat);

        ProBuilderMesh gridWindowGlass = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(0.2f, 14.5f, 49.5f));
        gridWindowGlass.gameObject.name = "Grand_Grid_Window_Glass";
        gridWindowGlass.transform.SetParent(rootObj.transform);
        gridWindowGlass.transform.position = new Vector3(floorW / 2f - 0.1f, 2.2f, -24.75f);
        ApplyMaterial(gridWindowGlass, windowGlassMat);

        ProBuilderMesh mezzanine2F = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(80f, 0.5f, 30f));
        mezzanine2F.gameObject.name = "Floor_2F_Mezzanine_Booths";
        mezzanine2F.transform.SetParent(rootObj.transform);
        mezzanine2F.transform.position = new Vector3(-40f, 6.5f, 8f);
        ApplyMaterial(mezzanine2F, floorMat);

        Material[] pastelMats = new Material[] { boothPastelPink, boothPastelMint, boothPastelYellow, boothPastelBlue };

        int rows = 4;
        int cols = 6;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float bx = -38f + c * 13f;
                float bz = 25f - r * 14f;
                float by = (r < 2) ? 6.5f : 0f;

                Material boothColor = pastelMats[(r + c) % pastelMats.Length];

                ProBuilderMesh dividerL = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(0.3f, 3.2f, 5.0f));
                dividerL.gameObject.name = "Booth_Divider_L_" + r + "_" + c;
                dividerL.transform.SetParent(rootObj.transform);
                dividerL.transform.position = new Vector3(bx - 3.5f, by, bz - 2.5f);
                ApplyMaterial(dividerL, boothColor);

                ProBuilderMesh dividerR = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(0.3f, 3.2f, 5.0f));
                dividerR.gameObject.name = "Booth_Divider_R_" + r + "_" + c;
                dividerR.transform.SetParent(rootObj.transform);
                dividerR.transform.position = new Vector3(bx + 3.5f, by, bz - 2.5f);
                ApplyMaterial(dividerR, boothColor);

                ProBuilderMesh deskSurface = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(6.7f, 0.85f, 4.0f));
                deskSurface.gameObject.name = "Booth_Desk_Surface_" + r + "_" + c;
                deskSurface.transform.SetParent(rootObj.transform);
                deskSurface.transform.position = new Vector3(bx - 3.35f, by, bz - 2.0f);
                ApplyMaterial(deskSurface, deskWoodMat);

                CreateBoothRecordingGear(rootObj.transform, new Vector3(bx, by + 0.85f, bz), waveformScreenMat, metalGearMat, headphonePastel, micGlowMat);

                ProBuilderMesh chair = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(2.0f, 2.5f, 2.0f));
                chair.gameObject.name = "Booth_Swivel_Chair_" + r + "_" + c;
                chair.transform.SetParent(rootObj.transform);
                chair.transform.position = new Vector3(bx - 1.0f, by, bz - 4.2f);
                ApplyMaterial(chair, boothColor);
            }
        }

        ProBuilderMesh teacherDesk = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(12f, 1.1f, 5.5f));
        teacherDesk.gameObject.name = "Grand_Instructor_Control_Center";
        teacherDesk.transform.SetParent(rootObj.transform);
        teacherDesk.transform.position = new Vector3(-35f, 0f, -32f);
        ApplyMaterial(teacherDesk, deskWoodMat);

        GameObject globe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        globe.name = "Grand_Instructor_Globe";
        globe.transform.SetParent(teacherDesk.transform);
        globe.transform.position = new Vector3(-38f, 1.8f, -30f);
        globe.transform.localScale = Vector3.one * 1.6f;
        globe.GetComponent<Renderer>().sharedMaterial = boothPastelBlue;

        string[] commConcepts = new string[] { "소통", "이해", "표현" };
        for (int cc = 0; cc < 8; cc++)
        {
            float cx = 25f + (cc % 2) * 9f;
            float cz = -25f + (cc / 2) * 15f;

            GameObject conceptCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            conceptCube.name = "Grand_Language_Concept_Cube_" + commConcepts[cc % 3] + "_" + cc;
            conceptCube.transform.SetParent(rootObj.transform);
            conceptCube.transform.position = new Vector3(cx, 3.5f + cc * 0.8f, cz);
            conceptCube.transform.localScale = Vector3.one * 1.8f;
            conceptCube.transform.rotation = Quaternion.Euler(20f, cc * 30f, 15f);
            conceptCube.GetComponent<Renderer>().sharedMaterial = (cc % 2 == 0) ? cubeComm : cubeExpress;
        }

        Undo.RegisterCreatedObjectUndo(rootObj, "Create Language Lab");
        Selection.activeGameObject = rootObj;
    }

    private static void CreateBoothRecordingGear(Transform parent, Vector3 center, Material screenMat, Material metalMat, Material headphoneMat, Material micMat)
    {
        GameObject pcMonitor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pcMonitor.name = "Recording_Waveform_Monitor";
        pcMonitor.transform.SetParent(parent);
        pcMonitor.transform.position = center + new Vector3(0f, 1.1f, 0.8f);
        pcMonitor.transform.localScale = new Vector3(2.5f, 1.5f, 0.15f);
        pcMonitor.GetComponent<Renderer>().sharedMaterial = screenMat;

        GameObject headphone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        headphone.name = "Pastel_Headset";
        headphone.transform.SetParent(parent);
        headphone.transform.position = center + new Vector3(-1.8f, 0.35f, -0.2f);
        headphone.transform.localScale = new Vector3(0.6f, 0.18f, 0.6f);
        headphone.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
        headphone.GetComponent<Renderer>().sharedMaterial = headphoneMat;

        GameObject micStand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        micStand.name = "Desktop_Recording_Mic";
        micStand.transform.SetParent(parent);
        micStand.transform.position = center + new Vector3(1.5f, 0.6f, 0.2f);
        micStand.transform.localScale = new Vector3(0.2f, 0.6f, 0.2f);
        micStand.GetComponent<Renderer>().sharedMaterial = metalMat;

        GameObject micHead = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        micHead.name = "Mic_Cap_Glow";
        micHead.transform.SetParent(micStand.transform);
        micHead.transform.position = center + new Vector3(1.5f, 1.2f, 0.2f);
        micHead.transform.localScale = Vector3.one * 0.4f;
        micHead.GetComponent<Renderer>().sharedMaterial = micMat;
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
