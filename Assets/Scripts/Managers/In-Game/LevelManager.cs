using System;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{
    public int LevelIndex;
    public bool IsGameEnded = false;

    public Action<bool> OnGameEnd;

    public void GameWon()
    {
        IsGameEnded = true;
        InGameUI.Instance.OnGameWin();
        OnGameEnd?.Invoke(true);
    }

    public void GameLost()
    {
        IsGameEnded = true;
        InGameUI.Instance.OnGameOver();
        OnGameEnd?.Invoke(false);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(Constants.SceneNames.MainMenu);
    }
    
    public void RetryLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
