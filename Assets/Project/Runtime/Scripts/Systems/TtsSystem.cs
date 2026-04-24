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

        public event Action OnTtsCompleted;

        private readonly Queue<string> _speechQueue = new Queue<string>();
        private bool _isPlaying;

        public void Speak(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            
            _speechQueue.Enqueue(text);
            
            if (!_isPlaying)
                StartCoroutine(ProcessSpeechQueueAsync());
        }

        private IEnumerator ProcessSpeechQueueAsync()
        {
            _isPlaying = true;

            while (_speechQueue.Count > 0)
            {
                var text = _speechQueue.Dequeue();
                var chunks = SplitText(text);

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