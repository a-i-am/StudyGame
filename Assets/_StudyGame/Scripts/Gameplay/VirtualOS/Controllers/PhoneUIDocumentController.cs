using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PhoneUIDocumentController : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset uxmlDocument;
    [SerializeField] private PanelSettings panelSettings;

    private UIDocument uiDocument;
    private VisualElement phoneFrame;
    private VisualElement profileBar;
    private VisualElement profileImage;
    private Label senderName;
    private ScrollView messageScroll;

    public VisualElement PhoneFrame => phoneFrame;
    public VisualElement ProfileBar => profileBar;
    public VisualElement ProfileImage => profileImage;
    public Label SenderName => senderName;
    public ScrollView MessageScroll => messageScroll;

    private void Awake()
    {
        EnsureDocumentConfigured();
        InitializeUI();
    }

    private void OnEnable()
    {
        InitializeUI();
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

        phoneFrame = uiDocument.rootVisualElement.Q<VisualElement>("PhoneFrame");
        profileBar = uiDocument.rootVisualElement.Q<VisualElement>("ProfileBar");
        profileImage = uiDocument.rootVisualElement.Q<VisualElement>("ProfileImage");
        senderName = uiDocument.rootVisualElement.Q<Label>("SenderName");
        messageScroll = uiDocument.rootVisualElement.Q<ScrollView>("MessageScroll");
    }

    private void EnsureDocumentConfigured()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

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

    public void SetVisible(bool visible)
    {
        if (phoneFrame != null)
        {
            phoneFrame.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
        else if (uiDocument != null && uiDocument.rootVisualElement != null)
        {
            uiDocument.rootVisualElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    public void SetSender(string name, Sprite profileSprite)
    {
        if (senderName != null)
        {
            senderName.text = name;
        }

        if (profileImage != null)
        {
            profileImage.style.backgroundImage = profileSprite != null ? new StyleBackground(profileSprite) : new StyleBackground(StyleKeyword.None);
        }
    }

    public void ClearMessages()
    {
        if (messageScroll != null)
        {
            messageScroll.Clear();
        }
    }

    public void AddMessage(string messageText)
    {
        if (messageScroll == null)
        {
            return;
        }

        VisualElement bubble = new VisualElement();
        bubble.AddToClassList("sns-bubble");

        Label label = new Label(messageText);
        label.AddToClassList("sns-bubble-text");
        bubble.Add(label);

        messageScroll.Add(bubble);
        messageScroll.ScrollTo(bubble);
    }
}
