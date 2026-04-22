using Project.Runtime.Scripts.Data;
using UnityEngine;

namespace Project.Runtime.Scripts.Player
{
    public class CameraBobbing : MonoBehaviour
    {
        [SerializeField] private CharacterController _controller;
        [SerializeField] private Transform _cameraTarget;
        [SerializeField] private CameraBobbingSettings _settings;

        private float _timer;
        private Vector3 _startPosition;

        private void Awake()
        {
            _startPosition = _cameraTarget.localPosition;
        }

        private void Update()
        {
            var speed = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z).magnitude;

            if (speed > _settings.SpeedThreshold && _controller.isGrounded)
            {
                _timer += Time.deltaTime * _settings.Frequency * (speed / _settings.SpeedDivisor); 
                
                var newY = _startPosition.y + Mathf.Sin(_timer) * _settings.Amplitude;
                var newX = _startPosition.x + Mathf.Cos(_timer / _settings.HorizontalTimeDivisor) * (_settings.Amplitude / _settings.HorizontalAmplitudeDivisor);
                
                _cameraTarget.localPosition = new Vector3(newX, newY, _startPosition.z);
            }
            else
            {
                _timer = 0f;
                _cameraTarget.localPosition = Vector3.Lerp(_cameraTarget.localPosition, _startPosition, Time.deltaTime * _settings.ResetSpeed);
            }
        }
    }
}