using UnityEngine;
using UnityEngine.UIElements;
using StudyGame.Player;
using StudyGame.Combat;
using StudyGame.Data;
using StudyGame.Managers;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("UI & FSM")]
    public UIDocument battleUIDocument;
    private VisualElement root;
    
    private BattleFSM fsm;
    private PlayerController curPlayer;
    private EnemyProfileSO curEnemy;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            fsm = gameObject.AddComponent<BattleFSM>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (battleUIDocument != null)
        {
            root = battleUIDocument.rootVisualElement;
            if (root != null)
            {
                root.style.display = DisplayStyle.None;
            }
        }
    }

    public void StartBattle(ScriptableObject data, PlayerController player)
    {
        curPlayer = player;
        curEnemy = data as EnemyProfileSO;

        if (root != null)
        {
            root.style.display = DisplayStyle.Flex;
        }

        CursorManager.Instance.RegisterModalOpen();
        
        // Start the FSM flow (Init State will be implemented later)
        // fsm.TransitionTo(new BattleInitState());
    }

    public void EndBattle()
    {
        fsm.Clear();

        if (root != null)
        {
            root.style.display = DisplayStyle.None;
        }

        CursorManager.Instance.RegisterModalClose();
    }
}
