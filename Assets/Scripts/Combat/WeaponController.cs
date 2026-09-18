using System.Collections;
using UnityEngine;

namespace StudyGame.Combat
{
    public enum WeaponType
    {
        Sword,
        Staff,
        Dagger
    }

    public class WeaponController : MonoBehaviour
    {
        [Header("Weapon Configuration")]
        [SerializeField] private WeaponType weaponType = WeaponType.Sword;
        [SerializeField] private float swingDuration = 0.22f;
        [SerializeField] private float swingArcAngle = 140f;
        [SerializeField] private float attackCooldown = 0.35f;

        [Header("Visual Effects")]
        [SerializeField] private Color weaponEnergyColor = new Color(0.2f, 0.8f, 1f, 1f);
        [SerializeField] private TrailRenderer trailRenderer;

        public bool IsAttacking { get; private set; } = false;
        private float lastAttackTime = -999f;
        private Quaternion initialLocalRotation;
        private Vector3 initialLocalPosition;

        private void Awake()
        {
            initialLocalRotation = transform.localRotation;
            initialLocalPosition = transform.localPosition;
            SetupDefaultTrail();
        }

        private void SetupDefaultTrail()
        {
            if (trailRenderer == null)
            {
                trailRenderer = GetComponentInChildren<TrailRenderer>();
            }

            if (trailRenderer == null)
            {
                GameObject trailObj = new GameObject("SlashTrail");
                trailObj.transform.SetParent(transform, false);
                trailObj.transform.localPosition = new Vector3(0f, 0.8f, 0f);

                trailRenderer = trailObj.AddComponent<TrailRenderer>();
                trailRenderer.time = 0.15f;
                trailRenderer.startWidth = 0.4f;
                trailRenderer.endWidth = 0.05f;
                trailRenderer.material = new Material(Shader.Find("Sprites/Default"));

                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(weaponEnergyColor, 0.0f), new GradientColorKey(Color.white, 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(0.9f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
                );
                trailRenderer.colorGradient = gradient;
                trailRenderer.emitting = false;
            }
        }

        public bool CanAttack()
        {
            return !IsAttacking && (Time.time - lastAttackTime >= attackCooldown);
        }

        public void PerformSwing()
        {
            if (!CanAttack()) return;
            StartCoroutine(SwingRoutine());
        }

        private IEnumerator SwingRoutine()
        {
            IsAttacking = true;
            lastAttackTime = Time.time;

            if (trailRenderer != null)
            {
                trailRenderer.Clear();
                trailRenderer.emitting = true;
            }

            float elapsed = 0f;
            Quaternion startRot = initialLocalRotation * Quaternion.Euler(0f, -swingArcAngle * 0.5f, -25f);
            Quaternion endRot = initialLocalRotation * Quaternion.Euler(0f, swingArcAngle * 0.5f, 35f);

            while (elapsed < swingDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / swingDuration;
                
                // Smooth cubic ease out
                float smoothT = 1f - Mathf.Pow(1f - t, 3);

                transform.localRotation = Quaternion.Slerp(startRot, endRot, smoothT);
                
                // Perform parry check during active frames
                if (t > 0.2f && t < 0.8f)
                {
                    CheckParryCollision();
                }
                
                yield return null;
            }

            if (trailRenderer != null)
            {
                trailRenderer.emitting = false;
            }

            // Return to idle position smoothly
            elapsed = 0f;
            float returnDuration = 0.12f;
            Quaternion currentRot = transform.localRotation;

            while (elapsed < returnDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / returnDuration;
                transform.localRotation = Quaternion.Slerp(currentRot, initialLocalRotation, t);
                yield return null;
            }

            transform.localRotation = initialLocalRotation;
            transform.localPosition = initialLocalPosition;
            IsAttacking = false;
        }

        public void SetEnergyColor(Color color)
        {
            weaponEnergyColor = color;
            if (trailRenderer != null)
            {
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(weaponEnergyColor, 0.0f), new GradientColorKey(Color.white, 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(0.9f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
                );
                trailRenderer.colorGradient = gradient;
            }
        }

        private void CheckParryCollision()
        {
            Vector3 extents = new Vector3(1.5f, 1f, 1.5f);
            Vector3 center = transform.position + transform.forward * 1.5f;

            Collider[] hits = Physics.OverlapBox(center, extents, transform.rotation);
            foreach (var hit in hits)
            {
                // Basic parry collision check, can be expanded to check for specific projectile components
                if (hit.CompareTag("EnemyProjectile"))
                {
                    Debug.Log("[WeaponController] PARRY SUCCESS!");
                    Destroy(hit.gameObject); // Simple reflect/destroy for now
                    // Implement stun linkage or reflect damage here later
                }
            }
        }
    }
}
