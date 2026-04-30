using System;
using System.Collections.Generic;
using System.Linq;
using Project.Runtime.Scripts.Data;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Project.Runtime.Scripts.Systems
{
    public class DrawingSystem : MonoBehaviour
    {
        private const int MAX_HISTORY_SIZE = 20;

        [Header("References")]
        [SerializeField] private DrawingCategoryDatabaseSO _categoryDatabase;

        private readonly Queue<string> _recentWords = new Queue<string>();

        public bool HasActiveTopic { get; private set; }
        public string CurrentCategory { get; private set; }
        public string CurrentWord { get; private set; }

        public event Action<string, string> OnTopicGenerated;

        public void GenerateNewTopic()
        {
            if (_categoryDatabase == null) return;
            if (_categoryDatabase.Categories == null || _categoryDatabase.Categories.Count == 0) return;

            var categoryIndex = Random.Range(0, _categoryDatabase.Categories.Count);
            var category = _categoryDatabase.Categories[categoryIndex];

            if (category.Words == null || category.Words.Count == 0) return;

            var availableWords = category.Words.Where(word => !_recentWords.Contains(word)).ToList();

            if (availableWords.Count == 0)
                availableWords = category.Words;

            var wordIndex = Random.Range(0, availableWords.Count);

            CurrentCategory = category.CategoryName;
            CurrentWord = availableWords[wordIndex];
            HasActiveTopic = true;

            _recentWords.Enqueue(CurrentWord);

            if (_recentWords.Count > MAX_HISTORY_SIZE)
                _recentWords.Dequeue();

            OnTopicGenerated?.Invoke(CurrentCategory, CurrentWord);
        }
    }
}