using UnityEngine;

[CreateAssetMenu(fileName = "New NPC Data", menuName = "StudyGame/Data/NPC Data")]
public class NPCData : ScriptableObject
{
    public string npcId;
    public string npcName;
    public MBTIType mbtiType = MBTIType.Unknown;
    
    [Tooltip("기본적으로 장착할 탈(가면)의 ID")]
    public string defaultMaskId;
    
    [TextArea(3, 5)]
    [Tooltip("고유 성격에 대한 상세 묘사")]
    public string personalityDescription;

    [TextArea(3, 5)]
    [Tooltip("말투, 억양, 자주 쓰는 단어 등 언어적 특징")]
    public string speechStyle;

    [TextArea(3, 5)]
    [Tooltip("LLM 프롬프트에 주입할 추가적인 지시문")]
    public string promptInjection;

    public Sprite defaultStanding;
}
