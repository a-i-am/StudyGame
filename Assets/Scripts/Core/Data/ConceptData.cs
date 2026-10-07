using System;
using System.Collections.Generic;
using UnityEngine;

public enum SubjectType
{
    Calculus,
    SequenceLimit,
    ProbabilityStatistics
}

[CreateAssetMenu(fileName = "New Concept Data", menuName = "StudyGame/Data/Concept Data")]
public class ConceptData : ScriptableObject
{
    public string conceptId;
    public string title;
    public SubjectType subject;
    public List<ConceptData> prerequisites = new List<ConceptData>();
    [TextArea(2, 5)]
    public string archiveSummary;
    [TextArea(3, 6)]
    public string loreFlavorText;
    public Sprite diaryIcon;
    public AudioClip cassetteAudio;
}
