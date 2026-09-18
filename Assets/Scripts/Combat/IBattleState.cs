namespace StudyGame.Combat
{
    public enum BattlePhase
    {
        None,
        BattleInit,
        PlayerTurn,
        EnemyTurn,
        Resolution,
        BattleEnd
    }

    public interface IBattleState
    {
        BattlePhase Phase { get; }
        void Enter(BattleFSM fsm);
        void Execute(BattleFSM fsm);
        void Exit(BattleFSM fsm);
    }
}
