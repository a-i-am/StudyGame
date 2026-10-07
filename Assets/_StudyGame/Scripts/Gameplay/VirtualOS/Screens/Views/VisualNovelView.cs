using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using UxmlBindings;

namespace StudyGame.UI.Views
{
    [UxmlElement]
    public partial class VisualNovelView : VisualElement
    {
        private struct DialogLine
        {
            public string Speaker;
            public string Text;
            public bool Amber, Cyan, Emerald;
            public string FX;
        }

        private readonly DialogLine[] _scenario = new DialogLine[]
        {
            new DialogLine { Speaker = "セ・セ・ナ", Text = "「そしてワンナイトはさらに二枚追加し、余りカードが伏せられた状態であるんですぞ。」", Amber = true, Cyan = false, Emerald = false, FX = "glitch" },
            new DialogLine { Speaker = "セ・セ・ナ", Text = "「誰もが自分の正体を偽り、ステンドグラスの影で毒杯を交わす……。これが我らの定めた戯曲です。」", Amber = true, Cyan = true, Emerald = false, FX = "shake" },
            new DialogLine { Speaker = "セ・セ・ナ", Text = "「……気付きましたか？ 映写機が映し出しているのは、記憶ではなく貴方の『罪』そのものですよ。」", Amber = false, Cyan = true, Emerald = true, FX = "flash" },
            new DialogLine { Speaker = "セ・セ・ナ", Text = "「さあ、伏せられた２枚のカードをめくりなさい。狼の遠吠えが夜を切り裂く前に。」", Amber = true, Cyan = true, Emerald = true, FX = "shake" }
        };

        private bool _initialized;

        // UI 캐싱 변수
        private VisualElement _bgTeal, _bgMagenta, _layerVignette, _bgContainer;
        private VisualElement _zoneClickable, _badgeProjection, _markerSigil;
        private VisualElement _gemAmber, _gemCyan, _gemEmerald;
        private VisualElement _modalBacklog, _toastMessage;
        private Button _btnAuto, _btnSkip, _btnLog, _btnSound, _btnCloseLog;
        private Label _lblCharacterName, _lblDialogueText, _lblToastText;
        private ScrollView _scrollLog;

        private int _currentIndex = 0;
        private int _charIndex = 0;
        private bool _isTyping = false;
        private bool _isAutoPlay = false;
        private bool _isSkip = false;
        
        private readonly StringBuilder _textBuffer = new StringBuilder();
        private IVisualElementScheduledItem _typewriterTask;
        private IVisualElementScheduledItem _animTask;
        private IVisualElementScheduledItem _autoPlayTask;

        private static Texture2D _tealHalftone;
        private static Texture2D _magentaHalftone;
        private readonly List<DialogLine> _historyLog = new List<DialogLine>();

        public VisualNovelView()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        private void OnAttach(AttachToPanelEvent evt)
        {
            if (!_initialized)
            {
                QueryUIElements();
                if (_bgTeal != null) GenerateBackgrounds();
                BindEvents();
                _initialized = true;
            }

            _animTask = schedule.Execute(OnAnimate).Every(33); // 30fps 애니메이션
            RenderLine(_currentIndex);
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            if (!_initialized) return;
            UnbindEvents();
            _animTask?.Pause();
            _typewriterTask?.Pause();
            _autoPlayTask?.Pause();
        }

        private void QueryUIElements()
        {
            _bgTeal = this.Q<VisualElement>("Bg_Teal");
            _bgMagenta = this.Q<VisualElement>("Bg_Magenta");
            _layerVignette = this.Q<VisualElement>("Layer_Vignette");
            _bgContainer = this.Q<VisualElement>("Bg_Container");
            
            _zoneClickable = this.Q<VisualElement>("Zone_Clickable");
            _badgeProjection = this.Q<VisualElement>("Badge_Projection");
            _markerSigil = this.Q<VisualElement>("Marker_Sigil");
            
            _gemAmber = this.Q<VisualElement>("Gem_Amber");
            _gemCyan = this.Q<VisualElement>("Gem_Cyan");
            _gemEmerald = this.Q<VisualElement>("Gem_Emerald");
            
            _modalBacklog = this.Q<VisualElement>("Modal_Backlog");
            _toastMessage = this.Q<VisualElement>("Toast_Message");
            _scrollLog = this.Q<ScrollView>("Scroll_Log");
            
            _btnAuto = this.Q<Button>("Btn_Auto");
            _btnSkip = this.Q<Button>("Btn_Skip");
            _btnLog = this.Q<Button>("Btn_Log");
            _btnSound = this.Q<Button>("Btn_Sound");
            _btnCloseLog = this.Q<Button>("Btn_CloseLog");
            
            _lblCharacterName = this.Q<Label>("Lbl_CharacterName");
            _lblDialogueText = this.Q<Label>("Lbl_DialogueText");
            _lblToastText = this.Q<Label>("Lbl_ToastText");
        }

