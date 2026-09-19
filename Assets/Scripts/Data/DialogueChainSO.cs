using System;
using System.Collections.Generic;
using UnityEngine;

namespace StudyGame.Data
{
    public enum Speaker
    {
        Player,
        Doyoung,
        Jimin,
        Mugyeol,
        System,
        NPC
    }

    [Serializable]
    public class DialogueBlock
    {
        public Speaker Speaker;
        [TextArea(3, 5)] public string Text;
        public string Emotion; // Optional emotion/animation trigger
        public float TypingSpeed = 0.05f;
        public bool CameraShake = false;
        
        // Branching / Options
        public List<string> Options = new List<string>();
        public List<DialogueChainSO> NextChains = new List<DialogueChainSO>();
    }

    [CreateAssetMenu(fileName = "NewDialogueChain", menuName = "StudyGame/Dialogue Chain")]
    public class DialogueChainSO : ScriptableObject
    {
        public List<DialogueBlock> Blocks = new List<DialogueBlock>();
    }
}
