using System.Collections.Generic;
using Project.Runtime.Scripts.Data;
using UnityEngine;

namespace Project.Runtime.Scripts.Interaction.Interactables.Base
{
    public abstract class BaseInteractable : MonoBehaviour, IInteractable
    {
        private const float FOCUS_RIM_SIZE = 0.5f;
        private const float FOCUS_RIM_SMOOTHNESS = 1f;
        private const float DEFAULT_RIM_SIZE = 0f;
        private const float DEFAULT_RIM_SMOOTHNESS = 0f;

        protected static readonly int FlatRimSize = Shader.PropertyToID("_FlatRimSize");
        protected static readonly int FlatRimEdgeSmoothness = Shader.PropertyToID("_FlatRimEdgeSmoothness");
        protected static readonly int OutlineWidth = Shader.PropertyToID("_OutlineWidth");
        protected static readonly int OutlineScale = Shader.PropertyToID("_OutlineScale");
        protected static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
        protected static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        protected static readonly int ColorProperty = Shader.PropertyToID("_Color");
        
        [Header("Audio")]
        [SerializeField] protected AudioClip _interactionSound;

        [Header("Focus Visuals")]
        [SerializeField] protected Color _focusOutlineColor = Color.white;
        protected float _focusOutlineWidth = 1f;
        protected float _focusOutlineScale = 1f;

        protected Material[] _instancedMaterials;
        protected AudioSource _audioSource;

        public abstract InteractionAction Action { get; }

        protected virtual void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            
            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();

            var renderers = new List<Renderer>();
            var rootRenderer = GetComponent<Renderer>();
            
            if (rootRenderer != null) renderers.Add(rootRenderer);
            
            var childRenderers = GetComponentsInChildren<Renderer>(true);
            
            foreach (var childRenderer in childRenderers)
            {
                if (childRenderer != rootRenderer) renderers.Add(childRenderer);
            }
            
            _instancedMaterials = new Material[renderers.Count];
            
            for (var i = 0; i < renderers.Count; i++)
            {
                _instancedMaterials[i] = renderers[i].material;
                SetRimProperties(_instancedMaterials[i], DEFAULT_RIM_SIZE, DEFAULT_RIM_SMOOTHNESS);
                SetOutlineProperties(_instancedMaterials[i], Color.white, 0f, 0f);
            }
        }

        public virtual void Focus()
        {
            if (_instancedMaterials == null) return;
            if (Action == InteractionAction.None) return;
            
            foreach (var material in _instancedMaterials)
            {
                SetRimProperties(material, FOCUS_RIM_SIZE, FOCUS_RIM_SMOOTHNESS);
                SetOutlineProperties(material, _focusOutlineColor, _focusOutlineWidth, _focusOutlineScale);
            }
        }

        public virtual void Unfocus()
        {
            if (_instancedMaterials == null) return;
            
            foreach (var material in _instancedMaterials)
            {
                SetRimProperties(material, DEFAULT_RIM_SIZE, DEFAULT_RIM_SMOOTHNESS);
                SetOutlineProperties(material, Color.white, 0f, 0f);
            }
        }

        protected void SetRimProperties(Material material, float size, float smoothness)
        {
            if (material == null) return;
            
            if (material.HasProperty(FlatRimSize))
                material.SetFloat(FlatRimSize, size);
                
            if (material.HasProperty(FlatRimEdgeSmoothness))
                material.SetFloat(FlatRimEdgeSmoothness, smoothness);
        }

        protected void SetOutlineProperties(Material material, Color color, float width, float scale)
        {
            if (material == null) return;

            if (material.HasProperty(OutlineColor))
                material.SetColor(OutlineColor, color);

            if (material.HasProperty(OutlineWidth))
                material.SetFloat(OutlineWidth, width);

            if (material.HasProperty(OutlineScale))
                material.SetFloat(OutlineScale, scale);
        }
        
        public void Interact(PlayerInteractionController interactor)
        {
            if (interactor == null) return;
            
            ExecuteInteraction(interactor);
            PlayInteractionSound();
        }

        public void ForceInteract(bool playSound = true)
        {
            ExecuteInteraction(null);
            
            if (playSound) PlayInteractionSound();
        }
        
        protected virtual void PlayInteractionSound()
        {
            var clip = GetInteractionSound();
            
            if (clip == null || _audioSource == null) return;

            _audioSource.PlayOneShot(clip);
        }

        protected virtual AudioClip GetInteractionSound()
        {
            return _interactionSound;
        }

        protected abstract void ExecuteInteraction(PlayerInteractionController interactor);

        protected virtual void OnDestroy()
        {
            if (_instancedMaterials == null) return;
            
            foreach (var material in _instancedMaterials)
            {
                if (material != null) Destroy(material);
            }
        }
    }
}