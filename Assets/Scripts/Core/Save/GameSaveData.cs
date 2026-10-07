using System;
using System.Collections.Generic;

namespace StudyGame.Core
{
    [Serializable]
    public class StudentStatusData
    {
        public string npcId;
        public bool isRescued;
        public int affectionLevel;
        public List<string> unlockedNodeIds = new List<string>();
    }

    [Serializable]
    public class GameSaveData
    {
        public int saveVersion = 1;
        public string lastSavedTimestamp;
        public List<string> unlockedConceptIds = new List<string>();
        public List<string> collectedSentenceItemIds = new List<string>();
        public List<StudentStatusData> studentStatuses = new List<StudentStatusData>();
        public List<string> clearedStageIds = new List<string>();
    }
}
