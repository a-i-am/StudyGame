using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using StudyGame.Data;
using StudyGame.UI;

namespace StudyGame.Managers
{
    public class EpisodePlayer : MonoBehaviour
    {
        [Header("Settings")]
        public float typingSpeed = 0.03f;
        
        [Header("Runtime Data")]
        private EpisodeGraphData currentGraph;
        private string currentNodeGuid;
        private Dictionary<string, EpisodeNodeSaveData> nodeDict;
        
        private UIDocument dialogueUiDocument;
        private PhoneUIDocumentController phoneController;
        
        private VisualElement dialogueOverlay;
        private VisualElement characterStanding;
        private Label speakerNameLabel;
        private Label dialogueTextLabel;
        private VisualElement choicesContainer;
        
        private bool isTyping = false;
        private string currentFullText = "";
        private Coroutine typingCoroutine;
        
        // State for Dialogue Table
        private List<TableRowData> currentDialogueRows;
        private int currentDialogueRowIndex = 0;
        
        // State for SNS
        private bool isSNSPlaying = false;

        private void Awake()
        {
            InitializeUI();
        }

        private void Start()
        {
            // Auto-start for testing
            PlayEpisode(1);
        }

        private void InitializeUI()
        {
            UIDialogueController dialogueController = FindFirstObjectByType<UIDialogueController>(FindObjectsInactive.Include);
            if (dialogueController != null)
            {
                dialogueUiDocument = dialogueController.GetComponent<UIDocument>();
                if (dialogueUiDocument != null && dialogueUiDocument.rootVisualElement != null)
                {
                    var root = dialogueUiDocument.rootVisualElement;
                    dialogueOverlay = root.Q<VisualElement>("dialogue-overlay");
                    characterStanding = root.Q<VisualElement>("character-standing");
                    speakerNameLabel = root.Q<Label>("speaker-name");
                    dialogueTextLabel = root.Q<Label>("dialogue-text");
                    choicesContainer = root.Q<VisualElement>("choices-container");
                    
                    var dialogueBox = root.Q<VisualElement>("dialogue-box");
                    if (dialogueBox != null)
                    {
                        dialogueBox.RegisterCallback<ClickEvent>(OnDialogueBoxClicked);
                    }
                }
            }
            
            phoneController = FindFirstObjectByType<PhoneUIDocumentController>(FindObjectsInactive.Include);
            if (phoneController != null)
            {
                phoneController.InitializeUI(); 
                if (phoneController.PhoneFrame != null)
                {
                    phoneController.PhoneFrame.RegisterCallback<ClickEvent>(OnPhoneClicked);
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                Debug.Log("[EpisodePlayer] F5 눌림 - 에피소드 1 재생 시작");
                PlayEpisode(1);
            }
        }

        public void PlayEpisode(int episodeNumber)
        {
            currentGraph = Resources.Load<EpisodeGraphData>("Scenarios/EpisodeGraph");
            if (currentGraph == null || currentGraph.Nodes.Count == 0)
            {
                Debug.LogError($"[EpisodePlayer] EpisodeGraph 데이터를 찾을 수 없거나 비어 있습니다.");
                return;
            }

            nodeDict = new Dictionary<string, EpisodeNodeSaveData>();
            HashSet<string> targetNodes = new HashSet<string>();

            foreach (var node in currentGraph.Nodes)
            {
                nodeDict[node.NodeGuid] = node;
            }

            foreach (var link in currentGraph.NodeLinks)
            {
                targetNodes.Add(link.TargetNodeGuid);
            }

            // Find root (node with no incoming links)
            currentNodeGuid = null;
            foreach (var node in currentGraph.Nodes)
            {
                if (!targetNodes.Contains(node.NodeGuid))
                {
                    currentNodeGuid = node.NodeGuid;
                    break;
                }
            }

            if (string.IsNullOrEmpty(currentNodeGuid))
            {
                // Fallback to first node if graph is circular or invalid
                currentNodeGuid = currentGraph.Nodes[0].NodeGuid;
            }

            Debug.Log($"[EpisodePlayer] 에피소드 그래프 로드 완료. 루트 노드부터 시작합니다.");
            PlayCurrentNode();
        }

        private void PlayCurrentNode()
        {
            if (string.IsNullOrEmpty(currentNodeGuid) || !nodeDict.ContainsKey(currentNodeGuid))
            {
                Debug.Log("[EpisodePlayer] 에피소드 재생 완료");
                if (dialogueOverlay != null) dialogueOverlay.style.display = DisplayStyle.None;
                if (phoneController != null && phoneController.PhoneFrame != null) phoneController.PhoneFrame.style.display = DisplayStyle.None;
                return;
            }

            WorkspaceNodeData node = nodeDict[currentNodeGuid].NodeData;
            if (node == null)
            {
                GoToNextNode();
                return;
            }

            Debug.Log($"[EpisodePlayer] 노드 재생: {node.NodeTitle} (Type: {node.TemplateType})");

            if (node.TemplateType == "SNS")
            {
                ProcessSNS(node);
            }
            else 
            {
                ProcessDialogue(node);
            }
        }

        private void GoToNextNode()
        {
            if (currentGraph == null) return;
            
            var link = currentGraph.NodeLinks.FirstOrDefault(l => l.BaseNodeGuid == currentNodeGuid);
            if (link != null)
            {
                currentNodeGuid = link.TargetNodeGuid;
                PlayCurrentNode();
            }
            else
            {
                currentNodeGuid = null;
                PlayCurrentNode(); // will trigger end
            }
        }

        private void ProcessDialogue(WorkspaceNodeData node)
        {
            if (phoneController != null && phoneController.PhoneFrame != null)
            {
                phoneController.PhoneFrame.style.display = DisplayStyle.None;
            }

            DynamicProperty dialogueProp = node.Properties.FirstOrDefault(p => p.PropertyName == "Dialogues");
            if (dialogueProp != null && dialogueProp.TableRows != null && dialogueProp.TableRows.Count > 0)
            {
                currentDialogueRows = dialogueProp.TableRows;
                currentDialogueRowIndex = 0;
                
                if (dialogueOverlay != null)
                {
                    dialogueOverlay.style.display = DisplayStyle.Flex;
                }
                if (characterStanding != null)
                {
                    characterStanding.style.display = DisplayStyle.None;
                }
                if (choicesContainer != null)
                {
                    choicesContainer.Clear();
                }
                
                ShowNextDialogueLine();
            }
            else
            {
                Debug.LogWarning($"[EpisodePlayer] {node.NodeTitle}에 'Dialogues' 프로퍼티가 없습니다. 다음 노드로 넘어갑니다.");
                GoToNextNode();
            }
        }

        private void ShowNextDialogueLine()
        {
            if (currentDialogueRows == null || currentDialogueRowIndex >= currentDialogueRows.Count)
            {
                GoToNextNode();
                return;
            }

            TableRowData row = currentDialogueRows[currentDialogueRowIndex];
            string speaker = row.Cells.Count > 0 ? row.Cells[0] : "";
            string line = row.Cells.Count > 1 ? row.Cells[1] : "";
            
            if (speakerNameLabel != null) speakerNameLabel.text = speaker;
            
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypewriterRoutine(line));
            
            currentDialogueRowIndex++;
        }

