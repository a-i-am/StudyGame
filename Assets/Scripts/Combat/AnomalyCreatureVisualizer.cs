using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudyGame.Data;

namespace StudyGame.Combat
{
    public class AnomalyCreatureVisualizer : MonoBehaviour
    {
        [Header("Profile Configuration")]
        [SerializeField] private AnomalyCreatureProfileData profile;

        [Header("Visual Components")]
        [SerializeField] private Transform creatureCorePivot;
        [SerializeField] private TextMesh symbolTextMesh;
        [SerializeField] private List<Transform> childVisualNodes = new List<Transform>();

        private Material creatureMaterial;
        private float baseGlitchTimer = 0f;
        private Vector3 initialScale;

        private void Awake()
        {
            initialScale = transform.localScale;
            if (creatureCorePivot == null)
            {
                GameObject pivotObj = new GameObject("CorePivot");
                pivotObj.transform.SetParent(transform, false);
                creatureCorePivot = pivotObj.transform;
            }
        }

        public void SetupProfile(AnomalyCreatureProfileData data)
        {
            profile = data;
            BuildVisuals();
        }

        private void Start()
        {
            if (profile != null && childVisualNodes.Count == 0)
            {
                BuildVisuals();
            }
        }

        public void BuildVisuals()
        {
            if (profile == null) return;

            // Clear previous visual children
            foreach (Transform child in creatureCorePivot)
            {
                Destroy(child.gameObject);
            }
            childVisualNodes.Clear();

            Color profileColor = profile.GetParsedColor();

            // Create Glow Wireframe Material
            creatureMaterial = new Material(Shader.Find("Standard"));
            creatureMaterial.color = profileColor;
            creatureMaterial.EnableKeyword("_EMISSION");
            creatureMaterial.SetColor("_EmissionColor", profileColor * 2.5f);

            // Build Geometry Based on Archetype
            switch (profile.archetype)
            {
                case AnomalyArchetype.LinearChain:
                    BuildLinearChain(profileColor);
                    break;
                case AnomalyArchetype.OrbitalSphere:
                    BuildOrbitalSphere(profileColor);
                    break;
                case AnomalyArchetype.Polyhedron:
                    BuildPolyhedron(profileColor);
                    break;
                case AnomalyArchetype.HumanoidShadow:
                    BuildHumanoidShadow(profileColor);
                    break;
                case AnomalyArchetype.SwarmCloud:
                    BuildSwarmCloud(profileColor);
                    break;
            }

            // Create Floating 3D Symbol Text
            BuildFloatingSymbol(profile.primarySymbolText, profileColor);
        }

        private void BuildLinearChain(Color color)
        {
            int nodes = 6;
            for (int i = 0; i < nodes; i++)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.SetParent(creatureCorePivot, false);
                float angle = i * 0.8f;
                sphere.transform.localPosition = new Vector3(Mathf.Sin(angle) * 1.2f, i * 0.4f - 1.0f, Mathf.Cos(angle) * 1.2f);
                sphere.transform.localScale = Vector3.one * (0.35f + i * 0.05f);
                
                MeshRenderer mr = sphere.GetComponent<MeshRenderer>();
                if (mr != null) mr.sharedMaterial = creatureMaterial;
                childVisualNodes.Add(sphere.transform);
            }
        }

        private void BuildOrbitalSphere(Color color)
        {
            // Core Sphere
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.transform.SetParent(creatureCorePivot, false);
            core.transform.localScale = Vector3.one * 1.4f;
            MeshRenderer mrCore = core.GetComponent<MeshRenderer>();
            if (mrCore != null) mrCore.sharedMaterial = creatureMaterial;
            childVisualNodes.Add(core.transform);

            // Orbit Ring
            for (int i = 0; i < 8; i++)
            {
                GameObject orbital = GameObject.CreatePrimitive(PrimitiveType.Cube);
                orbital.transform.SetParent(creatureCorePivot, false);
                float angle = i * (Mathf.PI * 2f / 8f);
                orbital.transform.localPosition = new Vector3(Mathf.Cos(angle) * 2.2f, Mathf.Sin(angle * 2f) * 0.5f, Mathf.Sin(angle) * 2.2f);
                orbital.transform.localScale = new Vector3(0.2f, 0.2f, 0.6f);
                orbital.transform.localRotation = Quaternion.Euler(angle * Mathf.Rad2Deg, 45f, 0f);

                MeshRenderer mrOrbital = orbital.GetComponent<MeshRenderer>();
                if (mrOrbital != null) mrOrbital.sharedMaterial = creatureMaterial;
                childVisualNodes.Add(orbital.transform);
            }
        }

