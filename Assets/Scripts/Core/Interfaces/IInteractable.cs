using UnityEngine;

namespace StudyGame.Core
{
    public interface IInteractable
    {
        string InteractionPrompt { get; }
        bool CanInteract(GameObject instigator);
        void OnInteract(GameObject instigator);
    }
}
