using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;

public class EnemyWaveSystem : Singleton<EnemyWaveSystem>
{
    public WaveInfo[] Waves;
    [HideInInspector] public int CurrentWaveIndex = -1;

    // waiting state parameters
    private float _timeSinceLastWave = 0;
    private float _timeSinceLastSpawn = 0;
    private float _waitingStateSpawnDelay;
    
    // spawning state parameters
    private int _currentWaveEnemySpawnCount = 0;
    private int _currentWaveMaxEnemyCount;
    
    
    public enum WaveState
    {
        Waiting, Spawning, Active
    }
    private WaveState _currentState;
    public Action<WaveState> OnWaveStateChanged;

#region Unity callbacks

    protected override void Start()
    {
        if (Waves.Length == 0)
        {
            Debug.LogError("No waves present for the level!");
            return;
        }

        OnWaveStateChanged += OnWaveStateChanged_Waiting;
        OnWaveStateChanged += OnWaveStateChanged_Spawning;
        OnWaveStateChanged += OnWaveStateChanged_Active;
        EnemySpawner.Instance.OnAllEnemiesKilled += OnAllEnemiesKilled;
        
        ChangeState(WaveState.Waiting);
    }

    protected override void OnDestroy()
    {
        OnWaveStateChanged -= OnWaveStateChanged_Waiting;
        OnWaveStateChanged -= OnWaveStateChanged_Spawning;
        OnWaveStateChanged -= OnWaveStateChanged_Active;

        if (EnemySpawner.Instance != null)
            EnemySpawner.Instance.OnAllEnemiesKilled -= OnAllEnemiesKilled;
    }

    private void Update()
    {
        if (LevelManager.Instance.IsGameEnded)
            return;
        
        switch (_currentState)
        {
            case WaveState.Waiting:
                if (_timeSinceLastWave < Waves[CurrentWaveIndex].TimeBeforeWaveBegin)
                {
                    _timeSinceLastWave += Time.deltaTime;
                    // update the progress bar UI
                    InGameUI.Instance.InGameScreen.UpdateProgressBar(_timeSinceLastWave);
                    
                    if (_timeSinceLastSpawn < _waitingStateSpawnDelay)
                    {
                        _timeSinceLastSpawn += Time.deltaTime;
                    }
                    else
                    {
                        _timeSinceLastSpawn = 0;
                        EnemySpawner.Instance.SpawnEnemy();     // spawn 1 enemy
                    }
                }
                else
                {
                    _timeSinceLastWave = 0;
                    _timeSinceLastSpawn = 0;
                    ChangeState(WaveState.Spawning);
                }

                break;
            case WaveState.Spawning:

                if (_timeSinceLastSpawn < Waves[CurrentWaveIndex].AvgTimeBetweenEnemySpawns)
                {
                    _timeSinceLastSpawn += Time.deltaTime;
                }
                else
                {
                    _timeSinceLastSpawn = 0;
                    EnemySpawner.Instance.SpawnEnemy();
                    _currentWaveEnemySpawnCount++;

                    if (_currentWaveEnemySpawnCount >= _currentWaveMaxEnemyCount)
                    {
                        ChangeState(WaveState.Active);
                    }
                }

                break;
            case WaveState.Active:
                break;
        }
    }

#endregion


    private void ComputeWaveInfo()
    {
        WaveInfo currentWave = Waves[CurrentWaveIndex];
        _waitingStateSpawnDelay = currentWave.TimeBeforeWaveBegin / (currentWave.PreWaveEnemyCount + 1);
    }

    private void ChangeState(WaveState newState)
    {
        _currentState = newState;
        OnWaveStateChanged?.Invoke(_currentState);
    }


#region Event listeners

    private void OnWaveStateChanged_Waiting(WaveState newState)
    {
        if (newState != WaveState.Waiting)
            return;

        // if UI wave indicator is blinking, stop it.
        if (CurrentWaveIndex >= 0)
            InGameUI.Instance.InGameScreen.ToggleBlinkWaveIndicator(false);

        CurrentWaveIndex++;
        if(CurrentWaveIndex < Waves.Length)
            ComputeWaveInfo();
        else
        {
            ChangeState(WaveState.Active);
            LevelManager.Instance.GameWon();
        }
    }

    private void OnWaveStateChanged_Spawning(WaveState newState)
    {
        if (newState != WaveState.Spawning)
            return;

        // blink the progress bar wave icon indicating wave in-progress state.
        InGameUI.Instance.InGameScreen.ToggleBlinkWaveIndicator(true);
        InGameUI.Instance.InGameScreen.DisplayWaveInfo(CurrentWaveIndex);

        _timeSinceLastSpawn = Waves[CurrentWaveIndex].AvgTimeBetweenEnemySpawns;
        _currentWaveMaxEnemyCount = Waves[CurrentWaveIndex].WaveEnemyCount;
        _currentWaveEnemySpawnCount = 0;
    }

    private void OnWaveStateChanged_Active(WaveState newState)
    {
        if (newState != WaveState.Active)
            return;
    }

    private void OnAllEnemiesKilled()
    {
        if (LevelManager.Instance.IsGameEnded)
            return;

        if(_currentState == WaveState.Active)
            ChangeState(WaveState.Waiting);
    }

#endregion
}

[System.Serializable]
public class WaveInfo
{
    public float TimeBeforeWaveBegin;
    public int PreWaveEnemyCount;
    public int WaveEnemyCount;
    public float AvgTimeBetweenEnemySpawns;
}
