using System;
using System.Collections;
using System.Text;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.AI
{
    [Serializable]
    public class AiTextureRequest
    {
        public string Category;
        public string Word;
    }

    [Serializable]
    public class AiTextureResponse
    {
        public string ImageBase64;
    }

    public class AiTextureDrawer : MonoBehaviour
    {
        private const float FADE_DURATION = 1.5f;
        private const string CONTENT_TYPE = "application/json";
        private const string POST_METHOD = "POST";

        [Header("References")]
        [SerializeField] private RawImage _paperImage;
        [SerializeField] private string _endpoint = "http://127.0.0.1:8000/generate-drawing";

        public event Action OnDrawingRevealed;

        public bool IsGenerating { get; private set; }

        public void RequestAiDrawing(string category, string word)
        {
            if (IsGenerating) return;
            
            StartCoroutine(FetchAndDisplayTextureAsync(category, word));
        }

        public void ClearPaper()
        {
            if (_paperImage == null) return;

            _paperImage.DOKill();
            _paperImage.texture = null;
            _paperImage.color = new Color(1f, 1f, 1f, 0f);
        }

        private IEnumerator FetchAndDisplayTextureAsync(string category, string word)
        {
            IsGenerating = true;

            var requestData = new AiTextureRequest 
            { 
                Category = category, 
                Word = word 
            };
            
            var jsonPayload = JsonUtility.ToJson(requestData);

            using (var request = new UnityWebRequest(_endpoint, POST_METHOD))
            {
                var uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonPayload));
                uploadHandler.contentType = CONTENT_TYPE;
                
                request.uploadHandler = uploadHandler;
                request.downloadHandler = new DownloadHandlerBuffer();

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    IsGenerating = false;
                    yield break;
                }

                var response = JsonUtility.FromJson<AiTextureResponse>(request.downloadHandler.text);
                ApplyTexture(response.ImageBase64);
            }
        }

        private void ApplyTexture(string base64)
        {
            if (string.IsNullOrEmpty(base64))
            {
                IsGenerating = false;
                return;
            }

            var imageBytes = Convert.FromBase64String(base64);
            var texture = new Texture2D(2, 2);
            
            if (!texture.LoadImage(imageBytes))
            {
                IsGenerating = false;
                return;
            }

            _paperImage.texture = texture;
            _paperImage.color = new Color(1f, 1f, 1f, 0f);

            _paperImage.DOFade(1f, FADE_DURATION).OnComplete(() => 
            {
                IsGenerating = false;
                OnDrawingRevealed?.Invoke();
            });
        }
    }
}