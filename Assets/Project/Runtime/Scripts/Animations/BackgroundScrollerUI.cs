using UnityEngine;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.Animations
{
    public class BackgroundScrollerUI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _speedX = 0.2f;
        [SerializeField] private float _speedY = 0.2f;

        [Header("References")]
        [SerializeField] private RawImage[] _rawImages;
        
        private Rect _uvRect;

        private void Update()
        {
            if (!gameObject.activeInHierarchy)
                return;
            
            foreach (var rawImage in _rawImages)
            {
                _uvRect = rawImage.uvRect;
                _uvRect.x += _speedX * Time.deltaTime;
                _uvRect.y += _speedY * Time.deltaTime;
                rawImage.uvRect = _uvRect;
            }
        }
    }
}