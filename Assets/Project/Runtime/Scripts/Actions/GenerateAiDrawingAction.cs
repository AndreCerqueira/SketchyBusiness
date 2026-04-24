using Project.Runtime.Scripts.AI;
using Project.Runtime.Scripts.Interaction;
using Project.Runtime.Scripts.Systems;
using UnityEngine;

namespace Project.Runtime.Scripts.Actions
{
    [RequireComponent(typeof(ButtonInteractable))]
    public class GenerateAiDrawingAction : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DrawingSystem _drawingSystem;
        [SerializeField] private AiTextureDrawer _aiTextureDrawer;

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
            if (_drawingSystem == null || _aiTextureDrawer == null) return;
            if (!_drawingSystem.HasActiveTopic) return;
            if (_aiTextureDrawer.IsGenerating) return;

            _aiTextureDrawer.RequestAiDrawing(_drawingSystem.CurrentCategory, _drawingSystem.CurrentWord);
        }
    }
}