using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Project.Runtime.Scripts.Systems
{
    [RequireComponent(typeof(AudioSource))]
    public class TtsSystem : MonoBehaviour
    {
        private const string TTS_URL_FORMAT = "https://translate.google.com/translate_tts?ie=UTF-8&total=1&idx=0&textlen={0}&client=tw-ob&q={1}&tl=en";

        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;

        public event Action OnTtsCompleted;

        public void Speak(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            
            StartCoroutine(PlayVoiceAudioAsync(text));
        }

        private IEnumerator PlayVoiceAudioAsync(string text)
        {
            var escapedText = UnityWebRequest.EscapeURL(text);
            var url = string.Format(TTS_URL_FORMAT, text.Length, escapedText);

            using (var request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
            {
                yield return request.SendWebRequest();
                
                if (request.result != UnityWebRequest.Result.Success) yield break;

                var clip = DownloadHandlerAudioClip.GetContent(request);
                
                if (_audioSource == null) yield break;

                _audioSource.clip = clip;
                _audioSource.Play();

                yield return new WaitForSeconds(clip.length);
                
                OnTtsCompleted?.Invoke();
            }
        }
    }
}