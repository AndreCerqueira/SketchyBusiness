using System;
using UnityEngine;

namespace Project.Runtime.Scripts.Systems
{
    public class ScoreSystem : MonoBehaviour
    {
        private const string PLAYER_KEY = "Player";
        private const string AI_KEY = "AI";

        [SerializeField] private int _maxScore = 5;
        
        [Header("References")]
        [SerializeField] private UiSystem _uiSystem;

        public int PlayerScore { get; private set; }
        public int AiScore { get; private set; }
        public bool HasGameEnded { get; private set; }

        public event Action<string> OnGameEnded;

        public void AddPoint(string winner)
        {
            if (HasGameEnded) return;

            if (winner == PLAYER_KEY)
            {
                PlayerScore++;
                
                if (_uiSystem != null)
                {
                    _uiSystem.UpdatePlayerScore(PlayerScore);
                    _uiSystem.UpdatePlayerProgress(PlayerScore, _maxScore);
                }
            }
            else if (winner == AI_KEY)
            {
                AiScore++;
                
                if (_uiSystem != null)
                {
                    _uiSystem.UpdateAiScore(AiScore);
                    _uiSystem.UpdateAiProgress(AiScore, _maxScore);
                }
            }

            CheckForWinner(winner);
        }

        private void CheckForWinner(string lastWinner)
        {
            if (PlayerScore < _maxScore && AiScore < _maxScore) return;

            HasGameEnded = true;
            OnGameEnded?.Invoke(lastWinner);
        }
    }
}