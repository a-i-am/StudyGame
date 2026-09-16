using UnityEngine;

namespace StudyGame.Combat
{
    public class AnomalyPulseProjectile : MonoBehaviour
    {
        private Vector3 targetDirection;
        private float speed = 10f;
        private float lifetime = 3f;

        public void Initialize(Vector3 direction, Color glowColor)
        {
            targetDirection = direction.normalized;

            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = glowColor;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", glowColor * 3.5f);
                mr.sharedMaterial = mat;
            }

            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            transform.position += targetDirection * speed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") || other.GetComponentInParent<StudyGame.Player.PlayerController>() != null)
            {
                // Hit Player visual recoil effect
                Destroy(gameObject);
            }
        }
    }
}
