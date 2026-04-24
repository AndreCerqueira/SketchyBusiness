using Project.Runtime.Scripts.Interaction;
using Project.Runtime.Scripts.Systems;
using UnityEngine;

namespace Project.Runtime.Scripts.Actions
{
    [RequireComponent(typeof(ButtonInteractable))]
    public class SubmitDrawingAction : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameLoopSystem _gameLoopSystem;

        private ButtonInteractable _buttonInteractable;

        private void Awake()
        {
            _buttonInteractable = GetComponent<ButtonInteractable>();
        }

        private void OnEnable()
        {
            if (_buttonInteractable != null)
                _buttonInteractable.OnPressed += HandleButtonPressed;
        }

        private void OnDisable()
        {
            if (_buttonInteractable != null)
                _buttonInteractable.OnPressed -= HandleButtonPressed;
        }

        private void HandleButtonPressed()
        {
            if (_gameLoopSystem == null) return;
            
            _gameLoopSystem.SubmitAndAnalyzeDrawing();
        }
    }
}