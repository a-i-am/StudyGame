using System;
using System.Collections.Generic;
using UnityEngine;

public enum SeqType { Combat, Narrative, Exploration }

[Serializable]
public class DialogueChoice
{
    public string portGuid;
    public string choiceText = "Next Choice";
    public SequenceNode targetNode;
    public bool isSubBranch = true;
}

[Serializable]
public class PersonaDialogueGroup
{
    public NPCType personality;
    public List<DialogueLine> lines = new List<DialogueLine>();
}

[CreateAssetMenu(fileName = "New Sequence Node", menuName = "StudyGame/Data/Sequence Node")]
public class SequenceNode : ScriptableObject
{
    public string guid;
    public Vector2 graphPosition;
    public SeqType sequenceType;
    public GameObject mapPrefab;
    public Sprite characterStanding;
    public SNSData snsData;
    public ConceptData unlockConcept;

    [TextArea(2, 5)]
    public string discoveredClue;
    public int apCost = 1;

    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
    public List<PersonaDialogueGroup> personaDialogues = new List<PersonaDialogueGroup>();

    [Obsolete("Use choices instead.")]
    public SequenceNode nextNode;

    public List<DialogueChoice> choices = new List<DialogueChoice>();

    public List<DialogueLine> GetDialogueLines(NPCType type)
    {
        if (personaDialogues != null && personaDialogues.Count > 0)
        {
            PersonaDialogueGroup match = personaDialogues.Find(p => p != null && p.personality == type && p.lines != null && p.lines.Count > 0);
            if (match != null) return match.lines;

            PersonaDialogueGroup standard = personaDialogues.Find(p => p != null && p.personality == NPCType.Standard && p.lines != null && p.lines.Count > 0);
            if (standard != null) return standard.lines;
        }

        return dialogueLines != null ? dialogueLines : new List<DialogueLine>();
    }
}