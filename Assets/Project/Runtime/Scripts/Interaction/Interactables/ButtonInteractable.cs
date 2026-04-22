using System;
using DG.Tweening;
using Project.Runtime.Scripts.Data;
using Project.Runtime.Scripts.Interaction.Interactables.Base;
using UnityEngine;
using UnityEngine.Events;

namespace Project.Runtime.Scripts.Interaction.Interactables
{
    public class ButtonInteractable : BaseInteractable
    {
        private const float HALF_DIVISOR = 2f;

        [Header("Button Settings")]
        [SerializeField] private float _pushDistance = 0.05f;
        [SerializeField] private float _animationDuration = 0.2f;

        [Header("Events")]
        [SerializeField] private UnityEvent _onButtonPressed;

        public event Action OnPressed;

        private Vector3 _originalPosition;
        private bool _isPressed;

        public override InteractionAction Action => InteractionAction.Press;

        protected override void Awake()
        {
            base.Awake();
            
            _originalPosition = transform.localPosition;
        }

        protected override void ExecuteInteraction(PlayerInteractionController interactor)
        {
            if (_isPressed) return;

            _isPressed = true;

            transform.DOKill();

            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOLocalMoveY(_originalPosition.y - _pushDistance, _animationDuration / HALF_DIVISOR));
            sequence.AppendCallback(TriggerEvents);
            sequence.Append(transform.DOLocalMoveY(_originalPosition.y, _animationDuration / HALF_DIVISOR));
            sequence.OnComplete(ResetButton);

            if (interactor != null)
                interactor.ForceUpdateAction();
        }

        private void TriggerEvents()
        {
            OnPressed?.Invoke();
            _onButtonPressed?.Invoke();
        }

        private void ResetButton()
        {
            _isPressed = false;
        }
    }
}