using UnityEngine;

namespace Project.Runtime.Scripts.Player
{
    public class PlayerFootsteps : MonoBehaviour
    {
        private const float SPEED_THRESHOLD = 0.1f;
        private const float BASE_STEP_INTERVAL = 0.5f;
        private const float SPEED_DIVISOR = 4f;

        [SerializeField] private CharacterController _controller;
        [SerializeField] private AudioClip[] _footstepSounds;

        private AudioSource _audioSource;
        private float _stepTimer;
        private bool _wasMoving;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
        }

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
            if (_footstepSounds == null || _footstepSounds.Length == 0 || _audioSource == null) return;
            
            var clip = _footstepSounds[Random.Range(0, _footstepSounds.Length)];
            
            if (clip != null)
                _audioSource.PlayOneShot(clip);
        }
    }
}