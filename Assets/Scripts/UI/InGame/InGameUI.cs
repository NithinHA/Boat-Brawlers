using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI.Screens;

namespace UI
{
    public class InGameUI : Singleton<InGameUI>
    {
        public InGameScreen InGameScreen;
        public PauseGameScreen PauseGameScreen;
        public GameOverScreen GameOverScreen;
        public GameWinScreen GameWinScreen;

        
#region GameState change

        public void OnGamePause()
        {
            PauseGameScreen.Show();
        }

        public void OnGameResume()
        {
            PauseGameScreen.Hide();
        }

        public void OnGameOver()
        {
            GameOverScreen.Show();
        }

        public void OnGameWin()
        {
            GameWinScreen.Show();
        }
        
#endregion

    }
}
