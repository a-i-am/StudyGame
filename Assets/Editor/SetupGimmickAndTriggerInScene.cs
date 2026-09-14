using UnityEngine;
using UnityEditor;
using System.IO;

public class SetupGimmickAndTriggerInScene
{
    [MenuItem("Tools/Setup Field Trigger and Math Gimmick Scene Objects")]
    public static void SetupSceneObjects()
    {
        EnsureAssetsExist();

        if (Object.FindFirstObjectByType<StageManager>() == null)
        {
            GameObject stageManagerObj = new GameObject("StageManager");
            stageManagerObj.AddComponent<StageManager>();
            Undo.RegisterCreatedObjectUndo(stageManagerObj, "Create StageManager");
        }

        MathGimmickProfile profileAsset = AssetDatabase.LoadAssetAtPath<MathGimmickProfile>("Assets/Data/DerivativeShatterGimmickProfile.asset");
        SequenceNode sequenceNodeAsset = AssetDatabase.LoadAssetAtPath<SequenceNode>("Assets/Data/SampleSequenceNode.asset");
        GameObject fractionPrefabObj = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/CubeFraction.prefab");

        GameObject triggerObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        triggerObj.name = "FieldTriggerZone";
        triggerObj.transform.position = new Vector3(0f, 1f, 15f);
        triggerObj.transform.localScale = new Vector3(3f, 3f, 1f);

        BoxCollider triggerCollider = triggerObj.GetComponent<BoxCollider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }

        MeshRenderer triggerRenderer = triggerObj.GetComponent<MeshRenderer>();
        if (triggerRenderer != null)
        {
            triggerRenderer.enabled = false;
        }

        FieldTrigger fieldTriggerComponent = triggerObj.AddComponent<FieldTrigger>();
        fieldTriggerComponent.targetNode = sequenceNodeAsset;

        GameObject giantBlockObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        giantBlockObj.name = "Gimmick_GiantBlock";
        giantBlockObj.transform.position = new Vector3(0f, 1.5f, 8f);
        giantBlockObj.transform.localScale = new Vector3(3f, 3f, 3f);

        MathGimmick mathGimmickComponent = giantBlockObj.AddComponent<MathGimmick>();
        Rigidbody rb = giantBlockObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        mathGimmickComponent.profile = profileAsset;
        mathGimmickComponent.fractionPrefab = fractionPrefabObj;

        Undo.RegisterCreatedObjectUndo(triggerObj, "Create Field Trigger Zone");
        Undo.RegisterCreatedObjectUndo(giantBlockObj, "Create Giant Block Gimmick");

        Selection.activeGameObject = giantBlockObj;
    }

    [MenuItem("Tools/Test Apply Derivative Skill on Gimmick")]
    public static void TestDerivativeSkill()
    {
        GameObject giantBlock = GameObject.Find("Gimmick_GiantBlock");
        if (giantBlock != null)
        {
            MathGimmick gimmick = giantBlock.GetComponent<MathGimmick>();
            if (gimmick != null)
            {
                gimmick.ApplySkill(MathSkill.Derivative);
            }
        }
    }

    private static void EnsureAssetsExist()
    {
        if (!Directory.Exists("Assets/Data"))
        {
            Directory.CreateDirectory("Assets/Data");
        }

        if (!Directory.Exists("Assets/Prefabs"))
        {
            Directory.CreateDirectory("Assets/Prefabs");
        }

        string profilePath = "Assets/Data/DerivativeShatterGimmickProfile.asset";
        if (AssetDatabase.LoadAssetAtPath<MathGimmickProfile>(profilePath) == null)
        {
            MathGimmickProfile profile = ScriptableObject.CreateInstance<MathGimmickProfile>();
            profile.profileName = "DerivativeShatterProfile";
            profile.inputExpression = "f'(x) Acceleration Control";
            profile.operationType = "Derivative";
            profile.effectType = "Shatter";
            AssetDatabase.CreateAsset(profile, profilePath);
        }

        string nodePath = "Assets/Data/SampleSequenceNode.asset";
        if (AssetDatabase.LoadAssetAtPath<SequenceNode>(nodePath) == null)
        {
            SequenceNode node = ScriptableObject.CreateInstance<SequenceNode>();
            node.sequenceType = SeqType.Combat;
            AssetDatabase.CreateAsset(node, nodePath);
        }

        string prefabPath = "Assets/Prefabs/CubeFraction.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
        {
            GameObject smallCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            smallCube.name = "CubeFraction";
            smallCube.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            Rigidbody smallRb = smallCube.AddComponent<Rigidbody>();
            smallRb.mass = 0.5f;
            smallRb.isKinematic = false;

            PrefabUtility.SaveAsPrefabAsset(smallCube, prefabPath);
            Object.DestroyImmediate(smallCube);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
