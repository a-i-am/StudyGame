using UnityEngine;
using UnityEngine.UIElements;
using StudyGame.Player;

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

        if (root != null)
        {
            root.style.display = DisplayStyle.Flex;
        }
    }

    public void EndBattle()
    {
        if (root != null)
        {
            root.style.display = DisplayStyle.None;
        }

        if (curPlayer != null)
        {
            curPlayer.SetMovementEnabled(true);
        }
    }
}
