using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class PhoneUIController : MonoBehaviour
{
    public UIDocument phoneUIDocument;

    private VisualElement root;
    private ScrollView messageScroll;
    private Label senderNameLabel;
    private VisualElement profileImage;

    private void OnEnable()
    {
        if (StageManager.Instance != null)
            StageManager.Instance.OnSequenceStarted += HandleSequence;
    }

    private void OnDisable()
    {
        if (StageManager.Instance != null)
            StageManager.Instance.OnSequenceStarted -= HandleSequence;
    }

    private void Start()
    {
        root = phoneUIDocument.rootVisualElement;
        messageScroll = root.Q<ScrollView>("MessageScroll");
        senderNameLabel = root.Q<Label>("SenderName");
        profileImage = root.Q<VisualElement>("ProfileImage");

        root.style.display = DisplayStyle.None;
    }

    private void HandleSequence(SequenceNode node)
    {
        if (node.sequenceType == SeqType.Narrative && node.snsData != null)
        {
            root.style.display = DisplayStyle.Flex;
            StartCoroutine(DisplayMessages(node.snsData));
        }
    }

    private IEnumerator DisplayMessages(SNSData data)
    {
        senderNameLabel.text = data.senderName;
        profileImage.style.backgroundImage = new StyleBackground(data.profileImage);
        messageScroll.Clear(); // 이전 메시지 초기화

        foreach (string msg in data.messages)
        {
            yield return new WaitForSeconds(0.8f); // 수신 딜레이

            // 새 메시지 말풍선 생성 및 렌더링
            Label newBubble = new Label(msg);
            newBubble.AddToClassList("sns-bubble");
            messageScroll.Add(newBubble);

            // 스크롤 최하단으로 강제 이동
            messageScroll.ScrollTo(newBubble);
        }

        // 메시지 출력이 끝나면 탐험 상태로 복귀하거나 다음 시퀀스 진행
        yield return new WaitForSeconds(2.0f);
        root.style.display = DisplayStyle.None;
        StageManager.Instance.ChangeState(StageManager.GameState.Exploration);
    }
}