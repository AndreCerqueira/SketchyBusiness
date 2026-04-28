using System.Collections;
using Project.Runtime.Scripts.AI;
using Project.Runtime.Scripts.Player;
using Project.Runtime.Scripts.UI;
using UnityEngine;

namespace Project.Runtime.Scripts.Systems
{
    public class GameLoopSystem : MonoBehaviour
    {
        private const float START_TRANSITION_DELAY = 0f;
        private const float ROUND_TRANSITION_DELAY = 2f;
        private const float INTRO_DAMPING = 10f;
        private const float GAME_DAMPING = 2f;

        [Header("Debug Settings")]
        [SerializeField] private bool _skipIntro;

        [Header("References")]
        [SerializeField] private DrawingSystem _drawingSystem;
        [SerializeField] private GameCameraController _cameraController;
        [SerializeField] private DrawablePaper _drawablePaper;
        [SerializeField] private AiDrawingAnalyzer _aiAnalyzer;
        [SerializeField] private AiTextureDrawer _aiTextureDrawer;
        [SerializeField] private UiSystem _uiSystem;
        [SerializeField] private GameDialogueSystem _dialogueSystem;
        [SerializeField] private TtsSystem _ttsSystem;
        [SerializeField] private ScoreSystem _scoreSystem;
        [SerializeField] private ShowAudioSystem _audioSystem;
        [SerializeField] private GameOverViewUI _gameOverUI;
        [SerializeField] private ParticleSystem _winParticles;

        private bool _isWaitingForIntroTts;
        private bool _isWaitingForTopicTts;
        private bool _isWaitingForJudgeTts;
        private bool _isPlayerDone;
        private bool _isAiDone;
        private bool _isJudgingPhase;
        private bool _isFirstTransition = true;
        private int _currentRound;
        private string _currentAiBase64;
        private string _pendingWinner;

        private void Start()
        {
            if (_uiSystem != null) _uiSystem.Initialize();
            if (_cameraController != null) _cameraController.SwitchToMainMenu();
        }

        private void OnEnable()
        {
            if (_drawingSystem != null) _drawingSystem.OnTopicGenerated += HandleTopicGenerated;
            if (_ttsSystem != null) _ttsSystem.OnTtsCompleted += HandleTtsCompleted;
            if (_aiTextureDrawer != null) _aiTextureDrawer.OnDrawingRevealed += HandleAiDrawingRevealed;
            if (_aiAnalyzer != null) _aiAnalyzer.OnJudgeCompleted += HandleJudgeCompleted;
            if (_scoreSystem != null) _scoreSystem.OnGameEnded += HandleGameEnded;
        }

        private void OnDisable()
        {
            if (_drawingSystem != null) _drawingSystem.OnTopicGenerated -= HandleTopicGenerated;
            if (_ttsSystem != null) _ttsSystem.OnTtsCompleted -= HandleTtsCompleted;
            if (_aiTextureDrawer != null) _aiTextureDrawer.OnDrawingRevealed -= HandleAiDrawingRevealed;
            if (_aiAnalyzer != null) _aiAnalyzer.OnJudgeCompleted -= HandleJudgeCompleted;
            if (_scoreSystem != null) _scoreSystem.OnGameEnded -= HandleGameEnded;
        }

        public void StartGame()
        {
            if (_skipIntro)
            {
                StartCoroutine(PrepareDrawingPhaseAsync());
                return;
            }

            if (_cameraController != null)
            {
                _cameraController.SetDamping(INTRO_DAMPING);
                _cameraController.SwitchToStadium();
            }

            if (_audioSystem != null) _audioSystem.PlayIntroTheme();
            
            _isWaitingForIntroTts = true;
            
            if (_dialogueSystem != null) _dialogueSystem.PlayIntro();
        }

        public void SubmitAndAnalyzeDrawing()
        {
            if (_isPlayerDone) return;

            if (_drawablePaper != null) _drawablePaper.CanDraw = false;
            
            if (_cameraController != null)
            {
                _cameraController.SetDamping(GAME_DAMPING);
                _cameraController.SwitchToStadium();
            }
                
            _isPlayerDone = true;

            CheckJudgingCondition();
        }

