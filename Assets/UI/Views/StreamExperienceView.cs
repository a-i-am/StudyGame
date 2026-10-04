using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using UxmlBindings;

namespace StudyGame.UI.Views
{
    /// <summary>
    /// UI Toolkit port of the "Aura Glassmorphism Stream Experience" reference page.
    /// Passive view: owns only presentation state (scene tabs, animations, toasts) and exposes
    /// <see cref="AppendStreamingText"/> for LLM chunk streaming with StringBuilder + dirty-flag batching.
    /// Used as the root element of StreamExperienceView.uxml so it runs on VisualTreeAsset.Instantiate().
    /// </summary>
    [UxmlElement]
    public partial class StreamExperienceView : VisualElement
    {
        private enum Scene { Starting, Break, Ending, Ingame, Alerts }

        private struct Particle
        {
            public VisualElement Element;
            public float X, Y, SpeedX, SpeedY, Rotation, RotSpeed;
        }

        private static readonly (string type, string user, string details, string icon)[] AlertPresets =
        {
            ("raid", "StreamerSquad", "raided with 45 viewers!", "♟"),
            ("cheer", "LunaStar", "cheered 500 Bits!", "◆"),
            ("follower", "AuraGlow", "is now following!", "✚"),
            ("sub", "Starlight_9", "subscribed for 6 months Tier 1!", "♛"),
            ("tip", "KindSoul", "sent a $25.00 tip!", "$"),
            ("donation", "DreamCatcher", "donated $50.00 with message!", "✦"),
        };

        private static Texture2D s_auroraTexture;

        private StreamExperienceViewBinding _ui;
        private bool _initialized;
        private Scene _scene = Scene.Starting;
        private bool _soundEnabled = true;
        private int _countdownSeconds = 299;

        // LLM streaming optimization (StringBuilder + dirty flag, flushed every 100ms)
        private readonly StringBuilder _streamBuffer = new StringBuilder(1024);
        private bool _isTextDirty;

        private readonly List<Particle> _particles = new List<Particle>();
        private readonly System.Random _rng = new System.Random();

        private IVisualElementScheduledItem _animTask;
        private IVisualElementScheduledItem _textTask;
        private IVisualElementScheduledItem _secondTask;
        private IVisualElementScheduledItem _toastHideTask;
        private IVisualElementScheduledItem _alertHideTask;

        public StreamExperienceView()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        // ------------------------------------------------------------------ lifecycle

        private void OnAttach(AttachToPanelEvent evt)
        {
            if (!_initialized)
            {
                _ui = new StreamExperienceViewBinding(this);
                if (_ui.Root_Container == null) return; // used standalone without the UXML tree
                _ui.Root_Container.style.backgroundImage = GetAuroraTexture();
                BuildAlertCards();
                BuildParticles();
                _initialized = true;
            }

            BindEvents();
            SwitchScene(_scene, silent: true);
            UpdateClock();

            _animTask = schedule.Execute(OnAnimate).Every(33);
            _textTask = schedule.Execute(FlushStreamingText).Every(100);
            _secondTask = schedule.Execute(OnSecondTick).Every(1000);
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            if (!_initialized) return;
            UnbindEvents();
            _animTask?.Pause();
            _textTask?.Pause();
            _secondTask?.Pause();
            _toastHideTask?.Pause();
            _alertHideTask?.Pause();
        }

