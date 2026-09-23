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
        public DominantSubject dominantSubject;

        [Header("Story & SNS")]
        public SNSData initialSNSNotification;

        [Header("Environment & Visuals")]
        public GameObject mapEnvironmentPrefab;
        public GameObject playerPrefab;
        public Material skyboxMaterial;
        public VolumeProfile postProcessProfile;

        [Header("Combat & Anomaly")]
        public GameObject anomalyMonsterPrefab; // TODO : AnomalyCreatureProfileData를 참조하는 형태로 추후 개선 권장
        public int anomalyMaxHp = 100;
        public List<ConceptData> candidatePool = new List<ConceptData>(); // 몬스터가 드랍할 키워드/단서

        [Header("Deduction & Dialogue")]
        public SequenceGraphData dialogueGraph;
        public ConceptData targetConcept;
        public int startingAP = 8;

        // 파트너(조력자)는 미니보스 구출 후 합류하므로 분리
        public GameObject rescuedPartnerPrefab;
    }
}
