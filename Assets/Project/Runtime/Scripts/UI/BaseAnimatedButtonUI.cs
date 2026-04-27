using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Project.Runtime.Scripts.UI
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class BaseAnimatedButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private const float HOVER_SCALE = 1.05f;
        private const float HOVER_DURATION = 0.2f;
        private const float PUNCH_STRENGTH = 0.1f;
        private const float PUNCH_DURATION = 0.3f;
        private const int PUNCH_VIBRATO = 2;

        protected RectTransform _rectTransform;
        private Vector3 _originalScale;
        private bool _canInteract = true;

        protected virtual void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _originalScale = _rectTransform.localScale;
        }

        protected virtual void OnDisable()
        {
            _rectTransform.DOKill();
            _rectTransform.localScale = _originalScale;
            _canInteract = true;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_canInteract) return;

            _rectTransform.DOKill();
            _rectTransform.DOScale(_originalScale * HOVER_SCALE, HOVER_DURATION);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_canInteract) return;

            _rectTransform.DOKill();
            _rectTransform.DOScale(_originalScale, HOVER_DURATION);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_canInteract) return;

            _canInteract = false;
            
            _rectTransform.DOKill();
            _rectTransform.localScale = _originalScale;
            _rectTransform.DOPunchScale(Vector3.one * PUNCH_STRENGTH, PUNCH_DURATION, PUNCH_VIBRATO).OnComplete(HandleButtonClick);
        }

        protected abstract void HandleButtonClick();

        protected void ResetInteraction()
        {
            _canInteract = true;
        }
    }
}