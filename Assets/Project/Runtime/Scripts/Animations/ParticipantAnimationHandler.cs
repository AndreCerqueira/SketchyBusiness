using UnityEngine;

namespace Project.Runtime.Scripts.Animations
{
    public class ParticipantAnimationHandler : MonoBehaviour
    {
        private const string HAPPY_TRIGGER = "Happy";
        private const string SAD_TRIGGER = "Sad";
        private const string IS_SPEAKING_PARAM = "IsSpeaking";
        private const string IS_VICTORY_LOOP_PARAM = "IsVictoryLoop";
        private const string IS_SAD_LOOP_PARAM = "IsSadLoop";

        [Header("References")]
        [SerializeField] private Animator _animator;

        public void PlayHappy()
        {
            if (_animator == null) return;
            
            _animator.SetTrigger(HAPPY_TRIGGER);
        }

        public void PlaySad()
        {
            if (_animator == null) return;
            
            _animator.SetTrigger(SAD_TRIGGER);
        }

        public void SetSpeaking(bool isSpeaking)
        {
            if (_animator == null) return;
            
            _animator.SetBool(IS_SPEAKING_PARAM, isSpeaking);
        }

        public void SetVictoryLoop(bool isVictory)
        {
            if (_animator == null) return;
            
            _animator.SetBool(IS_VICTORY_LOOP_PARAM, isVictory);
        }

        public void SetSadLoop(bool isSad)
        {
            if (_animator == null) return;
            
            _animator.SetBool(IS_SAD_LOOP_PARAM, isSad);
        }
    }
}