        private void BuildPolyhedron(Color color)
        {
            GameObject mainPoly = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mainPoly.transform.SetParent(creatureCorePivot, false);
            mainPoly.transform.localScale = Vector3.one * 1.6f;
            mainPoly.transform.localRotation = Quaternion.Euler(45f, 45f, 0f);

            MeshRenderer mr = mainPoly.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = creatureMaterial;
            childVisualNodes.Add(mainPoly.transform);

            // Sub fragments
            for (int i = 0; i < 4; i++)
            {
                GameObject sub = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sub.transform.SetParent(creatureCorePivot, false);
                Vector3 dir = Random.onUnitSphere * 1.5f;
                sub.transform.localPosition = dir;
                sub.transform.localScale = Vector3.one * 0.4f;
                MeshRenderer subMr = sub.GetComponent<MeshRenderer>();
                if (subMr != null) subMr.sharedMaterial = creatureMaterial;
                childVisualNodes.Add(sub.transform);
            }
        }

        private void BuildHumanoidShadow(Color color)
        {
            // Body pillar
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            body.transform.SetParent(creatureCorePivot, false);
            body.transform.localScale = new Vector3(0.5f, 1.2f, 0.5f);
            MeshRenderer bodyMr = body.GetComponent<MeshRenderer>();
            if (bodyMr != null) bodyMr.sharedMaterial = creatureMaterial;
            childVisualNodes.Add(body.transform);

            // Head diamond
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.transform.SetParent(creatureCorePivot, false);
            head.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            head.transform.localScale = Vector3.one * 0.6f;
            head.transform.localRotation = Quaternion.Euler(0f, 45f, 45f);
            MeshRenderer headMr = head.GetComponent<MeshRenderer>();
            if (headMr != null) headMr.sharedMaterial = creatureMaterial;
            childVisualNodes.Add(head.transform);
        }

        private void BuildSwarmCloud(Color color)
        {
            int swarmCount = 12;
            for (int i = 0; i < swarmCount; i++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.SetParent(creatureCorePivot, false);
                cube.transform.localPosition = Random.insideUnitSphere * 1.8f;
                cube.transform.localScale = Vector3.one * Random.Range(0.2f, 0.45f);
                MeshRenderer mr = cube.GetComponent<MeshRenderer>();
                if (mr != null) mr.sharedMaterial = creatureMaterial;
                childVisualNodes.Add(cube.transform);
            }
        }

        private void BuildFloatingSymbol(string symbolText, Color color)
        {
            if (string.IsNullOrEmpty(symbolText)) return;

            GameObject textObj = new GameObject("SymbolText_3D");
            textObj.transform.SetParent(creatureCorePivot, false);
            textObj.transform.localPosition = new Vector3(0f, 2.2f, 0f);

            symbolTextMesh = textObj.AddComponent<TextMesh>();
            symbolTextMesh.text = symbolText;
            symbolTextMesh.fontSize = 32;
            symbolTextMesh.characterSize = 0.12f;
            symbolTextMesh.anchor = TextAnchor.MiddleCenter;
            symbolTextMesh.alignment = TextAlignment.Center;
            symbolTextMesh.color = color;
        }

        private void Update()
        {
            if (profile == null) return;

            baseGlitchTimer += Time.deltaTime * profile.glitchFrequency;

            // Rotate core pivot
            creatureCorePivot.Rotate(Vector3.up, 25f * Time.deltaTime, Space.World);

            // Face 3D Text to Main Camera
            if (symbolTextMesh != null && Camera.main != null)
            {
                symbolTextMesh.transform.rotation = Quaternion.LookRotation(symbolTextMesh.transform.position - Camera.main.transform.position);
            }

            // Glitch Scaling & Distortion Pulse
            float glitchPulse = Mathf.Sin(baseGlitchTimer * 5f) * 0.08f;
            if (Random.value < 0.05f * profile.glitchFrequency)
            {
                // Sudden Glitch Jitter
                creatureCorePivot.localPosition = Random.insideUnitSphere * 0.15f;
            }
            else
            {
                creatureCorePivot.localPosition = Vector3.Lerp(creatureCorePivot.localPosition, Vector3.zero, Time.deltaTime * 10f);
            }

            transform.localScale = initialScale + Vector3.one * glitchPulse;
        }

        private Coroutine attackCoroutine;

        public void StartAttacking(Transform playerTransform)
        {
            if (attackCoroutine != null) StopCoroutine(attackCoroutine);
            attackCoroutine = StartCoroutine(AttackLoopRoutine(playerTransform));
        }

        public void StopAttacking()
        {
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
        }

        private IEnumerator AttackLoopRoutine(Transform playerTransform)
        {
            Color attackColor = profile != null ? profile.GetParsedColor() : Color.red;

            while (true)
            {
                yield return new WaitForSeconds(1.4f);

                Vector3 targetPos = playerTransform != null ? playerTransform.position + Vector3.up * 0.8f : transform.position + Vector3.forward * 5f;
                Vector3 spawnPos = creatureCorePivot.position + Vector3.up * 0.5f;
                Vector3 direction = targetPos - spawnPos;

                GameObject projObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                projObj.name = "AnomalyPulseProjectile";
                projObj.transform.position = spawnPos;
                projObj.transform.localScale = Vector3.one * 0.45f;
                SphereCollider col = projObj.GetComponent<SphereCollider>();
                if (col != null) col.isTrigger = true;

                AnomalyPulseProjectile proj = projObj.AddComponent<AnomalyPulseProjectile>();
                proj.Initialize(direction, attackColor);
            }
        }
    }
}