        private void HandleTtsCompleted()
        {
            if (_isWaitingForIntroTts)
            {
                _isWaitingForIntroTts = false;
                StartCoroutine(PrepareDrawingPhaseAsync());
                return;
            }

            if (_isWaitingForTopicTts)
            {
                _isWaitingForTopicTts = false;
                if (_dialogueSystem != null) _dialogueSystem.StartFillerRoutine();
                return;
            }

            if (_isWaitingForJudgeTts)
            {
                _isWaitingForJudgeTts = false;
                
                if (_scoreSystem != null && !string.IsNullOrEmpty(_pendingWinner))
                    _scoreSystem.AddPoint(_pendingWinner);

                if (_scoreSystem != null && _scoreSystem.HasGameEnded) return;

                StartCoroutine(PrepareDrawingPhaseAsync());
            }
        }

        private void HandleGameEnded(string winner)
        {
            if (_winParticles != null) _winParticles.Play();
            if (_gameOverUI != null) _gameOverUI.Show(winner);
            if (_dialogueSystem != null) _dialogueSystem.PlayGameOver(winner);
        }

        private IEnumerator PrepareDrawingPhaseAsync()
        {
            if (_cameraController != null) _cameraController.SetDamping(GAME_DAMPING);
            if (_audioSystem != null) _audioSystem.PlayTopicSuspense();
            
            var delay = _isFirstTransition ? START_TRANSITION_DELAY : ROUND_TRANSITION_DELAY;
            yield return new WaitForSeconds(delay);
            _isFirstTransition = false;

            _isPlayerDone = false;
            _isAiDone = false;
            _isJudgingPhase = false;
            _currentAiBase64 = string.Empty;
            _pendingWinner = string.Empty;

            if (_uiSystem != null) _uiSystem.FadeOutJudgingUi();
            if (_aiTextureDrawer != null) _aiTextureDrawer.ClearPaper();
            if (_drawablePaper != null) _drawablePaper.ClearDrawing();
            if (_cameraController != null) _cameraController.SwitchToDrawingBoard();

            if (_audioSystem != null) _audioSystem.PlayTopicReveal();

            if (_drawingSystem != null)
            {
                _currentRound++;
                _drawingSystem.GenerateNewTopic();
                
                if (_drawablePaper != null) _drawablePaper.CanDraw = true;
                _isWaitingForTopicTts = true;
                
                if (_dialogueSystem != null)
                    _dialogueSystem.PlayTopicAnnouncement(_currentRound, _drawingSystem.CurrentCategory, _drawingSystem.CurrentWord);
            }
        }

        private void HandleTopicGenerated(string category, string word)
        {
            if (_aiTextureDrawer != null) _aiTextureDrawer.RequestAiDrawing(category, word);
        }

        private void HandleAiDrawingRevealed(string base64)
        {
            if (_isAiDone) return;

            _currentAiBase64 = base64;
            _isAiDone = true;

            CheckJudgingCondition();
        }

        private void CheckJudgingCondition()
        {
            if (!_isPlayerDone || !_isAiDone || _isJudgingPhase) return;

            _isJudgingPhase = true;
            
            if (_audioSystem != null) _audioSystem.PlayJudgingTension();
            
            if (_dialogueSystem != null)
            {
                _dialogueSystem.CancelAllDialogues();
                _dialogueSystem.PlayJudgingIntro();
                _dialogueSystem.StartThinkingFiller();
            }

            if (_uiSystem != null) _uiSystem.FadeInJudgingUi();

            if (_aiAnalyzer != null && _drawingSystem != null)
                _aiAnalyzer.JudgeCurrentDrawing(_currentAiBase64, _drawingSystem.CurrentCategory, _drawingSystem.CurrentWord);
        }

        private void HandleJudgeCompleted(string feedback, string winner)
        {
            _pendingWinner = winner;
            _isWaitingForJudgeTts = true;

            if (_audioSystem != null) _audioSystem.PlayWinSfx(winner);

            if (_dialogueSystem != null)
            {
                _dialogueSystem.StopThinkingFiller();
                _dialogueSystem.PlayJudgeFeedback(feedback);
            }
        }
    }
}