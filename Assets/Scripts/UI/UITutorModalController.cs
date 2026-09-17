using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using StudyGame.Data;
using StudyGame.Managers;

namespace StudyGame.UI
{
    public class UITutorModalController : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;

        private VisualElement rootVisualElement;
        private VisualElement skillTreeContainer;
        private Label lblStudentName;
        private Label lblSkillDescription;
        private Button btnUnlockSkill;
        private Button btnClose;

        private string currentNpcId;
        private TutorSkillNodeData selectedNode;

        private void Awake()
        {
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }
        }

        private void OnEnable()
        {
            if (uiDocument != null)
            {
                rootVisualElement = uiDocument.rootVisualElement;
                InitializeUI();
            }
        }

        private void InitializeUI()
        {
            if (rootVisualElement == null) return;

            skillTreeContainer = rootVisualElement.Q<VisualElement>("skill-tree-container");
            lblStudentName = rootVisualElement.Q<Label>("lbl-student-name");
            lblSkillDescription = rootVisualElement.Q<Label>("lbl-skill-description");
            btnUnlockSkill = rootVisualElement.Q<Button>("btn-unlock-skill");
            btnClose = rootVisualElement.Q<Button>("btn-close");

            if (btnUnlockSkill != null)
            {
                btnUnlockSkill.clicked += OnUnlockClicked;
            }

            if (btnClose != null)
            {
                btnClose.clicked += HideModal;
            }
        }

        public void DisplayTutorSession(string npcId, string studentName, List<TutorSkillNodeData> skillNodes)
        {
            currentNpcId = npcId;
            if (lblStudentName != null)
            {
                lblStudentName.text = studentName;
            }

            if (skillTreeContainer != null)
            {
                skillTreeContainer.Clear();
                if (skillNodes != null)
                {
                    foreach (var node in skillNodes)
                    {
                        if (node == null) continue;

                        Button nodeBtn = new Button();
                        nodeBtn.text = node.skillName;
                        nodeBtn.AddToClassList("tutor-skill-btn");

                        bool canUnlock = TutorManager.Instance != null && TutorManager.Instance.CanUnlockNode(npcId, node);
                        nodeBtn.SetEnabled(canUnlock);

                        nodeBtn.clicked += () => SelectSkillNode(node);
                        skillTreeContainer.Add(nodeBtn);
                    }
                }
            }

            ShowModal();
        }

        private void SelectSkillNode(TutorSkillNodeData node)
        {
            selectedNode = node;
            if (lblSkillDescription != null && node != null)
            {
                lblSkillDescription.text = $"{node.skillName}\n{node.description}";
            }

            if (btnUnlockSkill != null)
            {
                bool canUnlock = TutorManager.Instance != null && TutorManager.Instance.CanUnlockNode(currentNpcId, node);
                btnUnlockSkill.SetEnabled(canUnlock);
            }
        }

        private void OnUnlockClicked()
        {
            if (selectedNode != null && TutorManager.Instance != null)
            {
                bool success = TutorManager.Instance.TryUnlockNode(currentNpcId, selectedNode);
                if (success)
                {
                    if (lblSkillDescription != null)
                    {
                        lblSkillDescription.text = $"[해금 완료] {selectedNode.skillName}";
                    }
                }
            }
        }

        public void ShowModal()
        {
            if (rootVisualElement != null)
            {
                rootVisualElement.style.display = DisplayStyle.Flex;
            }
        }

        public void HideModal()
        {
            if (rootVisualElement != null)
            {
                rootVisualElement.style.display = DisplayStyle.None;
            }
        }
    }
}
