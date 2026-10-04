using System;
using UnityEngine;
using UnityEngine.UIElements;
using UxmlBindings;

namespace StudyGame.UI.Views
{
    /// <summary>
    /// Static UI Window for the Lobby.
    /// Uses Endava Binding Generator struct for resolving UI elements.
    /// </summary>
    public class AnimeLobbyView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<AnimeLobbyView, UxmlTraits> { }

        // Data model (Mock)
        public interface ILobbyModel
        {
            int Level { get; }
            int Energy { get; }
            int Gold { get; }
            int Maigo { get; }
            
            event Action OnDataChanged;
        }

        private ILobbyModel _model;
        private AnimeLobbyViewBinding _binding;

        public AnimeLobbyView()
        {
            this.RegisterCallback<GeometryChangedEvent>(OnFirstLayout);
        }

        private void OnFirstLayout(GeometryChangedEvent evt)
        {
            this.UnregisterCallback<GeometryChangedEvent>(OnFirstLayout);
            _binding = new AnimeLobbyViewBinding(this);
            BindEvents();
        }

        private void BindEvents()
        {
            _binding.Button_Gacha?.RegisterCallback<ClickEvent>(e => Debug.Log("Gacha Clicked"));
            _binding.Button_Roster?.RegisterCallback<ClickEvent>(e => Debug.Log("Roster Clicked"));
            _binding.Button_Inventory?.RegisterCallback<ClickEvent>(e => Debug.Log("Inventory Clicked"));

            this.RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        public void BindModel(ILobbyModel model)
        {
            if (_model != null)
            {
                _model.OnDataChanged -= HandleDataChanged;
            }

            _model = model;
            if (_model != null)
            {
                _model.OnDataChanged += HandleDataChanged;
                HandleDataChanged(); // Initial update
            }
        }

        private void HandleDataChanged()
        {
            if (_model == null) return;

            if (_binding.Label_PlayerLevel != null) _binding.Label_PlayerLevel.text = $"Lv. {_model.Level}";
            if (_binding.Label_Energy != null) _binding.Label_Energy.text = _model.Energy.ToString();
            if (_binding.Label_Coin != null) _binding.Label_Coin.text = _model.Gold.ToString();
            if (_binding.Label_Maigo != null) _binding.Label_Maigo.text = _model.Maigo.ToString();
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            if (_model != null)
            {
                _model.OnDataChanged -= HandleDataChanged;
            }
        }
    }
}
