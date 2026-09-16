using System;
using UnityEngine;

namespace StudyGame.Combat
{
    [RequireComponent(typeof(Collider))]
    public class AnomalyTrigger : MonoBehaviour
    {
        public event Action OnPlayerEncountered;
        private bool hasTriggered = false;

        private void OnTriggerEnter(Collider other)
        {
            CheckTrigger(other.gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            CheckTrigger(collision.gameObject);
        }

        private void CheckTrigger(GameObject obj)
        {
            if (hasTriggered) return;

            if (obj.CompareTag("Player") || obj.GetComponentInParent<StudyGame.Player.PlayerController>() != null)
            {
                hasTriggered = true;
                OnPlayerEncountered?.Invoke();
                if (StudyGame.Managers.StageRunnerController.Instance != null)
                {
                    StudyGame.Managers.StageRunnerController.Instance.NotifyAnomalyEncounter(gameObject);
                }
            }
        }
    }
}
