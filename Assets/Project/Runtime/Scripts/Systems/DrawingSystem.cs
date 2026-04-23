using System;
using Project.Runtime.Scripts.AI;
using Project.Runtime.Scripts.Data;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Project.Runtime.Scripts.Systems
{
    public class DrawingSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AiDrawingAnalyzer _aiAnalyzer;
        [SerializeField] private DrawingCategoryDatabaseSO _categoryDatabase;

        public bool IsProcessing { get; private set; }
        public bool HasActiveTopic { get; private set; }
        public string CurrentCategory { get; private set; }
        public string CurrentWord { get; private set; }

        public event Action<string, string> OnTopicGenerated;
        public event Action<string> OnAnalysisCompleted;
        public event Action<string> OnAnalysisFailed;

        private void OnEnable()
        {
            if (_aiAnalyzer == null) return;
            
            _aiAnalyzer.OnAnalysisCompleted += HandleAnalysisCompleted;
            _aiAnalyzer.OnAnalysisFailed += HandleAnalysisFailed;
        }

        private void OnDisable()
        {
            if (_aiAnalyzer == null) return;
            
            _aiAnalyzer.OnAnalysisCompleted -= HandleAnalysisCompleted;
            _aiAnalyzer.OnAnalysisFailed -= HandleAnalysisFailed;
        }

        public void GenerateNewTopic()
        {
            if (IsProcessing || HasActiveTopic)
            {
                Debug.LogWarning("Action blocked: Cannot generate a new topic right now.");
                return;
            }

            if (_categoryDatabase == null) return;
            if (_categoryDatabase.Categories == null || _categoryDatabase.Categories.Count == 0) return;

            var categoryIndex = Random.Range(0, _categoryDatabase.Categories.Count);
            var category = _categoryDatabase.Categories[categoryIndex];

            if (category.Words == null || category.Words.Count == 0) return;

            var wordIndex = Random.Range(0, category.Words.Count);
            
            CurrentCategory = category.CategoryName;
            CurrentWord = category.Words[wordIndex];
            HasActiveTopic = true;

            Debug.Log($"New Topic: {CurrentCategory} | Word: {CurrentWord}");
            OnTopicGenerated?.Invoke(CurrentCategory, CurrentWord);
        }

        public void SubmitDrawing()
        {
            if (IsProcessing || !HasActiveTopic)
            {
                Debug.LogWarning("Action blocked: Cannot submit drawing right now.");
                return;
            }

            if (_aiAnalyzer == null) return;
            
            IsProcessing = true;
            _aiAnalyzer.AnalyzeCurrentDrawing(CurrentCategory);
        }

        private void HandleAnalysisCompleted(string result)
        {
            Debug.Log($"AI Analysis Success: {result}");
            
            IsProcessing = false;
            HasActiveTopic = false;
            CurrentCategory = string.Empty;
            CurrentWord = string.Empty;

            OnAnalysisCompleted?.Invoke(result);
        }

        private void HandleAnalysisFailed(string error)
        {
            Debug.LogError(error);
            
            IsProcessing = false;
            OnAnalysisFailed?.Invoke(error);
        }
    }
}