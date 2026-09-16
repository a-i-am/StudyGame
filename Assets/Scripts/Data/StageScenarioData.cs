using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace StudyGame.Data
{
    [CreateAssetMenu(fileName = "Scenario_", menuName = "StudyGame/Stage Scenario Data")]
    public class StageScenarioData : ScriptableObject
    {
        [Header("Meta Information")]
        public string scenarioId;
        public string title;
        public SubjectType subject;

        [Header("Environment & Visuals")]
        public GameObject mapEnvironmentPrefab;
        public GameObject playerPrefab;
        public GameObject partnerPrefab;
        public Material skyboxMaterial;
        public VolumeProfile postProcessProfile;

        [Header("Combat & Anomaly")]
        public GameObject anomalyMonsterPrefab;
        public int anomalyMaxHp = 100;

        [Header("Deduction & Dialogue")]
        public SequenceGraphData dialogueGraph;
        public ConceptData targetConcept;
        public int startingAP = 8;
        public List<ConceptData> candidatePool = new List<ConceptData>();
    }
}
