using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Spawner : MonoBehaviour
{
    #region Singleton & Events
    public static Spawner Instance { get; private set; }
    public static event Action<int> OnWaveChanged;
    public static event Action OnMissionComplete;
    #endregion

    #region Serialized Fields
    [SerializeField] private ObjectPooler alphaCritterPool;
    [SerializeField] private ObjectPooler alphaGruntPool;
    [SerializeField] private ObjectPooler alphaBossPool;
    [SerializeField] private ObjectPooler betaCritterPool;
    [SerializeField] private ObjectPooler betaGruntPool;
    [SerializeField] private ObjectPooler betaBossPool;
    [SerializeField] private ObjectPooler deltaGruntPool;
    [SerializeField] private ObjectPooler deltaBossPool;
    #endregion

    #region Private Field
    private Dictionary<EnemyType,ObjectPooler> _poolDictionary;
    private List<WaveData> _waves => LevelManager.Instance.CurrentLevel.waves;
    private Path _currentPath;
    private Coroutine _spawnCoroutine;

    private int _currentWaveIndex = 0;
    private int _waveCounter = 0;
    private int _totalEnemiesInWave = 0;
    private int _enemiesRemoved = 0;
    private float _timeBetweenWaves = 2f;
    private float _waveCoolDown;

    private bool _isBetweenWaves = false;
    private bool _isSpawning = false;
    private bool _isEndlessMode = false;
    private bool _isGamePlayScene = false;
    private WaveData CurrentWave
    {
        get
        {
            if(_waves == null || _currentWaveIndex >= _waves.Count)
            {
                return null;
            }
            return _waves[_currentWaveIndex];
        }
    }
    #endregion

    private void Awake()
    {
        _poolDictionary = new Dictionary<EnemyType, ObjectPooler>()
        {
            {EnemyType.AlphaCritter, alphaCritterPool },
            {EnemyType.AlphaGrunt, alphaGruntPool },
            {EnemyType.AlphaBoss, alphaBossPool },
            {EnemyType.BetaCritter, betaCritterPool },
            {EnemyType.BetaGrunt, betaGruntPool },
            {EnemyType.BetaBoss, betaBossPool },
            {EnemyType.DeltaGrunt, deltaGruntPool },
            {EnemyType.DeltaBoss, deltaBossPool },
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
    //====PUBLIC
    public void EnableEndlessMode()
    {
        _isEndlessMode = true;
    }

    //====PRIVATE
    private void StartWave()
    {
        if (CurrentWave == null)
        {
            return;
        }
        _enemiesRemoved = 0;
        _totalEnemiesInWave = CurrentWave.GetTotalEnemyCount();
        _isBetweenWaves = false;
        OnWaveChanged?.Invoke(_waveCounter);
        if(_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
        }
        _spawnCoroutine = StartCoroutine(ProcessWave());
    }
    private IEnumerator ProcessWave()
    {
        _isSpawning = true;
        foreach (var group in CurrentWave.groups)
        {
            if (group.initialDelay > 0)
            {
                yield return new WaitForSeconds(group.initialDelay);
            }
            for (int i = 0; i < group.count; i++)
            {
                SpawnEnemy(group.enemyType);
                if (i < group.count - 1)
                {
                    yield return new WaitForSeconds(group.spawnInterval);
                }
            }
        }
        _isSpawning = false;
    }
    private void SpawnEnemy(EnemyType type)
    {
        if(_poolDictionary.TryGetValue(type, out var pool))
        {
            GameObject spawnedObject = pool.GetPooledObjected();
            spawnedObject.transform.position = transform.position;
            float healthMultiplier = 1f + (_waveCounter * 0.1f);
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
        if(_waveCounter + 1 >= LevelManager.Instance.CurrentLevel.wavesToWin && !_isEndlessMode)
        {
            OnMissionComplete?.Invoke();
            return;
        }
        _isBetweenWaves = true;
        _waveCoolDown = _timeBetweenWaves;
    }
    private void StartNextWaveLogic()
    {
        _currentWaveIndex = (_currentWaveIndex + 1) % _waves.Count;
        _waveCounter++;
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

    //====HANDLE
    private void HandleEnemyReachedEnd(EnemyData data)
    {
        _enemiesRemoved++;
        CheckWaveCompletion();
    }
    private void HandleEnemyDestroyed(Enemy enemy)
    {
        _enemiesRemoved++;
        CheckWaveCompletion();
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
}
