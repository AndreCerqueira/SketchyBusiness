using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.Systems
{
    public class UiSystem : MonoBehaviour
    {
        private const float FADE_DURATION = 1f;
        private const float PUNCH_DURATION = 0.5f;
        private const float PUNCH_STRENGTH = 0.2f;
        private const float PROGRESS_DURATION = 0.5f;

        [Header("Canvas Groups")]
        [SerializeField] private CanvasGroup _playerCanvasGroup;
        [SerializeField] private CanvasGroup _aiCanvasGroup;

        [Header("Score Texts")]
        [SerializeField] private TextMeshProUGUI _playerScoreText;
        [SerializeField] private TextMeshProUGUI _aiScoreText;

        [Header("Progress Bars")]
        [SerializeField] private Image _playerProgressBar;
        [SerializeField] private Image _aiProgressBar;

        public void Initialize()
        {
            if (_playerCanvasGroup != null) _playerCanvasGroup.alpha = 0f;
            if (_aiCanvasGroup != null) _aiCanvasGroup.alpha = 0f;
            
            UpdatePlayerScore(0);
            UpdateAiScore(0);

            if (_playerProgressBar != null) _playerProgressBar.fillAmount = 0f;
            if (_aiProgressBar != null) _aiProgressBar.fillAmount = 0f;
        }

        public void FadeInJudgingUi()
        {
            FadeCanvasGroup(_playerCanvasGroup, 1f);
            FadeCanvasGroup(_aiCanvasGroup, 1f);
        }

        public void FadeOutJudgingUi()
        {
            FadeCanvasGroup(_playerCanvasGroup, 0f);
            FadeCanvasGroup(_aiCanvasGroup, 0f);
        }

        public void UpdatePlayerScore(int score)
        {
            if (_playerScoreText == null) return;

            _playerScoreText.text = score.ToString();
            AnimateScore(_playerScoreText.transform);
        }

        public void UpdateAiScore(int score)
        {
            if (_aiScoreText == null) return;

            _aiScoreText.text = score.ToString();
            AnimateScore(_aiScoreText.transform);
        }

        public void UpdatePlayerProgress(int currentScore, int maxScore)
        {
            if (_playerProgressBar == null) return;

            var targetFill = (float)currentScore / maxScore;
            _playerProgressBar.DOFillAmount(targetFill, PROGRESS_DURATION).SetEase(Ease.OutCubic);
        }

        public void UpdateAiProgress(int currentScore, int maxScore)
        {
            if (_aiProgressBar == null) return;

            var targetFill = (float)currentScore / maxScore;
            _aiProgressBar.DOFillAmount(targetFill, PROGRESS_DURATION).SetEase(Ease.OutCubic);
        }

        private void FadeCanvasGroup(CanvasGroup group, float targetAlpha)
        {
            if (group == null) return;

            group.DOKill();
            group.DOFade(targetAlpha, FADE_DURATION);
        }

        private void AnimateScore(Transform target)
        {
            target.DOKill();
            target.localScale = Vector3.one;
            target.DOPunchScale(Vector3.one * PUNCH_STRENGTH, PUNCH_DURATION);
        }
    }
}