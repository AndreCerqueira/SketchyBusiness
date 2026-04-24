using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.AI
{
    [Serializable]
    public class ImageAnalysisRequest
    {
        public string ImageBase64;
        public string Topic;
    }

    [Serializable]
    public class ImageAnalysisResponse
    {
        public string Description;
    }

    public class AiDrawingAnalyzer : MonoBehaviour
    {
        private const string CONTENT_TYPE = "application/json";
        private const string POST_METHOD = "POST";
        private const int TEXTURE_SIZE = 512;
        private const int BRUSH_RADIUS = 5;
        private const float PADDING_FACTOR = 0.1f;
        private const string TTS_URL_FORMAT = "https://translate.google.com/translate_tts?ie=UTF-8&total=1&idx=0&textlen={0}&client=tw-ob&q={1}&tl=nl";

        public event Action<string> OnAnalysisCompleted;
        public event Action<string> OnAnalysisFailed;

        [Header("Network Settings")]
        [SerializeField] private string _endpoint = "http://127.0.0.1:8000/analyze-drawing";

        [Header("References")]
        [SerializeField] private DrawablePaper _drawablePaper;
        [SerializeField] private RawImage _debugDisplayImage;
        [SerializeField] private AudioSource _audioSource;

        private Texture2D _lastGeneratedTexture;

        public bool IsAnalyzing { get; private set; }

        private void Awake()
        {
            if (_audioSource != null) return;
            
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.spatialBlend = 0f;
        }

        public void AnalyzeCurrentDrawing(string topic)
        {
            if (IsAnalyzing) return;
            if (_drawablePaper == null) return;

            var strokes = ExtractStrokes();
            
            if (strokes.Count == 0) return;

            StartCoroutine(RasterizeAndAnalyzeAsync(strokes, topic));
        }

        private List<List<Vector3>> ExtractStrokes()
        {
            var strokes = new List<List<Vector3>>();
            var lineRenderers = _drawablePaper.GetComponentsInChildren<LineRenderer>();

            foreach (var line in lineRenderers)
            {
                if (line.positionCount < 2) continue;

                var points = new Vector3[line.positionCount];
                line.GetPositions(points);
                strokes.Add(new List<Vector3>(points));
            }

            return strokes;
        }

        private IEnumerator RasterizeAndAnalyzeAsync(List<List<Vector3>> strokes, string topic)
        {
            IsAnalyzing = true;

            if (_lastGeneratedTexture != null) Destroy(_lastGeneratedTexture);

            _lastGeneratedTexture = GenerateTexture(strokes);
            
            if (_debugDisplayImage != null)
            {
                _debugDisplayImage.texture = _lastGeneratedTexture;
                _debugDisplayImage.enabled = true;
            }

            var imageBytes = _lastGeneratedTexture.EncodeToPNG();
            var base64Image = Convert.ToBase64String(imageBytes);
            
            var requestData = new ImageAnalysisRequest 
            { 
                ImageBase64 = base64Image,
                Topic = topic
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
                    OnAnalysisFailed?.Invoke(request.error);
                    IsAnalyzing = false;
                    yield break;
                }
                
                var response = JsonUtility.FromJson<ImageAnalysisResponse>(request.downloadHandler.text);
                
                StartCoroutine(PlayVoiceAudioAsync(response.Description));

                OnAnalysisCompleted?.Invoke(response.Description);
            }

            IsAnalyzing = false;
        }

        private IEnumerator PlayVoiceAudioAsync(string text)
        {
            if (string.IsNullOrEmpty(text)) yield break;

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
            }
        }

        private Texture2D GenerateTexture(List<List<Vector3>> strokes)
        {
            var texture = new Texture2D(TEXTURE_SIZE, TEXTURE_SIZE, TextureFormat.RGB24, false);
            var pixels = new Color[TEXTURE_SIZE * TEXTURE_SIZE];

            for (var i = 0; i < pixels.Length; i++)
                pixels[i] = Color.white;

            var bounds = CalculateBounds(strokes);

            foreach (var stroke in strokes)
            {
                for (var i = 0; i < stroke.Count - 1; i++)
                {
                    var start = MapToPixel(stroke[i], bounds);
                    var end = MapToPixel(stroke[i + 1], bounds);
                    RasterizeLine(pixels, start, end);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }

        private Bounds CalculateBounds(List<List<Vector3>> strokes)
        {
            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (var stroke in strokes)
            {
                foreach (var point in stroke)
                {
                    if (point.x < min.x) min.x = point.x;
                    if (point.y < min.y) min.y = point.y;
                    if (point.z < min.z) min.z = point.z;

                    if (point.x > max.x) max.x = point.x;
                    if (point.y > max.y) max.y = point.y;
                    if (point.z > max.z) max.z = point.z;
                }
            }

            var bounds = new Bounds();
            bounds.SetMinMax(min, max);
            
            return bounds;
        }

        private Vector2 MapToPixel(Vector3 point, Bounds bounds)
        {
            var size = bounds.size;
            
            bool isFlatX = size.x <= size.y && size.x <= size.z;
            bool isFlatY = size.y <= size.x && size.y <= size.z;
            
            float pX, pY, minX, minY, sX, sY;

            if (isFlatX)
            {
                pX = point.z; pY = point.y;
                minX = bounds.min.z; minY = bounds.min.y;
                sX = size.z; sY = size.y;
            }
            else if (isFlatY)
            {
                pX = point.x; pY = point.z;
                minX = bounds.min.x; minY = bounds.min.z;
                sX = size.x; sY = size.z;
            }
            else 
            {
                pX = point.x; pY = point.y;
                minX = bounds.min.x; minY = bounds.min.y;
                sX = size.x; sY = size.y;
            }

            float maxDimension = Mathf.Max(sX, sY);
            if (maxDimension <= 0.0001f) maxDimension = 1f;

            float normalizedX = ((pX - minX) / maxDimension) + ((maxDimension - sX) / (2f * maxDimension));
            float normalizedY = ((pY - minY) / maxDimension) + ((maxDimension - sY) / (2f * maxDimension));

            float padding = TEXTURE_SIZE * PADDING_FACTOR;
            float drawableSize = TEXTURE_SIZE - (padding * 2f);

            float x = padding + (normalizedX * drawableSize);
            float y = padding + (normalizedY * drawableSize);

            return new Vector2(x, y);
        }

        private void RasterizeLine(Color[] pixels, Vector2 p1, Vector2 p2)
        {
            var dist = Vector2.Distance(p1, p2);
            var steps = Mathf.CeilToInt(dist);

            for (var i = 0; i <= steps; i++)
            {
                var t = i / (float)steps;
                var current = Vector2.Lerp(p1, p2, t);
                DrawBrush(pixels, (int)current.x, (int)current.y);
            }
        }

        private void DrawBrush(Color[] pixels, int cx, int cy)
        {
            var r = BRUSH_RADIUS;
            var rSqr = r * r;

            for (var x = -r; x <= r; x++)
            {
                for (var y = -r; y <= r; y++)
                {
                    if (x * x + y * y <= rSqr)
                    {
                        var px = cx + x;
                        var py = cy + y;

                        if (px >= 0 && px < TEXTURE_SIZE && py >= 0 && py < TEXTURE_SIZE)
                        {
                            pixels[py * TEXTURE_SIZE + px] = Color.black;
                        }
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (_lastGeneratedTexture != null) Destroy(_lastGeneratedTexture);
        }
    }
}