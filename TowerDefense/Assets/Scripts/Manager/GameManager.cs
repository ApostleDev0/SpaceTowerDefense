using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    #region Singleton & Events
    public static GameManager Instance { get; private set; }
    public static event Action<int> OnLivesChanged;
    public static event Action<int> OnResourcesChanged;
    public static event Action<int, int> OnBountyProgressChanged;
    #endregion

    #region Serialized Fields
    [SerializeField] private TMP_FontAsset globalFont; // think twice to remove
    [SerializeField] private int enemiesToBonus = 10;
    [SerializeField] private int bonusAmount = 30;
    #endregion

    #region Public Properties
    public int Resources => _resource;
    public float GameSpeed => _gameSpeed;
    #endregion

    #region Private Fields
    private int _lives = 5;
    private int _resource = 200;
    private float _gameSpeed = 1f;
    private int _currentKillCount = 0;
    #endregion

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ApplyGlobalFont();
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
        // Init UI at First
        OnLivesChanged?.Invoke(_lives);
        OnResourcesChanged?.Invoke(_resource);
    }

    //====PUBLIC
    public void AddResources(int amount)
    {
        _resource += amount;
        OnResourcesChanged?.Invoke(_resource);
    }
    public bool TrySpendResources(int amount)
    {
        if(_resource >= amount)
        {
            _resource -= amount;
            OnResourcesChanged?.Invoke(_resource);
            return true;
        }
        return false;
    }
    public void SetGameSpeed(float newSpeed)
    {
        _gameSpeed = newSpeed;
        SetTimeScale(newSpeed);
    }
    public void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
    }
    public void ResetGameState()
    {
        // Check before access LevelManager
        if (LevelManager.Instance == null || LevelManager.Instance.CurrentLevel == null)
        {
            return;
        }
        _lives = LevelManager.Instance.CurrentLevel.StartingLives;
        _resource = LevelManager.Instance.CurrentLevel.StartingResources;
        _currentKillCount = 0;

        SetGameSpeed(1f);

        // update UI
        OnLivesChanged?.Invoke(_lives);
        OnResourcesChanged?.Invoke(_resource);
        OnBountyProgressChanged?.Invoke(0, enemiesToBonus);
    }

    //====PRIVATE
    private void ApplyGlobalFont() // need to set up font 
    {
        foreach(var tmp in UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            tmp.font = globalFont;
        }
    }

    //====HANDLE
    private void HandleEnemyReachedEnd(EnemyData data)
    {
        _lives = Mathf.Max(0, _lives - data.DamageToBase);
        OnLivesChanged?.Invoke(_lives);
        if (_lives <= 0)
        {
            SetTimeScale(0f);
        }
    }
    private void HandleEnemyDestroyed(Enemy enemy)
    {
        // check enemy data
        if(enemy == null || enemy.Data == null)
        {
            return;
        }
        AddResources(Mathf.RoundToInt(enemy.Data.GoldReward));

        // bonus gold when kill
        _currentKillCount++;
        if (_currentKillCount >= enemiesToBonus)
        {
            AddResources(bonusAmount);
            _currentKillCount = 0;    
        }
        OnBountyProgressChanged?.Invoke(_currentKillCount, enemiesToBonus);
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // optimization
        bool isMainMenu = scene.name.Equals("MainMenu");

        // check before play music
        if (isMainMenu)
        {
            if(AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusic(AudioManager.Instance.mainMenuMusic);
            }
        }
        else
        {
            if(AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusic(AudioManager.Instance.gameplayMusic);
            }
            ResetGameState();
        }
    }
}
