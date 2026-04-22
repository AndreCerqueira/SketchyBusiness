using System.Text.RegularExpressions;
using Project.Runtime.Scripts.Data;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Runtime.Scripts.Interaction
{
    public class InteractionWarningViewUI : MonoBehaviour
    {
        private const string PROMPT_FORMAT = "[{0}] {1}";
        private const int DEFAULT_BINDING_INDEX = 0;
        private const float VISIBLE_ALPHA = 1f;
        private const float INVISIBLE_ALPHA = 0f;
        
        [Header("References")]
        [SerializeField] private PlayerInteractionController _interactionController;
        [SerializeField] private CanvasGroup _uiPrompt;
        [SerializeField] private TextMeshProUGUI _promptText;

        private InteractionAction _currentRaycastAction = InteractionAction.None;
        private InteractionAction _lastDisplayedAction = InteractionAction.None;
        private bool _wasTaskActive;

        private void OnEnable()
        {
            _interactionController.OnInteractionStateChanged += HandleInteractionStateChanged;
        }

        private void OnDisable()
        {
            _interactionController.OnInteractionStateChanged -= HandleInteractionStateChanged;
        }

        private void Start()
        {
            UpdatePromptVisibility(InteractionAction.None);
        }

        private void Update()
        {
            if (_currentRaycastAction == _lastDisplayedAction) return;
            UpdatePromptVisibility(_currentRaycastAction);
            _lastDisplayedAction = _currentRaycastAction;
        }

        private void HandleInteractionStateChanged(InteractionAction action)
        {
            _currentRaycastAction = action;
        }

        private void UpdatePromptVisibility(InteractionAction action)
        {
            var isVisible = action != InteractionAction.None;
            
            _uiPrompt.alpha = isVisible ? VISIBLE_ALPHA : INVISIBLE_ALPHA;
            _uiPrompt.interactable = isVisible;
            _uiPrompt.blocksRaycasts = isVisible;

            if (!isVisible) return;
            
            var bindingKey = _interactionController.InteractActionInput.action.GetBindingDisplayString(DEFAULT_BINDING_INDEX);
            var formattedAction = Regex.Replace(action.ToString(), "([a-z])([A-Z])", "$1 $2");
            
            _promptText.text = string.Format(PROMPT_FORMAT, bindingKey, formattedAction);
        }
    }
}