using Project.Runtime.Scripts.AI;
using Project.Runtime.Scripts.Interaction.Interactables;
using UnityEngine;

namespace Project.Runtime.Scripts
{
    [RequireComponent(typeof(ButtonInteractable))]
    public class PaperAnalyzeAction : MonoBehaviour
    {
        [SerializeField] private AiDrawingAnalyzer _aiAnalyzer;

        private ButtonInteractable _buttonInteractable;

        private void Awake()
        {
            _buttonInteractable = GetComponent<ButtonInteractable>();
        }

        private void OnEnable()
        {
            if (_buttonInteractable != null)
                _buttonInteractable.OnPressed += HandleButtonPressed;

            if (_aiAnalyzer != null)
            {
                _aiAnalyzer.OnAnalysisCompleted += HandleAnalysisCompleted;
                _aiAnalyzer.OnAnalysisFailed += HandleAnalysisFailed;
            }
        }

        private void OnDisable()
        {
            if (_buttonInteractable != null)
                _buttonInteractable.OnPressed -= HandleButtonPressed;

            if (_aiAnalyzer != null)
            {
                _aiAnalyzer.OnAnalysisCompleted -= HandleAnalysisCompleted;
                _aiAnalyzer.OnAnalysisFailed -= HandleAnalysisFailed;
            }
        }

        private void HandleButtonPressed()
        {
            if (_aiAnalyzer == null) return;
            
            Debug.Log("Sending drawing to AI...");
            _aiAnalyzer.AnalyzeCurrentDrawing();
        }

        private void HandleAnalysisCompleted(string result)
        {
            Debug.Log($"AI Analysis Success: {result}");
        }

        private void HandleAnalysisFailed(string error)
        {
            Debug.LogError($"AI Analysis Failed: {error}");
        }
    }
}