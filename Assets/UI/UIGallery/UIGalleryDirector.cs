using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIGalleryDirector : MonoBehaviour
{
    [Serializable]
    public class UITemplateItem
    {
        public string displayName;
        public string description;
        public VisualTreeAsset templateAsset;
        public string assetPath;
    }

    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private List<UITemplateItem> templates = new List<UITemplateItem>();

    private VisualElement _galleryGrid;
    private VisualElement _inspectorOverlay;
    private Label _inspectorTitle;
    private VisualElement _inspectorViewport;
    private Button _inspectorCloseBtn;
    private Label _countBadge;

    private void Awake()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }
    }

    private void Start()
    {
        if (uiDocument == null)
        {
            return;
        }

        var root = uiDocument.rootVisualElement;
        if (root == null)
        {
            return;
        }

        _galleryGrid = root.Q<VisualElement>("gallery-grid");
        _inspectorOverlay = root.Q<VisualElement>("inspector-overlay");
        _inspectorTitle = root.Q<Label>("inspector-title");
        _inspectorViewport = root.Q<VisualElement>("inspector-viewport");
        _inspectorCloseBtn = root.Q<Button>("inspector-close-btn");
        _countBadge = root.Q<Label>("gallery-count-badge");

        if (_inspectorCloseBtn != null)
        {
            _inspectorCloseBtn.clicked += CloseInspector;
        }

        BuildGalleryCards();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseInspector();
        }
    }

    private void BuildGalleryCards()
    {
        if (_galleryGrid == null)
        {
            return;
        }

        _galleryGrid.Clear();

        if (_countBadge != null)
        {
            _countBadge.text = $"{templates.Count} Templates";
        }

        for (int i = 0; i < templates.Count; i++)
        {
            var item = templates[i];
            var card = CreateCardElement(item);
            _galleryGrid.Add(card);
        }
    }

    private VisualElement CreateCardElement(UITemplateItem item)
    {
        var card = new VisualElement();
        card.AddToClassList("window-card");

        var header = new VisualElement();
        header.AddToClassList("window-card-header");

        var dots = new VisualElement();
        dots.AddToClassList("window-dots");

        var dotRed = new VisualElement();
        dotRed.AddToClassList("dot");
        dotRed.AddToClassList("dot-red");

        var dotYellow = new VisualElement();
        dotYellow.AddToClassList("dot");
        dotYellow.AddToClassList("dot-yellow");

        var dotGreen = new VisualElement();
        dotGreen.AddToClassList("dot");
        dotGreen.AddToClassList("dot-green");

        dots.Add(dotRed);
        dots.Add(dotYellow);
        dots.Add(dotGreen);
        header.Add(dots);

        var nav = new VisualElement();
        nav.AddToClassList("window-nav");

        var navText = new Label("< > C");
        navText.AddToClassList("window-nav-text");
        nav.Add(navText);
        header.Add(nav);

        var addressBar = new VisualElement();
        addressBar.AddToClassList("window-address-bar");

        var addressText = new Label(string.IsNullOrEmpty(item.assetPath) ? item.displayName : item.assetPath);
        addressText.AddToClassList("window-address-text");
        addressBar.Add(addressText);
        header.Add(addressBar);

        card.Add(header);

        var body = new VisualElement();
        body.AddToClassList("window-card-body");

        var preview = new VisualElement();
        preview.AddToClassList("wireframe-preview");

        var row1 = new VisualElement();
        row1.AddToClassList("wireframe-box-row");
        var sq1 = new VisualElement();
        sq1.AddToClassList("wireframe-box-square");
        var box1 = new VisualElement();
        box1.AddToClassList("wireframe-box");
        row1.Add(sq1);
        row1.Add(box1);
        preview.Add(row1);

        var row2 = new VisualElement();
        row2.AddToClassList("wireframe-box-row");
        var box2 = new VisualElement();
        box2.AddToClassList("wireframe-box");
        var box3 = new VisualElement();
        box3.AddToClassList("wireframe-box");
        row2.Add(box2);
        row2.Add(box3);
        preview.Add(row2);

        body.Add(preview);

        var info = new VisualElement();
        info.AddToClassList("card-info");

        var titleLabel = new Label(item.displayName);
        titleLabel.AddToClassList("card-name");
        info.Add(titleLabel);

        var descLabel = new Label(item.description);
        descLabel.AddToClassList("card-desc");
        info.Add(descLabel);

        body.Add(info);

        var openBtn = new Button(() => OpenInspector(item));
        openBtn.text = "상세 보기 (Inspect)";
        openBtn.AddToClassList("card-open-btn");
        body.Add(openBtn);

        card.Add(body);
        return card;
    }

    private void OpenInspector(UITemplateItem item)
    {
        if (_inspectorOverlay == null || _inspectorViewport == null)
        {
            return;
        }

        _inspectorViewport.Clear();
        _inspectorTitle.text = $"{item.displayName} ({item.assetPath})";

        if (item.templateAsset != null)
        {
            var instance = item.templateAsset.Instantiate();
            instance.style.flexGrow = 1;
            instance.style.width = Length.Percent(100);
            instance.style.height = Length.Percent(100);
            _inspectorViewport.Add(instance);
        }
        else
        {
            var fallback = new Label("VisualTreeAsset이 할당되지 않았습니다.");
            fallback.style.color = Color.white;
            fallback.style.alignSelf = Align.Center;
            _inspectorViewport.Add(fallback);
        }

        _inspectorOverlay.RemoveFromClassList("hidden");
    }

    private void CloseInspector()
    {
        if (_inspectorOverlay == null)
        {
            return;
        }

        if (_inspectorViewport != null)
        {
            _inspectorViewport.Clear();
        }

        _inspectorOverlay.AddToClassList("hidden");
    }

#if UNITY_EDITOR
    [ContextMenu("Auto Populate All UXML")]
    public void PopulateAllUxml()
    {
        templates.Clear();
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:VisualTreeAsset");
        foreach (var guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            if (!path.StartsWith("Assets/"))
            {
                continue;
            }
            if (path.StartsWith("Assets/Editor/"))
            {
                continue;
            }
            if (path.Contains("UIGallery"))
            {
                continue;
            }

            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            if (asset != null)
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                string desc = GetDescriptionForUXML(fileName);
                templates.Add(new UITemplateItem
                {
                    displayName = fileName,
                    description = desc,
                    templateAsset = asset,
                    assetPath = path
                });
            }
        }
        UnityEditor.EditorUtility.SetDirty(this);
    }

    private string GetDescriptionForUXML(string name)
    {
        switch (name)
        {
            case "ConceptToastView": return "개념 획득/알림 팝업 토스트";
            case "DeductionModalView": return "단서 조합 및 개념 추론 모달";
            case "DialogueView": return "NPC 대화 및 선택지 시스템";
            case "DiaryModalView": return "교과 개념 아카이브 및 다이어리";
            case "InventoryView": return "아이템 및 개념 조각 인벤토리";
            case "QuestionSynthesizerView": return "수학/과학 질문 합성 및 퀴즈 UI";
            case "SkillDeckView": return "3D 전투 스킬 슬롯 및 쿨다운 덱";
            case "TutorModalView": return "튜터 힌트 및 AI 해설 모달";
            case "MathProofUI": return "수학 증명 시각화 단계 인터페이스";
            case "PhoneUIDocument": return "인게임 가상 스마트폰 OS 메인 화면";
            default: return "UI 템플릿";
        }
    }
#endif
}
