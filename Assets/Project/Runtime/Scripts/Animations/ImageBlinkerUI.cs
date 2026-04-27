using System.Collections;
using UnityEngine;

namespace Project.Runtime.Scripts.Animations
{
    [RequireComponent(typeof(CanvasGroup))]
    public class AlphaPulseViewUI : MonoBehaviour
    {
        [SerializeField] private float _speed = 2.0f;
        [SerializeField] private float _minAlpha = 0.4f;
        [SerializeField] private float _maxAlpha = 1.0f;
        [SerializeField] private bool _enableRandomDelay;

        private const float MIN_DELAY = 0.1f;
        private const float MAX_DELAY = 1.5f;

        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            StartCoroutine(PerformPulse());
        }

        private IEnumerator PerformPulse()
        {
            var timeOffset = 0f;

            if (_enableRandomDelay)
            {
                var delay = Random.Range(MIN_DELAY, MAX_DELAY);
                yield return new WaitForSeconds(delay);
                timeOffset = Time.time;
            }

            while (true)
            {
                var time = _enableRandomDelay ? Time.time - timeOffset : Time.time;
                var t = (Mathf.Sin(time * _speed) + 1f) / 2f;
                _canvasGroup.alpha = Mathf.Lerp(_minAlpha, _maxAlpha, t);
                yield return null;
            }
        }
    }
}