using UnityEngine;

namespace Project.Runtime.Scripts.Data
{
    [CreateAssetMenu(fileName = "CameraBobbingSettings", menuName = "Project/Player/Camera Bobbing Settings")]
    public class CameraBobbingSettings : ScriptableObject
    {
        [SerializeField] private float _frequency = 10f;
        [SerializeField] private float _amplitude = 0.05f;
        [SerializeField] private float _speedThreshold = 0.1f;
        [SerializeField] private float _resetSpeed = 5f;
        [SerializeField] private float _speedDivisor = 4f;
        [SerializeField] private float _horizontalTimeDivisor = 2f;
        [SerializeField] private float _horizontalAmplitudeDivisor = 2f;

        public float Frequency => _frequency;
        public float Amplitude => _amplitude;
        public float SpeedThreshold => _speedThreshold;
        public float ResetSpeed => _resetSpeed;
        public float SpeedDivisor => _speedDivisor;
        public float HorizontalTimeDivisor => _horizontalTimeDivisor;
        public float HorizontalAmplitudeDivisor => _horizontalAmplitudeDivisor;
    }
}