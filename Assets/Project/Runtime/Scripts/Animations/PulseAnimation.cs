using DG.Tweening;
using UnityEngine;

namespace Project.Runtime.Scripts.Animations
{
    public class PulseAnimation : MonoBehaviour
    {
        [SerializeField] private float _scaleMultiplier = 1.1f;
        [SerializeField] private float _duration = 0.6f;
        [SerializeField] private Ease _easeType = Ease.InOutSine;

        private Tween _tween;
        private Vector3 _startScale;

        private void Start()
        {
            _startScale = transform.localScale;
            StartAnimation();
        }

        private void OnDestroy()
        {
            _tween?.Kill();
        }

        private void StartAnimation()
        {
            _tween = transform
                .DOScale(_startScale * _scaleMultiplier, _duration)
                .SetEase(_easeType)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
}