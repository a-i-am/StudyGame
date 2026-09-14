using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using StudyGame.Managers;
using StudyGame.LLM;

namespace StudyGame.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class UIDialogueController : MonoBehaviour
    {
        public event Action OnDialogueComplete;

        [SerializeField] private SequenceGraphData autoStartGraph;
        [SerializeField] private NPCData activeNPC;
        [SerializeField] private float typingSpeed = 0.03f;

        private UIDocument uiDocument;
        private VisualElement dialogueOverlay;
        private VisualElement dialogueBox;
        private VisualElement characterStanding;
        private Label speakerNameLabel;
        private Label dialogueTextLabel;
        private VisualElement choicesContainer;
        private Label apDisplayLabel;

        private TextField customQuestionField;
        private Button sendQuestionButton;
        private Label loadingIndicator;

        private SequenceGraphData currentGraph;
        private SequenceNode currentNode;

        private Queue<DialogueLine> dialogueQueue = new Queue<DialogueLine>();
        private Stack<SequenceNode> navigationStack = new Stack<SequenceNode>();

        private Queue<char> streamCharQueue = new Queue<char>();
        private Coroutine typingCoroutine;
        private Coroutine streamTypewriterCoroutine;
        private bool isTyping = false;
        private bool isStreamingActive = false;
        private string currentFullText = "";

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            VisualElement root = uiDocument.rootVisualElement;

            dialogueOverlay = root.Q<VisualElement>("dialogue-overlay");
            dialogueBox = root.Q<VisualElement>("dialogue-box");
            characterStanding = root.Q<VisualElement>("character-standing");
            speakerNameLabel = root.Q<Label>("speaker-name");
            dialogueTextLabel = root.Q<Label>("dialogue-text");
            choicesContainer = root.Q<VisualElement>("choices-container");
            apDisplayLabel = root.Q<Label>("ap-display-label");

            customQuestionField = root.Q<TextField>("custom-question-field");
            sendQuestionButton = root.Q<Button>("send-question-button");
            loadingIndicator = root.Q<Label>("loading-indicator");

            if (dialogueBox != null)
            {
                dialogueBox.RegisterCallback<ClickEvent>(OnDialogueBoxClicked);
            }

            if (sendQuestionButton != null)
            {
                sendQuestionButton.clicked += OnSendCustomQuestion;
            }

            if (dialogueOverlay != null)
            {
                dialogueOverlay.style.display = DisplayStyle.None;
            }
        }

        private void Start()
        {
            if (autoStartGraph != null)
            {
                StartDialogue(autoStartGraph);
            }
        }

        private void OnEnable()
        {
            StartCoroutine(SubscribeToDeductionEngine());
        }

        private IEnumerator SubscribeToDeductionEngine()
        {
            while (DeductionRuleEngine.Instance == null)
            {
                yield return null;
            }
            DeductionRuleEngine.Instance.OnAPChanged += UpdateAPDisplay;
            UpdateAPDisplay(DeductionRuleEngine.Instance.CurrentAP);
        }

        private void OnDisable()
        {
            if (DeductionRuleEngine.Instance != null)
            {
                DeductionRuleEngine.Instance.OnAPChanged -= UpdateAPDisplay;
            }
        }

        private void UpdateAPDisplay(int currentAP)
        {
            if (apDisplayLabel != null)
            {
                apDisplayLabel.text = $"AP: {currentAP}";
            }
        }

        public void SetActiveNPC(NPCData npc)
        {
            activeNPC = npc;
        }

        public void StartDialogue(SequenceGraphData graph)
        {
            if (graph == null || graph.allNodes == null || graph.allNodes.Count == 0) return;

            currentGraph = graph;
            navigationStack.Clear();
            SequenceNode startNode = graph.entryNode != null ? graph.entryNode : graph.allNodes[0];

            EnsureDeductionSession(graph);

            if (dialogueOverlay != null)
            {
                dialogueOverlay.style.display = DisplayStyle.Flex;
            }

            ShowNode(startNode, false);
        }

        private void EnsureDeductionSession(SequenceGraphData graph)
        {
            if (DeductionRuleEngine.Instance == null || graph == null) return;

            if (DeductionRuleEngine.Instance.CandidatePool == null || DeductionRuleEngine.Instance.CandidatePool.Count == 0)
            {
                List<ConceptData> candidates = new List<ConceptData>();
                ConceptData targetConcept = null;

                if (graph.entryNode != null && graph.entryNode.unlockConcept != null)
                {
                    targetConcept = graph.entryNode.unlockConcept;
                }

                foreach (var node in graph.allNodes)
                {
                    if (node == null || node.unlockConcept == null) continue;
                    if (!candidates.Contains(node.unlockConcept))
                    {
                        candidates.Add(node.unlockConcept);
                    }
                    if (node.unlockConcept.prerequisites != null)
                    {
                        foreach (var prereq in node.unlockConcept.prerequisites)
                        {
                            if (prereq != null && !candidates.Contains(prereq))
                            {
                                candidates.Add(prereq);
                            }
                        }
                    }
                }

                if (targetConcept == null && candidates.Count > 0)
                {
                    targetConcept = candidates[0];
                }

                DeductionRuleEngine.Instance.InitializeSession(targetConcept, 8, candidates);
            }
        }

        public void ShowNode(SequenceNode node, bool isReturning = false)
        {
            if (node == null)
            {
                EndDialogue();
                return;
            }

            currentNode = node;

            if (node.unlockConcept != null && ConceptArchiveManager.Instance != null)
            {
                bool isTargetOfDeduction = DeductionRuleEngine.Instance != null && DeductionRuleEngine.Instance.TargetConcept != null && DeductionRuleEngine.Instance.TargetConcept.conceptId == node.unlockConcept.conceptId;
                if (!isTargetOfDeduction)
                {
                    ConceptArchiveManager.Instance.TryUnlockConcept(node.unlockConcept);
                }
            }

            if (!string.IsNullOrEmpty(node.discoveredClue) && DeductionRuleEngine.Instance != null)
            {
                DeductionRuleEngine.Instance.CollectClue(node.discoveredClue);
            }

            NPCType personaType = activeNPC != null ? activeNPC.personalityType : NPCType.Standard;
            List<DialogueLine> lines = node.GetDialogueLines(personaType);

            dialogueQueue.Clear();
            if (lines != null)
            {
                foreach (DialogueLine line in lines)
                {
                    dialogueQueue.Enqueue(line);
                }
            }

            if (isReturning && dialogueQueue.Count > 0)
            {
                DialogueLine lastLine = null;
                while (dialogueQueue.Count > 0)
                {
                    lastLine = dialogueQueue.Dequeue();
                }
                RenderLineInstant(lastLine);
                BuildChoices();
            }
            else
            {
                NextDialogueLine();
            }
        }

        private void OnDialogueBoxClicked(ClickEvent evt)
        {
            if (isStreamingActive) return;

            if (isTyping)
            {
                if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                isTyping = false;
                if (dialogueTextLabel != null) dialogueTextLabel.text = currentFullText;

                if (dialogueQueue.Count == 0)
                {
                    BuildChoices();
                }
            }
            else
            {
                if (dialogueQueue.Count > 0)
                {
                    NextDialogueLine();
                }
            }
        }

        private void NextDialogueLine()
        {
            if (dialogueQueue.Count == 0)
            {
                BuildChoices();
                return;
            }

            DialogueLine line = dialogueQueue.Dequeue();
            RenderStandingAndSpeaker(line);

            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypewriterRoutine(line.text));
        }

        private void RenderStandingAndSpeaker(DialogueLine line)
        {
            Sprite standing = line != null && line.standingSprite != null ? line.standingSprite : (activeNPC != null ? activeNPC.defaultStanding : (currentNode != null ? currentNode.characterStanding : null));

            if (characterStanding != null)
            {
                if (standing != null)
                {
                    characterStanding.style.backgroundImage = new StyleBackground(standing);
                    characterStanding.style.display = DisplayStyle.Flex;
                }
                else
                {
                    characterStanding.style.backgroundImage = null;
                    characterStanding.style.display = DisplayStyle.None;
                }
            }

            if (speakerNameLabel != null)
            {
                if (line != null && !string.IsNullOrEmpty(line.speakerName))
                    speakerNameLabel.text = line.speakerName;
                else if (activeNPC != null && !string.IsNullOrEmpty(activeNPC.npcName))
                    speakerNameLabel.text = activeNPC.npcName;
                else if (currentNode != null && currentNode.snsData != null && !string.IsNullOrEmpty(currentNode.snsData.senderName))
                    speakerNameLabel.text = currentNode.snsData.senderName;
                else
                    speakerNameLabel.text = currentNode != null ? currentNode.name : "Speaker";
            }
        }

        private void RenderLineInstant(DialogueLine line)
        {
            RenderStandingAndSpeaker(line);
            currentFullText = line != null ? line.text : "";
            if (dialogueTextLabel != null) dialogueTextLabel.text = currentFullText;
            isTyping = false;
        }

        private IEnumerator TypewriterRoutine(string targetText)
        {
            isTyping = true;
            currentFullText = targetText ?? "";
            if (dialogueTextLabel != null) dialogueTextLabel.text = "";
            if (choicesContainer != null) choicesContainer.Clear();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < currentFullText.Length; i++)
            {
                sb.Append(currentFullText[i]);
                if (dialogueTextLabel != null) dialogueTextLabel.text = sb.ToString();
                yield return new WaitForSeconds(typingSpeed);
            }

            isTyping = false;

            if (dialogueQueue.Count == 0)
            {
                BuildChoices();
            }
        }

        private void BuildChoices()
        {
            if (choicesContainer == null) return;
            choicesContainer.Clear();

            if (currentNode == null) return;

            InjectPrerequisiteChoices();

            if (currentNode.choices != null && currentNode.choices.Count > 0)
            {
                foreach (DialogueChoice choice in currentNode.choices)
                {
                    Button choiceBtn = new Button();
                    choiceBtn.text = choice.choiceText;
                    choiceBtn.AddToClassList("choice-button");
                    DialogueChoice targetChoice = choice;
                    choiceBtn.clicked += () => OnChoiceSelected(targetChoice);
                    choicesContainer.Add(choiceBtn);
                }
            }

            if (navigationStack.Count > 0)
            {
                SequenceNode parentNode = navigationStack.Peek();
                Button returnBtn = new Button();
                returnBtn.text = $"↩ [돌아가기] {parentNode.name} 맥락으로";
                returnBtn.AddToClassList("choice-button");
                returnBtn.clicked += OnReturnToParentContext;
                choicesContainer.Add(returnBtn);
            }

            Button deductionBtn = new Button();
            deductionBtn.text = "🔍 [정답 추론]";
            deductionBtn.AddToClassList("choice-button");
            deductionBtn.clicked += OpenDeductionModal;
            choicesContainer.Add(deductionBtn);

            if ((currentNode.choices == null || currentNode.choices.Count == 0) && navigationStack.Count == 0)
            {
                Button completeBtn = new Button();
                completeBtn.text = "대화 완료";
                completeBtn.AddToClassList("choice-button");
                completeBtn.clicked += EndDialogue;
                choicesContainer.Add(completeBtn);
            }
        }

        private void InjectPrerequisiteChoices()
        {
            if (currentNode == null || currentNode.unlockConcept == null || currentGraph == null) return;

            ConceptData concept = currentNode.unlockConcept;
            if (concept.prerequisites == null) return;

            foreach (ConceptData prereq in concept.prerequisites)
            {
                if (prereq == null) continue;
                bool isUnlocked = ConceptArchiveManager.Instance != null && ConceptArchiveManager.Instance.IsUnlocked(prereq);
                if (!isUnlocked)
                {
                    SequenceNode prereqNode = currentGraph.FindNodeByConcept(prereq);
                    if (prereqNode != null)
                    {
                        Button prereqBtn = new Button();
                        prereqBtn.text = $"❓ [기초 질문] \"{prereq.title}\"(이)가 대체 뭐야?";
                        prereqBtn.AddToClassList("choice-button");
                        Color orangeColor = new Color(0.9f, 0.4f, 0.2f);
                        prereqBtn.style.borderTopColor = orangeColor;
                        prereqBtn.style.borderBottomColor = orangeColor;
                        prereqBtn.style.borderLeftColor = orangeColor;
                        prereqBtn.style.borderRightColor = orangeColor;
                        SequenceNode target = prereqNode;
                        prereqBtn.clicked += () =>
                        {
                            navigationStack.Push(currentNode);
                            ShowNode(target, false);
                        };
                        choicesContainer.Add(prereqBtn);
                    }
                }
            }
        }

        private void OnChoiceSelected(DialogueChoice choice)
        {
            if (choice == null) return;

            if (choice.isSubBranch && currentNode != null)
            {
                navigationStack.Push(currentNode);
            }

            ShowNode(choice.targetNode, false);
        }

        private void OnReturnToParentContext()
        {
            if (navigationStack.Count > 0)
            {
                SequenceNode parentNode = navigationStack.Pop();
                ShowNode(parentNode, true);
            }
        }

        private void OpenDeductionModal()
        {
            UIDeductionModalController deductionModal = FindFirstObjectByType<UIDeductionModalController>();
            if (deductionModal != null)
            {
                deductionModal.OpenModal();
            }
        }

        private void OnSendCustomQuestion()
        {
            if (customQuestionField == null || string.IsNullOrEmpty(customQuestionField.value)) return;
            if (isStreamingActive) return;

            if (DeductionRuleEngine.Instance != null)
            {
                bool canProceed = DeductionRuleEngine.Instance.TryConsumeAP(1);
                if (!canProceed)
                {
                    OpenDeductionModal();
                    return;
                }
            }

            string userQuestion = customQuestionField.value;
            customQuestionField.value = "";

            if (choicesContainer != null) choicesContainer.Clear();
            if (loadingIndicator != null) loadingIndicator.style.display = DisplayStyle.Flex;

            DialogueLine npcLine = new DialogueLine
            {
                speakerName = activeNPC != null ? activeNPC.npcName : "AI Tutor",
                text = "",
                standingSprite = activeNPC != null ? activeNPC.defaultStanding : null
            };
            RenderStandingAndSpeaker(npcLine);

            if (dialogueTextLabel != null) dialogueTextLabel.text = "";

            streamCharQueue.Clear();
            isStreamingActive = true;

            List<string> forbiddenWords = new List<string>();
            if (DeductionRuleEngine.Instance != null && DeductionRuleEngine.Instance.TargetConcept != null)
            {
                ConceptData target = DeductionRuleEngine.Instance.TargetConcept;
                if (!string.IsNullOrEmpty(target.title)) forbiddenWords.Add(target.title);
                if (!string.IsNullOrEmpty(target.conceptId)) forbiddenWords.Add(target.conceptId);
            }
            else if (currentNode != null && currentNode.unlockConcept != null)
            {
                ConceptData target = currentNode.unlockConcept;
                if (!string.IsNullOrEmpty(target.title)) forbiddenWords.Add(target.title);
                if (!string.IsNullOrEmpty(target.conceptId)) forbiddenWords.Add(target.conceptId);
            }

            StreamingSpoilerBuffer spoilerBuffer = new StreamingSpoilerBuffer(forbiddenWords, safeChar =>
            {
                streamCharQueue.Enqueue(safeChar);
            }, 12);

            if (streamTypewriterCoroutine != null) StopCoroutine(streamTypewriterCoroutine);
            streamTypewriterCoroutine = StartCoroutine(SmoothStreamTypewriterRoutine());

            ConceptData targetConcept = currentNode != null ? currentNode.unlockConcept : null;
            string systemPrompt = LLMContextBuilder.BuildSystemPrompt(currentNode, activeNPC, targetConcept);

            if (LLMStreamSender.Instance != null)
            {
                LLMStreamSender.Instance.StartStream(systemPrompt, userQuestion,
                    token =>
                    {
                        if (loadingIndicator != null && loadingIndicator.style.display == DisplayStyle.Flex)
                        {
                            loadingIndicator.style.display = DisplayStyle.None;
                        }
                        spoilerBuffer.AppendChunk(token);
                    },
                    () =>
                    {
                        spoilerBuffer.FlushRemaining();
                        isStreamingActive = false;
                    },
                    errorText =>
                    {
                        if (loadingIndicator != null) loadingIndicator.style.display = DisplayStyle.None;
                        Debug.LogError($"[LLM Stream Error]: {errorText}");
                        isStreamingActive = false;
                    });
            }
            else
            {
                isStreamingActive = false;
                if (loadingIndicator != null) loadingIndicator.style.display = DisplayStyle.None;
            }
        }

        private IEnumerator SmoothStreamTypewriterRoutine()
        {
            StringBuilder sb = new StringBuilder();

            while (isStreamingActive || streamCharQueue.Count > 0)
            {
                if (streamCharQueue.Count > 0)
                {
                    char c = streamCharQueue.Dequeue();
                    sb.Append(c);
                    if (dialogueTextLabel != null) dialogueTextLabel.text = sb.ToString();
                    yield return new WaitForSeconds(typingSpeed);
                }
                else
                {
                    yield return null;
                }
            }

            BuildChoices();
        }

        private void EndDialogue()
        {
            if (dialogueOverlay != null)
            {
                dialogueOverlay.style.display = DisplayStyle.None;
            }
            OnDialogueComplete?.Invoke();
        }
    }
}
