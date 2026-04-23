using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Project.Runtime.Scripts.Data;

namespace Project.Editor.Scripts
{
    public static class DrawingCategoryDatabaseBuilder
    {
        private const string FOLDER_PATH = "Assets/Project/Settings";
        private const string FILE_PATH = "Assets/Project/Settings/DrawingCategoryDatabase.asset";

        [MenuItem("Tools/Project/Generate Drawing Topics")]
        public static void GenerateDatabase()
        {
            if (!Directory.Exists(FOLDER_PATH))
                Directory.CreateDirectory(FOLDER_PATH);

            var existingDatabase = AssetDatabase.LoadAssetAtPath<DrawingCategoryDatabaseSO>(FILE_PATH);
            
            if (existingDatabase != null)
            {
                Debug.LogWarning("The database already exists! Delete it first if you want to regenerate.");
                return;
            }

            var database = ScriptableObject.CreateInstance<DrawingCategoryDatabaseSO>();

            database.Categories.AddRange(new List<DrawingCategory>
            {
                new DrawingCategory 
                { 
                    CategoryName = "Animals", 
                    Words = new List<string> { "Cat", "Dog", "Elephant", "Giraffe", "Bird", "Lion", "Snake", "Fish", "Frog", "Butterfly" } 
                },
                new DrawingCategory 
                { 
                    CategoryName = "Vehicles", 
                    Words = new List<string> { "Car", "Airplane", "Boat", "Bicycle", "Train", "Helicopter", "Submarine", "Truck", "Motorcycle", "Rocket" } 
                },
                new DrawingCategory 
                { 
                    CategoryName = "Household Objects", 
                    Words = new List<string> { "Chair", "Table", "Lamp", "Television", "Bed", "Sofa", "Refrigerator", "Fork", "Knife", "Glass" } 
                },
                new DrawingCategory 
                { 
                    CategoryName = "Nature", 
                    Words = new List<string> { "Tree", "Flower", "Mountain", "Sun", "Cloud", "Star", "Moon", "River", "Fire", "Leaf" } 
                },
                new DrawingCategory 
                { 
                    CategoryName = "Professions", 
                    Words = new List<string> { "Doctor", "Police Officer", "Firefighter", "Teacher", "Astronaut", "Painter", "Chef", "Mail Carrier", "Judge", "Mechanic" } 
                }
            });

            AssetDatabase.CreateAsset(database, FILE_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Drawing Category Database successfully created at: {FILE_PATH}");
        }
    }
}