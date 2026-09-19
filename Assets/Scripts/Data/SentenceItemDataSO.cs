using UnityEngine;

namespace StudyGame.Data
{
    public enum SentenceTagType
    {
        Premise,    // 전제
        Paradox,    // 모순
        Conclusion  // 종결
    }

    [CreateAssetMenu(fileName = "NewSentenceItem", menuName = "StudyGame/Sentence Item Data")]
    public class SentenceItemDataSO : ScriptableObject
    {
        public string ItemId;
        public SentenceTagType TagType;
        [TextArea(2, 4)] public string Text;
    }
}
