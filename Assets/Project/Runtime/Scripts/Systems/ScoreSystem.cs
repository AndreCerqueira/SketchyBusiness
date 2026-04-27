using UnityEngine;

namespace Project.Runtime.Scripts.Systems
{
    public class ScoreSystem : MonoBehaviour
    {
        private const int MAX_SCORE = 7;
        private const string PLAYER_KEY = "Player";
        private const string AI_KEY = "AI";

        [Header("References")]
        [SerializeField] private UiSystem _uiSystem;

        public int PlayerScore { get; private set; }
        public int AiScore { get; private set; }
        public bool HasGameEnded { get; private set; }

        public void AddPoint(string winner)
        {
            if (HasGameEnded) return;

            if (winner == PLAYER_KEY)
            {
                PlayerScore++;
                
                if (_uiSystem != null)
                {
                    _uiSystem.UpdatePlayerScore(PlayerScore);
                    _uiSystem.UpdatePlayerProgress(PlayerScore, MAX_SCORE);
                }
            }
            else if (winner == AI_KEY)
            {
                AiScore++;
                
                if (_uiSystem != null)
                {
                    _uiSystem.UpdateAiScore(AiScore);
                    _uiSystem.UpdateAiProgress(AiScore, MAX_SCORE);
                }
            }

            CheckForWinner(winner);
        }

        private void CheckForWinner(string lastWinner)
        {
            if (PlayerScore < MAX_SCORE && AiScore < MAX_SCORE) return;

            HasGameEnded = true;
            Debug.Log($"Game over! Winner: {lastWinner}");
        }
    }
}