using System;
using UnityEngine;
using UnityEngine.UIElements;
using UxmlBindings;

namespace StudyGame.UI.Views
{
    /// <summary>
    /// UI Toolkit view for the Neo-Punk Visual Novel Dialog System.
    /// </summary>
    public class VisualNovelView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<VisualNovelView, UxmlTraits> { }

        // Data model (Mock)
        public interface IDialogModel
        {
            string CurrentSpeaker { get; }
            string CurrentText { get; }
            bool HasAmber { get; }
            bool HasCyan { get; }
            bool HasEmerald { get; }
            
            event Action OnNodeChanged;
            void AdvanceDialog();
        }

        private IDialogModel _model;
        private VisualNovelViewBinding _binding;

        public VisualNovelView()
        {
            this.RegisterCallback<GeometryChangedEvent>(OnFirstLayout);
        }

        private void OnFirstLayout(GeometryChangedEvent evt)
        {
            this.UnregisterCallback<GeometryChangedEvent>(OnFirstLayout);
            _binding = new VisualNovelViewBinding(this);
            BindUIEvents();
        }

        private void BindUIEvents()
        {
            _binding.VisualElement_ClickZone?.RegisterCallback<ClickEvent>(e =>
            {
                _model?.AdvanceDialog();
            });

            _binding.Button_Log?.RegisterCallback<ClickEvent>(e =>
            {
                if (_binding.VisualElement_BacklogModal != null) _binding.VisualElement_BacklogModal.style.display = DisplayStyle.Flex;
            });

            _binding.Button_CloseLog?.RegisterCallback<ClickEvent>(e =>
            {
                if (_binding.VisualElement_BacklogModal != null) _binding.VisualElement_BacklogModal.style.display = DisplayStyle.None;
            });

            this.RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        public void BindModel(IDialogModel model)
        {
            if (_model != null)
            {
                _model.OnNodeChanged -= HandleNodeChanged;
            }

            _model = model;
            
            if (_model != null)
            {
                _model.OnNodeChanged += HandleNodeChanged;
                HandleNodeChanged(); // Initial update
            }
        }

        private void HandleNodeChanged()
        {
            if (_model == null) return;

            if (_binding.Label_CharacterName != null) _binding.Label_CharacterName.text = _model.CurrentSpeaker;
            if (_binding.Label_DialogueText != null) _binding.Label_DialogueText.text = _model.CurrentText;

            // Update Metadata Gems
            if (_binding.Gem_Amber != null) _binding.Gem_Amber.style.opacity = _model.HasAmber ? 1f : 0.4f;
            if (_binding.Gem_Cyan != null) _binding.Gem_Cyan.style.opacity = _model.HasCyan ? 1f : 0.4f;
            if (_binding.Gem_Emerald != null) _binding.Gem_Emerald.style.opacity = _model.HasEmerald ? 1f : 0.4f;
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            if (_model != null)
            {
                _model.OnNodeChanged -= HandleNodeChanged;
                _model = null; 
            }
        }
    }
}
