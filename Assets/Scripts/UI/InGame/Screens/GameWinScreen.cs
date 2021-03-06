using TMPro;
using UnityEngine;

namespace UI.Screens
{
    public class GameWinScreen : BaseScreen
    {
        public override void Show()
        {
            base.Show();
        }

        public override void Hide()
        {
            base.Hide();
        }
        
#region Button OnClicks

        public void OnClick_Retry()
        {
            LevelManager.Instance.RetryLevel();
        }
        
        public void OnClick_Home()
        {
            LevelManager.Instance.GoToMainMenu();
        }

#endregion
    }
}