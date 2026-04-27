using System.Collections;
using UnityEngine;

namespace Project.Runtime.Scripts.Animations
{
    public class FloatingViewUI : MonoBehaviour
    {
        [SerializeField] private float _floatSpeed = 2.0f;
        [SerializeField] private float _floatHeight = 2f;
        [SerializeField] private bool _enableRandomDelay;

        private const float MinDelay = 0.1f;
        private const float MaxDelay = 1.5f;

        private Vector3 _startPos;

        private void Start()
        {
            _startPos = transform.localPosition;
            StartCoroutine(PerformFloat());
        }

        private IEnumerator PerformFloat()
        {
            var timeOffset = 0f;

            if (_enableRandomDelay)
            {
                var delay = Random.Range(MinDelay, MaxDelay);
                yield return new WaitForSeconds(delay);
                timeOffset = Time.time;
            }

            while (true)
            {
                var time = _enableRandomDelay ? Time.time - timeOffset : Time.time;
                var newY = _startPos.y + Mathf.Sin(time * _floatSpeed) * _floatHeight;
                transform.localPosition = new Vector3(_startPos.x, newY, _startPos.z);
                yield return null;
            }
        }
    }
}