using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    private float _spawnTimer;
    private float _timeInterval = 1f;

    public GameObject Prefab;

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
        GameObject spawnedObject = GameObject.Instantiate(Prefab);
        spawnedObject.transform.position = transform.position;
    }
}
