using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.Runtime.Scripts.Animations
{
    public class PingPongRotationUI : MonoBehaviour
    {
        [SerializeField] private float _rotationSpeed = 2.0f;
        [SerializeField] private float _rotationAngle = 15f;
        [SerializeField] private bool _useRandomStartDelay;
        
        [ShowIf(nameof(_useRandomStartDelay))]
        [SerializeField] private float _minDelay;
        [ShowIf(nameof(_useRandomStartDelay))]
        [SerializeField] private float _maxDelay = 2f;

        private Vector3 _startRotation;

        private void Start()
        {
            _startRotation = transform.localEulerAngles;
            StartCoroutine(PerformRotation());
        }

        private IEnumerator PerformRotation()
        {
            if (_useRandomStartDelay)
            {
                var delay = Random.Range(_minDelay, _maxDelay);
                yield return new WaitForSeconds(delay);
            }

            var elapsedTime = 0f;

            while (true)
            {
                var newZ = _startRotation.z + Mathf.Sin(elapsedTime * _rotationSpeed) * _rotationAngle;
                transform.localRotation = Quaternion.Euler(_startRotation.x, _startRotation.y, newZ);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}