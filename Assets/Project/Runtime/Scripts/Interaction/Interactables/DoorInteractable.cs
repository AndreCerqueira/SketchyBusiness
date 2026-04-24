using DG.Tweening;
using Project.Runtime.Scripts.Data;
using Project.Runtime.Scripts.Interaction.Interactables.Base;
using UnityEngine;

namespace Project.Runtime.Scripts.Interaction.Interactables
{
    public class DoorInteractable : BaseInteractable
    {
        private const float OPEN_ANGLE = 90f;
        
        [Header("Open Settings")]
        [SerializeField] private bool _invertRotation;
        [SerializeField] private bool _startOpen;
        [SerializeField] private bool _adjustForRotatedParent;
        
        [Header("Animation Settings")]
        [SerializeField] private float _animationDuration = 1f;

        [Header("Audio Settings")]
        [SerializeField] private AudioClip _openSound;
        [SerializeField] private AudioClip _closeSound;
        
        private bool _isOpen;
        
        public override InteractionAction Action => _isOpen ? InteractionAction.Close : InteractionAction.Open;
        
        protected override void Awake()
        {
            base.Awake();
            
            _isOpen = _startOpen;

            if (!_isOpen) return;

            var initialAngle = _invertRotation ? -OPEN_ANGLE : OPEN_ANGLE; 
            
            transform.localEulerAngles = _adjustForRotatedParent 
                ? new Vector3(0f, 0f, initialAngle) 
                : new Vector3(0f, initialAngle, 0f);
        }

        protected override void ExecuteInteraction(PlayerInteractionController interactor)
        {
            _isOpen = !_isOpen;
            var targetAngle = _isOpen ? OPEN_ANGLE : 0f;
            
            if (_invertRotation && _isOpen)
                targetAngle = -targetAngle;
            
            var targetRotation = _adjustForRotatedParent 
                ? new Vector3(0f, 0f, targetAngle) 
                : new Vector3(0f, targetAngle, 0f);

            transform.DOKill();
            transform.DOLocalRotate(targetRotation, _animationDuration);
            
            if (interactor != null) 
                interactor.ForceUpdateAction();
        }

        protected override AudioClip GetInteractionSound()
        {
            return _isOpen ? _openSound : _closeSound;
        }
    }
}