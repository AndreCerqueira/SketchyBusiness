using System.Collections;
using Project.Runtime.Scripts.AI;
using Project.Runtime.Scripts.Animations;
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
        private const string PLAYER_KEY = "Player";

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
        [SerializeField] private AudienceSystem _audienceSystem;
        [SerializeField] private CameraFocusSwitchSystem _cameraFocusSystem;

        [Header("Animation Handlers")]
        [SerializeField] private ParticipantAnimationHandler _playerAnimationHandler;
        [SerializeField] private ParticipantAnimationHandler _aiAnimationHandler;
        [SerializeField] private ParticipantAnimationHandler _presenterAnimationHandler;

        private bool _isWaitingForIntroTts;
        private bool _isWaitingForTopicTts;
        private bool _isWaitingForJudgeTts;
        private bool _isWaitingForGameOverTts;
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
            if (_audioSystem != null) _audioSystem.PlayMainMenuMusic();
        }

        private void OnEnable()
        {
            if (_drawingSystem != null) _drawingSystem.OnTopicGenerated += HandleTopicGenerated;
            if (_ttsSystem != null)
            {
                _ttsSystem.OnTtsStarted += HandleTtsStarted;
                _ttsSystem.OnTtsCompleted += HandleTtsCompleted;
            }
            if (_aiTextureDrawer != null) _aiTextureDrawer.OnDrawingRevealed += HandleAiDrawingRevealed;
            if (_aiAnalyzer != null) _aiAnalyzer.OnJudgeCompleted += HandleJudgeCompleted;
            if (_scoreSystem != null) _scoreSystem.OnGameEnded += HandleGameEnded;
        }

        private void OnDisable()
        {
            if (_drawingSystem != null) _drawingSystem.OnTopicGenerated -= HandleTopicGenerated;
            if (_ttsSystem != null)
            {
                _ttsSystem.OnTtsStarted -= HandleTtsStarted;
                _ttsSystem.OnTtsCompleted -= HandleTtsCompleted;
            }
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

        private void HandleTtsStarted()
        {
            if (_presenterAnimationHandler != null)
                _presenterAnimationHandler.SetSpeaking(true);
        }

        private void HandleTtsCompleted()
        {
            if (_presenterAnimationHandler != null)
                _presenterAnimationHandler.SetSpeaking(false);

            if (_isWaitingForGameOverTts)
            {
                _isWaitingForGameOverTts = false;
                if (_ttsSystem != null) _ttsSystem.TurnOff();
                return;
            }

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

                if (_scoreSystem != null && _scoreSystem.HasGameEnded)
                {
                    TriggerGameOverAnimations(_pendingWinner);
                    if (_audienceSystem != null) _audienceSystem.PlayGameOverClaps();
                    return;
                }

                TriggerParticipantAnimations(_pendingWinner);
                if (_audienceSystem != null) _audienceSystem.PlayRoundWinClaps();
                StartCoroutine(PrepareDrawingPhaseAsync());
            }
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

        private void HandleGameEnded(string winner)
        {
            _pendingWinner = winner;

            if (_winParticles != null) _winParticles.Play();
            if (_gameOverUI != null) _gameOverUI.Show(winner);

            if (_dialogueSystem != null)
            {
                _dialogueSystem.CancelAllDialogues();
                _dialogueSystem.PlayGameOver(winner);
            }

            _isWaitingForGameOverTts = true;
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

            if (_audioSystem != null) _audioSystem.PlayThinkingMusic();

            if (_dialogueSystem != null)
            {
                _dialogueSystem.CancelAllDialogues();
                _dialogueSystem.PlayJudgingIntro();
                _dialogueSystem.StartThinkingFiller();
            }

            if (_cameraFocusSystem != null) _cameraFocusSystem.StartRandomFocusSwitches();

            if (_uiSystem != null) _uiSystem.FadeInJudgingUi();

            if (_aiAnalyzer != null && _drawingSystem != null)
                _aiAnalyzer.JudgeCurrentDrawing(_currentAiBase64, _drawingSystem.CurrentCategory, _drawingSystem.CurrentWord);
        }

        private void HandleJudgeCompleted(string feedback, string winner)
        {
            _pendingWinner = winner;
            _isWaitingForJudgeTts = true;

            if (_cameraFocusSystem != null) _cameraFocusSystem.StopFocusSwitches();

            if (_audioSystem != null) _audioSystem.PlayWinSfx(winner);

            if (_dialogueSystem != null)
            {
                _dialogueSystem.StopThinkingFiller();
                _dialogueSystem.PlayJudgeFeedback(feedback);
            }
        }

        private void TriggerParticipantAnimations(string winner)
        {
            if (_playerAnimationHandler == null || _aiAnimationHandler == null) return;

            if (winner == PLAYER_KEY)
            {
                _playerAnimationHandler.PlayHappy();
                _aiAnimationHandler.PlaySad();
            }
            else
            {
                _playerAnimationHandler.PlaySad();
                _aiAnimationHandler.PlayHappy();
            }
        }

        private void TriggerGameOverAnimations(string winner)
        {
            if (_playerAnimationHandler == null || _aiAnimationHandler == null) return;

            if (winner == PLAYER_KEY)
            {
                _playerAnimationHandler.SetVictoryLoop(true);
                _aiAnimationHandler.SetSadLoop(true);
            }
            else
            {
                _aiAnimationHandler.SetVictoryLoop(true);
                _playerAnimationHandler.SetSadLoop(true);
            }
        }
    }
}