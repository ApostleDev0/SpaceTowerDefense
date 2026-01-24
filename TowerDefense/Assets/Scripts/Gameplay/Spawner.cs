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

    #region Private Field
    // Dictionary: Enum -> Entry
    private Dictionary<EnemyType, ObjectPooler> _poolDictionary;
    private List<WaveData> _waves => LevelManager.Instance.CurrentLevel.Waves;
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
            if (_waves == null || _currentWaveIndex >= _waves.Count) return null;
            return _waves[_currentWaveIndex];
        }
    }
    #endregion

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializePoolsAuto();
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
    public void StartWave()
    {
        if (LevelManager.Instance == null || LevelManager.Instance.CurrentLevel == null)
        {
            return;
        }
        if (CurrentWave == null)
        {
            return;
        }

        _enemiesRemoved = 0;
        _totalEnemiesInWave = CurrentWave.TotalEnemyCount;
        _isBetweenWaves = false;

        OnWaveChanged?.Invoke(_waveCounter);
        if (_waveCounter == 0)
        {
            if (UIController.Instance != null)
            {
                UIController.Instance.PlayLevelTitleSequence(() =>
                {
                    ProcessWaveLogic();
                });
            }
            else
            {
                ProcessWaveLogic();
            }
        }
        else
        {
            ProcessWaveLogic();
        }
    }

    //====PRIVATE
    private void InitializePoolsAuto()
    {
        _poolDictionary = new Dictionary<EnemyType, ObjectPooler>();

        // find all ObjectPooler inside child object of Spawner
        ObjectPooler[] pools = GetComponentsInChildren<ObjectPooler>();

        foreach (ObjectPooler pool in pools)
        {
            if (!_poolDictionary.ContainsKey(pool.PoolType))
            {
                _poolDictionary.Add(pool.PoolType, pool);
            }
        }
    }
    private void ProcessWaveLogic()
    {
        if (CurrentWave.OpeningDialogue != null && !_isEndlessMode)
        {
            UIController.Instance.StartDialogue(CurrentWave.OpeningDialogue, BeginSpawning);
        }
        else
        {
            BeginSpawning();
        }
    }
    private void BeginSpawning()
    {
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
        }
        _spawnCoroutine = StartCoroutine(ProcessWave());
    }
    private IEnumerator ProcessWave()
    {
        _isSpawning = true;
        foreach (var group in CurrentWave.Groups)
        {
            if (group.InitialDelay > 0)
            {
                yield return new WaitForSeconds(group.InitialDelay);
            }

            for (int i = 0; i < group.Count; i++)
            {
                SpawnEnemy(group.Type);
                if (i < group.Count - 1) yield return new WaitForSeconds(group.SpawnInterval);
            }
        }
        _isSpawning = false;
    }
    private void SpawnEnemy(EnemyType type)
    {
        // fast search
        if (_poolDictionary.TryGetValue(type, out var pool))
        {
            GameObject spawnedObject = pool.GetInstance();
            spawnedObject.transform.position = transform.position;

            float healthMultiplier = 1f + (_waveCounter * 0.1f);
            if (spawnedObject.TryGetComponent<Enemy>(out Enemy enemy))
            {
                enemy.Initialize(_currentPath, healthMultiplier);
            }

            spawnedObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"Spawner: Pool Not Found for '{type}'! Check Again Spawner.");
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
        if(_waveCounter + 1 >= LevelManager.Instance.CurrentLevel.TotalWaves && !_isEndlessMode)
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

        // Reset Pool
        if (_poolDictionary != null)
        {
            foreach (var pool in _poolDictionary.Values)
            {
                if (pool != null) pool.ResetPool();
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
        _currentPath = FindObjectOfType<Path>();
        if (_currentPath == null)
        {
            _currentPath = GameObject.FindAnyObjectByType<Path>();
        }
        if (LevelManager.Instance != null && LevelManager.Instance.CurrentLevel != null)
        {
            transform.position = LevelManager.Instance.CurrentLevel.InitialSpawnPosition;
        }
    }
}
