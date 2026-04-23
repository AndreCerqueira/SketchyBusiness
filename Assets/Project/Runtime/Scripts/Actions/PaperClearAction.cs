using Project.Runtime.Scripts.Interaction.Interactables;
using UnityEngine;

namespace Project.Runtime.Scripts.Actions
{
    [RequireComponent(typeof(ButtonInteractable))]
    public class PaperClearAction : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DrawablePaper _drawablePaper;

        private ButtonInteractable _buttonInteractable;

        private void Awake()
        {
            _buttonInteractable = GetComponent<ButtonInteractable>();
        }

        private void OnEnable()
        {
            if (_buttonInteractable == null) return;
            
            _buttonInteractable.OnPressed += HandleButtonPressed;
        }

        private void OnDisable()
        {
            if (_buttonInteractable == null) return;
            
            _buttonInteractable.OnPressed -= HandleButtonPressed;
        }

        private void HandleButtonPressed()
        {
            if (_drawablePaper == null) return;
            
            _drawablePaper.ClearDrawing();
        }
    }
}