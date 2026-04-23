using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Runtime.Scripts.Data
{
    [Serializable]
    public class DrawingCategory
    {
        public string CategoryName;
        public List<string> Words;
    }

    [CreateAssetMenu(fileName = "DrawingCategoryDatabase", menuName = "Project/Data/Drawing Category Database")]
    public class DrawingCategoryDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<DrawingCategory> _categories = new List<DrawingCategory>();

        public List<DrawingCategory> Categories => _categories;
    }
}