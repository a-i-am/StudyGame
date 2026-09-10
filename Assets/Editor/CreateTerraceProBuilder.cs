using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using System.Collections.Generic;

public class CreateTerraceProBuilder : EditorWindow
{
    [MenuItem("Tools/ProBuilder/Create L-Terrace with Stairs")]
    public static void CreateTerrace()
    {
        GameObject rootObj = new GameObject("L_Terrace_Diorama");

        Material stoneMat = CreateSimpleMaterial("Mat_Stone_Diorama", new Color(0.8f, 0.78f, 0.74f));
        Material woodMat = CreateSimpleMaterial("Mat_Wood_Diorama", new Color(0.62f, 0.42f, 0.24f));

        ProBuilderMesh floorMesh = ProBuilderMesh.Create();
        floorMesh.gameObject.name = "Floor_L_Shape";
        floorMesh.transform.SetParent(rootObj.transform);

        Vector3[] floorVertices = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(10, 0, 0),
            new Vector3(10, 0, 4),
            new Vector3(4, 0, 4),
            new Vector3(4, 0, 10),
            new Vector3(0, 0, 10)
        };

        floorMesh.CreateShapeFromPolygon(floorVertices, 0.3f, false);
        ApplyMaterial(floorMesh, stoneMat);

        ProBuilderMesh rightWall = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(0.5f, 4.0f, 4.0f));
        rightWall.gameObject.name = "Wall_Right";
        rightWall.transform.SetParent(rootObj.transform);
        rightWall.transform.position = new Vector3(9.5f, 0f, 0f);
        ApplyMaterial(rightWall, stoneMat);

        ProBuilderMesh backWall = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(4.0f, 4.0f, 0.5f));
        backWall.gameObject.name = "Wall_Back";
        backWall.transform.SetParent(rootObj.transform);
        backWall.transform.position = new Vector3(0f, 0f, 9.5f);
        ApplyMaterial(backWall, stoneMat);

        ProBuilderMesh terracePlatform = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(2.5f, 0.3f, 3.5f));
        terracePlatform.gameObject.name = "Terrace_Platform";
        terracePlatform.transform.SetParent(rootObj.transform);
        terracePlatform.transform.position = new Vector3(7.0f, 2.0f, 0.5f);
        ApplyMaterial(terracePlatform, woodMat);

        int stepCount = 6;
        float stairWidth = 1.8f;
        float totalHeight = 2.0f;
        float totalDepth = 3.0f;
        float stepHeight = totalHeight / stepCount;
        float stepDepth = totalDepth / stepCount;

        for (int i = 0; i < stepCount; i++)
        {
            ProBuilderMesh step = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(stairWidth, stepHeight, stepDepth));
            step.gameObject.name = "Stair_Step_" + (i + 1);
            step.transform.SetParent(rootObj.transform);
            step.transform.position = new Vector3(5.2f, stepHeight * i, 0.5f + (i * stepDepth));
            ApplyMaterial(step, woodMat);
        }

        ProBuilderMesh railingBar = ShapeGenerator.GenerateCube(PivotLocation.FirstVertex, new Vector3(0.1f, 0.8f, 3.5f));
        railingBar.gameObject.name = "Terrace_Railing";
        railingBar.transform.SetParent(rootObj.transform);
        railingBar.transform.position = new Vector3(7.0f, 2.3f, 0.5f);
        ApplyMaterial(railingBar, woodMat);

        Undo.RegisterCreatedObjectUndo(rootObj, "Create L-Terrace Diorama");
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

    private static Material CreateSimpleMaterial(string name, Color color)
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
}
