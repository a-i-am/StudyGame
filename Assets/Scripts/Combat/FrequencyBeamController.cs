using UnityEngine;
using StudyGame.Player;
using UnityEngine.InputSystem;

namespace StudyGame.Combat
{
    public class FrequencyBeamController : MonoBehaviour
    {
        [Header("Beam Settings")]
        public GameObject projectilePrefab;
        public Transform firePoint;
        public float fireRate = 0.15f;
        
        [Header("Overheat System")]
        public float maxHeat = 100f;
        public float heatPerShot = 15f;
        public float coolingRate = 25f;
        public float overheatPenaltyDuration = 2.0f;
        
        private float currentHeat = 0f;
        private bool isOverheated = false;
        private float overheatTimer = 0f;
        private float lastFireTime = 0f;

        private InputAction fireAction;

        private void Awake()
        {
            SetupInput();
        }

        private void SetupInput()
        {
            fireAction = new InputAction("FireBeam", binding: "<Mouse>/rightButton");
            fireAction.AddBinding("<Gamepad>/rightTrigger");
        }

        private void OnEnable()
        {
            fireAction.Enable();
        }

        private void OnDisable()
        {
            fireAction.Disable();
        }

        private void Update()
        {
            HandleOverheat();
            HandleFiring();
        }

        private void HandleOverheat()
        {
            if (isOverheated)
            {
                overheatTimer -= Time.deltaTime;
                if (overheatTimer <= 0)
                {
                    isOverheated = false;
                    currentHeat = 0; // Fully cooled after penalty
                }
            }
            else
            {
                if (currentHeat > 0)
                {
                    currentHeat -= coolingRate * Time.deltaTime;
                    currentHeat = Mathf.Max(0, currentHeat);
                }
            }
        }

        private void HandleFiring()
        {
            if (isOverheated) return;

            // Using IsPressed for rapid fire
            if (fireAction.IsPressed() && Time.time - lastFireTime >= fireRate)
            {
                FireBeam();
            }
        }

        private void FireBeam()
        {
            lastFireTime = Time.time;
            currentHeat += heatPerShot;

            if (currentHeat >= maxHeat)
            {
                currentHeat = maxHeat;
                isOverheated = true;
                overheatTimer = overheatPenaltyDuration;
                Debug.Log("[FrequencyBeam] OVERHEATED!");
            }

            if (projectilePrefab != null && firePoint != null)
            {
                Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            }
        }

        public float GetHeatNormalized()
        {
            return currentHeat / maxHeat;
        }

        public bool IsOverheated()
        {
            return isOverheated;
        }
    }
}