        private void BindEvents()
        {
            _zoneClickable?.RegisterCallback<ClickEvent>(OnClickZone);
            if (_btnAuto != null) _btnAuto.clicked += ToggleAuto;
            if (_btnSkip != null) _btnSkip.clicked += ToggleSkip;
            if (_btnLog != null) _btnLog.clicked += OpenLog;
            if (_btnSound != null) _btnSound.clicked += () => ShowToast("AUDIO: TOGGLED");
            if (_btnCloseLog != null) _btnCloseLog.clicked += CloseLog;
            _badgeProjection?.RegisterCallback<ClickEvent>(ToggleProjection);
            
            _gemAmber?.RegisterCallback<ClickEvent>(e => { e.StopPropagation(); ShowToast("호박석 공명 활성화"); });
            _gemCyan?.RegisterCallback<ClickEvent>(e => { e.StopPropagation(); ShowToast("청금석 공명 활성화"); });
            _gemEmerald?.RegisterCallback<ClickEvent>(e => { e.StopPropagation(); ShowToast("비취 공명 활성화"); });
        }

        private void UnbindEvents()
        {
            _zoneClickable?.UnregisterCallback<ClickEvent>(OnClickZone);
            _badgeProjection?.UnregisterCallback<ClickEvent>(ToggleProjection);
            
            if (_btnAuto != null) _btnAuto.clicked -= ToggleAuto;
            if (_btnSkip != null) _btnSkip.clicked -= ToggleSkip;
            if (_btnLog != null) _btnLog.clicked -= OpenLog;
            if (_btnCloseLog != null) _btnCloseLog.clicked -= CloseLog;
        }

        // ---------- Core Dialog Logic ---------- //

        private void OnClickZone(ClickEvent evt)
        {
            if (_isTyping) FinishTyping();
            else AdvanceDialogue();
        }

        private void AdvanceDialogue()
        {
            _currentIndex = (_currentIndex + 1) % _scenario.Length;
            RenderLine(_currentIndex);
        }

        private void RenderLine(int index)
        {
            var line = _scenario[index];
            if (_lblCharacterName != null) _lblCharacterName.text = line.Speaker;
            
            if (!_historyLog.Exists(l => l.Text == line.Text))
                _historyLog.Add(line);

            UpdateJewels(line.Amber, line.Cyan, line.Emerald);
            TriggerFX(line.FX);

            _charIndex = 0;
            _isTyping = true;
            _textBuffer.Clear();
            if (_lblDialogueText != null) _lblDialogueText.text = "";
            if (_markerSigil != null) _markerSigil.style.visibility = Visibility.Hidden;

            _typewriterTask?.Pause();
            _typewriterTask = schedule.Execute(TypeNextChar).Every(_isSkip ? 10 : 40);
        }

        private void TypeNextChar()
        {
            if (!_isTyping) return;
            var line = _scenario[_currentIndex];

            if (_charIndex < line.Text.Length)
            {
                _textBuffer.Append(line.Text[_charIndex]);
                if (_lblDialogueText != null) _lblDialogueText.text = _textBuffer.ToString();
                _charIndex++;
            }
            else
            {
                FinishTyping();
            }
        }

        private void FinishTyping()
        {
            _typewriterTask?.Pause();
            _isTyping = false;
            if (_lblDialogueText != null) _lblDialogueText.text = _scenario[_currentIndex].Text;
            if (_markerSigil != null) _markerSigil.style.visibility = Visibility.Visible;

            if (_isAutoPlay && !_isSkip)
            {
                _autoPlayTask?.Pause();
                _autoPlayTask = schedule.Execute(AdvanceDialogue).StartingIn(2400);
            }
        }

        // ---------- UI Updates & FX ---------- //

        private void UpdateJewels(bool amber, bool cyan, bool emerald)
        {
            if (_gemAmber != null) _gemAmber.style.opacity = amber ? 1f : 0.4f;
            if (_gemCyan != null) _gemCyan.style.opacity = cyan ? 1f : 0.4f;
            if (_gemEmerald != null) _gemEmerald.style.opacity = emerald ? 1f : 0.4f;
        }

        private void TriggerFX(string fx)
        {
            if (fx == "flash" && _layerVignette != null)
            {
                _layerVignette.RemoveFromClassList("vn-vignette--hidden");
                schedule.Execute(() => _layerVignette.AddToClassList("vn-vignette--hidden")).StartingIn(350);
            }
            else if (fx == "shake" && _bgContainer != null)
            {
                _bgContainer.schedule.Execute(() => _bgContainer.style.translate = new Translate(3, -2)).StartingIn(20);
                _bgContainer.schedule.Execute(() => _bgContainer.style.translate = new Translate(-3, 3)).StartingIn(60);
                _bgContainer.schedule.Execute(() => _bgContainer.style.translate = new Translate(3, 1)).StartingIn(100);
                _bgContainer.schedule.Execute(() => _bgContainer.style.translate = new Translate(0, 0)).StartingIn(140);
            }
            else if (fx == "glitch" && _lblDialogueText != null)
            {
                _lblDialogueText.schedule.Execute(() => _lblDialogueText.style.translate = new Translate(-2, 1)).StartingIn(10);
                _lblDialogueText.schedule.Execute(() => _lblDialogueText.style.translate = new Translate(2, -1)).StartingIn(50);
                _lblDialogueText.schedule.Execute(() => _lblDialogueText.style.translate = new Translate(0, 0)).StartingIn(90);
            }
        }

