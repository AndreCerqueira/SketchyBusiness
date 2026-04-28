using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Project.Runtime.Scripts.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class GameOverViewUI : MonoBehaviour
    {
        private const string PLAYER_KEY = "Player";
        private const string PLAYER_COLOR_HEX = "#527ACB";
        private const string AI_COLOR_HEX = "#DB7F4A";

        [Header("References")]
        [SerializeField] private TextMeshProUGUI _winnerTitleText;
        [SerializeField] private TextMeshProUGUI _thankYouText;
        [SerializeField] private TextMeshProUGUI _restartInstructionText;
        [SerializeField] private AudioSource _audioSource;

        [Header("Audio")]
        [SerializeField] private AudioClip _victorySound;

        [Header("Settings")]
        [SerializeField] private float _initialDelay = 2.0f;
        [SerializeField] private float _fadeDuration = 1.5f;
        [SerializeField] private float _textAnimationInterval = 0.3f;

        private CanvasGroup _canvasGroup;
        private bool _canRestart;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private void Update()
        {
            if (!_canRestart) return;
            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
                RestartGame();
        }

        public void Show(string winner)
        {
            ApplyWinnerStyle(winner);
            
            if (_audioSource != null && _victorySound != null)
                _audioSource.PlayOneShot(_victorySound);
            
            var sequence = DOTween.Sequence();
            
            sequence.AppendInterval(_initialDelay);
            sequence.Append(_canvasGroup.DOFade(1f, _fadeDuration));
            sequence.AppendCallback(() => _canvasGroup.blocksRaycasts = true);
            
            sequence.Append(AnimateTextScale(_winnerTitleText));
            sequence.AppendInterval(_textAnimationInterval);
            sequence.Append(AnimateTextScale(_thankYouText));
            sequence.AppendInterval(_textAnimationInterval);
            sequence.Append(AnimateTextScale(_restartInstructionText));
            
            sequence.OnComplete(() => _canRestart = true);
        }

        private void ApplyWinnerStyle(string winner)
        {
            var hexColor = winner == PLAYER_KEY ? PLAYER_COLOR_HEX : AI_COLOR_HEX;
            
            if (ColorUtility.TryParseHtmlString(hexColor, out var color))
                _winnerTitleText.color = color;

            _winnerTitleText.text = $"<wave a=0.1 f=1>{winner} Won!</wave>";
        }

        private Tween AnimateTextScale(TextMeshProUGUI text)
        {
            text.gameObject.SetActive(true);
            text.transform.localScale = Vector3.zero;
            return text.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }

        private void RestartGame()
        {
            _canRestart = false;
            DOTween.KillAll();
            
            var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex);
        }
    }
}