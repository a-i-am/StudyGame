using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New SNS Data", menuName = "StudyGame/Data/SNS Data")]
public class SNSData : ScriptableObject
{
    [Header("Sender Info")]
    public string senderName;
    public Sprite profileImage;

    [Header("Message Content")]
    [TextArea(3, 5)]
    public List<string> messages;

    [Header("Unlock Condition")]
    [Tooltip("이 대화를 읽었을 때 다이어리에 해금될 키워드나 ID")]
    public string unlockDiaryKeyword;
}