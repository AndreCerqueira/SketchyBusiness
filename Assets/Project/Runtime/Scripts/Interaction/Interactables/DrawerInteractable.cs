using DG.Tweening;
using Project.Runtime.Scripts.Data;
using Project.Runtime.Scripts.Interaction.Interactables.Base;
using UnityEngine;

namespace Project.Runtime.Scripts.Interaction.Interactables
{
    public class DrawerInteractable : BaseInteractable
    {
        [Header("Drawer Settings")]
        [SerializeField] private Vector3 _openOffset;
        [SerializeField] private float _animationDuration = 1f;
        [SerializeField] private bool _startOpen;

        [Header("Audio Settings")]
        [SerializeField] private AudioClip _openSound;
        [SerializeField] private AudioClip _closeSound;

        private Vector3 _closedPosition;
        private Vector3 _openPosition;
        private bool _isOpen;

        public override InteractionAction Action => _isOpen ? InteractionAction.Close : InteractionAction.Open;

        protected override void Awake()
        {
            base.Awake();
            
            _closedPosition = transform.localPosition;
            _openPosition = _closedPosition + _openOffset;
            _isOpen = _startOpen;

            if (_isOpen)
                transform.localPosition = _openPosition;
        }

        protected override void ExecuteInteraction(PlayerInteractionController interactor)
        {
            _isOpen = !_isOpen;
            var targetPosition = _isOpen ? _openPosition : _closedPosition;
            
            transform.DOKill();
            transform.DOLocalMove(targetPosition, _animationDuration);
            
            if (interactor != null) interactor.ForceUpdateAction();
        }

        protected override AudioClip GetInteractionSound()
        {
            return _isOpen ? _openSound : _closeSound;
        }
    }
}