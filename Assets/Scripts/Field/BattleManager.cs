using UnityEngine;
using UnityEngine.UIElements;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    public UIDocument battleUIDocument;
    private VisualElement root;
    private PlayerController curPlayer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        root = battleUIDocument.rootVisualElement;
        root.style.display = DisplayStyle.None; // 필드 탐색 중에는 숨김
    }

    public void StartBattle(ScriptableObject data, PlayerController player)
    {
        curPlayer = player;

        // 1. 전투 UI 켜기
        root.style.display = DisplayStyle.Flex;

        // 2. 데이터 바인딩 로직 호출 (중앙 보드에 문제 렌더링 등)
        // RenderProblem(data);
    }

    public void EndBattle()
    {
        // 1. 전투 UI 끄기
        root.style.display = DisplayStyle.None;

        // 2. 플레이어 이동 권한 복구
        if (curPlayer != null) curPlayer.enabled = true;

        Debug.Log("오류 해결! 필드로 복귀");
    }
}
