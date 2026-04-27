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