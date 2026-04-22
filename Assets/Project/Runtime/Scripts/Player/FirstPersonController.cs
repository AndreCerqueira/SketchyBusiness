using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Runtime.Scripts.Player
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class FirstPersonController : MonoBehaviour
    {
        private const float TERMINAL_VELOCITY = 53.0f;
        private const float THRESHOLD = 0.01f;
        private const float SPEED_OFFSET = 0.1f;
        private const float ROUNDING_MULTIPLIER = 1000f;
        private const float GROUNDED_GRAVITY = -2f;
        private const float JUMP_GRAVITY_MULTIPLIER = -2f;
        private const float FULL_CIRCLE = 360f;
        private const float HALF_CIRCLE = 180f;

        [Header("Player")]
        public float MoveSpeed = 4.0f;
        public float SprintSpeed = 6.0f;
        public float RotationSpeed = 1.0f;
        public float SpeedChangeRate = 10.0f;

        [Space(10)]
        public float JumpHeight = 1.2f;
        public float Gravity = -15.0f;

        [Space(10)]
        public float JumpTimeout = 0.1f;
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        public bool Grounded = true;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.5f;
        public LayerMask GroundLayers;
        public LayerMask InteractableLayers;

        [Header("Cinemachine")]
        public GameObject CinemachineCameraTarget;
        public float TopClamp = 90.0f;
        public float BottomClamp = -90.0f;

        private bool _isInputSuspended;

        public bool SuspendInput
        {
            get
            {
                return _isInputSuspended;
            }
            set
            {
                _isInputSuspended = value;

                if (_input == null) return;

                _input.cursorLocked = !value;
                _input.cursorInputForLook = !value;
            }
        }

        private float _cinemachineTargetPitch;
        private float _speed;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }

        private void Awake()
        {
            if (_mainCamera == null)
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
            
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#endif
        }

        private void Start()
        {
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            JumpAndGravity();
            GroundedCheck();
            Move();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            if (_controller == null) return;

            _controller.enabled = false;
            
            transform.position = position;
            transform.rotation = Quaternion.Euler(0f, rotation.eulerAngles.y, 0f);

            _cinemachineTargetPitch = rotation.eulerAngles.x;
            
            if (_cinemachineTargetPitch > HALF_CIRCLE)
                _cinemachineTargetPitch -= FULL_CIRCLE;

            if (CinemachineCameraTarget != null)
                CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0f, 0f);

            _controller.enabled = true;
        }

        private void GroundedCheck()
        {
            var spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            var combinedLayers = GroundLayers | InteractableLayers;
            
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, combinedLayers, QueryTriggerInteraction.Ignore);
        }

        private void CameraRotation()
        {
            if (SuspendInput) return;

            if (_input.look.sqrMagnitude >= THRESHOLD)
            {
                var deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
                
                _cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
                _rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

                _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

                CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);
                transform.Rotate(Vector3.up * _rotationVelocity);
            }
        }

        private void Move()
        {
            var targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            if (_input.move == Vector2.zero || SuspendInput)
                targetSpeed = 0.0f;

            var currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
            var inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            if (currentHorizontalSpeed < targetSpeed - SPEED_OFFSET || currentHorizontalSpeed > targetSpeed + SPEED_OFFSET)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
                _speed = Mathf.Round(_speed * ROUNDING_MULTIPLIER) / ROUNDING_MULTIPLIER;
            }
            else
            {
                _speed = targetSpeed;
            }

            var inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            if (_input.move != Vector2.zero && !SuspendInput)
                inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;

            _controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;

                if (_verticalVelocity < 0.0f)
                    _verticalVelocity = GROUNDED_GRAVITY;

                if (_input.jump && _jumpTimeoutDelta <= 0.0f && !SuspendInput)
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * JUMP_GRAVITY_MULTIPLIER * Gravity);

                if (_jumpTimeoutDelta >= 0.0f)
                    _jumpTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                _jumpTimeoutDelta = JumpTimeout;

                if (_fallTimeoutDelta >= 0.0f)
                    _fallTimeoutDelta -= Time.deltaTime;

                _input.jump = false;
            }

            if (_verticalVelocity < TERMINAL_VELOCITY)
                _verticalVelocity += Gravity * Time.deltaTime;
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -FULL_CIRCLE) lfAngle += FULL_CIRCLE;
            if (lfAngle > FULL_CIRCLE) lfAngle -= FULL_CIRCLE;
            
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            var transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            var transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            Gizmos.color = Grounded ? transparentGreen : transparentRed;
            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
        }
    }
}