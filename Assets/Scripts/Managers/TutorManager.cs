using System;
using System.Collections.Generic;
using UnityEngine;
using StudyGame.Data;
using StudyGame.Core;

namespace StudyGame.Managers
{
    [Serializable]
    public class StudentStatus
    {
        public string npcId;
        public bool isRescued;
        public int affectionLevel;
        public List<string> unlockedNodeIds = new List<string>();
    }

    public class TutorManager : MonoBehaviour
    {
        public static TutorManager Instance { get; private set; }

        private Dictionary<string, StudentStatus> studentStatuses = new Dictionary<string, StudentStatus>();

        public event Action<string, TutorSkillNodeData> OnSkillUnlocked;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool CanUnlockNode(string npcId, TutorSkillNodeData node)
        {
            if (node == null) return false;
            if (!studentStatuses.TryGetValue(npcId, out StudentStatus status) || !status.isRescued) return false;

            if (node.requiredConcept != null && ConceptArchiveManager.Instance != null)
            {
                if (!ConceptArchiveManager.Instance.IsUnlocked(node.requiredConcept)) return false;
            }

            foreach (var prereq in node.prerequisites)
            {
                if (prereq != null && !status.unlockedNodeIds.Contains(prereq.nodeId)) return false;
            }

            return true;
        }

        public bool TryUnlockNode(string npcId, TutorSkillNodeData node)
        {
            if (!CanUnlockNode(npcId, node)) return false;

            StudentStatus status = studentStatuses[npcId];
            if (status.unlockedNodeIds.Contains(node.nodeId)) return false;

            status.unlockedNodeIds.Add(node.nodeId);
            OnSkillUnlocked?.Invoke(npcId, node);
            return true;
        }

        public void SetStudentRescued(string npcId, bool rescued)
        {
            if (!studentStatuses.ContainsKey(npcId))
            {
                studentStatuses[npcId] = new StudentStatus { npcId = npcId };
            }
            studentStatuses[npcId].isRescued = rescued;
        }

        public StudentStatus GetStudentStatus(string npcId)
        {
            if (studentStatuses.TryGetValue(npcId, out StudentStatus status))
            {
                return status;
            }
            return null;
        }

        public List<StudentStatus> GetAllStudentStatuses()
        {
            return new List<StudentStatus>(studentStatuses.Values);
        }

        public void LoadStatuses(List<StudentStatusData> statuses)
        {
            studentStatuses.Clear();
            if (statuses == null) return;
            foreach (var status in statuses)
            {
                if (status != null && !string.IsNullOrEmpty(status.npcId))
                {
                    studentStatuses[status.npcId] = new StudentStatus
                    {
                        npcId = status.npcId,
                        isRescued = status.isRescued,
                        affectionLevel = status.affectionLevel,
                        unlockedNodeIds = new List<string>(status.unlockedNodeIds)
                    };
                }
            }
        }
    }
}