        private void BindEvents()
        {
            _ui.Tab_Starting.clicked += OnStarting;
            _ui.Tab_Break.clicked += OnBreak;
            _ui.Tab_Ending.clicked += OnEnding;
            _ui.Tab_Ingame.clicked += OnIngame;
            _ui.Tab_Alerts.clicked += OnAlerts;
            _ui.Dock_Starting.clicked += OnStarting;
            _ui.Dock_Break.clicked += OnBreak;
            _ui.Dock_Ending.clicked += OnEnding;
            _ui.Dock_Ingame.clicked += OnIngame;
            _ui.Dock_Alerts.clicked += OnAlerts;

            _ui.Button_Sound.clicked += OnToggleSound;
            _ui.Button_TestAlert.clicked += OnRandomAlert;
            _ui.Button_Fullscreen.clicked += OnToggleFullscreen;
            _ui.Button_Send.clicked += SendChatMessage;
            _ui.Button_Share.clicked += OnShare;
            _ui.Button_Bookmark.clicked += OnBookmark;
            _ui.Button_Comment.clicked += OnComment;
            _ui.Button_AlertClose.clicked += DismissAlert;
            _ui.Button_Love.RegisterCallback<ClickEvent>(OnHeartClick);
            _ui.Button_Heart.RegisterCallback<ClickEvent>(OnHeartClick);
            _ui.TextField_ChatInput.RegisterCallback<KeyDownEvent>(OnChatKeyDown, TrickleDown.TrickleDown);
            _ui.Hero_Right.RegisterCallback<GeometryChangedEvent>(OnHeroResized);
        }

        private void UnbindEvents()
        {
            _ui.Tab_Starting.clicked -= OnStarting;
            _ui.Tab_Break.clicked -= OnBreak;
            _ui.Tab_Ending.clicked -= OnEnding;
            _ui.Tab_Ingame.clicked -= OnIngame;
            _ui.Tab_Alerts.clicked -= OnAlerts;
            _ui.Dock_Starting.clicked -= OnStarting;
            _ui.Dock_Break.clicked -= OnBreak;
            _ui.Dock_Ending.clicked -= OnEnding;
            _ui.Dock_Ingame.clicked -= OnIngame;
            _ui.Dock_Alerts.clicked -= OnAlerts;

            _ui.Button_Sound.clicked -= OnToggleSound;
            _ui.Button_TestAlert.clicked -= OnRandomAlert;
            _ui.Button_Fullscreen.clicked -= OnToggleFullscreen;
            _ui.Button_Send.clicked -= SendChatMessage;
            _ui.Button_Share.clicked -= OnShare;
            _ui.Button_Bookmark.clicked -= OnBookmark;
            _ui.Button_Comment.clicked -= OnComment;
            _ui.Button_AlertClose.clicked -= DismissAlert;
            _ui.Button_Love.UnregisterCallback<ClickEvent>(OnHeartClick);
            _ui.Button_Heart.UnregisterCallback<ClickEvent>(OnHeartClick);
            _ui.TextField_ChatInput.UnregisterCallback<KeyDownEvent>(OnChatKeyDown, TrickleDown.TrickleDown);
            _ui.Hero_Right.UnregisterCallback<GeometryChangedEvent>(OnHeroResized);
        }

        // ------------------------------------------------------------------ public API

        /// <summary>Buffers an LLM chunk; the label is refreshed at most every 100ms.</summary>
        public void AppendStreamingText(string chunk)
        {
            _streamBuffer.Append(chunk);
            _isTextDirty = true;
        }

        public void ClearStreamingText()
        {
            _streamBuffer.Clear();
            if (_initialized) _ui.Label_StreamingText.text = string.Empty;
            _isTextDirty = false;
        }

        public void AddChatMessage(string user, string message, string colorHex = "#A7F3D0")
        {
            if (!_initialized) return;
            string rich = $"<b><color={colorHex}>{user}:</color></b> <noparse>{message}</noparse>";

            var bubble = new Label(rich);
            bubble.AddToClassList("glass");
            bubble.AddToClassList("chat-bubble");
            bubble.AddToClassList("chat-bubble--mine");
            _ui.Scroll_Chat.Add(bubble);
            _ui.Scroll_Chat.schedule.Execute(() => _ui.Scroll_Chat.ScrollTo(bubble)).StartingIn(30);

            var mini = new Label(rich);
            mini.AddToClassList("mini-line");
            _ui.Scroll_MiniChat.Add(mini);
            _ui.Scroll_MiniChat.schedule.Execute(() => _ui.Scroll_MiniChat.ScrollTo(mini)).StartingIn(30);
        }

