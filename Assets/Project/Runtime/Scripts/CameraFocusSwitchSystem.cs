using System.Collections;
using UnityEngine;
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

        private Transform _originalFollow;
        private Transform _originalLookAt;
        private float _originalPositionDamping;
        private float _originalRotationDamping;
        private Coroutine _switchCoroutine;
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

            _originalFollow = _vcam.Follow;
            _originalLookAt = _vcam.LookAt;

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

            if (_switchCoroutine != null) StopCoroutine(_switchCoroutine);

            _switchCoroutine = StartCoroutine(FocusSwitchRoutine());
        }

        public void StopFocusSwitches()
        {
            if (_switchCoroutine != null)
            {
                StopCoroutine(_switchCoroutine);
                _switchCoroutine = null;
            }

            _vcam.Follow = _originalFollow;
            _vcam.LookAt = _originalLookAt;

            if (_positionControl != null)
                _positionControl.Damping = _originalPositionDamping;

            if (_rotationControl != null)
                _rotationControl.Damping = _originalRotationDamping;
        }

        private IEnumerator FocusSwitchRoutine()
        {
            yield return new WaitForSeconds(INITIAL_DELAY);

            while (true)
            {
                var randomTarget = _focusTargets[Random.Range(0, _focusTargets.Length)];

                if (randomTarget != null)
                {
                    _vcam.Follow = randomTarget;
                    _vcam.LookAt = null;
                }

                yield return new WaitForSeconds(SWITCH_INTERVAL);
            }
        }
    }
}