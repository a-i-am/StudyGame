using UnityEngine;
using System;

public class StageManager : MonoBehaviour
{
    private static StageManager _instance;
    public static StageManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<StageManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("StageManager");
                    _instance = go.AddComponent<StageManager>();
                }
            }
            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }

    public enum GameState { Exploration, Combat, Narrative }
    public GameState CurrentState { get; private set; }

    public Action<GameState> OnStateChanged;
    public Action<SequenceNode> OnSequenceStarted;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
        Debug.Log($"[StageManager] 시스템 상태 전환: {newState}");
    }

    public void ExecuteSequence(SequenceNode node)
    {
        if (node == null) return;

        OnSequenceStarted?.Invoke(node);

        switch (node.sequenceType)
        {
            case SeqType.Exploration:
                ChangeState(GameState.Exploration);
                break;
            case SeqType.Combat:
                ChangeState(GameState.Combat);
                break;
            case SeqType.Narrative:
                ChangeState(GameState.Narrative);

                if (node.snsData != null && ArchiveManager.Instance != null)
                {
                    ArchiveManager.Instance.UnlockDiary(node.snsData.unlockDiaryKeyword);
                }
                break;
        }
    }
}