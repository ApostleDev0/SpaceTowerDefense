using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private ObjectPooler pool;

    private float _spawnTimer;
    private float _timeInterval = 2f;

    // Update is called once per frame
    void Update()
    {
        // time down to 0 to spawn enemy
        _spawnTimer -= Time.deltaTime;
        if( _spawnTimer <= 0 )
        {
            _spawnTimer = _timeInterval;
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        GameObject spawnedObject = pool.GetPooledObjected();
        spawnedObject.transform.position = transform.position;
        spawnedObject.SetActive(true);
    }
}
