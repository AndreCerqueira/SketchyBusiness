using DG.Tweening;
using Project.Runtime.Scripts.Systems;
using TMPro;
using UnityEngine;

namespace Project.Runtime.Scripts
{
    [RequireComponent(typeof(TextMeshPro))]
    public class DrawingTopicDisplay : MonoBehaviour
    {
        private const float FADE_DURATION = 0.5f;

        [Header("References")]
        [SerializeField] private DrawingSystem _drawingSystem;

        private TextMeshPro _textMesh;

        private void Awake()
        {
            _textMesh = GetComponent<TextMeshPro>();
            _textMesh.text = string.Empty;
        }

        private void OnEnable()
        {
            if (_drawingSystem != null)
                _drawingSystem.OnTopicGenerated += HandleTopicGenerated;
        }

        private void OnDisable()
        {
            if (_drawingSystem != null)
                _drawingSystem.OnTopicGenerated -= HandleTopicGenerated;
        }

        private void HandleTopicGenerated(string category, string word)
        {
            _textMesh.DOKill();
            _textMesh.text = word;
            
            var targetColor = _textMesh.color;
            _textMesh.color = new Color(targetColor.r, targetColor.g, targetColor.b, 0f);
            
            _textMesh.DOFade(1f, FADE_DURATION);
        }
    }
}