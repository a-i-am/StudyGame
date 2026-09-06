using UnityEngine;

[CreateAssetMenu(fileName = "NewBookmark", menuName = "StudyGame/Bookmark Data")]
public class BookmarkData : ScriptableObject
{
    [Header("시각 정보")]
    public Sprite thumbnail;        // 썸네일 이미지
    public string title;            // 영상/글 제목 (예: 로그의 성질)
    [TextArea(2, 4)]
    public string description;      // 요약 설명

    [Header("연결 정보")]
    public string youtubeVideoID;   // 유튜브 ID (예: giBw0L5wE2I)
    public int rewardID;            // 학습 완료 시 지급할 보상 ID (추후 사용)

}
