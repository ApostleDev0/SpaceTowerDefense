using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    #region Singleton & Events
    public static UIController Instance { get; private set; }
    #endregion

    #region Serialized Fields - HUD
    [Header("HUD Elements")]
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text resourcesText;
    [SerializeField] private TMP_Text warningText;
    [SerializeField] private TMP_Text levelTitleText;
    [SerializeField] private CanvasGroup levelTitleGroup;
    [SerializeField] private GameObject topInfoPanel;

    [Header("Sliders")]
    [SerializeField] private Slider waveSlider;
    [SerializeField] private Slider livesSlider;
    [SerializeField] private Slider resourcesSlider;
    #endregion

    #region Serialized Fields - Panels
    [Header("Panels")]
    [SerializeField] private GameObject towerPanel;
    [SerializeField] private GameObject towerCardPrefab;
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject missionCompletePanel;
    [SerializeField] private Transform cardsContainer;
    #endregion

    #region Serialized Fields - Upgrade Info
    [Header("Upgrade UI")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text upgradeCostText;  
    [SerializeField] private TMP_Text sellPriceText;    
    [SerializeField] private Button upgradeButton;
    #endregion

    #region Serialized Fields - Controls
    [Header("Controls")]
    [SerializeField] private Button speed1Button;
    [SerializeField] private Button speed2Button;
    [SerializeField] private Button speed3Button;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button nextLevelButton;
    #endregion

    #region Serialized Fields - External References
    [Header("Data & Refs")]
    [SerializeField] private TowerData[] towers;
    [SerializeField] private DialogueController dialogueController;
    #endregion

    #region Private Fields
    private bool _isGamePaused = false;
    private bool _missionCompleteSoundPlayed = false;

    private List<GameObject> activeCards = new List<GameObject>();
    private Platform _currentPlatform;
    private Tower _selectedTower;

    private readonly Color normalButtonColor = Color.white;
    private readonly Color selectedButtonColor = new Color(0f, 0.447f, 1f, 0.47f); // (0, 114, 255, 120)
    private readonly Color normalTextColor = Color.black;
    private readonly Color selectedTextColor = Color.white;
    #endregion

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
        InitializeButtons();
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
        if (dialogueController != null)
        {
            dialogueController.gameObject.SetActive(false);
        }
        if (warningText != null)
        {
            warningText.gameObject.SetActive(false);
        }
    }
    private void OnEnable()
    {
        Spawner.OnWaveChanged += UpdateWaveText;
        Spawner.OnMissionComplete += ShowMissionComplete;

        GameManager.OnLivesChanged += UpdateLivesText;
        GameManager.OnResourcesChanged += UpdateResourcesText;
        SceneManager.sceneLoaded += OnSceneLoaded;

        Platform.OnPLatformClicked += HandlePLatformClicked;
        TowerCard.OnTowerSelected += HandleTowerSelected;

        GameManager.OnBountyProgressChanged += UpdateResourcesSlider;
    }
    private void OnDisable()
    {
        Spawner.OnWaveChanged -= UpdateWaveText;
        Spawner.OnMissionComplete -= ShowMissionComplete;

        GameManager.OnLivesChanged -= UpdateLivesText;
        GameManager.OnResourcesChanged -= UpdateResourcesText;
        GameManager.OnBountyProgressChanged -= UpdateResourcesSlider;

        Platform.OnPLatformClicked -= HandlePLatformClicked;
        TowerCard.OnTowerSelected -= HandleTowerSelected;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    //====PUBLIC (UI)
    public void ShowUpgradePanel(Tower tower)
    {
        _selectedTower = tower;
        upgradePanel.SetActive(true);
        Platform.towerPanelOpen = true;

        GameManager.Instance.SetTimeScale(0f);
        AudioManager.Instance.PlayPanelToggle();

        _selectedTower.ToggleRange(true);
        UpdateUpgradePanelInfo();
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

            // reset normal speed
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

        if(GameManager.Instance.TrySpendResources(currentData.upgradeCost))
        {
            AudioManager.Instance.PlayTowerPlaced();

            // save old position
            Vector3 pos = _selectedTower.transform.position;
            Quaternion rot = _selectedTower.transform.rotation;

            // delete old tower
            Destroy(_selectedTower.gameObject);

            //create new tower next level
            GameObject newTowerObj = Instantiate(currentData.nextLevelData.prefab, pos, rot);
            Tower newTowerScript = newTowerObj.GetComponent<Tower>();

            // up to date platfrom
            _currentPlatform.SetTower(newTowerScript);
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

        // reset platform & close panel
        _currentPlatform.ResetPlatform();
        HideUpgradePanel();
    }
    public void OnCloseUpgradePanelClicked()
    {
        HideUpgradePanel();
    }
    public void HideTowerPanel()
    {
        towerPanel.SetActive(false);
        Platform.towerPanelOpen = false;
        GameManager.Instance.SetTimeScale(GameManager.Instance.GameSpeed);
    }

    //====PUBLIC (Game Flow)
    public void TogglePause()
    {
        // check pause at MainMenu or open Tower Panel
        if(SceneManager.GetActiveScene().name == "MainMenu")
        {
            return;
        }
        if(towerPanel.activeSelf || upgradePanel.activeSelf)
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
        LevelManager.Instance.LoadMainMenu();
    }
    public void EnterEndlessMode()
    {
        missionCompletePanel.SetActive(false);
        GameManager.Instance.SetTimeScale(GameManager.Instance.GameSpeed);
        Spawner.Instance.EnableEndlessMode();
    }
    public void LoadNextLevel()
    {
        missionCompletePanel.SetActive(false);
        LevelManager.Instance.LoadNextLevel();
    }

    //==== PUBLIC (DIALOGUE & CINEMATIC)
    public void StartDialogue(DialogueData data, Action onFinished)
    {
        if (dialogueController != null)
        {
            dialogueController.Initialize(data, onFinished);
        }
        else
        {
            onFinished?.Invoke();
        }
    }
    public void PlayLevelTitleSequence(Action onComplete)
    {
        StartCoroutine(ShowLevelTitle(onComplete));
    }

    //====PRIVATE
    private void UpdateWaveText(int currentWave)
    {
        if (LevelManager.Instance.CurrentLevel == null)
        {
            return;
        }
        int totalWaves = LevelManager.Instance.CurrentLevel.waves.Count;

        if (waveText != null)
        {
            waveText.text = $"Waves: {currentWave + 1} / {totalWaves}";
            bool isFinalWave = (currentWave + 1 == totalWaves);

            waveText.color = isFinalWave ? Color.red : Color.white;
            waveText.fontStyle = isFinalWave ? FontStyles.Bold : FontStyles.Normal;
        }
        if (waveSlider != null)
        {
            waveSlider.maxValue = totalWaves;
            waveSlider.value = currentWave + 1;
        }
    }
    private void UpdateLivesText(int currentLives)
    {
        int maxLives = 100;
        if (LevelManager.Instance.CurrentLevel != null)
        {
            maxLives = LevelManager.Instance.CurrentLevel.startingLives;
        }

        if (livesText != null)
        {
            livesText.text = $"Lives: {currentLives}";
        }
        if (livesSlider != null)
        {
            livesSlider.maxValue = maxLives;
            livesSlider.value = currentLives;
        }
        if (currentLives <= 0)
        {
            ShowGameOver();
        }
    }
    private void UpdateResourcesText(int currentResources)
    {
        if(resourcesText != null)
        { 
            resourcesText.text = $"Gold: {currentResources}$";
        }
    }
    private void UpdateResourcesSlider(int currentProgress, int maxProgress)
    {
        if (resourcesSlider != null)
        {
            resourcesSlider.maxValue = maxProgress;
            resourcesSlider.value = currentProgress;
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
    private void PopulateTowerCards()
    {
        // delete old cards
        foreach (var card in activeCards)
        {
            Destroy(card);
        }
        activeCards.Clear();

        // create new cards
        foreach (var data in towers)
        {
            GameObject cardObj = Instantiate(towerCardPrefab, cardsContainer);
            TowerCard card = cardObj.GetComponent<TowerCard>();
            if(card != null)
            {
                card.Initialize(data);
                activeCards.Add(cardObj);
            }
        }
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
    private void UpdateNextLevelButton()
    {
        if (nextLevelButton != null)
        {
            nextLevelButton.interactable = LevelManager.Instance.HasNextLevel();
        }
    }
    private void SetGameSpeed(float timeScale, bool playSound = false)
    {
        HighlightSelectedSpeedButton(timeScale);
        GameManager.Instance.SetGameSpeed(timeScale);

        if (playSound)
        {
            if (timeScale < 1f)
            {
                AudioManager.Instance.PlaySpeedSlow();
            }
            else if (timeScale > 1f)
            {
                AudioManager.Instance.PlaySpeedFast();
            }
            else
            {
                AudioManager.Instance.PlaySpeedNormal();
            }
        }
    }
    private void HighlightSelectedSpeedButton(float selectedSpeed)
    {
        UpdateButtonVisual(speed1Button, Mathf.Approximately(selectedSpeed, 0.2f));
        UpdateButtonVisual(speed2Button, Mathf.Approximately(selectedSpeed, 1f));
        UpdateButtonVisual(speed3Button, Mathf.Approximately(selectedSpeed, 2.25f));
    }
    private void UpdateButtonVisual(Button button, bool isSelected)
    {
        if (button == null)
        {
            return;
        }
        button.image.color = isSelected ? selectedButtonColor : normalButtonColor;

        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.color = isSelected ? selectedTextColor : normalTextColor;
        }
    }
    private void ShowGameOver()
    {
        GameManager.Instance.SetTimeScale(0f);
        gameOverPanel.SetActive(true);
        AudioManager.Instance.PlayGameOver();
    }
    private void InitializeButtons()
    {
        speed1Button.onClick.AddListener(() => SetGameSpeed(0.2f, true));
        speed2Button.onClick.AddListener(() => SetGameSpeed(1f, true));
        speed3Button.onClick.AddListener(() => SetGameSpeed(2.25f, true));

        // Highlight normal speed
        if (GameManager.Instance != null)
            HighlightSelectedSpeedButton(GameManager.Instance.GameSpeed);
    }
    private void UpdateUpgradePanelInfo()
    {
        // check selected tower
        if(_selectedTower == null)
        {
            return;
        }
        TowerData data = _selectedTower.GetData();
        levelText.text = data.displayLevel;
        sellPriceText.text = $"${data.sellPrice}";

        if (data.nextLevelData != null)
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

    //====HANDLE
    private void HandlePLatformClicked(Platform platform)
    {
        _currentPlatform = platform;
        if(_currentPlatform.CurrentTower == null)
        {
            HideUpgradePanel();
            ShowTowerPanel();
        }
        else
        {
            HideTowerPanel();
            ShowUpgradePanel(_currentPlatform.CurrentTower);
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
        if(GameManager.Instance.TrySpendResources(towerData.cost))
        {
            AudioManager.Instance.PlayTowerPlaced();
            _currentPlatform.PlaceTower(towerData);
        }
        else
        {
            StartCoroutine(ShowWarningMessage("Not enough Gold$ !"));
        }
        HideTowerPanel();
    }
    private IEnumerator ShowWarningMessage(string message)
    {
        if(warningText != null)
        {
            warningText.text = message;
            warningText.gameObject.SetActive(true);    
            AudioManager.Instance.PlayWarning();

            yield return new WaitForSecondsRealtime(2f);

            warningText.gameObject.SetActive(false);
        }
    }    
    private IEnumerator ShowLevelTitle(Action onComplete = null)
    {
        string levelName = "MISSION START";
        if (LevelManager.Instance.CurrentLevel != null && !string.IsNullOrEmpty(LevelManager.Instance.CurrentLevel.levelName))
        {
            levelName = LevelManager.Instance.CurrentLevel.levelName;
        }

        if(levelTitleText != null)
        {
            levelTitleText.text = levelName;
            levelTitleText.gameObject.SetActive(true);
        }

        // Fade in
        if (levelTitleGroup != null)
        {
            levelTitleGroup.alpha = 0f;
            float duration = 0.5f;
            float currentTime = 0f;
            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;
                levelTitleGroup.alpha = Mathf.Lerp(0f, 1f, currentTime / duration);
                yield return null;
            }
            levelTitleGroup.alpha = 1f;
        }

        yield return new WaitForSeconds(2.0f);

        // Fade out
        if (levelTitleGroup != null)
        {
            float duration = 0.5f;
            float currentTime = 0f;
            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;
                levelTitleGroup.alpha = Mathf.Lerp(1f, 0f, currentTime / duration);
                yield return null;
            }
        }

        if(levelTitleText != null)
        {
            levelTitleText.gameObject.SetActive(false);
        }
        onComplete?.Invoke();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject camObj = GameObject.FindGameObjectWithTag("MainCamera");
        if(camObj != null)
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.worldCamera = camObj.GetComponent<Camera>();
            }
        }
        HideAllPanels();
        _missionCompleteSoundPlayed = false;

        if(scene.name == "MainMenu")
        {
            HideHUD();
        }
        else
        {
            ShowHUD();
            SetGameSpeed(1f, false);
        }
    }    
    private void HideAllPanels()
    {
        if (pausePanel)
        {
            pausePanel.SetActive(false);
        }
        if (gameOverPanel)
        {
            gameOverPanel.SetActive(false);
        }
        if (missionCompletePanel)
        {
            missionCompletePanel.SetActive(false);
        }
        if (dialogueController != null)
        {
            dialogueController.gameObject.SetActive(false);
        }
        HideUpgradePanel();
        HideTowerPanel();
    }
    private void HideHUD()
    {
        if (topInfoPanel) topInfoPanel.SetActive(false);
        if (waveText) waveText.gameObject.SetActive(false);
        if (resourcesText) resourcesText.gameObject.SetActive(false);
        if (livesText) livesText.gameObject.SetActive(false);
        if (warningText) warningText.gameObject.SetActive(false);

        if (speed1Button) speed1Button.gameObject.SetActive(false);
        if (speed2Button) speed2Button.gameObject.SetActive(false);
        if (speed3Button) speed3Button.gameObject.SetActive(false);
        if (pauseButton) pauseButton.gameObject.SetActive(false);
    }
    private void ShowHUD()
    {
        if (topInfoPanel) topInfoPanel.SetActive(true);
        if (waveText) waveText.gameObject.SetActive(true);
        if (resourcesText) resourcesText.gameObject.SetActive(true);
        if (livesText) livesText.gameObject.SetActive(true);

        if (speed1Button) speed1Button.gameObject.SetActive(true);
        if (speed2Button) speed2Button.gameObject.SetActive(true);
        if (speed3Button) speed3Button.gameObject.SetActive(true);
        if (pauseButton) pauseButton.gameObject.SetActive(true);
    }
}
