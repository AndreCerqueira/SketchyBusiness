using System.Collections;
using Project.Runtime.Scripts.Player;
using UnityEngine;

namespace Project.Runtime.Scripts.Systems
{
    public class GameLoopSystem : MonoBehaviour
    {
        private const float TRANSITION_DELAY = 2f;
        private const string WELCOME_MESSAGE = "Welcome to the show! Let's start round one.";

        [Header("Debug Settings")]
        [SerializeField] private bool _skipIntro;

        [Header("References")]
        [SerializeField] private TtsSystem _ttsSystem;
        [SerializeField] private DrawingSystem _drawingSystem;
        [SerializeField] private GameCameraController _cameraController;
        [SerializeField] private DrawablePaper _drawablePaper;

        private bool _isWaitingForTts;

        private void Start()
        {
            StartGameLoop();
        }

        private void OnEnable()
        {
            if (_drawingSystem != null)
                _drawingSystem.OnAnalysisCompleted += HandleAnalysisCompleted;
                
            if (_ttsSystem != null)
                _ttsSystem.OnTtsCompleted += HandleTtsCompleted;
        }

        private void OnDisable()
        {
            if (_drawingSystem != null)
                _drawingSystem.OnAnalysisCompleted -= HandleAnalysisCompleted;
                
            if (_ttsSystem != null)
                _ttsSystem.OnTtsCompleted -= HandleTtsCompleted;
        }

        public void SubmitAndAnalyzeDrawing()
        {
            if (_drawablePaper != null)
                _drawablePaper.CanDraw = false;

            if (_cameraController != null)
                _cameraController.SwitchToStadium();
                
            if (_drawingSystem != null)
                _drawingSystem.SubmitDrawing();
        }

        private void StartGameLoop()
        {
            if (_skipIntro)
            {
                StartCoroutine(PrepareDrawingPhaseAsync());
                return;
            }

            if (_cameraController != null)
                _cameraController.SwitchToStadium();

            _isWaitingForTts = true;
            
            if (_ttsSystem != null)
                _ttsSystem.Speak(WELCOME_MESSAGE);
        }

        private void HandleTtsCompleted()
        {
            if (!_isWaitingForTts) return;
            
            _isWaitingForTts = false;
            StartCoroutine(PrepareDrawingPhaseAsync());
        }

        private IEnumerator PrepareDrawingPhaseAsync()
        {
            yield return new WaitForSeconds(TRANSITION_DELAY);

            if (_cameraController != null)
                _cameraController.SwitchToDrawingBoard();

            if (_drawingSystem != null)
                _drawingSystem.GenerateNewTopic();

            if (_drawablePaper != null)
                _drawablePaper.CanDraw = true;
        }

        private void HandleAnalysisCompleted(string feedback)
        {
            _isWaitingForTts = true;
            
            if (_ttsSystem != null)
                _ttsSystem.Speak(feedback);
        }
    }
}