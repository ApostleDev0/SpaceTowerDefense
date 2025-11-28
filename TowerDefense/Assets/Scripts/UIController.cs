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

    private Platform _currentPlatform;

    [SerializeField] private Button speed1Button;
    [SerializeField] private Button speed2Button;
    [SerializeField] private Button speed3Button;
    [SerializeField] private Button pauseButton;

    [SerializeField] private Color normalButtonColor = Color.white;
    [SerializeField] private Color selectedButtonColor = Color.blue;
    [SerializeField] private Color normalTextColor = Color.black;
    [SerializeField] private Color selectedTextColor = Color.white;

    [SerializeField] private GameObject pausePanel;
    private bool _isGamePaused = false;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject missionCompletePanel;
    [SerializeField] private TMP_Text questText;

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
        speed1Button.onClick.AddListener(() => SetGameSpeed(0.2f));
        speed2Button.onClick.AddListener(() => SetGameSpeed(1f));
        speed3Button.onClick.AddListener(() => SetGameSpeed(3f));

        HighlightSelectedButton(GameManager.Instance.GameSpeed);
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
        ShowTowerPanel();
    }
    private void ShowTowerPanel()
    {
        towerPanel.SetActive(true);
        Platform.towerPanelOpen = true;
        GameManager.Instance.SetTimeScale(0f);
        PopulateTowerCards();
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
            GameManager.Instance.SpendResources(towerData.cost);
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
        warningText.gameObject.SetActive(true);    
        yield return new WaitForSecondsRealtime(3f);
        warningText.gameObject.SetActive(false);
    }
    private void SetGameSpeed(float timeScale)
    {
        HighlightSelectedButton(timeScale);
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
    private void HighlightSelectedButton(float selectedSpeed)
    {
        UpdateButtonVisual(speed1Button, selectedSpeed == 0.2f);
        UpdateButtonVisual(speed2Button, selectedSpeed == 1f);
        UpdateButtonVisual(speed3Button, selectedSpeed == 3f);
    }
    public void TogglePause()
    {
        if(towerPanel.activeSelf)
        {
            return;
        }
        if(_isGamePaused)
        {
            pausePanel.SetActive(false);
            _isGamePaused = false;
            GameManager.Instance.SetTimeScale(GameManager.Instance.GameSpeed);
        }
        else
        {
            pausePanel.SetActive(true);
            _isGamePaused = true;
            GameManager.Instance.SetTimeScale(0f);
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
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Camera mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        Canvas canvas = GetComponent<Canvas>();
        canvas.worldCamera = mainCamera;

        HidePanel();

        if(scene.name == "MainMenu")
        {
            HideUI();
        }
        else
        {
            ShowUI();
        }
        StartCoroutine(ShowQuest());
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
        missionCompletePanel.SetActive(true);
        GameManager.Instance.SetTimeScale(0f);
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

        speed1Button.gameObject.SetActive(false);
        speed2Button.gameObject.SetActive(false);
        speed3Button.gameObject.SetActive(false);
        pauseButton.gameObject.SetActive(false);
    }
    private void ShowUI()
    {
        waveText.gameObject.SetActive(true);
        resourcesText.gameObject.SetActive(true);
        livesText.gameObject.SetActive(true);
        warningText.gameObject.SetActive(true);

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
    }
    private void ShowPanel()
    {
        pausePanel.SetActive(true);
        gameOverPanel.SetActive(true);
        missionCompletePanel.SetActive(true);
    }
}
