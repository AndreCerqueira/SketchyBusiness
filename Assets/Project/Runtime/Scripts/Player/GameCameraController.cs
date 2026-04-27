using Unity.Cinemachine;
using UnityEngine;

namespace Project.Runtime.Scripts.Player
{
    public class GameCameraController : MonoBehaviour
    {
        [Header("Cameras")]
        [SerializeField] private CinemachineCamera _mainVirtualCamera;
        
        [Header("Targets")]
        [SerializeField] private Transform _mainMenuTarget;
        [SerializeField] private Transform _stadiumTarget;
        [SerializeField] private Transform _drawingBoardTarget;

        private CinemachineHardLockToTarget _hardLock;
        private CinemachineRotateWithFollowTarget _rotateHardLock;

        private void Awake()
        {
            _hardLock = _mainVirtualCamera.GetComponent<CinemachineHardLockToTarget>();
            _rotateHardLock = _mainVirtualCamera.GetComponent<CinemachineRotateWithFollowTarget>();
        }

        public void SetDamping(float damping)
        {
            if (_hardLock == null) return;
            _hardLock.Damping = damping;
            if (_rotateHardLock != null) _rotateHardLock.Damping = damping;
        }

        public void SwitchToMainMenu()
        {
            if (_mainVirtualCamera == null || _mainMenuTarget == null) return;

            _mainVirtualCamera.Follow = _mainMenuTarget;
            _mainVirtualCamera.LookAt = _mainMenuTarget;
        }

        public void SwitchToStadium()
        {
            if (_mainVirtualCamera == null || _stadiumTarget == null) return;

            _mainVirtualCamera.Follow = _stadiumTarget;
            _mainVirtualCamera.LookAt = _stadiumTarget;
        }

        public void SwitchToDrawingBoard()
        {
            if (_mainVirtualCamera == null || _drawingBoardTarget == null) return;

            _mainVirtualCamera.Follow = _drawingBoardTarget;
            _mainVirtualCamera.LookAt = _drawingBoardTarget;
        }
    }
}