using UnityEngine;

namespace StudyGame.Core
{
    public class InteractionManager : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private float detectionRadius = 2.5f;
        [SerializeField] private LayerMask interactableMask = ~0;

        private IInteractable currentTarget;
        private Collider[] overlapResults = new Collider[8];

        public IInteractable CurrentTarget => currentTarget;
        public bool HasTarget => currentTarget != null;

        public System.Action<IInteractable> OnTargetChanged;
        public System.Action<IInteractable> OnInteracted;

        private void FixedUpdate()
        {
            ScanForInteractables();
        }

        private void ScanForInteractables()
        {
            int count = Physics.OverlapSphereNonAlloc(
                transform.position, detectionRadius, overlapResults, interactableMask);

            IInteractable closest = null;
            float closestDist = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var interactable = overlapResults[i].GetComponent<IInteractable>();
                if (interactable == null) continue;
                if (!interactable.CanInteract(gameObject)) continue;

                float dist = Vector3.SqrMagnitude(
                    overlapResults[i].transform.position - transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = interactable;
                }
            }

            if (closest != currentTarget)
            {
                currentTarget = closest;
                OnTargetChanged?.Invoke(currentTarget);
            }
        }

        public void TryInteract()
        {
            if (currentTarget == null) return;
            if (!currentTarget.CanInteract(gameObject)) return;

            currentTarget.OnInteract(gameObject);
            OnInteracted?.Invoke(currentTarget);
        }
    }
}
