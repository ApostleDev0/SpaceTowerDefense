using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class TutorialStep
{
    public Sprite Image;
    [TextArea(3, 5)]
    public string Description;
}
public class TutorialController : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] private string targetSceneName = "Level 1";
    [SerializeField] private bool showOnlyOnce = false;

    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private Image tutorialImage;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [SerializeField] private Button nextButton;
    [SerializeField] private TextMeshProUGUI nextButtonLabel;
    [SerializeField] private Button backButton;

    [SerializeField] private TutorialStep[] steps;
    #endregion

    #region Private Fields
    private int _currentIndex = 0;
    private const string TUTORIAL_SHOWN_KEY = "TutorialShown_";
    #endregion

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void Start()
    {
        SetupButtons();
        HandleTutorialLogic();
    }

    //====PUBLIC
    public void ShowTutorial()
    {
        tutorialPanel.SetActive(true);
        _currentIndex = 0;
        UpdateVisuals();

        // Pause game
        Time.timeScale = 0f;
    }
    public void HideTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        // Resume game if pausing
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
    }

    //====PRIVATE
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HandleTutorialLogic();
    }

    private void HandleTutorialLogic()
    {
        if (steps == null || steps.Length == 0)
        {
            StartGameDirectly();
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == targetSceneName)
        {
            if (showOnlyOnce && PlayerPrefs.GetInt(TUTORIAL_SHOWN_KEY + targetSceneName, 0) == 1)
            {
                StartGameDirectly();
                return;
            }

            ShowTutorial();
        }
        else
        {
            HideTutorial();
        }
    }
    private void SetupButtons()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextClicked);
        }
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackClicked);
        }
    }
    private void OnNextClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();

        if (_currentIndex < steps.Length - 1)
        {
            _currentIndex++;
            UpdateVisuals();
        }
        else
        {
            FinishTutorial();
        }
    }
    private void OnBackClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();

        if (_currentIndex > 0)
        {
            _currentIndex--;
            UpdateVisuals();
        }
    }
    private void UpdateVisuals()
    {
        TutorialStep currentStep = steps[_currentIndex];

        if (tutorialImage != null)
        {
            tutorialImage.sprite = currentStep.Image;
            tutorialImage.gameObject.SetActive(currentStep.Image != null);
        }
        if (descriptionText != null)
        {
            descriptionText.text = currentStep.Description;
        }
        if (nextButtonLabel != null)
        {
            bool isLastStep = (_currentIndex == steps.Length - 1);
            nextButtonLabel.text = isLastStep ? "START" : "NEXT";
        }
        if (backButton != null)
        {
            backButton.gameObject.SetActive(_currentIndex > 0);
        }
    }
    private void FinishTutorial()
    {
        if (showOnlyOnce)
        {
            PlayerPrefs.SetInt(TUTORIAL_SHOWN_KEY + targetSceneName, 1);
            PlayerPrefs.Save();
        }

        StartGameDirectly();
    }
    private void StartGameDirectly()
    {
        HideTutorial();
        if (Spawner.Instance != null)
        {
            Spawner.Instance.StartWave();
        }
    }
}
