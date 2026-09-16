using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudyGame.Data;
using StudyGame.Combat;
using StudyGame.Partner;
using StudyGame.Managers;

namespace StudyGame.Environment
{
    public class WireframeEnvironmentBuilder : MonoBehaviour
    {
        [Header("Environment Settings")]
        [SerializeField] private Color chamberThemeColor = new Color(0.04f, 0.08f, 0.16f);
        [SerializeField] private Color wireframeGridColor = new Color(0.0f, 0.85f, 1.0f, 0.4f);

        [Header("Chamber Breathing Motion")]
        [SerializeField] private bool enableBreathingMotion = true;
        [SerializeField] private float breathingSpeed = 0.7f;
        [SerializeField] private float breathingDepth = 0.35f;

        private Transform tunnelTransform;
        private Vector3 baseTunnelScale = new Vector3(14f, 25f, 14f);

        private static readonly string JSON_SINGLE_CREATURE_DATA = @"[
  {
    ""id"": ""ANOMALY_MATH_GEOM_RATIO"",
    ""conceptTitle"": ""등비수열의 이상현상"",
    ""archetype"": 4,
    ""primarySymbolText"": ""a_n = a_1 · r^(n-1)"",
    ""hexColor"": ""#00E5FF"",
    ""glitchFrequency"": 1.8,
    ""visualDescription"": ""일정한 비율로 무한히 변형되는 네온 청록색 와이어프레임 결정체."",
    ""clueKeyword"": ""일정한 곱셈 비율로 변형되는 사슬""
  }
]";

        public void BuildWireframeChamber()
        {
            GameObject chamberObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            chamberObj.name = "WireframeIncubatorTunnel";
            chamberObj.transform.SetParent(transform, false);
            chamberObj.transform.localScale = baseTunnelScale;
            chamberObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            tunnelTransform = chamberObj.transform;

            MeshFilter mf = chamberObj.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                Mesh mesh = Instantiate(mf.sharedMesh);
                int[] triangles = mesh.triangles;
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int temp = triangles[i];
                    triangles[i] = triangles[i + 1];
                    triangles[i + 1] = temp;
                }
                mesh.triangles = triangles;
                mesh.RecalculateNormals();
                mf.sharedMesh = mesh;

                MeshCollider mc = chamberObj.GetComponent<MeshCollider>();
                if (mc != null) mc.sharedMesh = mesh;
            }

            Material tunnelMat = new Material(Shader.Find("Standard"));
            tunnelMat.color = chamberThemeColor;
            tunnelMat.EnableKeyword("_EMISSION");
            tunnelMat.SetColor("_EmissionColor", wireframeGridColor * 0.35f);
            MeshRenderer mr = chamberObj.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = tunnelMat;

            BuildStreetlampLight();
            SpawnSingleAnomalyCreature();
        }

        private void SpawnSingleAnomalyCreature()
        {
            AnomalyProfileListWrapper wrapper = JsonUtility.FromJson<AnomalyProfileListWrapper>("{\"profiles\":" + JSON_SINGLE_CREATURE_DATA + "}");
            if (wrapper != null && wrapper.profiles != null && wrapper.profiles.Length > 0)
            {
                AnomalyCreatureProfileData prof = wrapper.profiles[0];
                Vector3 spawnPos = new Vector3(0f, 1.0f, 10f);

                GameObject creatureObj = new GameObject($"Creature_{prof.id}");
                creatureObj.transform.SetParent(transform, false);
                creatureObj.transform.position = spawnPos;

                BoxCollider boxCol = creatureObj.AddComponent<BoxCollider>();
                boxCol.size = new Vector3(4.5f, 3.5f, 4.5f);
                boxCol.center = Vector3.zero;
                boxCol.isTrigger = true;

                AnomalyCreatureVisualizer viz = creatureObj.AddComponent<AnomalyCreatureVisualizer>();
                viz.SetupProfile(prof);

                creatureObj.AddComponent<AnomalyTrigger>();

                BuildGroundEncounterIndicator(spawnPos, prof.GetParsedColor());
            }
        }

        private void BuildGroundEncounterIndicator(Vector3 position, Color themeColor)
        {
            GameObject indicatorObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            indicatorObj.name = "TriggerArea_GlowRing";
            indicatorObj.transform.SetParent(transform, false);
            indicatorObj.transform.position = new Vector3(position.x, 0.05f, position.z);
            indicatorObj.transform.localScale = new Vector3(4.5f, 0.02f, 4.5f);
            DestroyImmediate(indicatorObj.GetComponent<Collider>());

            Material ringMat = new Material(Shader.Find("Standard"));
            ringMat.color = themeColor;
            ringMat.EnableKeyword("_EMISSION");
            ringMat.SetColor("_EmissionColor", themeColor * 3.0f);
            indicatorObj.GetComponent<MeshRenderer>().sharedMaterial = ringMat;

            GameObject textObj = new GameObject("EncounterGuideText");
            textObj.transform.SetParent(indicatorObj.transform, false);
            textObj.transform.localPosition = new Vector3(0f, 25f, 0.8f);
            textObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            TextMesh tm = textObj.AddComponent<TextMesh>();
            tm.text = "▼ 이상현상 영역 (진입 시 스킬덱 전투)";
            tm.fontSize = 28;
            tm.characterSize = 0.15f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = Color.white;
        }

        private void BuildStreetlampLight()
        {
            GameObject lampPost = new GameObject("Streetlamp_Post");
            lampPost.transform.SetParent(transform, false);
            lampPost.transform.position = new Vector3(-4f, 0f, 8f);

            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.transform.SetParent(lampPost.transform, false);
            pole.transform.localPosition = new Vector3(0f, 2.5f, 0f);
            pole.transform.localScale = new Vector3(0.15f, 2.5f, 0.15f);
            DestroyImmediate(pole.GetComponent<Collider>());

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.transform.SetParent(lampPost.transform, false);
            head.transform.localPosition = new Vector3(0.3f, 4.8f, 0f);
            head.transform.localScale = new Vector3(0.6f, 0.4f, 0.6f);
            DestroyImmediate(head.GetComponent<Collider>());

            Material headMat = new Material(Shader.Find("Standard"));
            headMat.color = new Color(1.0f, 0.85f, 0.5f);
            headMat.EnableKeyword("_EMISSION");
            headMat.SetColor("_EmissionColor", new Color(1.0f, 0.85f, 0.5f) * 3f);
            head.GetComponent<MeshRenderer>().sharedMaterial = headMat;

            GameObject lightObj = new GameObject("Streetlamp_PointLight");
            lightObj.transform.SetParent(lampPost.transform, false);
            lightObj.transform.localPosition = new Vector3(0.3f, 4.5f, 0f);

            Light lampLight = lightObj.AddComponent<Light>();
            lampLight.type = LightType.Point;
            lampLight.range = 28f;
            lampLight.color = new Color(1.0f, 0.88f, 0.65f);
            lampLight.intensity = 2.8f;
        }

        private void Update()
        {
            if (!enableBreathingMotion || tunnelTransform == null) return;

            float sinPulse = Mathf.Sin(Time.time * breathingSpeed);
            float scaleFactorX = 1f - (breathingDepth * 0.4f * (sinPulse + 1f) * 0.5f);
            float scaleFactorZ = 1f - (breathingDepth * 0.4f * (sinPulse + 1f) * 0.5f);

            tunnelTransform.localScale = new Vector3(
                baseTunnelScale.x * scaleFactorX,
                baseTunnelScale.y,
                baseTunnelScale.z * scaleFactorZ
            );
        }
    }
}
