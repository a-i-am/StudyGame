using System;
using UnityEngine;

namespace StudyGame.Combat
{
    public class BattleFSM : MonoBehaviour
    {
        public BattlePhase CurrentPhase => currentState?.Phase ?? BattlePhase.None;

        public event Action<BattlePhase> OnPhaseChanged;

        private IBattleState currentState;

        public void TransitionTo(IBattleState nextState)
        {
            currentState?.Exit(this);
            currentState = nextState;
            currentState?.Enter(this);
            OnPhaseChanged?.Invoke(CurrentPhase);
        }

        private void Update()
        {
            currentState?.Execute(this);
        }

        public void Clear()
        {
            currentState?.Exit(this);
            currentState = null;
        }
    }
}
