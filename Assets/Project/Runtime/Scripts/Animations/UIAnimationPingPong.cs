using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.Runtime.Scripts.Animations
{
    public enum Axis
    {
        X,
        Y,
        Z,
        All
    }

    public class UIAnimationPingPong : MonoBehaviour
    {
        private const float IDLE_INTENSITY = 0.15f;
        private const float ACTIVE_INTENSITY = 1f;
        private const float TRANSITION_SPEED = 5f;
        private const float ZERO_FLOAT = 0f;

        [FoldoutGroup("Movement")]
        [SerializeField] private bool _enableMovement;
        
        [FoldoutGroup("Movement")]
        [ShowIf(nameof(_enableMovement))]
        [SerializeField] private float _movementSpeed = 2.0f;
        
        [FoldoutGroup("Movement")]
        [ShowIf(nameof(_enableMovement))]
        [SerializeField] private float _movementDistance = 50f;
        
        [FoldoutGroup("Movement")]
        [ShowIf(nameof(_enableMovement))]
        [SerializeField] private Axis _movementAxis = Axis.Y;

        [FoldoutGroup("Rotation")]
        [SerializeField] private bool _enableRotation;
        
        [FoldoutGroup("Rotation")]
        [ShowIf(nameof(_enableRotation))]
        [SerializeField] private float _rotationSpeed = 2.0f;
        
        [FoldoutGroup("Rotation")]
        [ShowIf(nameof(_enableRotation))]
        [SerializeField] private float _rotationAngle = 15f;
        
        [FoldoutGroup("Rotation")]
        [ShowIf(nameof(_enableRotation))]
        [SerializeField] private Axis _rotationAxis = Axis.Z;

        [FoldoutGroup("Scale")]
        [SerializeField] private bool _enableScale;
        
        [FoldoutGroup("Scale")]
        [ShowIf(nameof(_enableScale))]
        [SerializeField] private float _scaleSpeed = 2.0f;
        
        [FoldoutGroup("Scale")]
        [ShowIf(nameof(_enableScale))]
        [SerializeField] private float _scaleAmplitude = 0.2f;
        
        [FoldoutGroup("Scale")]
        [ShowIf(nameof(_enableScale))]
        [SerializeField] private bool _squashAndStretch;
        
        [FoldoutGroup("Scale")]
        [ShowIf(nameof(_showStandardScaleAxis))]
        [SerializeField] private Axis _scaleAxis = Axis.All;
        
        [FoldoutGroup("Scale")]
        [ShowIf(nameof(_squashAndStretch))]
        [SerializeField] private Axis _stretchAxis = Axis.Y;

        [FoldoutGroup("Delay")]
        [SerializeField] private bool _useRandomStartDelay;
        
        [FoldoutGroup("Delay")]
        [ShowIf(nameof(_useRandomStartDelay))]
        [SerializeField] private float _minDelay;
        
        [FoldoutGroup("Delay")]
        [ShowIf(nameof(_useRandomStartDelay))]
        [SerializeField] private float _maxDelay = 2f;

        private Vector3 _startPosition;
        private Vector3 _startRotation;
        private Vector3 _startScale;

        private bool _isPlaying;
        private float _currentIntensity = IDLE_INTENSITY;

        private bool _showStandardScaleAxis => _enableScale && !_squashAndStretch;

        private void Start()
        {
            _startPosition = transform.localPosition;
            _startRotation = transform.localEulerAngles;
            _startScale = transform.localScale;
            
            StartCoroutine(PerformAnimation());
        }

        public void SetPlaying(bool isPlaying)
        {
            _isPlaying = isPlaying;
        }

        private IEnumerator PerformAnimation()
        {
            if (_useRandomStartDelay)
            {
                var delay = Random.Range(_minDelay, _maxDelay);
                yield return new WaitForSeconds(delay);
            }

            var elapsedTime = ZERO_FLOAT;

            while (true)
            {
                var targetIntensity = _isPlaying ? ACTIVE_INTENSITY : IDLE_INTENSITY;
                _currentIntensity = Mathf.Lerp(_currentIntensity, targetIntensity, Time.deltaTime * TRANSITION_SPEED);

                if (_enableMovement)
                {
                    var moveOffset = Mathf.Sin(elapsedTime * _movementSpeed) * _movementDistance * _currentIntensity;
                    var currentPosition = _startPosition;
                    
                    switch (_movementAxis)
                    {
                        case Axis.X: currentPosition.x += moveOffset; break;
                        case Axis.Y: currentPosition.y += moveOffset; break;
                        case Axis.Z: currentPosition.z += moveOffset; break;
                    }
                    
                    transform.localPosition = currentPosition;
                }

                if (_enableRotation)
                {
                    var rotOffset = Mathf.Sin(elapsedTime * _rotationSpeed) * _rotationAngle * _currentIntensity;
                    var currentRotation = _startRotation;
                    
                    switch (_rotationAxis)
                    {
                        case Axis.X: currentRotation.x += rotOffset; break;
                        case Axis.Y: currentRotation.y += rotOffset; break;
                        case Axis.Z: currentRotation.z += rotOffset; break;
                    }
                    
                    transform.localRotation = Quaternion.Euler(currentRotation);
                }

                if (_enableScale)
                {
                    var scaleOffset = Mathf.Sin(elapsedTime * _scaleSpeed) * _scaleAmplitude * _currentIntensity;
                    var currentScale = _startScale;

                    if (_squashAndStretch)
                    {
                        var squashOffset = -scaleOffset;
                        
                        switch (_stretchAxis)
                        {
                            case Axis.X:
                                currentScale.x += scaleOffset;
                                currentScale.y += squashOffset;
                                currentScale.z += squashOffset;
                                break;
                            case Axis.Y:
                                currentScale.x += squashOffset;
                                currentScale.y += scaleOffset;
                                currentScale.z += squashOffset;
                                break;
                            case Axis.Z:
                                currentScale.x += squashOffset;
                                currentScale.y += squashOffset;
                                currentScale.z += scaleOffset;
                                break;
                        }
                    }
                    else
                    {
                        switch (_scaleAxis)
                        {
                            case Axis.X: currentScale.x += scaleOffset; break;
                            case Axis.Y: currentScale.y += scaleOffset; break;
                            case Axis.Z: currentScale.z += scaleOffset; break;
                            case Axis.All:
                                currentScale.x += scaleOffset;
                                currentScale.y += scaleOffset;
                                currentScale.z += scaleOffset;
                                break;
                        }
                    }
                    
                    transform.localScale = currentScale;
                }
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}