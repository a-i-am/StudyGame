using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UILayoutCustomizer : MonoBehaviour
{
    [Serializable]
    public class ElementOffsetData
    {
        public string elementName;
        public float offsetX;
        public float offsetY;
    }

    [Serializable]
    public class LayoutSaveContainer
    {
        public List<ElementOffsetData> items = new List<ElementOffsetData>();
    }

    [SerializeField] private UIDocument targetDocument;
    [SerializeField] private string layoutKey = "PersonalLobby_Layout";
    [SerializeField] private bool startInEditMode = true;

    private readonly string[] _targetElementNames = new string[]
    {
        "top-status-bar",
        "anomaly-widget",
        "app-grid",
        "dive-cta",
        "quick-dock",
        "character-plane"
    };

    private VisualElement _root;
    private VisualElement _customizerToolbar;
    private Button _toggleEditBtn;
    private Button _saveBtn;
    private Button _resetBtn;
    private Button _exportBtn;
    private Label _statusLabel;

    private bool _isEditMode;
    private readonly Dictionary<VisualElement, Vector2> _currentOffsets = new Dictionary<VisualElement, Vector2>();
    private readonly Dictionary<VisualElement, Vector2> _dragStartPointer = new Dictionary<VisualElement, Vector2>();
    private readonly Dictionary<VisualElement, Vector2> _dragStartOffset = new Dictionary<VisualElement, Vector2>();
    private readonly List<VisualElement> _managedElements = new List<VisualElement>();

    private void Awake()
    {
        if (targetDocument == null)
        {
            targetDocument = GetComponent<UIDocument>();
        }
    }

    private void Start()
    {
        if (targetDocument == null || targetDocument.rootVisualElement == null)
        {
            return;
        }

        _root = targetDocument.rootVisualElement;
        InitializeManagedElements();
        LoadSavedLayout();
        BuildCustomizerToolbar();

        SetEditMode(startInEditMode);
    }

    public void AttachToRoot(VisualElement root, string key)
    {
        Detach();
        _root = root;
        layoutKey = key;
        InitializeManagedElements();
        LoadSavedLayout();
        BuildCustomizerToolbar();
        SetEditMode(startInEditMode);
    }

    public void Detach()
    {
        if (_customizerToolbar != null && _customizerToolbar.parent != null)
        {
            _customizerToolbar.RemoveFromHierarchy();
        }
        _managedElements.Clear();
        _currentOffsets.Clear();
        _dragStartPointer.Clear();
        _dragStartOffset.Clear();
        _root = null;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SetEditMode(!_isEditMode);
        }
    }

    private void InitializeManagedElements()
    {
        _managedElements.Clear();
        _currentOffsets.Clear();

        for (int i = 0; i < _targetElementNames.Length; i++)
        {
            var el = _root.Q<VisualElement>(_targetElementNames[i]);
            if (el != null)
            {
                RegisterElement(el);
            }
        }

        var customList = _root.Query<VisualElement>(className: "customizable-element").ToList();
        for (int i = 0; i < customList.Count; i++)
        {
            var el = customList[i];
            if (!_managedElements.Contains(el))
            {
                RegisterElement(el);
            }
        }
    }

    private void RegisterElement(VisualElement el)
    {
        _managedElements.Add(el);
        _currentOffsets[el] = Vector2.zero;

        el.RegisterCallback<PointerDownEvent>(evt => OnElementPointerDown(el, evt));
        el.RegisterCallback<PointerMoveEvent>(evt => OnElementPointerMove(el, evt));
        el.RegisterCallback<PointerUpEvent>(evt => OnElementPointerUp(el, evt));
        el.RegisterCallback<PointerCaptureOutEvent>(evt => OnElementCaptureOut(el, evt));
    }

    private void BuildCustomizerToolbar()
    {
        _customizerToolbar = new VisualElement();
        _customizerToolbar.style.position = Position.Absolute;
        _customizerToolbar.style.top = 8;
        _customizerToolbar.style.right = 8;
        _customizerToolbar.style.flexDirection = FlexDirection.Row;
        _customizerToolbar.style.backgroundColor = new Color(0.08f, 0.12f, 0.18f, 0.88f);
        _customizerToolbar.style.borderTopLeftRadius = 6;
        _customizerToolbar.style.borderTopRightRadius = 6;
        _customizerToolbar.style.borderBottomLeftRadius = 6;
        _customizerToolbar.style.borderBottomRightRadius = 6;
        _customizerToolbar.style.borderLeftWidth = 1;
        _customizerToolbar.style.borderRightWidth = 1;
        _customizerToolbar.style.borderTopWidth = 1;
        _customizerToolbar.style.borderBottomWidth = 1;
        _customizerToolbar.style.borderLeftColor = new Color(0.22f, 0.74f, 0.97f, 0.6f);
        _customizerToolbar.style.borderRightColor = new Color(0.22f, 0.74f, 0.97f, 0.6f);
        _customizerToolbar.style.borderTopColor = new Color(0.22f, 0.74f, 0.97f, 0.6f);
        _customizerToolbar.style.borderBottomColor = new Color(0.22f, 0.74f, 0.97f, 0.6f);
        _customizerToolbar.style.paddingLeft = 6;
        _customizerToolbar.style.paddingRight = 6;
        _customizerToolbar.style.paddingTop = 4;
        _customizerToolbar.style.paddingBottom = 4;
        _customizerToolbar.style.alignItems = Align.Center;

        _statusLabel = new Label("UI 편집 모드 (F1)");
        _statusLabel.style.fontSize = 10;
        _statusLabel.style.color = new Color(0.88f, 0.94f, 1f);
        _statusLabel.style.marginRight = 6;
        _statusLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        _customizerToolbar.Add(_statusLabel);

        _toggleEditBtn = CreateToolbarButton("편집 ON/OFF", () => SetEditMode(!_isEditMode));
        _customizerToolbar.Add(_toggleEditBtn);

        _saveBtn = CreateToolbarButton("저장", SaveLayout);
        _customizerToolbar.Add(_saveBtn);

        _resetBtn = CreateToolbarButton("초기화", ResetLayout);
        _customizerToolbar.Add(_resetBtn);

        _exportBtn = CreateToolbarButton("USS 출력", ExportLayoutToConsole);
        _customizerToolbar.Add(_exportBtn);

        _root.Add(_customizerToolbar);
    }

    private Button CreateToolbarButton(string text, Action onClick)
    {
        var btn = new Button(onClick);
        btn.text = text;
        btn.style.height = 22;
        btn.style.fontSize = 9;
        btn.style.backgroundColor = new Color(0.15f, 0.23f, 0.35f, 0.9f);
        btn.style.color = Color.white;
        btn.style.borderLeftWidth = 1;
        btn.style.borderRightWidth = 1;
        btn.style.borderTopWidth = 1;
        btn.style.borderBottomWidth = 1;
        btn.style.borderLeftColor = new Color(0.28f, 0.4f, 0.55f);
        btn.style.borderRightColor = new Color(0.28f, 0.4f, 0.55f);
        btn.style.borderTopColor = new Color(0.28f, 0.4f, 0.55f);
        btn.style.borderBottomColor = new Color(0.28f, 0.4f, 0.55f);
        btn.style.borderTopLeftRadius = 4;
        btn.style.borderTopRightRadius = 4;
        btn.style.borderBottomLeftRadius = 4;
        btn.style.borderBottomRightRadius = 4;
        btn.style.paddingLeft = 6;
        btn.style.paddingRight = 6;
        btn.style.marginLeft = 3;
        btn.style.marginRight = 3;
        return btn;
    }

    public void SetEditMode(bool enable)
    {
        _isEditMode = enable;

        if (_toggleEditBtn != null)
        {
            _toggleEditBtn.text = _isEditMode ? "편집: ON" : "편집: OFF";
            _toggleEditBtn.style.backgroundColor = _isEditMode ? new Color(0.08f, 0.52f, 0.78f) : new Color(0.2f, 0.25f, 0.32f);
        }

        for (int i = 0; i < _managedElements.Count; i++)
        {
            var el = _managedElements[i];
            if (_isEditMode)
            {
                el.style.borderLeftWidth = 1;
                el.style.borderRightWidth = 1;
                el.style.borderTopWidth = 1;
                el.style.borderBottomWidth = 1;
                el.style.borderLeftColor = new Color(0.22f, 0.74f, 0.97f, 0.75f);
                el.style.borderRightColor = new Color(0.22f, 0.74f, 0.97f, 0.75f);
                el.style.borderTopColor = new Color(0.22f, 0.74f, 0.97f, 0.75f);
                el.style.borderBottomColor = new Color(0.22f, 0.74f, 0.97f, 0.75f);
            }
            else
            {
                el.style.borderLeftWidth = StyleKeyword.Null;
                el.style.borderRightWidth = StyleKeyword.Null;
                el.style.borderTopWidth = StyleKeyword.Null;
                el.style.borderBottomWidth = StyleKeyword.Null;
                el.style.borderLeftColor = StyleKeyword.Null;
                el.style.borderRightColor = StyleKeyword.Null;
                el.style.borderTopColor = StyleKeyword.Null;
                el.style.borderBottomColor = StyleKeyword.Null;
            }
        }
    }

    private void OnElementPointerDown(VisualElement el, PointerDownEvent evt)
    {
        if (!_isEditMode || evt.button != 0)
        {
            return;
        }

        el.CapturePointer(evt.pointerId);
        _dragStartPointer[el] = evt.position;
        _dragStartOffset[el] = _currentOffsets.ContainsKey(el) ? _currentOffsets[el] : Vector2.zero;
        evt.StopPropagation();
    }

    private void OnElementPointerMove(VisualElement el, PointerMoveEvent evt)
    {
        if (!_isEditMode || !el.HasPointerCapture(evt.pointerId))
        {
            return;
        }

        if (!_dragStartPointer.ContainsKey(el) || !_dragStartOffset.ContainsKey(el))
        {
            return;
        }

        Vector2 delta = (Vector2)evt.position - _dragStartPointer[el];
        Vector2 targetOffset = _dragStartOffset[el] + delta;

        _currentOffsets[el] = targetOffset;
        el.transform.position = new Vector3(targetOffset.x, targetOffset.y, 0f);

        evt.StopPropagation();
    }

    private void OnElementPointerUp(VisualElement el, PointerUpEvent evt)
    {
        if (el.HasPointerCapture(evt.pointerId))
        {
            el.ReleasePointer(evt.pointerId);
            evt.StopPropagation();
        }
    }

    private void OnElementCaptureOut(VisualElement el, PointerCaptureOutEvent evt)
    {
        if (_dragStartPointer.ContainsKey(el))
        {
            _dragStartPointer.Remove(el);
        }
        if (_dragStartOffset.ContainsKey(el))
        {
            _dragStartOffset.Remove(el);
        }
    }

    public void SaveLayout()
    {
        var container = new LayoutSaveContainer();
        for (int i = 0; i < _managedElements.Count; i++)
        {
            var el = _managedElements[i];
            if (string.IsNullOrEmpty(el.name))
            {
                continue;
            }

            Vector2 offset = _currentOffsets.ContainsKey(el) ? _currentOffsets[el] : Vector2.zero;
            container.items.Add(new ElementOffsetData
            {
                elementName = el.name,
                offsetX = offset.x,
                offsetY = offset.y
            });
        }

        string json = JsonUtility.ToJson(container);
        PlayerPrefs.SetString("UILayout_" + layoutKey, json);
        PlayerPrefs.Save();
        Debug.Log($"[UILayoutCustomizer] Layout saved ({container.items.Count} items) to PlayerPrefs key: UILayout_{layoutKey}");
    }

    public void LoadSavedLayout()
    {
        string key = "UILayout_" + layoutKey;
        if (!PlayerPrefs.HasKey(key))
        {
            return;
        }

        string json = PlayerPrefs.GetString(key);
        if (string.IsNullOrEmpty(json))
        {
            return;
        }

        var container = JsonUtility.FromJson<LayoutSaveContainer>(json);
        if (container == null || container.items == null)
        {
            return;
        }

        for (int i = 0; i < container.items.Count; i++)
        {
            var item = container.items[i];
            var el = _root.Q<VisualElement>(item.elementName);
            if (el != null)
            {
                Vector2 offset = new Vector2(item.offsetX, item.offsetY);
                _currentOffsets[el] = offset;
                el.transform.position = new Vector3(offset.x, offset.y, 0f);
            }
        }
    }

    public void ResetLayout()
    {
        for (int i = 0; i < _managedElements.Count; i++)
        {
            var el = _managedElements[i];
            _currentOffsets[el] = Vector2.zero;
            el.transform.position = Vector3.zero;
        }

        string key = "UILayout_" + layoutKey;
        if (PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }

        Debug.Log($"[UILayoutCustomizer] Layout reset to defaults.");
    }

    public void ExportLayoutToConsole()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("/* [UILayoutCustomizer] USS Export Rules */");

        for (int i = 0; i < _managedElements.Count; i++)
        {
            var el = _managedElements[i];
            if (string.IsNullOrEmpty(el.name))
            {
                continue;
            }

            Vector2 offset = _currentOffsets.ContainsKey(el) ? _currentOffsets[el] : Vector2.zero;
            if (offset != Vector2.zero)
            {
                sb.AppendLine($"#{el.name} {{");
                sb.AppendLine($"    translate: {Mathf.RoundToInt(offset.x)}px {Mathf.RoundToInt(offset.y)}px;");
                sb.AppendLine($"}}");
            }
        }

        Debug.Log(sb.ToString());
    }
}
