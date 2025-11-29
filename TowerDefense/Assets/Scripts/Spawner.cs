using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Spawner : MonoBehaviour
{
    public static Spawner Instance { get; private set; }

    public static event Action<int> OnWaveChanged;
    public static event Action OnMissionComplete;

    [SerializeField] private WaveData[] waves;
    private int _currentWaveIndex = 0;
    private WaveData CurrentWave => waves[_currentWaveIndex];

    private float _spawnTimer;
    private float _spawnCounter;
    private int _enemiesRemoved;
    private int _waveCounter = 0;

    [SerializeField] private ObjectPooler goblinPool;
    [SerializeField] private ObjectPooler flyingPool;
    [SerializeField] private ObjectPooler heftyPool;

    private Dictionary<EnemyType,ObjectPooler> _poolDictionary;

    private float _timeBetweenWaves = 2f;
    private float _waveCoolDown;
    private bool _isBetweenWaves = false;
    private bool _isEndlessMode = false;
    private bool _isGamePlayScene = false;

    private Path _currentPath;

    private void Awake()
    {
        _poolDictionary = new Dictionary<EnemyType, ObjectPooler>()
        {
            {EnemyType.Golbin, goblinPool },
            {EnemyType.Flying, flyingPool },
            {EnemyType.Hefty, heftyPool },
        };
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnEnable()
    {
        Enemy.OnEnemyReachedEnd += HandleEnemyReachedEnd;
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        Enemy.OnEnemyReachedEnd -= HandleEnemyReachedEnd;
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void Start()
    {
        OnWaveChanged?.Invoke(_waveCounter);
    }

    // Update is called once per frame
    void Update()
    {
        if(!_isGamePlayScene)
        {
            return;
        }

        if(_isBetweenWaves)
        {
            _waveCoolDown -= Time.deltaTime;
            if(_waveCoolDown < 0f)
            {
                if(_waveCounter + 1 >= LevelManager.Instance.CurrentLevel.wavesToWin && !_isEndlessMode)
                {
                    OnMissionComplete?.Invoke();
                    return;
                }

                _currentWaveIndex = (_currentWaveIndex + 1) % waves.Length;
                _waveCounter++;
                OnWaveChanged?.Invoke(_waveCounter);
                _spawnCounter = 0;
                _enemiesRemoved = 0;
                _spawnTimer = 0f;
                _isBetweenWaves = false;
            }
        }
        else
        {
            // time down to 0 to spawn enemy
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0 && _spawnCounter < CurrentWave.enemiesPerWave)
            {
                _spawnTimer = CurrentWave.spawnInterval;
                SpawnEnemy();
                _spawnCounter++;
            }
            else if (_spawnCounter >= CurrentWave.enemiesPerWave && _enemiesRemoved >= CurrentWave.enemiesPerWave)
            {
                _isBetweenWaves = true;
                _waveCoolDown = _timeBetweenWaves;
            }
        }
    }
    private void SpawnEnemy()
    {
        if(_poolDictionary.TryGetValue(CurrentWave.enemyType, out var pool))
        {
            GameObject spawnedObject = pool.GetPooledObjected();
            spawnedObject.transform.position = transform.position;

            float healthMultiplier = 1f + (_waveCounter * 0.1f); // + 10% health per wave
            Enemy enemy = spawnedObject.GetComponent<Enemy>();
            enemy.Initialized(_currentPath, healthMultiplier);

            spawnedObject.SetActive(true);
        }
        
    }
    private void HandleEnemyReachedEnd(EnemyData data)
    {
        _enemiesRemoved++;
    }
    private void HandleEnemyDestroyed(Enemy enemy)
    {
        _enemiesRemoved++;
    }
    public void EnableEndlessMode()
    {
        _isEndlessMode = true;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _isGamePlayScene = scene.name != "MainMenu";
        ResetWaveState();
        if(!_isGamePlayScene)
        {
            return;
        }
        _currentPath = GameObject.Find("Path1").GetComponent<Path>();
        
        if(LevelManager.Instance.CurrentLevel != null)
        {
            transform.position = LevelManager.Instance.CurrentLevel.initialSpawnPosition;
        }
    }
    private void ResetWaveState()
    {
        _currentWaveIndex = 0;
        _waveCounter = 0;
        _spawnCounter = 0;
        _enemiesRemoved = 0;
        _spawnTimer = 0;
        _isBetweenWaves = false;

        foreach (var pool in _poolDictionary.Values)
        {
            if(pool != null)
            {
                pool.ResetPool();
            }
        }

    }
}
