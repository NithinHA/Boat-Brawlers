using System;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{
    public int LevelIndex;
    public bool IsGameEnded = false;
    public AudioManager m_AudioManagerPrefab;

    public Action<bool> OnGameEnd;

    protected override void Awake()
    {
        base.Awake();
        if (AudioManager.Instance == null)
        {
            GameObject audioManager = Instantiate(m_AudioManagerPrefab).gameObject;
            audioManager.name = "AudioManager";
        }

        AudioManager.Instance.PlaySound(Constants.SoundNames.OCEAN_BG);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopSound(Constants.SoundNames.OCEAN_BG);
    }

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
