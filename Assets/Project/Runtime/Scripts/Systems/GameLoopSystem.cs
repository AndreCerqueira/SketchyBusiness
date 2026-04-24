using System.Collections;
using Project.Runtime.Scripts.AI;
using Project.Runtime.Scripts.Player;
using UnityEngine;

namespace Project.Runtime.Scripts.Systems
{
    public class GameLoopSystem : MonoBehaviour
    {
        private const float TRANSITION_DELAY = 2f;
        private const string WELCOME_MESSAGE = "Welcome to the show! Let's start round one.";
        private const string PLAYER_DONE_MESSAGE = "The player has finished their drawing!";
        private const string AI_DONE_MESSAGE = "The AI has computed its masterpiece!";
        private const string JUDGING_MESSAGE = "Both are done! Let's see what the judge has to say.";

        [Header("Debug Settings")]
        [SerializeField] private bool _skipIntro;

        [Header("References")]
        [SerializeField] private TtsSystem _ttsSystem;
        [SerializeField] private DrawingSystem _drawingSystem;
        [SerializeField] private GameCameraController _cameraController;
        [SerializeField] private DrawablePaper _drawablePaper;
        [SerializeField] private AiDrawingAnalyzer _aiAnalyzer;
        [SerializeField] private AiTextureDrawer _aiTextureDrawer;
        [SerializeField] private UiSystem _uiSystem;

        public int PlayerScore { get; private set; }
        public int AiScore { get; private set; }

        private bool _isWaitingForIntroTts;
        private bool _isWaitingForJudgeTts;
        private bool _isPlayerDone;
        private bool _isAiDone;
        private bool _isJudgingPhase;
        private string _currentAiBase64;

        private void Start()
        {
            if (_uiSystem != null)
                _uiSystem.Initialize();

            StartGameLoop();
        }

        private void OnEnable()
        {
            if (_drawingSystem != null)
                _drawingSystem.OnTopicGenerated += HandleTopicGenerated;
                
            if (_ttsSystem != null)
                _ttsSystem.OnTtsCompleted += HandleTtsCompleted;

            if (_aiTextureDrawer != null)
                _aiTextureDrawer.OnDrawingRevealed += HandleAiDrawingRevealed;

            if (_aiAnalyzer != null)
                _aiAnalyzer.OnJudgeCompleted += HandleJudgeCompleted;
        }

        private void OnDisable()
        {
            if (_drawingSystem != null)
                _drawingSystem.OnTopicGenerated -= HandleTopicGenerated;
                
            if (_ttsSystem != null)
                _ttsSystem.OnTtsCompleted -= HandleTtsCompleted;

            if (_aiTextureDrawer != null)
                _aiTextureDrawer.OnDrawingRevealed -= HandleAiDrawingRevealed;

            if (_aiAnalyzer != null)
                _aiAnalyzer.OnJudgeCompleted -= HandleJudgeCompleted;
        }

        public void SubmitAndAnalyzeDrawing()
        {
            if (_isPlayerDone) return;

            if (_drawablePaper != null)
                _drawablePaper.CanDraw = false;

            if (_cameraController != null)
                _cameraController.SwitchToStadium();
                
            _isPlayerDone = true;
            
            if (_ttsSystem != null)
                _ttsSystem.Speak(PLAYER_DONE_MESSAGE);

            CheckJudgingCondition();
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

            _isWaitingForIntroTts = true;
            
            if (_ttsSystem != null)
                _ttsSystem.Speak(WELCOME_MESSAGE);
        }

        private void HandleTtsCompleted()
        {
            if (_isWaitingForIntroTts)
            {
                _isWaitingForIntroTts = false;
                StartCoroutine(PrepareDrawingPhaseAsync());
            }
            else if (_isWaitingForJudgeTts)
            {
                _isWaitingForJudgeTts = false;
                StartCoroutine(PrepareDrawingPhaseAsync());
            }
        }

        private IEnumerator PrepareDrawingPhaseAsync()
        {
            yield return new WaitForSeconds(TRANSITION_DELAY);

            _isPlayerDone = false;
            _isAiDone = false;
            _isJudgingPhase = false;
            _currentAiBase64 = string.Empty;

            if (_uiSystem != null)
                _uiSystem.FadeOutJudgingUi();

            if (_aiTextureDrawer != null)
                _aiTextureDrawer.ClearPaper();

            if (_drawablePaper != null)
            {
                _drawablePaper.ClearDrawing();
                _drawablePaper.CanDraw = true;
            }

            if (_cameraController != null)
                _cameraController.SwitchToDrawingBoard();

            if (_drawingSystem != null)
                _drawingSystem.GenerateNewTopic();
        }

        private void HandleTopicGenerated(string category, string word)
        {
            if (_aiTextureDrawer == null) return;
            
            _aiTextureDrawer.RequestAiDrawing(category, word);
        }

        private void HandleAiDrawingRevealed(string base64)
        {
            if (_isAiDone) return;

            _currentAiBase64 = base64;
            _isAiDone = true;

            if (_ttsSystem != null)
                _ttsSystem.Speak(AI_DONE_MESSAGE);

            CheckJudgingCondition();
        }

        private void CheckJudgingCondition()
        {
            if (_isPlayerDone && _isAiDone && !_isJudgingPhase)
            {
                _isJudgingPhase = true;

                if (_uiSystem != null)
                    _uiSystem.FadeInJudgingUi();
                
                if (_ttsSystem != null)
                    _ttsSystem.Speak(JUDGING_MESSAGE);

                if (_aiAnalyzer != null && _drawingSystem != null)
                    _aiAnalyzer.JudgeCurrentDrawing(_currentAiBase64, _drawingSystem.CurrentCategory, _drawingSystem.CurrentWord);
            }
        }

        private void HandleJudgeCompleted(string feedback, string winner)
        {
            if (winner == "Player")
            {
                PlayerScore++;
                if (_uiSystem != null) _uiSystem.UpdatePlayerScore(PlayerScore);
            }
            else if (winner == "AI")
            {
                AiScore++;
                if (_uiSystem != null) _uiSystem.UpdateAiScore(AiScore);
            }

            _isWaitingForJudgeTts = true;
            
            if (_ttsSystem != null)
                _ttsSystem.Speak(feedback);
        }
    }
}