using UnityEngine;

namespace Project.Runtime.Scripts.Systems
{
    public class ScoreSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UiSystem _uiSystem;

        public int PlayerScore { get; private set; }
        public int AiScore { get; private set; }

        public void AddPoint(string winner)
        {
            if (winner == "Player")
            {
                PlayerScore++;
                if (_uiSystem != null) _uiSystem.UpdatePlayerScore(PlayerScore);
            }
            else if (winner == "AI")
            {
                AiScore++;
                if (_uiSystem != null) _uiSystem.UpdateAiScore(AiScore);
            }
        }
    }
}