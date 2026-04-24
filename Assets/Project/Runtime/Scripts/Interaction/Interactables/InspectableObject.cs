using System;
using DG.Tweening;
using Project.Runtime.Scripts.Data;
using Project.Runtime.Scripts.Interaction.Interactables.Base;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.Runtime.Scripts.Interaction.Interactables
{
    public enum InspectMode
    {
        BringObjectToCamera,
        MoveCameraToObject
    }

    public class InspectableObject : BaseInteractable
    {
        private const float INSPECT_DURATION = 0.5f;
        private const float RETURN_DURATION = 0.5f;
        
        public event Action OnInspectionStarted;
        public event Action OnInspectionStopped;
        
        [ShowIf(nameof(_inspectMode), InspectMode.BringObjectToCamera)]
        [SerializeField] private AudioClip _dropSound;

        [SerializeField] private InspectMode _inspectMode = InspectMode.BringObjectToCamera;

        [ShowIf(nameof(_inspectMode), InspectMode.BringObjectToCamera)]
        [Header("Bring Object Settings")]
        [ShowIf(nameof(_inspectMode), InspectMode.BringObjectToCamera)]
        [SerializeField] private Vector3 _inspectOffset = Vector3.zero;
        [ShowIf(nameof(_inspectMode), InspectMode.BringObjectToCamera)]
        [SerializeField] private Vector3 _inspectRotation = Vector3.zero;

        [ShowIf(nameof(_inspectMode), InspectMode.MoveCameraToObject)]
        [Header("Move Camera Settings")]
        [ShowIf(nameof(_inspectMode), InspectMode.MoveCameraToObject)]
        [SerializeField] private Vector3 _cameraOffset = new Vector3(0f, 0f, -0.5f);
        [ShowIf(nameof(_inspectMode), InspectMode.MoveCameraToObject)]
        [SerializeField] private Vector3 _cameraRotationOffset = Vector3.zero;

        private Vector3 _originalPosition;
        private Quaternion _originalRotation;
        private Transform _originalParent;
        
        private Transform _originalCameraParent;
        private Vector3 _originalCameraLocalPos;
        private Quaternion _originalCameraLocalRot;

        private bool _isInspecting;
        private Rigidbody _rigidbody;
        private Collider _collider;
        private PlayerInteractionController _currentInteractor;

        public override InteractionAction Action => _isInspecting ? InteractionAction.StopInspect : InteractionAction.Inspect;

        protected override void Awake()
        {
            base.Awake();
            
            _originalParent = transform.parent;
            _originalPosition = transform.position;
            _originalRotation = transform.rotation;
            
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
        }

        protected override void ExecuteInteraction(PlayerInteractionController interactor)
        {
            if (_isInspecting)
                StopInspection(interactor);
            else
                StartInspection(interactor);
        }

        private void StartInspection(PlayerInteractionController interactor)
        {
            _isInspecting = true;
            _currentInteractor = interactor;
            _currentInteractor.SetInspectedObject(this);

            Unfocus();
            
            if (_inspectMode == InspectMode.BringObjectToCamera)
            {
                if (_rigidbody)
                    _rigidbody.isKinematic = true;

                if (_collider)
                    _collider.enabled = false;

                var targetTransform = interactor.InspectTransform;
                
                transform.SetParent(targetTransform);
                transform.DOKill();
                transform.DOLocalMove(_inspectOffset, INSPECT_DURATION);
                transform.DOLocalRotate(_inspectRotation, INSPECT_DURATION);
            }
            else
            {
                var cameraTarget = interactor.PlayerController.CinemachineCameraTarget.transform;
                
                _originalCameraParent = cameraTarget.parent;
                _originalCameraLocalPos = cameraTarget.localPosition;
                _originalCameraLocalRot = cameraTarget.localRotation;

                cameraTarget.SetParent(transform);
                cameraTarget.DOKill();
                cameraTarget.DOLocalMove(_cameraOffset, INSPECT_DURATION);
                cameraTarget.DOLocalRotate(_cameraRotationOffset, INSPECT_DURATION);
            }
            
            interactor.ForceUpdateAction();
            OnInspectionStarted?.Invoke();
        }

        private void StopInspection(PlayerInteractionController interactor)
        {
            _isInspecting = false;
            interactor.ClearInspectedObject();
            
            if (_inspectMode == InspectMode.BringObjectToCamera)
            {
                transform.SetParent(_originalParent);
                transform.DOKill();
                
                var sequence = DOTween.Sequence();
                sequence.Join(transform.DOMove(_originalPosition, RETURN_DURATION));
                sequence.Join(transform.DORotateQuaternion(_originalRotation, RETURN_DURATION));
                sequence.OnComplete(OnReturnComplete);
            }
            else
            {
                var cameraTarget = interactor.PlayerController.CinemachineCameraTarget.transform;
                
                cameraTarget.SetParent(_originalCameraParent);
                cameraTarget.DOKill();
                
                var sequence = DOTween.Sequence();
                sequence.Join(cameraTarget.DOLocalMove(_originalCameraLocalPos, RETURN_DURATION));
                sequence.Join(cameraTarget.DOLocalRotateQuaternion(_originalCameraLocalRot, RETURN_DURATION));
                sequence.OnComplete(OnReturnComplete);
            }
            
            OnInspectionStopped?.Invoke();
        }

        private void OnReturnComplete()
        {
            if (_currentInteractor != null)
            {
                _currentInteractor.ClearInspectedObject();
                _currentInteractor = null;
            }

            if (_inspectMode == InspectMode.BringObjectToCamera)
            {
                if (_rigidbody)
                    _rigidbody.isKinematic = false;

                if (_collider)
                    _collider.enabled = true;
                
                if (_dropSound != null)
                    AudioSource.PlayClipAtPoint(_dropSound, transform.position);
            }
        }
    }
}