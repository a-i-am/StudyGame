using UnityEngine;

[CreateAssetMenu(fileName = "New NPC Data", menuName = "StudyGame/Data/NPC Data")]
public class NPCData : ScriptableObject
{
    public string npcId;
    public string npcName;
    public NPCType personalityType = NPCType.Standard;
    public Sprite defaultStanding;
}