        // ------------------------------------------------------------------ scenes

        private void OnStarting() => SwitchScene(Scene.Starting);
        private void OnBreak() => SwitchScene(Scene.Break);
        private void OnEnding() => SwitchScene(Scene.Ending);
        private void OnIngame() => SwitchScene(Scene.Ingame);
        private void OnAlerts() => SwitchScene(Scene.Alerts);

        private void SwitchScene(Scene scene, bool silent = false)
        {
            _scene = scene;

            SetActive(_ui.Tab_Starting, "scene-tab--active", scene == Scene.Starting);
            SetActive(_ui.Tab_Break, "scene-tab--active", scene == Scene.Break);
            SetActive(_ui.Tab_Ending, "scene-tab--active", scene == Scene.Ending);
            SetActive(_ui.Tab_Ingame, "scene-tab--active", scene == Scene.Ingame);
            SetActive(_ui.Tab_Alerts, "scene-tab--active", scene == Scene.Alerts);
            SetActive(_ui.Dock_Starting, "dock-btn--active", scene == Scene.Starting);
            SetActive(_ui.Dock_Break, "dock-btn--active", scene == Scene.Break);
            SetActive(_ui.Dock_Ending, "dock-btn--active", scene == Scene.Ending);
            SetActive(_ui.Dock_Ingame, "dock-btn--active", scene == Scene.Ingame);
            SetActive(_ui.Dock_Alerts, "dock-btn--active", scene == Scene.Alerts);

            bool hero = scene <= Scene.Ending;
            ShowScene(_ui.Scene_Hero, hero);
            ShowScene(_ui.Scene_Ingame, scene == Scene.Ingame);
            ShowScene(_ui.Scene_Alerts, scene == Scene.Alerts);

            switch (scene)
            {
                case Scene.Starting:
                    SetHero("start\ning", "Starting Soon...", "The broadcast is about to begin. Grab your favorite drink");
                    break;
                case Scene.Break:
                    SetHero("break\ntime", "Be Right Back", "Stepping away for a moment. Stay cozy and enjoy the beats");
                    break;
                case Scene.Ending:
                    SetHero("end\ning", "Stream Concluded", "Thank you for spending time together today! See you next time");
                    break;
                case Scene.Ingame:
                    if (!silent) ShowToast("Switched to In-Game Stream Overlay");
                    break;
                case Scene.Alerts:
                    if (!silent) ShowToast("Viewing Alert & Micro-graphic Elements Pack");
                    break;
            }
        }

        private void SetHero(string title, string capsule, string subtitle)
        {
            _ui.Label_HeroTitle.text = title;
            _ui.Label_Capsule.text = capsule.ToUpperInvariant();
            _ui.Label_HeroSubtitle.text = subtitle;
        }

        private static void SetActive(VisualElement ve, string cls, bool on) => ve.EnableInClassList(cls, on);

        private void ShowScene(VisualElement scene, bool show)
        {
            if (show)
            {
                scene.style.opacity = 0f;
                scene.RemoveFromClassList("scene--hidden");
                // next frame -> opacity transition (0 -> 1), like the reference's setTimeout(20ms)
                scene.schedule.Execute(() => scene.style.opacity = StyleKeyword.Null).StartingIn(20);
            }
            else
            {
                scene.AddToClassList("scene--hidden");
            }
        }

        /// <summary>Fits the Pacifico title to the available column so it never clips on small viewports.</summary>
        private void OnHeroResized(GeometryChangedEvent evt)
        {
            float h = evt.newRect.height;
            float w = evt.newRect.width;
            float size = Mathf.Clamp(Mathf.Min(h * 0.2f, w * 0.22f), 36f, 128f);
            _ui.Label_HeroTitle.style.fontSize = size;
            _ui.Hero_Diamond.style.width = size * 2.2f;
            _ui.Hero_Diamond.style.height = size * 2.2f;
        }

