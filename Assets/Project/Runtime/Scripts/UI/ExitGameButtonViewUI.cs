using UnityEngine;

namespace Project.Runtime.Scripts.UI
{
    public class ExitGameButtonViewUI : BaseAnimatedButtonUI
    {
        protected override void HandleButtonClick()
        {
            Application.Quit();
        }
    }
}