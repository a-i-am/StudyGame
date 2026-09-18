using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using StudyGame.Core;
using StudyGame.Player;

namespace StudyGame.Field
{
    public enum TransitionType
    {
        SeamlessTeleport,
        FadeSceneLoad
    }

    public class PortalTrigger : MonoBehaviour, IInteractable
    {
        [Header("Transition Settings")]
        public TransitionType transitionType = TransitionType.FadeSceneLoad;
        public bool requireManualInteraction = true;
        public string interactPrompt = "문 열기 / 이동하기";
        
        [Header("Target Location")]
        public string targetSceneName;
        public Transform targetTeleportPoint;

        private bool isTransitioning = false;

        public string InteractionPrompt => interactPrompt;

        public bool CanInteract(GameObject instigator)
        {
            return !isTransitioning && requireManualInteraction;
        }

        public void OnInteract(GameObject instigator)
        {
            if (isTransitioning) return;
            StartTransition(instigator);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (requireManualInteraction || isTransitioning) return;

            if (other.CompareTag("Player"))
            {
                StartTransition(other.gameObject);
            }
        }

        private void StartTransition(GameObject player)
        {
            isTransitioning = true;
            
            var playerCtrl = player.GetComponent<PlayerController>();
            if (playerCtrl != null)
            {
                playerCtrl.SetMovementEnabled(false);
            }

            if (transitionType == TransitionType.SeamlessTeleport)
            {
                StartCoroutine(TeleportRoutine(player, playerCtrl));
            }
            else
            {
                StartCoroutine(SceneLoadRoutine());
            }
        }

        private IEnumerator TeleportRoutine(GameObject player, PlayerController playerCtrl)
        {
            // TODO: Call UIManager to fade to black
            yield return new WaitForSeconds(0.5f); // Simulate fade out

            if (targetTeleportPoint != null)
            {
                // Temporarily disable CharacterController to manually set position
                var cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                
                player.transform.position = targetTeleportPoint.position;
                player.transform.rotation = targetTeleportPoint.rotation;
                
                if (cc != null) cc.enabled = true;
                
                // Also update camera target immediately to prevent interpolation sweeping across the map
                var cam = Camera.main.GetComponent<CameraController>();
                if (cam != null)
                {
                    cam.transform.position = player.transform.position - cam.transform.forward * cam.distance;
                }
            }

            // TODO: Call UIManager to fade from black
            yield return new WaitForSeconds(0.5f); // Simulate fade in

            if (playerCtrl != null)
            {
                playerCtrl.SetMovementEnabled(true);
            }
            isTransitioning = false;
        }

        private IEnumerator SceneLoadRoutine()
        {
            // TODO: Call UIManager to fade to black
            yield return new WaitForSeconds(0.5f);
            
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                Debug.LogWarning("[PortalTrigger] Target scene name is empty!");
                isTransitioning = false;
            }
        }
    }
}
