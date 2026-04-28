using DG.Tweening;
using UnityEngine;

namespace Project.Runtime.Scripts.Systems
{
    public class ShowAudioSystem : MonoBehaviour
    {
        private const string PLAYER_KEY = "Player";
        private const float FADE_DURATION = 1f;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSourceA;
        [SerializeField] private AudioSource _musicSourceB;
        [SerializeField] private AudioSource _sfxSource;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip _mainMenuMusic;
        [SerializeField] private AudioClip _introTheme;
        [SerializeField] private AudioClip _topicSuspense;
        [SerializeField] private AudioClip _topicReveal;
        [SerializeField] private AudioClip _thinkingMusic;
        [SerializeField] private AudioClip _judgingTension;
        [SerializeField] private AudioClip _playerWin;
        [SerializeField] private AudioClip _aiWin;

        private AudioSource _activeSource;
        private float _maxVolume;

        private void Awake()
        {
            if (_musicSourceA != null)
                _maxVolume = _musicSourceA.volume;

            _activeSource = _musicSourceA;
            
            if (_musicSourceB != null)
            {
                _musicSourceB.volume = 0f;
                _musicSourceB.playOnAwake = false;
            }
        }

        public void PlayMainMenuMusic()
        {
            PlayMusic(_mainMenuMusic, true);
        }

        public void PlayIntroTheme()
        {
            FadeToMusic(_introTheme, false);
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

        public void PlayThinkingMusic()
        {
            FadeToMusic(_thinkingMusic, true);
        }

        public void PlayJudgingTension()
        {
            FadeToMusic(_judgingTension, false);
        }

        public void PlayWinSfx(string winner)
        {
            StopMusicFaded();
            
            var clip = winner == PLAYER_KEY ? _playerWin : _aiWin;
            PlaySfx(clip);
        }

        private void PlayMusic(AudioClip clip, bool isLooping)
        {
            if (_activeSource == null || clip == null) return;

            _activeSource.DOKill();
            _activeSource.volume = _maxVolume;
            _activeSource.loop = isLooping;
            _activeSource.clip = clip;
            _activeSource.Play();
        }

        private void FadeToMusic(AudioClip clip, bool isLooping)
        {
            if (_musicSourceA == null || _musicSourceB == null || clip == null) return;
            if (_activeSource.isPlaying && _activeSource.clip == clip) return;

            var oldSource = _activeSource;
            _activeSource = (_activeSource == _musicSourceA) ? _musicSourceB : _musicSourceA;

            oldSource.DOKill();
            oldSource.DOFade(0f, FADE_DURATION).OnComplete(() => oldSource.Stop());

            _activeSource.DOKill();
            _activeSource.clip = clip;
            _activeSource.loop = isLooping;
            _activeSource.volume = _maxVolume;
            _activeSource.Play();
        }

        private void PlaySfx(AudioClip clip)
        {
            if (_sfxSource == null || clip == null) return;

            _sfxSource.PlayOneShot(clip);
        }

        private void StopMusicFaded()
        {
            if (_activeSource == null) return;

            _activeSource.DOKill();
            _activeSource.DOFade(0f, FADE_DURATION).OnComplete(() => 
            {
                _activeSource.Stop();
                _activeSource.loop = false;
            });
        }
    }
}