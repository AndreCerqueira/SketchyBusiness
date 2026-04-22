using DG.Tweening;
using FMOD.Studio;
using Project.Runtime.Scripts.Data;
using Project.Runtime.Scripts.Interaction.Interactables.Base;
using UnityEngine;

namespace Project.Runtime.Scripts.Interaction.Interactables
{
    public class DrawerInteractable : BaseInteractable
    {
        private const string STATE_PARAMETER = "OpenableState";
        private const string STATE_OPEN = "Open";
        private const string STATE_CLOSE = "Close";
        
        [SerializeField] private Vector3 _openOffset;
        [SerializeField] private float _animationDuration = 1f;
        [SerializeField] private bool _startOpen;

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

        protected override void ConfigureInteractionSound(EventInstance instance)
        {
            var stateLabel = _isOpen ? STATE_OPEN : STATE_CLOSE;
            instance.setParameterByNameWithLabel(STATE_PARAMETER, stateLabel);
        }

        protected override void ExecuteInteraction(PlayerInteractionController interactor)
        {
            _isOpen = !_isOpen;
            var targetPosition = _isOpen ? _openPosition : _closedPosition;
            
            transform.DOKill();
            transform.DOLocalMove(targetPosition, _animationDuration);
            
            if (interactor != null) interactor.ForceUpdateAction();
        }
    }
}