using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Runtime.Scripts
{
    [RequireComponent(typeof(Collider))]
    public class DrawablePaper : MonoBehaviour
    {
        private const float MIN_DRAW_DISTANCE = 0.01f;
        private const float SURFACE_OFFSET = 0.001f;
        private const float MAX_RAYCAST_DISTANCE = 10f;

        [Header("Drawing Settings")]
        [SerializeField] private LineRenderer _linePrefab;
        [SerializeField] private LayerMask _drawableLayer;

        private Collider _collider;
        private Camera _mainCamera;
        private LineRenderer _currentLineRenderer;
        private Vector3 _lastLocalPoint;
        private readonly List<LineRenderer> _strokes = new List<LineRenderer>();

        public bool CanDraw { get; set; }

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (!CanDraw) return;
            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (GetRaycastPoint(out var localPoint))
                    StartStroke(localPoint);
            }
            else if (Mouse.current.leftButton.isPressed)
            {
                if (_currentLineRenderer != null && GetRaycastPoint(out var localPoint))
                    UpdateStroke(localPoint);
                else
                    EndStroke();
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                EndStroke();
            }
        }

        public void ClearDrawing()
        {
            foreach (var stroke in _strokes)
            {
                if (stroke != null) Destroy(stroke.gameObject);
            }
            
            _strokes.Clear();
        }

        private bool GetRaycastPoint(out Vector3 localPoint)
        {
            localPoint = Vector3.zero;
            var ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            
            if (!Physics.Raycast(ray, out var hit, MAX_RAYCAST_DISTANCE, _drawableLayer)) return false;
            if (hit.collider != _collider) return false;

            var worldPoint = hit.point + (hit.normal * SURFACE_OFFSET);
            localPoint = transform.InverseTransformPoint(worldPoint);
            
            return true;
        }

        private void StartStroke(Vector3 localPoint)
        {
            var lineObject = Instantiate(_linePrefab, transform);
            
            _currentLineRenderer = lineObject.GetComponent<LineRenderer>();
            _currentLineRenderer.useWorldSpace = false;
            _currentLineRenderer.positionCount = 1;
            _currentLineRenderer.SetPosition(0, localPoint);
            
            _strokes.Add(_currentLineRenderer);
            _lastLocalPoint = localPoint;
        }

        private void UpdateStroke(Vector3 targetLocalPoint)
        {
            if (Vector3.Distance(_lastLocalPoint, targetLocalPoint) <= MIN_DRAW_DISTANCE) return;

            _currentLineRenderer.positionCount++;
            _currentLineRenderer.SetPosition(_currentLineRenderer.positionCount - 1, targetLocalPoint);
            _lastLocalPoint = targetLocalPoint;
        }

        private void EndStroke()
        {
            _currentLineRenderer = null;
        }
    }
}