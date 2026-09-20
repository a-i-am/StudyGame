using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using StudyGame.UI;
using StudyGame.Managers;
using StudyGame.Utils;

namespace StudyGame.Editor
{
    public class TestSceneSetup : MonoBehaviour
    {
        [MenuItem("StudyGame/Setup Runtime Test Scene")]
        public static void SetupScene()
        {
            var levelData = StudyGame.Editor.Director.LevelGeometryManager.CreateDefault20x20Grid();
            StudyGame.Editor.Director.LevelGeometryManager.Rebuild(levelData);

            string levelJson = JsonUtility.ToJson(levelData);
            EditorPrefs.SetString("StudyGame_SandboxGeometry", levelJson);
            EditorPrefs.SetBool("StudyGame_SandboxPending", true);

            EpisodePlayer player = null;
            foreach (var p in Object.FindObjectsByType<EpisodePlayer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (p.gameObject.scene.isLoaded) { player = p; break; }
            }
            
            if (player == null)
            {
                var go = new GameObject("EpisodePlayer");
                player = go.GetOrAddComponent<EpisodePlayer>();
            }

            var panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>("Assets/UI Toolkit/PanelSettings.asset");

            UIDialogueController dialogUI = null;
            foreach (var d in Object.FindObjectsByType<UIDialogueController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (d.gameObject.scene.isLoaded) { dialogUI = d; break; }
                
            if (dialogUI == null)
            {
                var go = new GameObject("UI_Dialogue");
                dialogUI = go.GetOrAddComponent<UIDialogueController>();
                var uiDoc = go.GetOrAddComponent<UIDocument>();
                
                uiDoc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/DialogueView.uxml");
                if (panelSettings != null) uiDoc.panelSettings = panelSettings;
            }

            PhoneUIDocumentController phoneUI = null;
            foreach (var p in Object.FindObjectsByType<PhoneUIDocumentController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (p.gameObject.scene.isLoaded) { phoneUI = p; break; }
                
            if (phoneUI == null)
            {
                var go = new GameObject("UI_Phone");
                phoneUI = go.GetOrAddComponent<PhoneUIDocumentController>();
                var uiDoc = go.GetOrAddComponent<UIDocument>();
                
                uiDoc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI Toolkit/PhoneUIDocument.uxml");
                if (panelSettings != null) uiDoc.panelSettings = panelSettings;
            }

            Light light = null;
            foreach (var l in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (l.gameObject.scene.isLoaded && l.type == LightType.Directional) { light = l; break; }
                
            if (light == null)
            {
                var go = new GameObject("Directional Light");
                var l = go.GetOrAddComponent<Light>();
                l.type = LightType.Directional;
                go.transform.rotation = Quaternion.Euler(50, -30, 0);
            }

            Camera cam = null;
            foreach (var c in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (c.gameObject.scene.isLoaded) { cam = c; break; }
                
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                cam = go.GetOrAddComponent<Camera>();
                go.tag = "MainCamera";
                go.transform.position = new Vector3(0, 5, -10);
                go.transform.rotation = Quaternion.Euler(20, 0, 0);
                cam.clearFlags = CameraClearFlags.Skybox;
            }

            StudyGame.Player.PlayerController playerCtrl = null;
            foreach (var p in Object.FindObjectsByType<StudyGame.Player.PlayerController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (p.gameObject.scene.isLoaded) { playerCtrl = p; break; }

            if (playerCtrl == null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player_Test.prefab");
                if (prefab != null)
                {
                    var go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                    playerCtrl = go.GetComponent<StudyGame.Player.PlayerController>();
                }
            }
            if (playerCtrl != null)
            {
                playerCtrl.transform.position = new Vector3(0f, 1f, 0f);
                playerCtrl.SetMovementEnabled(true);
                
                if (cam != null)
                {
                    var camCtrl = cam.gameObject.GetOrAddComponent<StudyGame.Player.CameraController>();
                    camCtrl.SetTarget(playerCtrl.transform);
                    camCtrl.SetInputEnabled(true);
                }
            }

            Debug.Log("20x20 레벨 필드가 셋업되었습니다! 플레이어가 바닥 중앙(0, 1, 0) 위에 배치되었습니다.");
        }
    }
}