        // ------------------------------------------------------------------ interactions

        private void OnChatKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                SendChatMessage();
                evt.StopPropagation();
            }
        }

        private void SendChatMessage()
        {
            string text = _ui.TextField_ChatInput.value?.Trim();
            if (string.IsNullOrEmpty(text)) return;
            AddChatMessage("You", text);
            _ui.TextField_ChatInput.value = string.Empty;
            ShowToast("Message sent to stream chat!");
        }

        private void OnToggleSound()
        {
            _soundEnabled = !_soundEnabled;
            _ui.Button_Sound.text = _soundEnabled ? "♪" : "✕";
            ShowToast(_soundEnabled ? "Sound effects enabled" : "Sound effects muted");
        }

        private void OnToggleFullscreen()
        {
            Screen.fullScreen = !Screen.fullScreen;
            ShowToast("Fullscreen toggled");
        }

        private void OnShare() => ShowToast("Stream link copied to clipboard!");
        private void OnBookmark() => ShowToast("Stream saved to your cozy bookmarks!");
        private void OnComment() => ShowToast("Chat focused");

        private void OnRandomAlert()
        {
            var a = AlertPresets[_rng.Next(AlertPresets.Length)];
            ShowAlert(a.type, a.user, a.details, a.icon);
        }

        private void OnHeartClick(ClickEvent evt)
        {
            var root = _ui.Root_Container;
            Vector2 local = root.WorldToLocal(evt.position);

            var heart = new Label("♥");
            heart.AddToClassList("burst-heart");
            heart.pickingMode = PickingMode.Ignore;
            heart.style.left = local.x - 12f;
            heart.style.top = local.y - 20f;
            heart.style.scale = new Scale(new Vector3(0.6f, 0.6f, 1f));
            root.Add(heart);

            heart.schedule.Execute(() =>
            {
                heart.style.translate = new Translate(0, -90);
                heart.style.scale = new Scale(new Vector3(1.3f, 1.3f, 1f));
                heart.style.rotate = new Rotate(15f);
                heart.style.opacity = 0f;
            }).StartingIn(16);
            heart.schedule.Execute(heart.RemoveFromHierarchy).StartingIn(1650);
        }

        private void BuildAlertCards()
        {
            foreach (var a in AlertPresets)
            {
                var preset = a;
                var card = new VisualElement();
                card.AddToClassList("alert-card");
                card.RegisterCallback<ClickEvent>(_ => ShowAlert(preset.type, preset.user, preset.details, preset.icon));

                var left = new VisualElement();
                left.AddToClassList("row");
                var icon = new Label("♡");
                icon.AddToClassList("alert-card-icon");
                var title = new Label("new " + preset.type);
                title.AddToClassList("alert-card-title");
                left.Add(icon);
                left.Add(title);

                var right = new VisualElement();
                right.AddToClassList("row");
                var more = new Label("•••");
                more.AddToClassList("alert-card-more");
                var dot = new VisualElement();
                dot.AddToClassList("dot");
                dot.AddToClassList("dot--6");
                right.Add(more);
                right.Add(dot);

                var notch = new VisualElement { pickingMode = PickingMode.Ignore };
                notch.AddToClassList("alert-card-notch");

                card.Add(notch);
                card.Add(left);
                card.Add(right);
                _ui.Alerts_Grid.Add(card);
            }
        }

        // ------------------------------------------------------------------ toast / alert

        private void ShowToast(string message)
        {
            _ui.Label_Toast.text = message;
            _ui.Toast_Box.RemoveFromClassList("toast--hidden");
            _toastHideTask?.Pause();
            _toastHideTask = schedule.Execute(() => _ui.Toast_Box.AddToClassList("toast--hidden")).StartingIn(2600);
        }

        private void ShowAlert(string type, string user, string details, string icon)
        {
            _ui.Label_AlertTag.text = "new " + type;
            _ui.Label_AlertUser.text = user;
            _ui.Label_AlertMsg.text = details;
            _ui.Label_AlertIcon.text = icon;
            _ui.Alert_Popup.RemoveFromClassList("alert-popup--hidden");
            _ui.Alert_Popup.pickingMode = PickingMode.Position;
            _alertHideTask?.Pause();
            _alertHideTask = schedule.Execute(DismissAlert).StartingIn(4200);
        }

        private void DismissAlert()
        {
            _ui.Alert_Popup.AddToClassList("alert-popup--hidden");
            _ui.Alert_Popup.pickingMode = PickingMode.Ignore;
        }

        // ------------------------------------------------------------------ timers

        private void FlushStreamingText()
        {
            if (!_isTextDirty) return;
            _ui.Label_StreamingText.text = _streamBuffer.ToString();
            _isTextDirty = false;
        }

        private void OnSecondTick()
        {
            if (_countdownSeconds > 0) _countdownSeconds--;
            _ui.Label_Countdown.text = $"00:{_countdownSeconds / 60:00}:{_countdownSeconds % 60:00}";
            UpdateClock();
        }

        private void UpdateClock() => _ui.Label_Clock.text = DateTime.Now.ToString("HH:mm");

        /// <summary>Replaces the CSS @keyframes (float / bounce / ping / pulse) and the particle canvas.</summary>
        private void OnAnimate()
        {
            float t = Time.realtimeSinceStartup;

            Float(_ui.Phone_Card, t, 5f, 10f, -1.2f);
            Float(_ui.Hero_Diamond, t, 7f, 14f, 0.8f, 45f);
            Float(_ui.Hero_Capsule, t, 2f, 5f);
            Float(_ui.Typing_Tooltip, t, 2.2f, 6f);
            Float(_ui.Float_Heart, t, 3.8f, 6f);
            Float(_ui.Float_Diamond, t, 7f, 14f, 0.8f, 45f);
            Float(_ui.Float_Plus, t, 5f, 10f, -1.2f);

            Pulse(_ui.Orb_Blue, t);
            Pulse(_ui.Orb_Purple, t - 1.5f);
            Ping(_ui.Live_Ping, t);
            Ping(_ui.Capsule_Ping, t);
            _ui.Label_RecBadge.style.opacity = 0.75f + 0.25f * Mathf.Cos(t * Mathf.PI);

            UpdateParticles();
        }

        private static float Wave(float t, float period) => 0.5f - 0.5f * Mathf.Cos(t / period * Mathf.PI * 2f);

        private static void Float(VisualElement ve, float t, float period, float amp, float rot = 0f, float baseRot = 0f)
        {
            float k = Wave(t, period);
            ve.style.translate = new Translate(0, -amp * k);
            if (rot != 0f || baseRot != 0f) ve.style.rotate = new Rotate(baseRot + rot * k);
        }

        private static void Pulse(VisualElement ve, float t)
        {
            float k = Wave(t, 2.5f);
            ve.style.opacity = 0.85f + 0.15f * k;
            float s = 1f + 0.05f * k;
            ve.style.scale = new Scale(new Vector3(s, s, 1f));
        }

        private static void Ping(VisualElement ve, float t)
        {
            float k = t % 1f;
            float s = 1f + k;
            ve.style.scale = new Scale(new Vector3(s, s, 1f));
            ve.style.opacity = 1f - k;
        }

        private void BuildParticles()
        {
            for (int i = 0; i < 35; i++)
            {
                var ve = new VisualElement { pickingMode = PickingMode.Ignore };
                ve.AddToClassList("particle");
                float size = (float)_rng.NextDouble() * 8f + 4f;
                if (_rng.Next(3) == 2) { ve.AddToClassList("particle--round"); size *= 0.8f; }
                ve.style.width = size;
                ve.style.height = size;
                ve.style.opacity = (float)_rng.NextDouble() * 0.5f + 0.2f;
                _ui.Layer_Particles.Add(ve);

                var p = new Particle { Element = ve };
                ResetParticle(ref p, 1920f, 1080f, randomY: true);
                _particles.Add(p);
            }
        }

        private void ResetParticle(ref Particle p, float w, float h, bool randomY)
        {
            p.X = (float)_rng.NextDouble() * w;
            p.Y = randomY ? (float)_rng.NextDouble() * h : h + 20f;
            p.SpeedY = -((float)_rng.NextDouble() * 0.4f + 0.15f) * 2f; // reference runs at 60fps, we tick at ~30fps
            p.SpeedX = ((float)_rng.NextDouble() - 0.5f) * 0.6f;
            p.Rotation = (float)_rng.NextDouble() * 360f;
            p.RotSpeed = ((float)_rng.NextDouble() - 0.5f) * 2.3f;
        }

        private void UpdateParticles()
        {
            float w = _ui.Layer_Particles.resolvedStyle.width;
            float h = _ui.Layer_Particles.resolvedStyle.height;
            if (float.IsNaN(w) || w <= 0f) return;

            for (int i = 0; i < _particles.Count; i++)
            {
                var p = _particles[i];
                p.Y += p.SpeedY;
                p.X += p.SpeedX;
                p.Rotation += p.RotSpeed;
                if (p.Y < -20f || p.X < -20f || p.X > w + 20f) ResetParticle(ref p, w, h, randomY: false);
                if (p.Y > h + 20f) p.Y = (float)_rng.NextDouble() * h;

                p.Element.style.translate = new Translate(p.X, p.Y);
                p.Element.style.rotate = new Rotate(p.Rotation);
                _particles[i] = p;
            }
        }

        // ------------------------------------------------------------------ background

        /// <summary>
        /// Bakes the reference's ".aura-bg" (4 radial blobs over a 135deg linear gradient) into a texture,
        /// since USS has no gradient support.
        /// </summary>
        private static Texture2D GetAuroraTexture()
        {
            if (s_auroraTexture != null) return s_auroraTexture;

            const int w = 320, h = 180;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave,
                name = "AuroraBackground",
            };

            Color[] stops = { Hex("#4F46E5"), Hex("#6366F1"), Hex("#818CF8"), Hex("#93C5FD"), Hex("#C4B5FD") };
            // (cx, cy from top, color, radius as fraction of farthest-corner) — drawn bottom-most first
            var blobs = new (float cx, float cy, Color c, float r)[]
            {
                (0.75f, 0.75f, Hex("#A78BFA"), 0.40f),
                (0.50f, 0.80f, Hex("#93C5FD"), 0.50f),
                (0.85f, 0.25f, Hex("#C084FC"), 0.45f),
                (0.15f, 0.20f, Hex("#7F9CF5"), 0.40f),
            };
            float aspect = (float)w / h;
            var px = new Color[w * h];

            for (int y = 0; y < h; y++)
            {
                float vTop = 1f - (float)y / (h - 1);
                for (int x = 0; x < w; x++)
                {
                    float u = (float)x / (w - 1);
                    float g = Mathf.Clamp01((u * aspect + vTop) / (aspect + 1f)) * (stops.Length - 1);
                    int i0 = Mathf.Min((int)g, stops.Length - 2);
                    Color c = Color.Lerp(stops[i0], stops[i0 + 1], g - i0);

                    foreach (var b in blobs)
                    {
                        float fx = Mathf.Max(b.cx, 1f - b.cx) * aspect, fy = Mathf.Max(b.cy, 1f - b.cy);
                        float radius = b.r * Mathf.Sqrt(fx * fx + fy * fy);
                        float dx = (u - b.cx) * aspect, dy = vTop - b.cy;
                        float a = 1f - Mathf.Sqrt(dx * dx + dy * dy) / radius;
                        if (a > 0f) c = Color.Lerp(c, b.c, a * a * (3f - 2f * a));
                    }
                    px[y * w + x] = c;
                }
            }

            tex.SetPixels(px);
            tex.Apply(false, true);
            s_auroraTexture = tex;
            return tex;
        }

        private static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;
    }
}
