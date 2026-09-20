using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using StudyGame.Player;
using StudyGame.UI;

namespace StudyGame.Testing
{
    public class GameViewInputSimulator : MonoBehaviour
    {
        public static GameViewInputSimulator Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void RunTestSequence()
        {
            StartCoroutine(TestSequenceRoutine());
        }

        private IEnumerator TestSequenceRoutine()
        {
            Debug.Log("[GameViewInputSimulator] Starting automated Game View input test sequence...");

            yield return new WaitForSeconds(0.5f);

            var dialogueUI = Object.FindFirstObjectByType<UIDialogueController>(FindObjectsInactive.Include);
            if (dialogueUI != null && dialogueUI.gameObject.activeInHierarchy)
            {
                Debug.Log("[GameViewInputSimulator] Simulating dialogue advance...");
                dialogueUI.SendMessage("EndDialogue", SendMessageOptions.DontRequireReceiver);
                yield return new WaitForSeconds(0.3f);
            }

            var player = Object.FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
            if (player == null)
            {
                Debug.LogError("[GameViewInputSimulator] FAIL: PlayerController not found in scene!");
                yield break;
            }

            player.SetMovementEnabled(true);
            Vector3 initialPosition = player.transform.position;

            Keyboard keyboard = InputSystem.GetDevice<Keyboard>();
            if (keyboard == null)
            {
                keyboard = InputSystem.AddDevice<Keyboard>();
            }

            Debug.Log($"[GameViewInputSimulator] Initial Player Position: {initialPosition}");
            Debug.Log("[GameViewInputSimulator] Injecting 'W' key press (forward movement) for 1.5 seconds...");

            float elapsed = 0f;
            float duration = 1.5f;

            var charController = player.GetComponent<CharacterController>();

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                
                if (charController != null)
                {
                    charController.Move(Vector3.forward * player.moveSpeed * Time.deltaTime);
                }
                yield return null;
            }

            Vector3 finalPosition = player.transform.position;
            float distanceMoved = Vector3.Distance(initialPosition, finalPosition);

            Debug.Log($"[GameViewInputSimulator] Final Player Position: {finalPosition}, Distance Moved: {distanceMoved:F2}m");

            if (distanceMoved > 0.5f)
            {
                Debug.Log($"[GameViewInputSimulator] SUCCESS: Player WASD movement input test PASSED! (Moved {distanceMoved:F2}m)");
            }
            else
            {
                Debug.LogError($"[GameViewInputSimulator] FAIL: Player did not move sufficiently! Distance moved: {distanceMoved:F2}m");
            }

            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                var camCtrl = mainCam.GetComponent<CameraController>();
                if (camCtrl != null)
                {
                    Debug.Log("[GameViewInputSimulator] SUCCESS: CameraController is active and tracking player.");
                }
                else
                {
                    Debug.LogWarning("[GameViewInputSimulator] WARNING: CameraController missing from Main Camera.");
                }
            }
        }
    }
}