        private void ToggleAuto()
        {
            _isAutoPlay = !_isAutoPlay;
            ShowToast(_isAutoPlay ? "AUTO PLAY: ON" : "AUTO PLAY: OFF");
            if (_isAutoPlay && !_isTyping) AdvanceDialogue();
        }

        private void ToggleSkip()
        {
            _isSkip = !_isSkip;
            ShowToast(_isSkip ? "SKIP MODE: ACTIVE" : "SKIP MODE: OFF");
            if (_isSkip && _isTyping) FinishTyping();
        }

        private void ToggleProjection(ClickEvent evt)
        {
            evt.StopPropagation();
            ShowToast("FLASHBACK PROJECTION TOGGLED");
        }

        private void OpenLog()
        {
            if (_scrollLog == null || _modalBacklog == null) return;
            
            _scrollLog.Clear();
            foreach (var log in _historyLog)
            {
                var item = new VisualElement();
                item.AddToClassList("vn-log-item");
                
                var speakerLbl = new Label(log.Speaker);
                speakerLbl.AddToClassList("vn-log-speaker");
                item.Add(speakerLbl);
                
                var textLbl = new Label(log.Text);
                textLbl.AddToClassList("vn-log-text");
                item.Add(textLbl);
                
                _scrollLog.Add(item);
            }
            _modalBacklog.RemoveFromClassList("vn-modal--hidden");
        }

        private void CloseLog() => _modalBacklog?.AddToClassList("vn-modal--hidden");

        private void ShowToast(string msg)
        {
            if (_lblToastText == null || _toastMessage == null) return;
            
            _lblToastText.text = msg;
            _toastMessage.RemoveFromClassList("vn-toast--hidden");
            schedule.Execute(() => _toastMessage.AddToClassList("vn-toast--hidden")).StartingIn(1800);
        }

        // ---------- Procedural Animations & Textures ---------- //

        private void OnAnimate()
        {
            if (_markerSigil == null) return;
            
            float t = Time.realtimeSinceStartup;
            float cycle = t % 1.6f;
            float scale = 1f;
            
            if (cycle < 0.22f) scale = Mathf.Lerp(1f, 1.22f, cycle / 0.22f);
            else if (cycle < 0.44f) scale = Mathf.Lerp(1.22f, 1.03f, (cycle - 0.22f) / 0.22f);
            else if (cycle < 0.67f) scale = Mathf.Lerp(1.03f, 1.28f, (cycle - 0.44f) / 0.23f);
            else if (cycle < 1.12f) scale = Mathf.Lerp(1.28f, 1f, (cycle - 0.67f) / 0.45f);

            if (_markerSigil.style.visibility == Visibility.Visible)
            {
                _markerSigil.style.scale = new Scale(new Vector3(scale, scale, 1f));
            }
        }

        private void GenerateBackgrounds()
        {
            if (_tealHalftone == null) _tealHalftone = CreateHalftoneTexture(Hex("#1a575a"), Hex("#0b2f33"), 14, 2.5f);
            if (_magentaHalftone == null) _magentaHalftone = CreateHalftoneTexture(Hex("#b8145c"), Hex("#540628"), 16, 3f);

            if (_bgTeal != null)
            {
                _bgTeal.style.backgroundImage = new StyleBackground(_tealHalftone);
                _bgTeal.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;
            }
            
            if (_bgMagenta != null)
            {
                _bgMagenta.style.backgroundImage = new StyleBackground(_magentaHalftone);
                _bgMagenta.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;
            }
        }

        private Texture2D CreateHalftoneTexture(Color bg, Color dot, int size, float dotRadius)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Point };
            var colors = new Color[size * size];
            
            Vector2 center1 = new Vector2(size / 2f, size / 2f);
            Vector2 center2 = new Vector2(0, 0);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    bool isDot = Vector2.Distance(pos, center1) <= dotRadius || 
                                 Vector2.Distance(pos, center2) <= dotRadius ||
                                 Vector2.Distance(pos, new Vector2(size, size)) <= dotRadius ||
                                 Vector2.Distance(pos, new Vector2(0, size)) <= dotRadius ||
                                 Vector2.Distance(pos, new Vector2(size, 0)) <= dotRadius;
                    
                    colors[y * size + x] = isDot ? dot : bg;
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }

        private static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.black;
    }
}