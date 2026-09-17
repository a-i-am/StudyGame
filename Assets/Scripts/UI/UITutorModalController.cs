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
        private VisualElement modalOverlay;
        private VisualElement skillTreeContainer;
        private Label lblStudentName;
        private Label lblSkillDescription;
        private Button btnUnlockSkill;
        private Button btnClose;

        private string currentNpcId = "doyoung";
        private TutorSkillNodeData selectedNode;

        private void Awake()
        {
            EnsureUIInitialized();
        }

        private void OnEnable()
        {
            EnsureUIInitialized();
        }



        private void EnsureUIInitialized()
        {
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }

            if (uiDocument != null && uiDocument.rootVisualElement != null)
            {
                rootVisualElement = uiDocument.rootVisualElement;
                InitializeUI();
            }
        }

        private void InitializeUI()
        {
            if (rootVisualElement == null) return;

            modalOverlay = rootVisualElement.Q<VisualElement>("modal-overlay");
            skillTreeContainer = rootVisualElement.Q<VisualElement>("skill-tree-container");
            lblStudentName = rootVisualElement.Q<Label>("lbl-student-name");
            lblSkillDescription = rootVisualElement.Q<Label>("lbl-skill-description");
            btnUnlockSkill = rootVisualElement.Q<Button>("btn-unlock-skill");
            btnClose = rootVisualElement.Q<Button>("btn-close");

            if (btnUnlockSkill != null)
            {
                btnUnlockSkill.clicked -= OnUnlockClicked;
                btnUnlockSkill.clicked += OnUnlockClicked;
            }

            if (btnClose != null)
            {
                btnClose.clicked -= HideModal;
                btnClose.clicked += HideModal;
            }
        }

        public void DisplayTutorSession(string npcId, string studentName, List<TutorSkillNodeData> skillNodes)
        {
            EnsureUIInitialized();
            PopulateSkillTree(npcId, studentName, skillNodes);
            ShowModal();
        }

        private void PopulateSkillTree(string npcId, string studentName, List<TutorSkillNodeData> skillNodes)
        {
            currentNpcId = npcId;
            if (lblStudentName != null)
            {
                lblStudentName.text = $"{studentName} 과외 지도";
            }

            if (skillNodes == null || skillNodes.Count == 0)
            {
                skillNodes = CreateFallbackSkillNodes();
            }

            if (skillTreeContainer != null)
            {
                skillTreeContainer.Clear();
                foreach (var node in skillNodes)
                {
                    if (node == null) continue;

                    Button nodeBtn = new Button();
                    string nodeName = !string.IsNullOrEmpty(node.skillName) ? node.skillName : (!string.IsNullOrEmpty(node.nodeId) ? node.nodeId : node.name);
                    nodeBtn.text = nodeName;
                    nodeBtn.style.width = 160;
                    nodeBtn.style.height = 60;
                    nodeBtn.style.marginTop = 8;
                    nodeBtn.style.marginBottom = 8;
                    nodeBtn.style.marginLeft = 8;
                    nodeBtn.style.marginRight = 8;
                    nodeBtn.style.backgroundColor = new StyleColor(new Color(0.2f, 0.4f, 0.7f, 1f));
                    nodeBtn.style.color = new StyleColor(Color.white);
                    nodeBtn.style.borderTopLeftRadius = 6;
                    nodeBtn.style.borderTopRightRadius = 6;
                    nodeBtn.style.borderBottomLeftRadius = 6;
                    nodeBtn.style.borderBottomRightRadius = 6;

                    TutorSkillNodeData targetNode = node;
                    nodeBtn.clicked += () => SelectSkillNode(targetNode);
                    skillTreeContainer.Add(nodeBtn);
                }
            }
        }

        private List<TutorSkillNodeData> CreateFallbackSkillNodes()
        {
            var list = new List<TutorSkillNodeData>();

            var n1 = ScriptableObject.CreateInstance<TutorSkillNodeData>();
            n1.nodeId = "tutor_node_01";
            n1.skillName = "수열의 이해";
            n1.description = "AP 소비량을 1 감소시키고 질문 조립 속도를 높입니다.";
            list.Add(n1);

            var n2 = ScriptableObject.CreateInstance<TutorSkillNodeData>();
            n2.nodeId = "tutor_node_02";
            n2.skillName = "극한 직관 강화";
            n2.description = "이상현상 타격 시 보너스 추론 점수를 획득합니다.";
            list.Add(n2);

            return list;
        }

        private void SelectSkillNode(TutorSkillNodeData node)
        {
            selectedNode = node;
            if (lblSkillDescription != null && node != null)
            {
                string descStr = !string.IsNullOrEmpty(node.description) ? node.description : "선택한 과외 지도 패시브 스킬 효과입니다.";
                string nameStr = !string.IsNullOrEmpty(node.skillName) ? node.skillName : (!string.IsNullOrEmpty(node.nodeId) ? node.nodeId : node.name);
                lblSkillDescription.text = $"[{nameStr}]\n{descStr}";
            }

            if (btnUnlockSkill != null)
            {
                btnUnlockSkill.SetEnabled(true);
            }
        }

        private void OnUnlockClicked()
        {
            if (selectedNode != null && TutorManager.Instance != null)
            {
                bool success = TutorManager.Instance.TryUnlockNode(currentNpcId, selectedNode);
                string nameStr = !string.IsNullOrEmpty(selectedNode.skillName) ? selectedNode.skillName : selectedNode.nodeId;
                if (success)
                {
                    if (lblSkillDescription != null)
                    {
                        lblSkillDescription.text = $"[해금 완료] {nameStr}";
                    }
                }
                else
                {
                    if (lblSkillDescription != null)
                    {
                        lblSkillDescription.text = $"[스킬 적용 완료] {nameStr}";
                    }
                }
            }
        }

        public void ShowModal()
        {
            EnsureUIInitialized();

            if (skillTreeContainer != null && skillTreeContainer.childCount == 0)
            {
                PopulateSkillTree("doyoung", "도영", CreateFallbackSkillNodes());
            }

            if (rootVisualElement != null)
            {
                rootVisualElement.style.display = DisplayStyle.Flex;
            }

            if (modalOverlay != null)
            {
                modalOverlay.style.display = DisplayStyle.Flex;
            }

            CursorManager.Instance.RegisterModalOpen();
        }

        public void HideModal()
        {
            if (modalOverlay != null)
            {
                modalOverlay.style.display = DisplayStyle.None;
            }

            if (rootVisualElement != null)
            {
                rootVisualElement.style.display = DisplayStyle.None;
            }

            CursorManager.Instance.RegisterModalClose();
        }
    }
}
