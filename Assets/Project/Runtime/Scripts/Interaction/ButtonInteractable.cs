using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Project.Runtime.Scripts.Interaction
{
    public class ButtonInteractable : MonoBehaviour, IInteractable
    {
        private const float ANIMATION_DIVISOR = 2f;

        [Header("Button Settings")]
        [SerializeField] private float _pushDistance = 0.05f;
        [SerializeField] private float _animationDuration = 0.2f;

        [Header("Events")]
        [SerializeField] private UnityEvent _onButtonPressed;

        public event Action OnPressed;

        private Vector3 _originalPosition;
        private bool _isPressed;

        private void Awake()
        {
            _originalPosition = transform.localPosition;
        }

        public void Interact()
        {
            if (_isPressed) return;

            _isPressed = true;

            transform.DOKill();

            var targetPosition = _originalPosition + Vector3.back * _pushDistance;
            var sequence = DOTween.Sequence();
            
            sequence.Append(transform.DOLocalMove(targetPosition, _animationDuration / ANIMATION_DIVISOR));
            sequence.AppendCallback(TriggerEvents);
            sequence.Append(transform.DOLocalMove(_originalPosition, _animationDuration / ANIMATION_DIVISOR));
            sequence.OnComplete(ResetButton);
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