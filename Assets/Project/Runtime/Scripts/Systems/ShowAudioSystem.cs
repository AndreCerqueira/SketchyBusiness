using DG.Tweening;
using UnityEngine;

namespace Project.Runtime.Scripts.Systems
{
    public class ShowAudioSystem : MonoBehaviour
    {
        private const string PLAYER_KEY = "Player";
        private const float FADE_DURATION = 1f;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip _introTheme;
        [SerializeField] private AudioClip _topicSuspense;
        [SerializeField] private AudioClip _topicReveal;
        [SerializeField] private AudioClip _judgingTension;
        [SerializeField] private AudioClip _playerWin;
        [SerializeField] private AudioClip _aiWin;

        public void PlayIntroTheme()
        {
            PlayMusic(_introTheme);
        }

        public void PlayTopicSuspense()
        {
            PlaySfx(_topicSuspense);
        }

        public void PlayTopicReveal()
        {
            PlaySfx(_topicReveal);
            StopMusicFaded();
        }

        public void PlayJudgingTension()
        {
            PlayMusic(_judgingTension);
        }

        public void PlayWinSfx(string winner)
        {
            StopMusicFaded();
            
            var clip = winner == PLAYER_KEY ? _playerWin : _aiWin;
            PlaySfx(clip);
        }

        private void PlayMusic(AudioClip clip)
        {
            if (_musicSource == null || clip == null) return;

            _musicSource.DOKill();
            _musicSource.clip = clip;
            _musicSource.Play();
        }

        private void PlaySfx(AudioClip clip)
        {
            if (_sfxSource == null || clip == null) return;

            _sfxSource.PlayOneShot(clip);
        }

        private void StopMusicFaded()
        {
            if (_musicSource == null) return;

            _musicSource.DOFade(0f, FADE_DURATION).OnComplete(() => _musicSource.Stop());
        }
    }
}