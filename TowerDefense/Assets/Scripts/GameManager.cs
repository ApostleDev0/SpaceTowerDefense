using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action<int> OnLivesChanged;
    public static event Action<int> OnResourcesChanged;

    [SerializeField] private TMP_FontAsset globalFont;

    private int _lives = 5;
    private int _resource = 200;
    public int Resources => _resource;

    private float _gameSpeed = 1f;
    public float GameSpeed => _gameSpeed;
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
        OnLivesChanged?.Invoke(_lives);
        OnResourcesChanged?.Invoke(_resource);
    }
    private void HandleEnemyReachedEnd(EnemyData data)
    {
        _lives = Mathf.Max(0, _lives - data.damage);
        OnLivesChanged?.Invoke(_lives);
    }
    private void HandleEnemyDestroyed(Enemy enemy)
    {
        AddResources(Mathf.RoundToInt(enemy.Data.resourceReward));
    }
    private void AddResources(int amount)
    {
        _resource += amount;
        OnResourcesChanged?.Invoke(_resource);
    }

    // for pausing/unpausing UI 
    public void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
    }
    // for speed button
    public void SetGameSpeed(float newSpeed)
    {
        _gameSpeed = newSpeed;
        SetTimeScale(newSpeed);
    }

    public void SpendResources(int amount)
    {
        if(_resource >= amount)
        {
            _resource -= amount;
            OnResourcesChanged?.Invoke(_resource);
        }
    }
    public void ResetGameState()
    {
        _lives = LevelManager.Instance.CurrentLevel.startingLives;
        OnLivesChanged?.Invoke(_lives);
        _resource = LevelManager.Instance.CurrentLevel.startingResources;
        OnResourcesChanged?.Invoke(_resource);

        SetGameSpeed(1f);
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == "MainMenu")
        {
            AudioManager.Instance.PlayMusic(AudioManager.Instance.mainMenuMusic);
        }
        else if(LevelManager.Instance != null && LevelManager.Instance.CurrentLevel != null)
        {
            ResetGameState();
            AudioManager.Instance.PlayMusic(AudioManager.Instance.gameplayMusic);
        }
    }
    private void ApplyGlobalFont()
    {
        foreach(var tmp in UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            tmp.font = globalFont;
        }
    }
}
