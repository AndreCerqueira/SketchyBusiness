using UnityEngine;
using DG.Tweening;
using Unity.Cinemachine;

namespace Project.Runtime.Scripts.Animations
{
    public class CameraFocusSwitchSystem : MonoBehaviour
    {
        private const float INITIAL_DELAY = 2f;
        private const float SWITCH_INTERVAL = 3f;

        [Header("References")]
        [SerializeField] private CinemachineCamera _vcam;
        [SerializeField] private Transform[] _focusTargets;

        private Transform _originalTarget;
        private float _originalPositionDamping;
        private float _originalRotationDamping;
        private Sequence _switchSequence;
        private CinemachineHardLockToTarget _positionControl;
        private CinemachineRotateWithFollowTarget _rotationControl;

        private void Awake()
        {
            if (_vcam == null) return;

            _positionControl = _vcam.GetComponent<CinemachineHardLockToTarget>();
            _rotationControl = _vcam.GetComponent<CinemachineRotateWithFollowTarget>();
        }

        public void StartRandomFocusSwitches()
        {
            if (_vcam == null || _focusTargets == null || _focusTargets.Length == 0) return;

            _originalTarget = _vcam.LookAt;

            if (_positionControl != null)
            {
                _originalPositionDamping = _positionControl.Damping;
                _positionControl.Damping = 0f;
            }

            if (_rotationControl != null)
            {
                _originalRotationDamping = _rotationControl.Damping;
                _rotationControl.Damping = 0f;
            }

            _switchSequence = DOTween.Sequence();
            _switchSequence.AppendInterval(INITIAL_DELAY);
            _switchSequence.AppendCallback(SwitchToRandomTarget);
            _switchSequence.AppendInterval(SWITCH_INTERVAL);
            _switchSequence.SetLoops(-1);
        }

        public void StopFocusSwitches()
        {
            if (_switchSequence != null)
                _switchSequence.Kill();

            _vcam.Follow = _originalTarget;
            _vcam.LookAt = _originalTarget;

            if (_positionControl != null)
                _positionControl.Damping = _originalPositionDamping;

            if (_rotationControl != null)
                _rotationControl.Damping = _originalRotationDamping;
        }

        private void SwitchToRandomTarget()
        {
            var randomTarget = _focusTargets[Random.Range(0, _focusTargets.Length)];

            if (randomTarget != null)
            {
                _vcam.Follow = randomTarget;
                _vcam.LookAt = randomTarget;
            }
        }
    }
}