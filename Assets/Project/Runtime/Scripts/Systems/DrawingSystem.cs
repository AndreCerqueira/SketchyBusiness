using System;
using Project.Runtime.Scripts.Data;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Project.Runtime.Scripts.Systems
{
    public class DrawingSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DrawingCategoryDatabaseSO _categoryDatabase;

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

            var wordIndex = Random.Range(0, category.Words.Count);
            
            CurrentCategory = category.CategoryName;
            CurrentWord = category.Words[wordIndex];
            HasActiveTopic = true;

            Debug.Log($"New Topic: {CurrentCategory} | Word: {CurrentWord}");
            OnTopicGenerated?.Invoke(CurrentCategory, CurrentWord);
        }
    }
}