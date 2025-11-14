using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private WaveData[] waves;
    private int _currentWaveIndex = 0;
    private WaveData CurrentWave => waves[_currentWaveIndex];

    private float _spawnTimer;
    private float _spawnCounter;
    private int _enemiesRemoved;

    [SerializeField] private ObjectPooler goblinPool;
    [SerializeField] private ObjectPooler flyingPool;
    [SerializeField] private ObjectPooler heftyPool;

    private Dictionary<EnemyType,ObjectPooler> _poolDictionary;

    private void Awake()
    {
        _poolDictionary = new Dictionary<EnemyType, ObjectPooler>()
        {
            {EnemyType.Golbin, goblinPool },
            {EnemyType.Flying, flyingPool },
            {EnemyType.Hefty, heftyPool },
        };
    }

    // Update is called once per frame
    void Update()
    {
        // time down to 0 to spawn enemy
        _spawnTimer -= Time.deltaTime;
        if( _spawnTimer <= 0 && _spawnCounter <= CurrentWave.enemiesPerWave)
        {
            _spawnTimer = CurrentWave.spawnInterval;
            SpawnEnemy();
            _spawnCounter++;
        }
        else if(_spawnCounter >= CurrentWave.enemiesPerWave && _enemiesRemoved >= waves.Length)
        {
            _currentWaveIndex = (_currentWaveIndex + 1) % waves.Length;
            _spawnCounter = 0;
        }
    }

    private void SpawnEnemy()
    {
        if(_poolDictionary.TryGetValue(CurrentWave.enemyType, out var pool))
        {
            GameObject spawnedObject = pool.GetPooledObjected();
            spawnedObject.transform.position = transform.position;
            spawnedObject.SetActive(true);
        }
        
    }
}