        private IEnumerator TypewriterRoutine(string targetText)
        {
            isTyping = true;
            currentFullText = targetText ?? "";
            if (dialogueTextLabel != null) dialogueTextLabel.text = "";

            for (int i = 0; i < currentFullText.Length; i++)
            {
                if (dialogueTextLabel != null) dialogueTextLabel.text += currentFullText[i];
                yield return new WaitForSeconds(typingSpeed);
            }

            isTyping = false;
        }

        private void OnDialogueBoxClicked(ClickEvent evt)
        {
            if (isTyping)
            {
                if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                isTyping = false;
                if (dialogueTextLabel != null) dialogueTextLabel.text = currentFullText;
            }
            else
            {
                ShowNextDialogueLine();
            }
        }

        private void ProcessSNS(WorkspaceNodeData node)
        {
            if (dialogueOverlay != null)
            {
                dialogueOverlay.style.display = DisplayStyle.None;
            }

            if (phoneController != null)
            {
                if (phoneController.PhoneFrame != null)
                {
                    phoneController.PhoneFrame.style.display = DisplayStyle.Flex;
                }

                if (phoneController.MessageScroll != null)
                {
                    phoneController.MessageScroll.Clear();
                }

                DynamicProperty snsProp = node.Properties.FirstOrDefault(p => p.PropertyName == "SNS Feed");
                if (snsProp != null && snsProp.TableRows != null)
                {
                    isSNSPlaying = true;
                    StartCoroutine(AddSNSFeedsRoutine(snsProp.TableRows));
                }
                else
                {
                    Debug.LogWarning($"[EpisodePlayer] {node.NodeTitle}에 'SNS Feed' 프로퍼티가 없습니다. 다음 노드로 넘어갑니다.");
                    GoToNextNode();
                }
            }
            else
            {
                Debug.LogError("[EpisodePlayer] PhoneUIDocumentController를 찾을 수 없습니다.");
                GoToNextNode();
            }
        }

        private IEnumerator AddSNSFeedsRoutine(List<TableRowData> rows)
        {
            foreach (var row in rows)
            {
                string account = row.Cells.Count > 0 ? row.Cells[0] : "";
                string content = row.Cells.Count > 1 ? row.Cells[1] : "";
                
                if (phoneController != null)
                {
                    phoneController.AddMessage($"<b>{account}</b>\n{content}");
                }
                
                yield return new WaitForSeconds(0.5f);
            }
            
            isSNSPlaying = false;
        }

        private void OnPhoneClicked(ClickEvent evt)
        {
            if (!isSNSPlaying)
            {
                GoToNextNode();
            }
        }
    }
}
