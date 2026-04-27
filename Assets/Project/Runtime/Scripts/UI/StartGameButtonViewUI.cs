using Project.Runtime.Scripts.Systems;
using UnityEngine;

namespace Project.Runtime.Scripts.UI
{
    public class StartGameButtonViewUI : BaseAnimatedButtonUI
    {
        [Header("References")]
        [SerializeField] private GameLoopSystem _gameLoopSystem;

        protected override void HandleButtonClick()
        {
            if (_gameLoopSystem == null) return;
            
            _gameLoopSystem.StartGame();
        }
    }
}