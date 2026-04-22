using FMODUnity;
using UnityEngine;

namespace Project.Runtime.Scripts.Player
{
    public class PlayerFootsteps : MonoBehaviour
    {
        private const float SPEED_THRESHOLD = 0.1f;
        private const float BASE_STEP_INTERVAL = 0.5f;
        private const float SPEED_DIVISOR = 4f;

        [SerializeField] private CharacterController _controller;
        [SerializeField] private EventReference _footstepEvent;

        private float _stepTimer;
        private bool _wasMoving;

        private void Update()
        {
            var speed = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z).magnitude;
            var isMoving = speed > SPEED_THRESHOLD && _controller.isGrounded;

            if (isMoving)
            {
                if (!_wasMoving)
                {
                    PlayFootstepSound();
                    _stepTimer = 0f;
                }
                else
                {
                    var currentInterval = BASE_STEP_INTERVAL / (speed / SPEED_DIVISOR);
                    _stepTimer += Time.deltaTime;

                    if (_stepTimer >= currentInterval)
                    {
                        PlayFootstepSound();
                        _stepTimer = 0f;
                    }
                }
            }
            else
            {
                _stepTimer = 0f;
            }

            _wasMoving = isMoving;
        }

        private void PlayFootstepSound()
        {
            RuntimeManager.PlayOneShot(_footstepEvent, transform.position);
        }
    }
}