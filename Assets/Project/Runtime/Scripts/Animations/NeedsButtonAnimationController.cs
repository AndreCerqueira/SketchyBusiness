using DG.Tweening;
using UnityEngine;

namespace Project.Runtime.Scripts.UI
{
    public class NeedsButtonAnimationController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform _targetTransform;

        [Header("Floating Settings")]
        [SerializeField] private float _floatDistance = 15f;
        [SerializeField] private float _floatDuration = 2f;
        [SerializeField] private Ease _floatEase = Ease.InOutSine;

        [Header("Randomization")]
        [SerializeField] private bool _useRandomization = true;
        [SerializeField] private Vector2 _startDelayRange = new Vector2(0f, 1f);
        [SerializeField] private Vector2 _speedMultiplierRange = new Vector2(0.85f, 1.15f);

        [Header("Low Value Pulse Settings")]
        [SerializeField] private float _lowValueThreshold = 0.3f;
        [SerializeField] private float _pulseScale = 1.1f;
        [SerializeField] private float _pulseDuration = 0.5f;
        [SerializeField] private Ease _pulseEase = Ease.InOutQuad;

        private Tween _floatTween;
        private Tween _pulseTween;
        private bool _isPulsing;
        private Vector2 _initialPos;

        private void Awake()
        {
            if (!_targetTransform) 
                _targetTransform = GetComponent<RectTransform>();
            
            _initialPos = _targetTransform.anchoredPosition;
        }

        private void Start()
        {
            StartFloating();
        }

        private void OnDestroy()
        {
            _floatTween?.Kill();
            _pulseTween?.Kill();
        }

        private void StartFloating()
        {
            _floatTween?.Kill();
            
            _targetTransform.anchoredPosition = _initialPos;

            float delay = 0f;
            float duration = _floatDuration;

            if (_useRandomization)
            {
                delay = Random.Range(_startDelayRange.x, _startDelayRange.y);
                duration = _floatDuration * Random.Range(_speedMultiplierRange.x, _speedMultiplierRange.y);
            }

            _floatTween = _targetTransform
                .DOAnchorPosY(_initialPos.y + _floatDistance, duration)
                .SetEase(_floatEase)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(delay);
        }

        public void HandleValueUpdate(float normalizedValue)
        {
            var isLow = normalizedValue <= _lowValueThreshold;

            if (isLow && !_isPulsing)
            {
                StartPulsing();
            }
            else if (!isLow && _isPulsing)
            {
                StopPulsing();
            }
        }

        private void StartPulsing()
        {
            _isPulsing = true;
            _pulseTween?.Kill();

            _pulseTween = _targetTransform
                .DOScale(Vector3.one * _pulseScale, _pulseDuration)
                .SetEase(_pulseEase)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void StopPulsing()
        {
            _isPulsing = false;
            _pulseTween?.Kill();

            _targetTransform.DOScale(Vector3.one, 0.3f);
        }
    }
}