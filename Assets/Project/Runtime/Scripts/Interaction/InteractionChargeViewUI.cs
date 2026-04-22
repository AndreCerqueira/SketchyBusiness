using UnityEngine;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.Interaction
{
    public class InteractionChargeViewUI : MonoBehaviour
    {
        private const float VISIBLE_ALPHA = 1f;
        private const float INVISIBLE_ALPHA = 0f;

        [SerializeField] private PlayerInteractionController _interactionController;
        [SerializeField] private CanvasGroup _uiGroup;
        [SerializeField] private Image _fillImage;

        private void OnEnable()
        {
            _interactionController.OnThrowChargeStateChanged += HandleChargeStateChanged;
            _interactionController.OnThrowChargeChanged += HandleChargeValueChanged;
        }

        private void OnDisable()
        {
            _interactionController.OnThrowChargeStateChanged -= HandleChargeStateChanged;
            _interactionController.OnThrowChargeChanged -= HandleChargeValueChanged;
        }

        private void Start()
        {
            HandleChargeStateChanged(false);
            HandleChargeValueChanged(0f);
        }

        private void HandleChargeStateChanged(bool isCharging)
        {
            _uiGroup.alpha = isCharging ? VISIBLE_ALPHA : INVISIBLE_ALPHA;
        }

        private void HandleChargeValueChanged(float chargeValue)
        {
            _fillImage.fillAmount = chargeValue;
        }
    }
}