using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

[RequireComponent(typeof(UIDocument))]
public class MathProofUIDocumentController : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset uxmlDocument;
    [SerializeField] private PanelSettings panelSettings;

    private UIDocument uiDocument;
    private VisualElement rootElement;
    private VisualElement boardContainer;
    private ScrollView boardScrollView;
    private VisualElement targetSlot;
    private Label errorAlertHeader;
    private Label errorSubHeader;
    private Label errorHpLog;
    private VisualElement characterContainer;
    private VisualElement leftCharacterSlot;
    private VisualElement centerDeckArea;
    private ScrollView skillDeckScrollView;
    private VisualElement rightCharacterSlot;
    private Image playerStandingImage;
    private Image bossStandingImage;

    public VisualElement RootElement => rootElement;
    public VisualElement BoardContainer => boardContainer;
    public ScrollView BoardScrollView => boardScrollView;
    public VisualElement TargetSlot => targetSlot;
    public Label ErrorAlertHeader => errorAlertHeader;
    public Label ErrorSubHeader => errorSubHeader;
    public Label ErrorHpLog => errorHpLog;
    public VisualElement CharacterContainer => characterContainer;
    public VisualElement LeftCharacterSlot => leftCharacterSlot;
    public VisualElement CenterDeckArea => centerDeckArea;
    public ScrollView SkillDeckScrollView => skillDeckScrollView;
    public VisualElement RightCharacterSlot => rightCharacterSlot;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument != null)
        {
            if (panelSettings != null)
            {
                uiDocument.panelSettings = panelSettings;
            }
            if (uxmlDocument != null)
            {
                uiDocument.visualTreeAsset = uxmlDocument;
            }
        }
    }

    private void OnEnable()
    {
        InitializeUI();
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void Start()
    {
        InitializeUI();
        SubscribeEvents();
        SetDisplay(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseUIAndReturnToExploration();
        }
    }

    private void SubscribeEvents()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnSequenceStarted -= HandleSequence;
            StageManager.Instance.OnSequenceStarted += HandleSequence;
        }
    }

    private void UnsubscribeEvents()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnSequenceStarted -= HandleSequence;
        }
    }

    public void InitializeUI()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        if (uiDocument == null || uiDocument.rootVisualElement == null)
        {
            return;
        }

        rootElement = uiDocument.rootVisualElement.Q<VisualElement>("Root");
        boardContainer = uiDocument.rootVisualElement.Q<VisualElement>("TopHeaderContainer");
        boardScrollView = uiDocument.rootVisualElement.Q<ScrollView>("BoardScrollView");
        targetSlot = uiDocument.rootVisualElement.Q<VisualElement>("TargetSlot");

        errorAlertHeader = uiDocument.rootVisualElement.Q<Label>("ErrorAlertHeader");
        errorSubHeader = uiDocument.rootVisualElement.Q<Label>("ErrorSubHeader");
        errorHpLog = uiDocument.rootVisualElement.Q<Label>("ErrorHpLog");

        characterContainer = uiDocument.rootVisualElement.Q<VisualElement>("BottomHUDContainer");
        leftCharacterSlot = uiDocument.rootVisualElement.Q<VisualElement>("LeftCharacterSlot");
        centerDeckArea = uiDocument.rootVisualElement.Q<VisualElement>("CenterDeckArea");
        skillDeckScrollView = uiDocument.rootVisualElement.Q<ScrollView>("SkillDeckScrollView");
        rightCharacterSlot = uiDocument.rootVisualElement.Q<VisualElement>("RightCharacterSlot");

        playerStandingImage = uiDocument.rootVisualElement.Q<Image>("PlayerStandingImage");
        bossStandingImage = uiDocument.rootVisualElement.Q<Image>("BossStandingImage");
    }

    private void HandleSequence(SequenceNode node)
    {
        InitializeUI();
        if (node != null && node.sequenceType == SeqType.Combat)
        {
            SetDisplay(true);
            if (errorAlertHeader != null) errorAlertHeader.text = ">SYSTEM ERROR ALERT<";
            if (errorSubHeader != null) errorSubHeader.text = "f(x) = x^3 - 3x^2 + 4 발산 중";
            if (errorHpLog != null) errorHpLog.text = "inf HP";
            if (targetSlot != null) targetSlot.style.borderBottomColor = new StyleColor(new Color(0.35f, 0.58f, 0.88f, 0.5f));
            InitializeSkillDeck();
        }
        else
        {
            SetDisplay(false);
        }
    }

    private void InitializeSkillDeck()
    {
        if (skillDeckScrollView == null || targetSlot == null) return;

        var skillCards = skillDeckScrollView.Query<VisualElement>(className: "skill-card").ToList();
        foreach (var card in skillCards)
        {
            var dragHandler = new SkillBlockDragHandler(card, targetSlot, OnSkillDropped);
        }
    }

    private void OnSkillDropped(VisualElement skillCard)
    {
        if (skillCard == null) return;

        MathSkill usedSkill = MathSkill.Limit;
        if (skillCard.name.Contains("Log"))
        {
            usedSkill = MathSkill.Logarithm;
        }
        else if (skillCard.name.Contains("Derivative"))
        {
            usedSkill = MathSkill.Derivative;
        }
        else if (skillCard.name.Contains("Integral"))
        {
            usedSkill = MathSkill.Integral;
        }
        else if (skillCard.name.Contains("Limit"))
        {
            usedSkill = MathSkill.Limit;
        }
        else if (skillCard.name.Contains("Substitute"))
        {
            usedSkill = MathSkill.Substitute;
        }

        MathGimmick[] gimmicks = Object.FindObjectsByType<MathGimmick>(FindObjectsSortMode.None);
        bool anyCorrect = false;

        foreach (var gimmick in gimmicks)
        {
            if (gimmick != null)
            {
                if (gimmick.IsCorrectSkill(usedSkill))
                {
                    anyCorrect = true;
                }
                gimmick.ApplySkill(usedSkill);
            }
        }

        if (anyCorrect)
        {
            if (targetSlot != null) targetSlot.style.borderBottomColor = Color.cyan;

            switch (usedSkill)
            {
                case MathSkill.Logarithm:
                    if (errorSubHeader != null) errorSubHeader.text = "Logarithm 스케일 제어 적용. Enemy 1 & 2 로그 압축 완료.";
                    if (errorHpLog != null) errorHpLog.text = "HP: 스케일 로그 압축 완료";
                    break;
                case MathSkill.Derivative:
                    if (errorSubHeader != null) errorSubHeader.text = "Derivative 미분 분쇄 적용. Enemy 1 & 2 파편 분해 완료.";
                    if (errorHpLog != null) errorHpLog.text = "HP: 0 (미분 분쇄 완료)";
                    break;
                case MathSkill.Limit:
                    if (errorSubHeader != null) errorSubHeader.text = "Limit 수렴 제어 적용. Enemy 1 & 2 위치 수렴 완료.";
                    if (errorHpLog != null) errorHpLog.text = "HP: 수렴 제어 완료";
                    break;
                case MathSkill.Integral:
                    if (errorSubHeader != null) errorSubHeader.text = "Integral 적분 실체화 적용. Enemy 1 & 2 실체화 완료.";
                    if (errorHpLog != null) errorHpLog.text = "HP: 실체화 완료";
                    break;
                case MathSkill.Substitute:
                    if (errorSubHeader != null) errorSubHeader.text = "Substitute 대입 좌표축 변환 적용. Enemy 1 & 2 축 회전 완료.";
                    if (errorHpLog != null) errorHpLog.text = "HP: 좌표 대입 완료";
                    break;
            }

            StartCoroutine(CloseBattleUI());
        }
        else
        {
            if (targetSlot != null) targetSlot.style.borderBottomColor = Color.red;
            if (errorSubHeader != null) errorSubHeader.text = "오류: 부적절한 수학 스킬 연산! 방어막 제어 실패.";
            if (errorHpLog != null) errorHpLog.text = "HP: 발산 루프 지속 중... (다른 스킬을 시도하세요)";
        }
    }

    private IEnumerator CloseBattleUI()
    {
        yield return new WaitForSeconds(1.5f);
        CloseUIAndReturnToExploration();
    }

    public void CloseUIAndReturnToExploration()
    {
        SetDisplay(false);
        if (StageManager.Instance != null)
        {
            StageManager.Instance.ChangeState(StageManager.GameState.Exploration);
        }
    }

    public void SetDisplay(bool show)
    {
        InitializeUI();
        if (rootElement != null)
        {
            rootElement.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }
        if (uiDocument != null && uiDocument.rootVisualElement != null)
        {
            uiDocument.rootVisualElement.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    public void SetPlayerIllustration(Texture2D texture)
    {
        if (playerStandingImage != null && texture != null)
        {
            playerStandingImage.image = texture;
        }
    }

    public void SetBossIllustration(Texture2D texture)
    {
        if (bossStandingImage != null && texture != null)
        {
            bossStandingImage.image = texture;
        }
    }

    public void SetHpLogText(string text)
    {
        if (errorHpLog != null)
        {
            errorHpLog.text = text;
        }
    }
}
