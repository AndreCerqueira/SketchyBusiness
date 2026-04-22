using DG.Tweening;
using FMOD.Studio;
using Project.Runtime.Scripts.Data;
using Project.Runtime.Scripts.Interaction.Interactables.Base;
using UnityEngine;

namespace Project.Runtime.Scripts.Interaction.Interactables
{
    public enum RotationAxis
    {
        X,
        Y,
        Z
    }

    public class LidInteractable : BaseInteractable
    {
        private const float OPEN_ANGLE = 90f;
        private const string STATE_PARAMETER = "OpenableState";
        private const string STATE_OPEN = "Open";
        private const string STATE_CLOSE = "Close";
        
        [Header("Open Settings")]
        [SerializeField] private bool _isInvertedRotation;
        [SerializeField] private bool _isStartOpen;
        [SerializeField] private RotationAxis _rotationAxis;
        
        [Header("Animation Settings")]
        [SerializeField] private float _animationDuration = 1f;

        private bool _isOpen;

        public override InteractionAction Action => _isOpen ? InteractionAction.Close : InteractionAction.Open;

        protected override void Awake()
        {
            base.Awake();
            
            _isOpen = _isStartOpen;

            if (!_isOpen) return;

            var initialAngle = _isInvertedRotation ? -OPEN_ANGLE : OPEN_ANGLE;
            transform.localEulerAngles = GetTargetRotation(initialAngle);
        }

        protected override void ExecuteInteraction(PlayerInteractionController interactor)
        {
            _isOpen = !_isOpen;
            var targetAngle = _isOpen ? OPEN_ANGLE : 0f;
            
            if (_isInvertedRotation && _isOpen)
                targetAngle = -targetAngle;
            
            var targetRotation = GetTargetRotation(targetAngle);

            transform.DOKill();
            transform.DOLocalRotate(targetRotation, _animationDuration);
            
            interactor.ForceUpdateAction();
        }
        
        protected override void ConfigureInteractionSound(EventInstance instance)
        {
            var stateLabel = _isOpen ? STATE_OPEN : STATE_CLOSE;
            instance.setParameterByNameWithLabel(STATE_PARAMETER, stateLabel);
        }

        private Vector3 GetTargetRotation(float angle)
        {
            switch (_rotationAxis)
            {
                case RotationAxis.Y:
                    return new Vector3(0f, angle, 0f);
                case RotationAxis.Z:
                    return new Vector3(0f, 0f, angle);
                case RotationAxis.X:
                default:
                    return new Vector3(angle, 0f, 0f);
            }
        }
    }
}