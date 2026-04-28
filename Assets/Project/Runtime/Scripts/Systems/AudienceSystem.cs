using System.Collections.Generic;
using UnityEngine;

namespace Project.Runtime.Scripts.Systems
{
    public class AudienceSystem : MonoBehaviour
    {
        private const string CLAP_TRIGGER = "Clap";
        private const string IS_CLAP_LOOP_PARAM = "IsClapLoop";
        private const float ROUND_CLAP_PROBABILITY = 0.5f;

        [Header("References")]
        [SerializeField] private Transform[] _audienceGroups;

        private readonly List<Animator> _animators = new List<Animator>();

        private void Awake()
        {
            if (_audienceGroups == null) return;

            foreach (var group in _audienceGroups)
            {
                if (group == null) continue;

                var groupAnimators = group.GetComponentsInChildren<Animator>();
                _animators.AddRange(groupAnimators);
            }
        }

        public void PlayRoundWinClaps()
        {
            foreach (var animator in _animators)
            {
                if (animator == null) continue;
                
                if (Random.value <= ROUND_CLAP_PROBABILITY)
                    animator.SetTrigger(CLAP_TRIGGER);
            }
        }

        public void PlayGameOverClaps()
        {
            foreach (var animator in _animators)
            {
                if (animator == null) continue;
                
                animator.SetBool(IS_CLAP_LOOP_PARAM, true);
            }
        }
    }
}