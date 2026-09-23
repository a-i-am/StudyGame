namespace StudyGame.Data
{
    // 기획서 반영: 1판의 흐름을 구성하는 노드 타입
    public enum EpisodeNodeType
    {
        PrologueVideo,   // 프롤로그 영상 (홍보/안내 영상)
        SNSNotification, // SNS 알림 (대사/속마음 묘사 대체용)
        FieldExploration,// 3D 맵 탐험 구역
        AnomalyEncounter,// 괴이 조우 및 수학 기믹 전투
        BossDialogue     // 미니보스 대화 및 결과 도출
    }

    // 과목 락 인젝션을 위한 지배 속성 (기존 SubjectType 확장/활용)
    public enum DominantSubject
    {
        None,
        Math,
        English,
        Korean,
        Science
    }
}