using Project.Runtime.Scripts.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Runtime.Scripts.Systems
{
    public class MouseInteractionSystem : MonoBehaviour
    {
        private const float MAX_RAYCAST_DISTANCE = 10f;

        [Header("Interaction Settings")]
        [SerializeField] private LayerMask _interactableLayer;

        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
                HandleMouseClick();
        }

        private void HandleMouseClick()
        {
            var ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!Physics.Raycast(ray, out var hit, MAX_RAYCAST_DISTANCE, _interactableLayer)) return;

            var interactable = hit.collider.GetComponent<IInteractable>();
            
            if (interactable == null) return;

            interactable.Interact();
        }
    }
}