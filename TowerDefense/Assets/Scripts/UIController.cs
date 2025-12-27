using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; } 

    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text resourcesText;
    [SerializeField] private TMP_Text warningText;

    [SerializeField] private GameObject towerPanel;
    [SerializeField] private GameObject towerCardPrefab;
    [SerializeField] private Transform cardsContainer;

    [SerializeField] private TowerData[] towers;
    private List<GameObject> activeCards = new List<GameObject>();

    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private TMP_Text levelText;      
    [SerializeField] private TMP_Text upgradeCostText;  
    [SerializeField] private TMP_Text sellPriceText;    
    [SerializeField] private Button upgradeButton;

    private Platform _currentPlatform;
    private Tower _selectedTower;

    [SerializeField] private Button speed1Button;
    [SerializeField] private Button speed2Button;
    [SerializeField] private Button speed3Button;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button nextLevelButton;

    private Color normalButtonColor = Color.white;
    private Color selectedButtonColor = new Color(0f/255f,114f/255f,255f/255f,120f/255f);
    private Color normalTextColor = Color.black;
    private Color selectedTextColor = Color.white;

    [SerializeField] private GameObject pausePanel;
    private bool _isGamePaused = false;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text questText;

    [SerializeField] private GameObject missionCompletePanel;
    private bool _missionCompleteSoundPlayed = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    private void Start()
    {
        if(upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
        speed1Button.onClick.AddListener(() =>
        {
            SetGameSpeed(0.2f);
            AudioManager.Instance.PlaySpeedSlow();
        });
        speed2Button.onClick.AddListener(() =>
        {
            SetGameSpeed(1f);
            AudioManager.Instance.PlaySpeedNormal();
        });
        speed3Button.onClick.AddListener(() =>
        {
            SetGameSpeed(2.25f);
            AudioManager.Instance.PlaySpeedFast();
        });

        HighlightSelectedSpeedButton(GameManager.Instance.GameSpeed);
    }
    private void OnEnable()
    {
        Spawner.OnWaveChanged += UpdateWaveText;
        GameManager.OnLivesChanged += UpdateLivesText;
        GameManager.OnResourcesChanged += UpdateResourcesText;
        Platform.OnPLatformClicked += HandlePLatformClicked;
        TowerCard.OnTowerSelected += HandleTowerSelected;
        SceneManager.sceneLoaded += OnSceneLoaded;
        Spawner.OnMissionComplete += ShowMissionComplete;
    }
    private void OnDisable()
    {
        Spawner.OnWaveChanged -= UpdateWaveText;
        GameManager.OnLivesChanged -= UpdateLivesText;
        GameManager.OnResourcesChanged -= UpdateResourcesText;
        Platform.OnPLatformClicked -= HandlePLatformClicked;
        TowerCard.OnTowerSelected -= HandleTowerSelected;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Spawner.OnMissionComplete -= ShowMissionComplete;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
    private void UpdateWaveText(int currentWave)
    {
        waveText.text = $"Wave: {currentWave + 1}";
    }
    private void UpdateLivesText(int currentLives)
    {
        livesText.text = $"Lives: {currentLives}";

        if(currentLives <= 0)
        {
            ShowGameOver();
        }
    }
    private void UpdateResourcesText(int currentResources)
    {
        resourcesText.text = $"Gold: {currentResources}$";
    }
    private void HandlePLatformClicked(Platform platform)
    {
        _currentPlatform = platform;
        if(_currentPlatform.tower == null)
        {
            HideUpgradePanel();
            ShowTowerPanel();
        }
        else
        {
            HideTowerPanel();
            ShowUpgradePanel(_currentPlatform.tower);
        }
    }
    private void ShowTowerPanel()
    {
        towerPanel.SetActive(true);
        Platform.towerPanelOpen = true;
        GameManager.Instance.SetTimeScale(0f);
        PopulateTowerCards();
        AudioManager.Instance.PlayPanelToggle();
    }
    public void HideTowerPanel()
    {
        towerPanel.SetActive(false);
        Platform.towerPanelOpen = false;
        GameManager.Instance.SetTimeScale(GameManager.Instance.GameSpeed);
    }
    private void PopulateTowerCards()
    {
        foreach (var card in activeCards)
        {
            Destroy(card);
        }
        activeCards.Clear();

        foreach (var data in towers)
        {
            GameObject cardGameObject = Instantiate(towerCardPrefab, cardsContainer);
            TowerCard card = cardGameObject.GetComponent<TowerCard>();
            card.Initialize(data);
            activeCards.Add(cardGameObject);
        }
    }
    private void HandleTowerSelected(TowerData towerData)
    {
        if(_currentPlatform.transform.childCount > 0)
        {
            HideTowerPanel();
            StartCoroutine(ShowWarningMessage("You already place Tower!"));
            return;
        }
        if(GameManager.Instance.Resources >= towerData.cost)
        {
            AudioManager.Instance.PlayTowerPlaced();
            GameManager.Instance.SpendResources(towerData.cost);
            _currentPlatform.PlaceTower(towerData);
        }
        else
        {
            StartCoroutine(ShowWarningMessage("Not enough Gold$ !"));
        }
        HideTowerPanel();
    }
    public void ShowUpgradePanel(Tower tower)
    {
        _selectedTower = tower;
        upgradePanel.SetActive(true);
        Platform.towerPanelOpen = true;
        GameManager.Instance.SetTimeScale(0f);
        AudioManager.Instance.PlayPanelToggle();

        _selectedTower.ToggleRange(true);
        TowerData data = _selectedTower.GetData();
        levelText.text = data.displayLevel;
        sellPriceText.text = $"${data.sellPrice}";
        if(data.nextLevelData != null)
        {
            upgradeButton.interactable = true;
            upgradeCostText.text = $"${data.upgradeCost}";
        }
        else
        {
            upgradeButton.interactable = false;
            upgradeCostText.text = "MAX";
        }
    }
    public void HideUpgradePanel()
    {
        if(upgradePanel.activeSelf)
        {
            if(_selectedTower != null)
            {
                _selectedTower.ToggleRange(false);
                _selectedTower = null;
            }
            upgradePanel.SetActive(false);
            Platform.towerPanelOpen = false;
            GameManager.Instance.SetTimeScale(GameManager.Instance.GameSpeed);
        }
    }
    public void OnUpgradeButtonClicked()
    {
        if(_selectedTower == null)
        {
            return;
        }
        TowerData currentData = _selectedTower.GetData();
        if(GameManager.Instance.Resources >= currentData.upgradeCost)
        {
            // Minus money
            GameManager.Instance.SpendResources(currentData.upgradeCost);
            AudioManager.Instance.PlayTowerPlaced();
            // save old position
            Vector3 pos = _selectedTower.transform.position;
            Quaternion rot = _selectedTower.transform.rotation;
            // delete old tower
            Destroy(_selectedTower.gameObject);
            //create new tower
            GameObject newTowerObj = Instantiate(currentData.nextLevelData.prefab, pos, rot);
            Tower newTowerScript = newTowerObj.GetComponent<Tower>();
            // update platfrom & child of platform
            _currentPlatform.tower = newTowerScript;
            newTowerObj.transform.SetParent(_currentPlatform.transform);
            // display panel for new tower
            ShowUpgradePanel(newTowerScript);

        }
        else
        {
            StartCoroutine(ShowWarningMessage("Not enough Gold!"));
        }
    }
    public void OnSellButtonClicked()
    {
        if (_selectedTower == null)
        {
            return;
        }
        TowerData currentData = _selectedTower.GetData();

        // plus money
        GameManager.Instance.AddResources(currentData.sellPrice);
        AudioManager.Instance.PlayTowerPlaced();

        // delete tower
        Destroy(_selectedTower.gameObject);

        // Reset 
        _currentPlatform.ResetPlatform();

        // close panel
        HideUpgradePanel();
    }
    public void OnCloseUpgradePanelClicked()
    {
        HideUpgradePanel();
    }
    private IEnumerator ShowWarningMessage(string message)
    {
        warningText.text = message;
        AudioManager.Instance.PlayWarning();
        warningText.gameObject.SetActive(true);    
        yield return new WaitForSecondsRealtime(3f);
        warningText.gameObject.SetActive(false);
    }
    private void SetGameSpeed(float timeScale)
    {
        HighlightSelectedSpeedButton(timeScale);
        GameManager.Instance.SetGameSpeed(timeScale);
    }
    private void UpdateButtonVisual(Button button, bool isSelected)
    {
        button.image.color = isSelected ? selectedButtonColor : normalButtonColor;

        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.color = isSelected ? selectedTextColor : normalTextColor;
        }
    }
    private void HighlightSelectedSpeedButton(float selectedSpeed)
    {
        UpdateButtonVisual(speed1Button, selectedSpeed == 0.2f);
        UpdateButtonVisual(speed2Button, selectedSpeed == 1f);
        UpdateButtonVisual(speed3Button, selectedSpeed == 2.25f);
    }
    public void TogglePause()
    {
        if(SceneManager.GetActiveScene().name == "MainMenu")
        {
            return;
        }
        if(towerPanel.activeSelf)
        {
            return;
        }
        if(upgradePanel.activeSelf)
        {
            return;
        }
        if(_isGamePaused)
        {
            pausePanel.SetActive(false);
            _isGamePaused = false;
            GameManager.Instance.SetTimeScale(GameManager.Instance.GameSpeed);
            AudioManager.Instance.PlayUnPause();
        }
        else
        {
            pausePanel.SetActive(true);
            _isGamePaused = true;
            GameManager.Instance.SetTimeScale(0f);
            AudioManager.Instance.PlayPause();
        }
    }
    public void RestartLevel()
    {
        LevelManager.Instance.LoadLevel(LevelManager.Instance.CurrentLevel);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void GoToMainMenu()
    {
        GameManager.Instance.SetTimeScale(1f);
        SceneManager.LoadScene("MainMenu");
    }
    private void ShowGameOver()
    {
        GameManager.Instance.SetTimeScale(0f);
        gameOverPanel.SetActive(true);
        AudioManager.Instance.PlayGameOver();
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Camera mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        Canvas canvas = GetComponent<Canvas>();
        canvas.worldCamera = mainCamera;

        HidePanel();
        _missionCompleteSoundPlayed = false;

        if(scene.name == "MainMenu")
        {
            HideUI();
        }
        else
        {
            ShowUI();
            StartCoroutine(ShowQuest());
            SetGameSpeed(1f);
        }
    }
    private IEnumerator ShowQuest()
    {
        questText.text = $"Mission: Survive {LevelManager.Instance.CurrentLevel.wavesToWin} Waves! ";
        questText.gameObject.SetActive(true);
        yield return new WaitForSeconds(6f);
        questText.gameObject.SetActive(false);  
    }
    private void ShowMissionComplete()
    {
        if(!_missionCompleteSoundPlayed)
        {
            UpdateNextLevelButton();
            missionCompletePanel.SetActive(true);
            GameManager.Instance.SetTimeScale(0f);
            AudioManager.Instance.PlayMissionComplete();
            _missionCompleteSoundPlayed = true;
        }
    }
    public void EnterEndlessMode()
    {
        missionCompletePanel.SetActive(false);
        GameManager.Instance.SetTimeScale(GameManager.Instance.GameSpeed);
        Spawner.Instance.EnableEndlessMode();
    }
    private void HideUI()
    {
        HidePanel();
        waveText.gameObject.SetActive(false);
        resourcesText.gameObject.SetActive(false);
        livesText.gameObject.SetActive(false);
        warningText.gameObject.SetActive(false);
        questText.gameObject.SetActive(false);

        speed1Button.gameObject.SetActive(false);
        speed2Button.gameObject.SetActive(false);
        speed3Button.gameObject.SetActive(false);
        HighlightSelectedSpeedButton(GameManager.Instance.GameSpeed);
        pauseButton.gameObject.SetActive(false);
    }
    private void ShowUI()
    {
        waveText.gameObject.SetActive(true);
        resourcesText.gameObject.SetActive(true);
        livesText.gameObject.SetActive(true);

        speed1Button.gameObject.SetActive(true);
        speed2Button.gameObject.SetActive(true);
        speed3Button.gameObject.SetActive(true);
        pauseButton.gameObject.SetActive(true);
    }
    private void HidePanel()
    {
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        missionCompletePanel.SetActive(false);
        HideUpgradePanel();
        HideTowerPanel();
    }
    public void LoadNextLevel()
    {
        var levelManager = LevelManager.Instance;
        int currentIndex = Array.IndexOf(levelManager.allLevels, levelManager.CurrentLevel);
        
        int nextIndex = currentIndex + 1;
        if(nextIndex < levelManager.allLevels.Length)
        {
            missionCompletePanel.SetActive(false);
            levelManager.LoadLevel(levelManager.allLevels[nextIndex]);
        }
    }
    private void UpdateNextLevelButton()
    {
        var levelManager = LevelManager.Instance;
        int currentIndex = Array.IndexOf(levelManager.allLevels, levelManager.CurrentLevel);
        nextLevelButton.interactable = currentIndex + 1 < levelManager.allLevels.Length;
    }
}
