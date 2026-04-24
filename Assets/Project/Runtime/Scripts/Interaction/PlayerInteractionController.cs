using System;
using Project.Runtime.Scripts.Data;
using Project.Runtime.Scripts.Interaction.Interactables;
using Project.Runtime.Scripts.Interaction.Interactables.Base;
using Project.Runtime.Scripts.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Runtime.Scripts.Interaction
{
    public class PlayerInteractionController : MonoBehaviour
    {
        private const float MAX_INTERACTION_DISTANCE = 4.5f;
        private const float SCREEN_CENTER_DIVISOR = 2f;
        
        public event Action<InteractionAction> OnInteractionStateChanged;
        public event Action<float> OnThrowChargeChanged;
        public event Action<bool> OnThrowChargeStateChanged;
        public event Action<bool> OnInspectStateChanged;
        
        [Header("Audio")]
        [SerializeField] private AudioClip _chargeThrowSound;

        [Header("References")]
        [SerializeField] private FirstPersonController _playerController;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private LayerMask _interactableLayer;
        [SerializeField] private InputActionReference _interactActionInput;
        [SerializeField] private Transform _handTransform;
        [SerializeField] private Transform _inspectTransform;
        
        [Header("Throw Settings")]
        [SerializeField] private float _maxChargeTime = 2f;
        [SerializeField] private float _maxThrowForce = 15f;
        [SerializeField] private float _verticalThrowMultiplier = 0.35f;
        [SerializeField] private float _maxPullbackDistance = 0.5f;

        public InputActionReference InteractActionInput => _interactActionInput;
        public Transform HandTransform => _handTransform;
        public Transform InspectTransform => _inspectTransform;
        public FirstPersonController PlayerController => _playerController;
        public Collider PlayerCollider { get; private set; }
        public bool IsInspecting => _inspectedObject != null;
        public GrabbableObject HeldObject => _heldObject;
        
        private IInteractable _currentTarget;
        private GrabbableObject _heldObject;
        private InspectableObject _inspectedObject;
        private InteractionAction _currentAction;
        
        private bool _isChargingThrow;
        private float _currentChargeTime;
        
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();

            if (_playerController != null)
                PlayerCollider = _playerController.GetComponent<Collider>();
        }
        
        private void OnEnable()
        {
            if (_interactActionInput == null) return;
            
            _interactActionInput.action.Enable();
        }

        private void OnDisable()
        {
            if (_interactActionInput == null) return;
            
            _interactActionInput.action.Disable();
        }

        private void Update()
        {
            ScanForObjects();
            ProcessInput();
        }

        public void SetInteractionEnabled(bool isEnabled)
        {
            this.enabled = isEnabled;
            
            if (!isEnabled)
            {
                ClearTarget();
                UpdateActionState(InteractionAction.None);
            }
        }

        private void ScanForObjects()
        {
            if (_inspectedObject != null)
            {
                ClearTarget();
                UpdateActionState(InteractionAction.Drop);
                return;
            }

            var screenCenter = new Vector3(Screen.width / SCREEN_CENTER_DIVISOR, Screen.height / SCREEN_CENTER_DIVISOR, 0f);
            var ray = _mainCamera.ScreenPointToRay(screenCenter);

            if (Physics.Raycast(ray, out var hit, MAX_INTERACTION_DISTANCE, _interactableLayer))
            {
                var interactable = hit.collider.GetComponent<IInteractable>();
                
                if (interactable == null) 
                    interactable = hit.collider.GetComponentInParent<IInteractable>();

                if (interactable != null)
                {
                    if (_heldObject != null && interactable.Action == InteractionAction.Pick)
                    {
                        ClearTarget();
                        UpdateActionState(InteractionAction.Drop);
                        return;
                    }

                    if (_currentTarget != interactable)
                    {
                        ClearTarget();
                        _currentTarget = interactable;
                        _currentTarget.Focus();
                    }
                        
                    UpdateActionState(_currentTarget.Action);
                    return;
                }
            }

            ClearTarget();
            UpdateActionState(_heldObject != null ? InteractionAction.Drop : InteractionAction.None);
        }

        private void ClearTarget()
        {
            if (_currentTarget == null) return;
            
            _currentTarget.Unfocus();
            _currentTarget = null;
        }
        
        private void UpdateActionState(InteractionAction newAction)
        {
            if (_currentAction == newAction) return;
            
            _currentAction = newAction;
            OnInteractionStateChanged?.Invoke(_currentAction);
        }

        public void ForceUpdateAction()
        {
            if (_currentTarget == null) return;
            
            _currentAction = _currentTarget.Action;
            OnInteractionStateChanged?.Invoke(_currentAction);
        }

        public void SetHeldObject(GrabbableObject grabbable)
        {
            if (grabbable == null) return;

            _heldObject = grabbable;
            ClearTarget();
        }

        public void SetInspectedObject(InspectableObject inspectable)
        {
            if (inspectable == null) return;

            _inspectedObject = inspectable;
            ClearTarget();
            _playerController.SuspendInput = true;
            
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            
            OnInspectStateChanged?.Invoke(true);
        }

        public void ClearInspectedObject()
        {
            _inspectedObject = null;
            _playerController.SuspendInput = false;
            
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            
            OnInspectStateChanged?.Invoke(false);
        }

        private void ProcessInput()
        {
            if (_inspectedObject != null)
            {
                if (_interactActionInput.action.WasPressedThisFrame())
                    _inspectedObject.Interact(this);
                
                return;
            }

            if (_heldObject != null)
            {
                ProcessThrowInput();
                return;
            }

            if (!_interactActionInput.action.WasPressedThisFrame()) return;

            if (_currentTarget != null)
                _currentTarget.Interact(this);
        }

        private void ProcessThrowInput()
        {
            if (_interactActionInput.action.WasPressedThisFrame())
            {
                if (_currentTarget != null && !_isChargingThrow)
                {
                    _currentTarget.Interact(this);
                    return;
                }

                _isChargingThrow = true;
                _currentChargeTime = 0f;
                
                if (_chargeThrowSound != null && _audioSource != null)
                {
                    _audioSource.clip = _chargeThrowSound;
                    _audioSource.loop = true;
                    _audioSource.Play();
                }
                
                OnThrowChargeStateChanged?.Invoke(true);
            }

            if (_isChargingThrow && _interactActionInput.action.IsPressed())
            {
                _currentChargeTime += Time.deltaTime;
                _currentChargeTime = Mathf.Clamp(_currentChargeTime, 0f, _maxChargeTime);
                
                var chargePercentage = _currentChargeTime / _maxChargeTime;
                OnThrowChargeChanged?.Invoke(chargePercentage);
                
                var pullbackDistance = _maxPullbackDistance * chargePercentage;
                _heldObject.ApplyPullback(pullbackDistance);
            }

            if (_isChargingThrow && _interactActionInput.action.WasReleasedThisFrame())
            {
                var chargePercentage = _currentChargeTime / _maxChargeTime;
                var throwDirection = (_mainCamera.transform.forward + Vector3.up * _verticalThrowMultiplier).normalized;
                var throwForce = throwDirection * (_maxThrowForce * chargePercentage);
                
                _heldObject.Drop(throwForce);
                _heldObject = null;
                _isChargingThrow = false;
                
                StopChargeSound();
                
                OnThrowChargeStateChanged?.Invoke(false);
                OnThrowChargeChanged?.Invoke(0f);
            }
        }
        
        private void StopChargeSound()
        {
            if (_audioSource != null && _audioSource.isPlaying)
            {
                _audioSource.Stop();
            }
        }

        private void OnDestroy()
        {
            StopChargeSound();
        }
    }
}