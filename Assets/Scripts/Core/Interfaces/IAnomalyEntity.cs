using UnityEngine;

namespace StudyGame.Core
{
    public interface IAnomalyEntity
    {
        void TriggerStabilizeEffect();
        Transform GetMonsterTransform();
    }
}
