using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.Screens
{
    public class GameOverScreen : BaseScreen
    {

        public override void Show()
        {
            base.Show();
            
            // show progress bar data here
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
