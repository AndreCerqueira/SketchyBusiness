using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Project.Runtime.Scripts.Systems
{
    [RequireComponent(typeof(AudioSource))]
    public class TtsSystem : MonoBehaviour
    {
        private const string TTS_URL_FORMAT = "https://translate.google.com/translate_tts?ie=UTF-8&total=1&idx=0&textlen={0}&client=tw-ob&q={1}&tl=en";
        private const int MAX_CHUNK_LENGTH = 150;

        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;

        public event Action OnTtsStarted;
        public event Action OnTtsCompleted;

        private struct TtsMessage
        {
            public string Text;
            public bool IsMain;
        }

        private readonly List<TtsMessage> _speechQueue = new List<TtsMessage>();
        private bool _isPlaying;
        private bool _isCurrentMain;

        public void Speak(string text, bool isMain = true)
        {
            if (string.IsNullOrEmpty(text)) return;

            if (isMain)
                _speechQueue.RemoveAll(m => !m.IsMain);
            else if (_isCurrentMain || _speechQueue.Exists(m => m.IsMain)) return;

            _speechQueue.Add(new TtsMessage { Text = text, IsMain = isMain });

            if (!_isPlaying)
                StartCoroutine(ProcessSpeechQueueAsync());
        }

        public void TurnOff()
        {
            StopAllCoroutines();
            _speechQueue.Clear();
            _isPlaying = false;
            _isCurrentMain = false;

            if (_audioSource != null)
                _audioSource.Stop();

            enabled = false;
        }

        private IEnumerator ProcessSpeechQueueAsync()
        {
            _isPlaying = true;
            OnTtsStarted?.Invoke();

            while (_speechQueue.Count > 0)
            {
                var message = _speechQueue[0];
                _speechQueue.RemoveAt(0);

                _isCurrentMain = message.IsMain;

                var chunks = SplitText(message.Text);

                foreach (var chunk in chunks)
                {
                    var escapedText = UnityWebRequest.EscapeURL(chunk);
                    var url = string.Format(TTS_URL_FORMAT, chunk.Length, escapedText);

                    using (var request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
                    {
                        yield return request.SendWebRequest();
                        
                        if (request.result != UnityWebRequest.Result.Success)
                        {
                            Debug.LogError(request.error);
                            continue;
                        }

                        var clip = DownloadHandlerAudioClip.GetContent(request);
                        
                        if (_audioSource == null) break;

                        _audioSource.clip = clip;
                        _audioSource.Play();

                        yield return new WaitForSeconds(clip.length);
                    }
                }
            }
            
            _isPlaying = false;
            _isCurrentMain = false;
            OnTtsCompleted?.Invoke();
        }

        private List<string> SplitText(string text)
        {
            var words = text.Split(' ');
            var chunks = new List<string>();
            var currentChunk = string.Empty;

            foreach (var word in words)
            {
                if (currentChunk.Length + word.Length + 1 > MAX_CHUNK_LENGTH)
                {
                    chunks.Add(currentChunk.Trim());
                    currentChunk = string.Empty;
                }
                
                currentChunk += word + " ";
            }

            if (!string.IsNullOrEmpty(currentChunk))
                chunks.Add(currentChunk.Trim());

            return chunks;
        }
    }
}