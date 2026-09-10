using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using System.Collections.Generic;

public class CreateChronologyArchivesProBuilder : EditorWindow
{
    [MenuItem("Tools/ProBuilder/Create Chronology Archives")]
    public static void CreateArchives()
    {
        GameObject rootObj = new GameObject("Chronology_Archives");

        Material woodFloorMat = CreateMaterial("Mat_Chrono_Floor_Wood", new Color(0.48f, 0.32f, 0.22f));
        Material redCarpetMat = CreateMaterial("Mat_Chrono_Red_Carpet", new Color(0.65f, 0.18f, 0.18f));
        Material antiqueWoodMat = CreateMaterial("Mat_Chrono_Antique_Wood", new Color(0.35f, 0.22f, 0.14f));
        Material wallPanelMat = CreateMaterial("Mat_Chrono_Wall_Panel", new Color(0.42f, 0.28f, 0.18f));
        Material goldFrameMat = CreateEmissionMaterial("Mat_Chrono_Antique_Gold_Frame", new Color(0.92f, 0.78f, 0.35f), 1.5f);
        Material portraitCanvasMat = CreateMaterial("Mat_Chrono_Portrait_Canvas", new Color(0.78f, 0.72f, 0.62f));
        Material timelineGlowCyan = CreateEmissionMaterial("Mat_Chrono_Timeline_Cyan", new Color(0.25f, 0.88f, 0.95f), 3.2f);
        Material timelineGlowGold = CreateEmissionMaterial("Mat_Chrono_Timeline_Gold", new Color(1.0f, 0.82f, 0.25f), 3.2f);

        float floorW = 100f;
        float floorD = 80f;
        float wallH = 20f;

        ProBuilderMesh floor = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW, 0.6f, floorD));
        floor.gameObject.name = "Floor_1F_Base_Wood";
        floor.transform.SetParent(rootObj.transform);
        floor.transform.position = new Vector3(-floorW / 2f, -0.6f, -floorD / 2f);
        ApplyMaterial(floor, woodFloorMat);

        ProBuilderMesh carpetRunner = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(90f, 0.05f, 25f));
        carpetRunner.gameObject.name = "Red_Carpet_Runner_Main_Corridor";
        carpetRunner.transform.SetParent(rootObj.transform);
        carpetRunner.transform.position = new Vector3(-45f, 0.01f, -12.5f);
        ApplyMaterial(carpetRunner, redCarpetMat);

        ProBuilderMesh wallNorth = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(floorW, wallH, 1.0f));
        wallNorth.gameObject.name = "Wall_North_Gallery_Panels";
        wallNorth.transform.SetParent(rootObj.transform);
        wallNorth.transform.position = new Vector3(-floorW / 2f, 0f, floorD / 2f);
        ApplyMaterial(wallNorth, wallPanelMat);

        ProBuilderMesh wallWest = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(1.0f, wallH, floorD));
        wallWest.gameObject.name = "Wall_West_Gallery_Panels";
        wallWest.transform.SetParent(rootObj.transform);
        wallWest.transform.position = new Vector3(-floorW / 2f - 1.0f, 0f, -floorD / 2f);
        ApplyMaterial(wallWest, wallPanelMat);

        ProBuilderMesh gallery2F = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(85f, 0.6f, 20f));
        gallery2F.gameObject.name = "Gallery_2F_Upper_Corridor";
        gallery2F.transform.SetParent(rootObj.transform);
        gallery2F.transform.position = new Vector3(-42.5f, 7.5f, 18f);
        ApplyMaterial(gallery2F, woodFloorMat);

        ProBuilderMesh carpetRunner2F = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(80f, 0.05f, 14f));
        carpetRunner2F.gameObject.name = "Red_Carpet_Runner_2F";
        carpetRunner2F.transform.SetParent(gallery2F.transform);
        carpetRunner2F.transform.position = new Vector3(-40f, 8.12f, 21f);
        ApplyMaterial(carpetRunner2F, redCarpetMat);

        int stairSteps = 18;
        float stepW = 7.0f;
        float stepH = 7.5f / stairSteps;
        float stepD = 16.0f / stairSteps;
        for (int st = 0; st < stairSteps; st++)
        {
            ProBuilderMesh stairStep = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(stepW, stepH, stepD));
            stairStep.gameObject.name = "Grand_Gallery_Stair_Step_" + st;
            stairStep.transform.SetParent(rootObj.transform);
            stairStep.transform.position = new Vector3(35f, stepH * st, -15f + (st * stepD));
            ApplyMaterial(stairStep, antiqueWoodMat);
        }

        for (int f = 0; f < 20; f++)
        {
            float fx = -44f + (f % 10) * 8.8f;
            float fy = (f < 10) ? 2.5f : 10.0f;

            ProBuilderMesh frameBorder = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(5.5f, 4.2f, 0.2f));
            frameBorder.gameObject.name = "Antique_Gold_Portrait_Frame_" + f;
            frameBorder.transform.SetParent(rootObj.transform);
            frameBorder.transform.position = new Vector3(fx, fy, floorD / 2f - 0.4f);
            ApplyMaterial(frameBorder, goldFrameMat);

            ProBuilderMesh canvas = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(4.8f, 3.5f, 0.05f));
            canvas.gameObject.name = "Portrait_Canvas_" + f;
            canvas.transform.SetParent(frameBorder.transform);
            canvas.transform.position = new Vector3(fx + 0.35f, fy + 0.35f, floorD / 2f - 0.45f);
            ApplyMaterial(canvas, portraitCanvasMat);
        }

        for (int t = 0; t < 60; t++)
        {
            float tx = -46f + (t % 30) * 3.0f;
            float ty = 4.5f + Mathf.Sin(t * 0.4f) * 1.8f + ((t >= 30) ? 7.5f : 0f);

            ProBuilderMesh timelineSeg = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(2.8f, 0.15f, 0.15f));
            timelineSeg.gameObject.name = "Glowing_Timeline_Strand_Segment_" + t;
            timelineSeg.transform.SetParent(rootObj.transform);
            timelineSeg.transform.position = new Vector3(tx, ty, floorD / 2f - 0.6f);
            ApplyMaterial(timelineSeg, (t % 2 == 0) ? timelineGlowGold : timelineGlowCyan);

            if (t % 5 == 0)
            {
                ProBuilderMesh dataFragment = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(0.8f, 0.6f, 0.1f));
                dataFragment.gameObject.name = "Timeline_Data_Fragment_" + t;
                dataFragment.transform.SetParent(timelineSeg.transform);
                dataFragment.transform.position = new Vector3(tx + 1.0f, ty + 0.3f, floorD / 2f - 0.65f);
                ApplyMaterial(dataFragment, timelineGlowCyan);
            }
        }

        for (int b = 0; b < 6; b++)
        {
            float bx = -38f + b * 14f;
            ProBuilderMesh bookcase = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(10f, 6.0f, 2.2f));
            bookcase.gameObject.name = "Recessed_Chronology_Bookcase_" + b;
            bookcase.transform.SetParent(rootObj.transform);
            bookcase.transform.position = new Vector3(bx, 0f, -28f);
            ApplyMaterial(bookcase, antiqueWoodMat);

            ProBuilderMesh lectern = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(2.5f, 1.1f, 2.0f));
            lectern.gameObject.name = "Research_Lectern_Desk_" + b;
            lectern.transform.SetParent(rootObj.transform);
            lectern.transform.position = new Vector3(bx + 3.8f, 0f, -22f);
            ApplyMaterial(lectern, antiqueWoodMat);
        }

        string[] concepts = new string[] { "앤틱 골드", "앤틱 목재", "빛나는 연표" };
        Material[] cubeMats = new Material[] { goldFrameMat, antiqueWoodMat, timelineGlowCyan };
        for (int c = 0; c < 6; c++)
        {
            float cx = -25f + c * 10f;
            float cy = 3.5f + (c % 2) * 1.2f;
            float cz = -10f;

            GameObject conceptCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            conceptCube.name = "Archives_Concept_Cube_" + concepts[c % 3] + "_" + c;
            conceptCube.transform.SetParent(rootObj.transform);
            conceptCube.transform.position = new Vector3(cx, cy, cz);
            conceptCube.transform.localScale = Vector3.one * 1.8f;
            conceptCube.transform.rotation = Quaternion.Euler(15f, c * 35f, 10f);
            conceptCube.GetComponent<Renderer>().sharedMaterial = cubeMats[c % 3];
        }

        Undo.RegisterCreatedObjectUndo(rootObj, "Create Chronology Archives");
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
