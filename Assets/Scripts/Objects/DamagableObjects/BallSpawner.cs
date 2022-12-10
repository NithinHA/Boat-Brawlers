using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BallSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] m_SpawnPoints;
    [SerializeField] private float m_SpawnDelay = 4f;
    [SerializeField] private float m_BallWeights = 1f;

    private float _timeSinceLastSpawn = 0;

    private void Start()
    {
        _timeSinceLastSpawn = m_SpawnDelay;
    }

    private void Update()
    {
        if (_timeSinceLastSpawn > 0)
        {
            _timeSinceLastSpawn -= Time.deltaTime;
        }
        else
        {
            SpawnBall();
            _timeSinceLastSpawn = m_SpawnDelay;
        }
    }

    private void SpawnBall()
    {
        Transform sp = m_SpawnPoints[Random.Range(0, m_SpawnPoints.Length)];
        GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.transform.position = sp.position;
        ball.transform.parent = transform;
        ball.transform.localScale = Vector3.one * .5f;

        ball.AddComponent<Rigidbody>();
        SphereCollider col = ball.AddComponent<SphereCollider>();
        col.isTrigger = true;

        BallBehaviour ballBehaviour = ball.AddComponent<BallBehaviour>();
        ballBehaviour.Weight = m_BallWeights;
    }

    private void OnDrawGizmos()
    {
        if (m_SpawnPoints == null)
            return;

        Gizmos.color = Color.cyan;
        foreach (var spawnPoint in m_SpawnPoints)
        {
            Gizmos.DrawWireCube(spawnPoint.position, Vector3.one * .2f);
        }
    }
}
