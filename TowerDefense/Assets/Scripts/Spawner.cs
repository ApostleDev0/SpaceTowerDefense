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

    private WaveData[] _waves => LevelManager.Instance.CurrentLevel.waves;
    private int _currentWaveIndex = 0;
    private int _waveCounter = 0;
    private WaveData CurrentWave
    {
        get
        {
            if(_waves == null || _currentWaveIndex >= _waves.Length)
            {
                return null;
            }
            return _waves[_currentWaveIndex];
        }
    }

    private int _totalEnemiesInWave = 0;
    private int _enemiesRemoved = 0;

    //private float _spawnTimer;
    //private float _spawnCounter;
    

    [SerializeField] private ObjectPooler krogan1Pool;
    [SerializeField] private ObjectPooler krogan2Pool;
    [SerializeField] private ObjectPooler krypter1Pool;
    [SerializeField] private ObjectPooler beetle1Pool;
    [SerializeField] private ObjectPooler beetle2Pool;
    [SerializeField] private ObjectPooler boss1Pool;
    [SerializeField] private ObjectPooler boss2Pool;

    private Dictionary<EnemyType,ObjectPooler> _poolDictionary;

    private float _timeBetweenWaves = 2f;
    private float _waveCoolDown;

    private bool _isBetweenWaves = false;
    private bool _isSpawning = false;
    private bool _isEndlessMode = false;
    private bool _isGamePlayScene = false;

    private Path _currentPath;
    private Coroutine _spawnCoroutine;

    private void Awake()
    {
        _poolDictionary = new Dictionary<EnemyType, ObjectPooler>()
        {
            {EnemyType.Krogan_1, krogan1Pool },
            {EnemyType.Krogan_2, krogan2Pool },
            {EnemyType.Krypter_1, krypter1Pool },
            {EnemyType.Beetle_1, beetle1Pool },
            {EnemyType.Beetle_2, beetle2Pool },
            {EnemyType.Beetle_Boss1, boss1Pool },
            {EnemyType.Virdius_Boss2, boss2Pool },
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
        if(_isGamePlayScene)
        {
            StartWave();
        }
    }

    // Update is called once per frame
    private void Update()
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
                _isBetweenWaves = false;
                StartNextWaveLogic();
            }
        }
    }
    private void StartWave()
    {
        if (CurrentWave == null)
        {
            return;
        }
        _enemiesRemoved = 0;

        // calculate amount total enemies in wave data
        _totalEnemiesInWave = CurrentWave.GetTotalEnemyCount();
        _isBetweenWaves = false;

        // update UI
        OnWaveChanged?.Invoke(_waveCounter);

        // run corountine spawn enemies
        if(_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
        }
        _spawnCoroutine = StartCoroutine(ProcessWave());
    }

    // Corountine handle spawn Enemy
    private IEnumerator ProcessWave()
    {
        _isSpawning = true;

        // search each group in wave data
        foreach (var group in CurrentWave.groups)
        {
            // delay before each group
            if (group.initialDelay > 0)
            {
                yield return new WaitForSeconds(group.initialDelay);
            }

            // spawn enemy in group
            for (int i = 0; i < group.count; i++)
            {
                SpawnEnemy(group.enemyType);

                // check final enemy, waiting for spawnInterval
                if (i < group.count - 1)
                {
                    yield return new WaitForSeconds(group.spawnInterval);
                }
            }
        }

        // finish spawn all groups
        _isSpawning = false;
    }
    private void SpawnEnemy(EnemyType type)
    {
        if(_poolDictionary.TryGetValue(type, out var pool))
        {
            GameObject spawnedObject = pool.GetPooledObjected();
            spawnedObject.transform.position = transform.position;

            float healthMultiplier = 1f + (_waveCounter * 0.1f); // + 10% health per wave
            Enemy enemy = spawnedObject.GetComponent<Enemy>();
            enemy.Initialized(_currentPath, healthMultiplier);

            spawnedObject.SetActive(true);
        }
        
    }
    private void CheckWaveCompletion()
    {
        if(!_isSpawning && _enemiesRemoved >= _totalEnemiesInWave)
        {
            FinishWave();
        }
    }
    private void FinishWave()
    {
        // check win 
        if(_waveCounter + 1 >= LevelManager.Instance.CurrentLevel.wavesToWin && !_isEndlessMode)
        {
            OnMissionComplete?.Invoke();
            return;
        }
        // if lose
        _isBetweenWaves = true;
        _waveCoolDown = _timeBetweenWaves;
    }
    private void StartNextWaveLogic()
    {
        _currentWaveIndex = (_currentWaveIndex + 1) % _waves.Length;
        _waveCounter++;
        StartWave();
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

        StartWave();
    }
    private void ResetWaveState()
    {
        _currentWaveIndex = 0;
        _waveCounter = 0;
        _spawnCoroutine = null;
        _isBetweenWaves = false;
        _isEndlessMode = false;
        _isSpawning = false;
        _enemiesRemoved = 0;
        _totalEnemiesInWave = 0;

        foreach (var pool in _poolDictionary.Values)
        {
            if(pool != null)
            {
                pool.ResetPool();
            }
        }
        OnWaveChanged?.Invoke(_waveCounter);

    }
}
