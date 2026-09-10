using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using System.Collections.Generic;

public class CreateAlgorithmStatisticsRoomProBuilder : EditorWindow
{
    [MenuItem("Tools/ProBuilder/Create Algorithm Statistics Room")]
    public static void CreateRoom()
    {
        GameObject rootObj = new GameObject("Algorithm_Statistics_Room");

        Material floorMat = CreateMaterial("Mat_Algo_Concrete_Floor", new Color(0.72f, 0.72f, 0.74f));
        Material wallMat = CreateMaterial("Mat_Algo_Modern_Wall", new Color(0.88f, 0.88f, 0.86f));
        Material ledGridMat = CreateEmissionMaterial("Mat_Algo_LED_Line", new Color(1.0f, 0.92f, 0.65f), 2.5f);
        Material lockerMat = CreateMaterial("Mat_Algo_Locker_Metal", new Color(0.82f, 0.82f, 0.8f));
        Material vendingMat = CreateMaterial("Mat_Algo_Vending_Body", new Color(0.3f, 0.32f, 0.35f));
        Material screenGlowMat = CreateEmissionMaterial("Mat_Algo_Screen_Dashboard", new Color(0.2f, 0.75f, 0.95f), 2.8f);
        Material deskMat = CreateMaterial("Mat_Algo_White_Desk", new Color(0.92f, 0.92f, 0.92f));
        Material chairMat = CreateMaterial("Mat_Algo_Office_Chair", new Color(0.35f, 0.35f, 0.38f));
        Material glassPartitionMat = CreateTransparentMaterial("Mat_Algo_Glass_Partition", new Color(0.6f, 0.85f, 0.9f, 0.35f));
        Material outdoorGardenMat = CreateMaterial("Mat_Algo_Outdoor_Garden", new Color(0.35f, 0.62f, 0.35f));

        float floorW = 100f;
        float floorD = 80f;
        float wallH = 20f;

        ProBuilderMesh floor = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW, 0.6f, floorD));
        floor.gameObject.name = "Floor_1F_Base_Concrete";
        floor.transform.SetParent(rootObj.transform);
        floor.transform.position = new Vector3(-floorW / 2f, -0.6f, -floorD / 2f);
        ApplyMaterial(floor, floorMat);

        for (int gx = -5; gx <= 5; gx++)
        {
            ProBuilderMesh ledH = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW - 4f, 0.03f, 0.25f));
            ledH.gameObject.name = "LED_Grid_Line_H_" + gx;
            ledH.transform.SetParent(rootObj.transform);
            ledH.transform.position = new Vector3(-floorW / 2f + 2f, 0.01f, gx * 7.0f);
            ApplyMaterial(ledH, ledGridMat);
        }

        for (int gz = -5; gz <= 5; gz++)
        {
            ProBuilderMesh ledV = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(0.25f, 0.03f, floorD - 4f));
            ledV.gameObject.name = "LED_Grid_Line_V_" + gz;
            ledV.transform.SetParent(rootObj.transform);
            ledV.transform.position = new Vector3(gz * 9.0f, 0.01f, -floorD / 2f + 2f);
            ApplyMaterial(ledV, ledGridMat);
        }

        ProBuilderMesh wallBack = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW, wallH, 1.0f));
        wallBack.gameObject.name = "Wall_Back_Concrete";
        wallBack.transform.SetParent(rootObj.transform);
        wallBack.transform.position = new Vector3(-floorW / 2f, 0f, floorD / 2f);
        ApplyMaterial(wallBack, wallMat);

        ProBuilderMesh wallWest = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(1.0f, wallH, floorD));
        wallWest.gameObject.name = "Wall_West_Concrete";
        wallWest.transform.SetParent(rootObj.transform);
        wallWest.transform.position = new Vector3(-floorW / 2f - 1.0f, 0f, -floorD / 2f);
        ApplyMaterial(wallWest, wallMat);

        ProBuilderMesh gardenDoorFrame = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(1.0f, 12f, 40f));
        gardenDoorFrame.gameObject.name = "Sliding_Glass_Garden_Door_Frame";
        gardenDoorFrame.transform.SetParent(rootObj.transform);
        gardenDoorFrame.transform.position = new Vector3(floorW / 2f - 0.5f, 0f, -20f);
        ApplyMaterial(gardenDoorFrame, lockerMat);

        ProBuilderMesh gardenDoorGlass = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(0.2f, 11.5f, 39.5f));
        gardenDoorGlass.gameObject.name = "Sliding_Glass_Garden_Door_Glass";
        gardenDoorGlass.transform.SetParent(rootObj.transform);
        gardenDoorGlass.transform.position = new Vector3(floorW / 2f - 0.1f, 0.2f, -19.75f);
        ApplyMaterial(gardenDoorGlass, glassPartitionMat);

        ProBuilderMesh gardenOutside = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(30f, 0.3f, 50f));
        gardenOutside.gameObject.name = "Outdoor_Terrace_Garden_View";
        gardenOutside.transform.SetParent(rootObj.transform);
        gardenOutside.transform.position = new Vector3(floorW / 2f + 0.5f, -0.3f, -25f);
        ApplyMaterial(gardenOutside, outdoorGardenMat);

        for (int v = 0; v < 8; v++)
        {
            float vx = -42f + v * 5.5f;
            ProBuilderMesh vending = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(4.5f, 7.5f, 2.5f));
            vending.gameObject.name = "Grand_Vending_Machine_" + v;
            vending.transform.SetParent(rootObj.transform);
            vending.transform.position = new Vector3(vx, 0f, floorD / 2f - 3.0f);
            ApplyMaterial(vending, vendingMat);

            ProBuilderMesh screen = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(3.8f, 4.5f, 0.2f));
            screen.gameObject.name = "Vending_UI_Screen_" + v;
            screen.transform.SetParent(vending.transform);
            screen.transform.position = new Vector3(vx + 0.35f, 2.2f, floorD / 2f - 1.7f);
            ApplyMaterial(screen, screenGlowMat);
        }

        for (int l = 0; l < 24; l++)
        {
            float lx = 2f + l * 1.8f;
            ProBuilderMesh locker = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(1.7f, 8.0f, 2.2f));
            locker.gameObject.name = "Campus_Locker_Column_" + l;
            locker.transform.SetParent(rootObj.transform);
            locker.transform.position = new Vector3(lx, 0f, floorD / 2f - 2.8f);
            ApplyMaterial(locker, lockerMat);
        }

        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                float dx = -40f + col * 18f;
                float dz = 15f - row * 16f;

                ProBuilderMesh deskGroup = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(14f, 0.85f, 6.5f));
                deskGroup.gameObject.name = "Group_Collaboration_Desk_" + row + "_" + col;
                deskGroup.transform.SetParent(rootObj.transform);
                deskGroup.transform.position = new Vector3(dx, 0f, dz);
                ApplyMaterial(deskGroup, deskMat);

                for (int ch = 0; ch < 6; ch++)
                {
                    float chX = dx + 1f + (ch % 3) * 4.5f;
                    float chZ = dz + (ch < 3 ? 0.8f : 5.0f);
                    ProBuilderMesh chair = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(1.4f, 1.8f, 1.4f));
                    chair.gameObject.name = "Swivel_Chair_" + row + "_" + col + "_" + ch;
                    chair.transform.SetParent(rootObj.transform);
                    chair.transform.position = new Vector3(chX, 0f, chZ);
                    ApplyMaterial(chair, chairMat);
                }
            }
        }

        ProBuilderMesh floor2F = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(45f, 0.5f, 40f));
        floor2F.gameObject.name = "Floor_2F_Cubicle_Tower";
        floor2F.transform.SetParent(rootObj.transform);
        floor2F.transform.position = new Vector3(3f, 6.5f, -25f);
        ApplyMaterial(floor2F, deskMat);

        for (int cub = 0; cub < 4; cub++)
        {
            float cubZ = -5.0f - cub * 9.0f;

            ProBuilderMesh partition = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(35f, 3.5f, 0.3f));
            partition.gameObject.name = "Glass_Partition_Cubicle_" + cub;
            partition.transform.SetParent(rootObj.transform);
            partition.transform.position = new Vector3(8f, 6.5f, cubZ);
            ApplyMaterial(partition, glassPartitionMat);

            for (int m = 0; m < 4; m++)
            {
                GameObject monitor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                monitor.name = "Statistics_Dashboard_Monitor_" + cub + "_" + m;
                monitor.transform.SetParent(rootObj.transform);
                monitor.transform.position = new Vector3(10f + m * 8f, 8.2f, cubZ - 1.2f);
                monitor.transform.localScale = new Vector3(3.5f, 2.0f, 0.15f);
                monitor.GetComponent<Renderer>().sharedMaterial = screenGlowMat;
            }
        }

        Undo.RegisterCreatedObjectUndo(rootObj, "Create Algorithm Statistics Room");
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
