using UnityEngine;

namespace Project.Runtime.Scripts.Animations
{
    public class SunShadowSway : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _swaySpeed = 0.5f;
        [SerializeField] private Vector2 _swayAmount = new Vector2(1.5f, 0.5f);

        private Quaternion _baseRotation;
        private float _seed;

        private void Start()
        {
            _baseRotation = transform.localRotation;
            _seed = Random.Range(0f, 100f);
        }

        private void Update()
        {
            var time = Time.time * _swaySpeed + _seed;

            var xNoise = (Mathf.PerlinNoise(time, 0f) - 0.5f) * 2f * _swayAmount.x;
            var yNoise = (Mathf.PerlinNoise(0f, time) - 0.5f) * 2f * _swayAmount.y;

            var swayRotation = Quaternion.Euler(xNoise, yNoise, 0f);
            
            transform.localRotation = _baseRotation * swayRotation;
        }
    }
}