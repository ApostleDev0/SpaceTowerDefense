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

    #region Serialized Fields
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text resourcesText;
    [SerializeField] private TMP_Text warningText;
    [SerializeField] private TMP_Text levelText;      
    [SerializeField] private TMP_Text upgradeCostText;  
    [SerializeField] private TMP_Text sellPriceText;    
    [SerializeField] private TMP_Text levelTitleText;

    [SerializeField] private GameObject towerPanel;
    [SerializeField] private GameObject towerCardPrefab;
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject missionCompletePanel;
    [SerializeField] private GameObject topInfoPanel;

    [SerializeField] private Transform cardsContainer;
    [SerializeField] private TowerData[] towers;
    [SerializeField] private Slider waveSlider;
    [SerializeField] private Slider livesSlider;
    [SerializeField] private Slider resourcesSlider;
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private CanvasGroup levelTitleGroup;

    [SerializeField] private Button speed1Button;
    [SerializeField] private Button speed2Button;
    [SerializeField] private Button speed3Button;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button upgradeButton;
    #endregion

    #region Private Fields
    private bool _isGamePaused = false;
    private bool _missionCompleteSoundPlayed = false;

    private List<GameObject> activeCards = new List<GameObject>();
    private Platform _currentPlatform;
    private Tower _selectedTower;

    private Color normalButtonColor = Color.white;
    private Color selectedButtonColor = new Color(0f/255f,114f/255f,255f/255f,120f/255f);
    private Color normalTextColor = Color.black;
    private Color selectedTextColor = Color.white;
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
        if (dialogueController != null)
        {
            dialogueController.gameObject.SetActive(false);
        }
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
        GameManager.OnBountyProgressChanged += UpdateResourcesSlider;
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
        GameManager.OnBountyProgressChanged -= UpdateResourcesSlider;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    //====PUBLIC
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
            GameManager.Instance.TrySpendResources(currentData.upgradeCost);
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
    public void HideTowerPanel()
    {
        towerPanel.SetActive(false);
        Platform.towerPanelOpen = false;
        GameManager.Instance.SetTimeScale(GameManager.Instance.GameSpeed);
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
    public void EnterEndlessMode()
    {
        missionCompletePanel.SetActive(false);
        GameManager.Instance.SetTimeScale(GameManager.Instance.GameSpeed);
        Spawner.Instance.EnableEndlessMode();
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
    public void StartDialogue(DialogueData data, Action onFinished)
    {
        if (dialogueController != null)
        {
            dialogueController.Initialize(data, () =>
            {
                onFinished?.Invoke();
            });
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
        int totalWaves = 0;
        if (LevelManager.Instance.CurrentLevel != null)
        {
            totalWaves = LevelManager.Instance.CurrentLevel.waves.Count;
        }
        if (waveText != null)
        {
            waveText.text = $"Waves: {currentWave + 1} / {totalWaves}";
            if (currentWave + 1 == totalWaves)
            {
                waveText.color = Color.red;
                waveText.fontStyle = FontStyles.Bold;
            }
            else
            {
                waveText.color = Color.white;
                waveText.fontStyle = FontStyles.Normal;
            }
        }
        if (waveSlider != null)
        {
            waveSlider.maxValue = totalWaves;
            waveSlider.value = currentWave + 1;
        }
    }
    private void UpdateLivesText(int currentLives)
    {
        //livesText.text = $"Lives: {currentLives}";
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
        resourcesText.text = $"Gold: {currentResources}$";
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
    private void HideUI()
    {
        HidePanel();
        if (topInfoPanel != null)
        {
            topInfoPanel.SetActive(false);
        }
        waveText.gameObject.SetActive(false);
        resourcesText.gameObject.SetActive(false);
        livesText.gameObject.SetActive(false);
        warningText.gameObject.SetActive(false);
        if (levelTitleText != null) levelTitleText.gameObject.SetActive(false);
        speed1Button.gameObject.SetActive(false);
        speed2Button.gameObject.SetActive(false);
        speed3Button.gameObject.SetActive(false);
        HighlightSelectedSpeedButton(GameManager.Instance.GameSpeed);
        pauseButton.gameObject.SetActive(false);
    }
    private void ShowUI()
    {
        if (topInfoPanel != null)
        {
            topInfoPanel.SetActive(true);
        }
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
        if (dialogueController != null) dialogueController.gameObject.SetActive(false);
    }
    private void UpdateNextLevelButton()
    {
        var levelManager = LevelManager.Instance;
        int currentIndex = Array.IndexOf(levelManager.allLevels, levelManager.CurrentLevel);
        nextLevelButton.interactable = currentIndex + 1 < levelManager.allLevels.Length;
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
    private void ShowGameOver()
    {
        GameManager.Instance.SetTimeScale(0f);
        gameOverPanel.SetActive(true);
        AudioManager.Instance.PlayGameOver();
    }
    private void UpdateResourcesSlider(int currentProgress, int maxProgress)
    {
        if (resourcesSlider != null)
        {
            resourcesSlider.maxValue = maxProgress;
            resourcesSlider.value = currentProgress;
        }
    }

    //====HANDLE
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
            GameManager.Instance.TrySpendResources(towerData.cost);
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
        warningText.text = message;
        AudioManager.Instance.PlayWarning();
        warningText.gameObject.SetActive(true);    
        yield return new WaitForSecondsRealtime(3f);
        warningText.gameObject.SetActive(false);
    }    
    private IEnumerator ShowLevelTitle(Action onComplete = null)
    {
        string levelName = "MISSION START";
        if (LevelManager.Instance.CurrentLevel != null && !string.IsNullOrEmpty(LevelManager.Instance.CurrentLevel.levelName))
        {
            levelName = LevelManager.Instance.CurrentLevel.levelName;
        }

        levelTitleText.text = levelName;
        levelTitleText.gameObject.SetActive(true);
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
        yield return new WaitForSeconds(2.5f);
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
        levelTitleText.gameObject.SetActive(false);
        onComplete?.Invoke();
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
            SetGameSpeed(1f);
        }
    }
}
