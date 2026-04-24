using System.Collections;
using DG.Tweening;
using Project.Runtime.Scripts.Data;
using Project.Runtime.Scripts.Interaction.Interactables.Base;
using UnityEngine;

namespace Project.Runtime.Scripts.Interaction.Interactables
{
    public class GrabbableObject : BaseInteractable
    {
        private const string HELD_LAYER_NAME = "HeldObject";
        private const float PICKUP_DURATION = 0.25f;
        private const int INVALID_LAYER = -1;
        private const float LAYER_RESET_DELAY = 0.5f;
        private const float INITIAL_MUTE_DURATION = 2f;
        
        [SerializeField] private AudioClip _dropSound;
        [SerializeField] private bool _usePlacementMode;

        private Rigidbody _rigidbody;
        private Transform _originalParent;
        private int _originalLayer;
        private int _heldLayer;
        private Tween _layerResetTween;
        private bool _isCollisionMuted;
        private Collider[] _colliders;
        private Collider _currentPlayerCollider;
        private Coroutine _collisionRoutine;

        public override InteractionAction Action => InteractionAction.Pick;
        public bool UsePlacementMode => _usePlacementMode;

        protected override void Awake()
        {
            base.Awake();
            _rigidbody = GetComponent<Rigidbody>();
            _colliders = GetComponentsInChildren<Collider>(true);
            _originalParent = transform.parent;
            _originalLayer = gameObject.layer;
            _heldLayer = LayerMask.NameToLayer(HELD_LAYER_NAME);
        }

        private void Start()
        {
            MuteCollisionSoundsTemporarily(INITIAL_MUTE_DURATION);
        }

        protected override void ExecuteInteraction(PlayerInteractionController interactor)
        {
            interactor.SetHeldObject(this);
            _currentPlayerCollider = interactor.PlayerCollider;
            Pickup(interactor.HandTransform);
        }

        private void Pickup(Transform handTransform)
        {
            if (_layerResetTween != null) _layerResetTween.Kill();
            if (_collisionRoutine != null) StopCoroutine(_collisionRoutine);

            IgnorePlayerCollision(true);

            _rigidbody.isKinematic = true;
            transform.SetParent(handTransform);
            
            transform.DOKill();
            transform.DOLocalMove(Vector3.zero, PICKUP_DURATION);
            transform.DOLocalRotate(Vector3.zero, PICKUP_DURATION);
            
            SetLayerRecursively(gameObject, _heldLayer);
        }

        public void Place(Vector3 position, Quaternion rotation)
        {
            if (_layerResetTween != null) _layerResetTween.Kill();
            if (_collisionRoutine != null) StopCoroutine(_collisionRoutine);

            transform.DOKill();
            transform.SetParent(_originalParent);
            
            transform.position = position;
            transform.rotation = rotation;
            
            _rigidbody.isKinematic = false;
            
            SetLayerRecursively(gameObject, _originalLayer);
            IgnorePlayerCollision(false);
            _currentPlayerCollider = null;
        }

        public void Drop()
        {
            if (_layerResetTween != null) _layerResetTween.Kill();
            if (_collisionRoutine != null) StopCoroutine(_collisionRoutine);

            transform.DOKill();
            transform.SetParent(_originalParent);
            
            var currentRotation = transform.eulerAngles;
            transform.eulerAngles = new Vector3(0f, currentRotation.y, currentRotation.z);
            
            _rigidbody.isKinematic = false;
            
            _layerResetTween = DOVirtual.DelayedCall(LAYER_RESET_DELAY, () => SetLayerRecursively(gameObject, _originalLayer)).SetLink(gameObject);
            _collisionRoutine = StartCoroutine(RestoreCollisionRoutine());
        }

        public void Drop(Vector3 throwForce)
        {
            if (_layerResetTween != null) _layerResetTween.Kill();
            if (_collisionRoutine != null) StopCoroutine(_collisionRoutine);

            transform.DOKill();
            transform.SetParent(_originalParent);
            
            var currentRotation = transform.eulerAngles;
            transform.eulerAngles = new Vector3(0f, currentRotation.y, currentRotation.z);
            
            _rigidbody.isKinematic = false;
            _rigidbody.AddForce(throwForce, ForceMode.Impulse);
            
            _layerResetTween = DOVirtual.DelayedCall(LAYER_RESET_DELAY, () => SetLayerRecursively(gameObject, _originalLayer)).SetLink(gameObject);
            _collisionRoutine = StartCoroutine(RestoreCollisionRoutine());
        }
        
        public void ApplyPullback(float distance)
        {
            transform.DOKill();
            transform.localPosition = new Vector3(0f, 0f, -distance);
        }

        public void MuteCollisionSoundsTemporarily(float duration)
        {
            _isCollisionMuted = true;
            DOVirtual.DelayedCall(duration, () => _isCollisionMuted = false).SetLink(gameObject);
        }

        private IEnumerator RestoreCollisionRoutine()
        {
            if (_currentPlayerCollider == null) yield break;

            var isIntersecting = true;

            while (isIntersecting)
            {
                isIntersecting = false;

                foreach (var col in _colliders)
                {
                    if (col != null && col.gameObject.activeInHierarchy && _currentPlayerCollider.bounds.Intersects(col.bounds))
                    {
                        isIntersecting = true;
                        break;
                    }
                }

                yield return null;
            }

            IgnorePlayerCollision(false);
            _currentPlayerCollider = null;
        }

        private void IgnorePlayerCollision(bool isIgnored)
        {
            if (_currentPlayerCollider == null) return;

            foreach (var col in _colliders)
            {
                if (col != null)
                    Physics.IgnoreCollision(col, _currentPlayerCollider, isIgnored);
            }
        }

        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (newLayer <= INVALID_LAYER) return;
            
            obj.layer = newLayer;
            
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
        
        private void OnCollisionEnter(Collision _)
        {
            if (!_isCollisionMuted) PlayDropSound();
        }
        
        private void PlayDropSound()
        {
            if (_dropSound != null)
                AudioSource.PlayClipAtPoint(_dropSound, transform.position);
        }
    }
}