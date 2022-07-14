using System;
using System.Collections;
using System.Collections.Generic;
using BaseObjects.Enemy;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : Singleton<EnemySpawner>
{
    [SerializeField] private Transform m_Container;
    [SerializeField] private GameObject[] m_EnemyPrefabs;
    [SerializeField] private SpawnPointInfo[] m_SpawnInfo;
    [SerializeField] private Vector2 m_EnemyScaleRange = new Vector2(.75f, 1f);

    private float[] _spawnPointWeights;

    public List<Enemy> EnemiesSpawned = new List<Enemy>();
    public Action OnAllEnemiesKilled;

#region Unity callbacks

    protected override void Awake()
    {
        base.Awake();

        _spawnPointWeights = new float[m_SpawnInfo.Length];
        for (int i = 0; i < m_SpawnInfo.Length; i++)
            _spawnPointWeights[i] = m_SpawnInfo[i].Weight;

        Enemy.OnEnemyDestroyed += OnEnemyDestroyed;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Enemy.OnEnemyDestroyed -= OnEnemyDestroyed;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.H))
            SpawnEnemy();
    }

#endregion


    public void SpawnEnemy()
    {
        SpawnPointInfo spawnPoint = SelectSpawnPoint();

        Quaternion rotation = Quaternion.LookRotation(spawnPoint.Point.up);
        Enemy newEnemy = Instantiate(SelectRandomEnemy(), spawnPoint.Point.position, rotation).GetComponent<Enemy>();
        newEnemy.transform.SetParent(this.m_Container);

        EnemiesSpawned.Add(newEnemy);
        RandomizeEnemySize(newEnemy);
        ShootEnemyOntoRaft(newEnemy, spawnPoint);
    }

    /// <summary>
    /// Currently has only one enemy. Could have some specific algorithm for enemy selection.
    /// </summary>
    private GameObject SelectRandomEnemy()
    {
        return m_EnemyPrefabs[0];
    }

    /// <summary>
    /// Currently gives a weighted random item for SpawnPoint.
    /// Could incorporate a better algorithm that differentiates each spawn point based on direction(4D or 6D), also factors in Player's position before making a spawn decision.
    /// </summary>
    private SpawnPointInfo SelectSpawnPoint()
    {
        int index = Utilities.GetWeightedRandomIndex(_spawnPointWeights);
        return m_SpawnInfo[index];
    }

    /// <summary>
    /// Randomize enemy scale value. Round off to 1 decimal unit. ie- value 0.74331 will become 0.7
    /// </summary>
    private void RandomizeEnemySize(Enemy enemy)
    {
        float scale = (float) Math.Round(Random.Range(m_EnemyScaleRange.x, m_EnemyScaleRange.y), 1);
        enemy.transform.localScale = Vector3.one * scale;
        enemy.Weight *= scale;
    }

    private void ShootEnemyOntoRaft(Enemy enemy, SpawnPointInfo spawnPoint)
    {
        enemy.Rb.AddForce(spawnPoint.Point.forward * spawnPoint.ShootForce, ForceMode.Impulse);
    }

#region Event listeners

    private void OnEnemyDestroyed(Enemy enemy)
    {
        EnemiesSpawned.Remove(enemy);

        if (EnemiesSpawned.Count == 0)
            OnAllEnemiesKilled?.Invoke();
    }

#endregion

    private void OnDrawGizmos()
    {
        if (m_SpawnInfo == null)
            return;

        foreach (SpawnPointInfo spawnPointInfo in m_SpawnInfo)
        {
            Gizmos.color = Color.green;
            Transform point = spawnPointInfo.Point;
            Gizmos.DrawSphere(point.position, .1f);
            Gizmos.DrawLine(point.position, point.position + point.forward);
        }
    }
}

[System.Serializable]
public class SpawnPointInfo
{
    public Transform Point;
    public float Weight;
    public float ShootForce;
}
