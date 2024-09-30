using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{
    public int LevelIndex;
    public bool IsGameEnded = false;
    public AudioManager m_AudioManagerPrefab;
    [SerializeField] private SerializedDictionary<RaftType, GameObject> _raftMap = new SerializedDictionary<RaftType, GameObject>();
    [SerializeField] private SerializedDictionary<WeaponType, GameObject> _weaponMap = new SerializedDictionary<WeaponType, GameObject>();

    public Action<bool> OnGameEnd;

    protected override void Awake()
    {
        base.Awake();
        if (AudioManager.Instance == null)
        {
            GameObject audioManager = Instantiate(m_AudioManagerPrefab).gameObject;
            audioManager.name = "AudioManager";
        }

        foreach (var item in _raftMap)
        {
            item.Value.SetActive(false);
        }

        EnableRaft();
        EnableWeapon();
        RaftType activeRaft = GameManager.Instance ? GameManager.Instance.ActiveRaft : RaftType.Simple;
        _raftMap[activeRaft].SetActive(true);
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
        MenuCameraSwitcher.SwitchCamera(CameraHolder.Instance.LevelWin);
        DOVirtual.DelayedCall(2f, () => OnGameEnd?.Invoke(true))    // Wait for Camera to switch from gameplay to post-play.
            .OnComplete(() => DOVirtual.DelayedCall(1f, InGameUI.Instance.OnGameWin));      // Wait for character victory animation.
    }

    public void GameLost()
    {
        IsGameEnded = true;
        InGameUI.Instance.OnGameOver();
        OnGameEnd?.Invoke(false);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(Constants.SceneNames.Main_Menu);
    }
    
    public void RetryLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void EnableRaft()
    {
        foreach (KeyValuePair<RaftType, GameObject> item in _raftMap)
        {
            item.Value.SetActive(false);
        }
        _raftMap[GameManager.Instance.ActiveRaft].SetActive(true);
    }

    private void EnableWeapon()
    {
        foreach (KeyValuePair<WeaponType, GameObject> item in _weaponMap)
        {
            if(item.Value != null)
                item.Value.SetActive(false);
        }

        GameObject selectedWeapon = _weaponMap[GameManager.Instance.SelectedWeapon];
        if (selectedWeapon != null)
            selectedWeapon.SetActive(true);
    }
}
