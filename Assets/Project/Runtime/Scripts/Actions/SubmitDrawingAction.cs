using Project.Runtime.Scripts.Interaction.Interactables;
using Project.Runtime.Scripts.Systems;
using UnityEngine;

namespace Project.Runtime.Scripts.Actions
{
    [RequireComponent(typeof(ButtonInteractable))]
    public class SubmitDrawingAction : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DrawingSystem _drawingSystem;

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
            if (_drawingSystem == null) return;
            
            _drawingSystem.SubmitDrawing();
        }
    }
}