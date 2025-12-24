using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private string targetSceneName = "Level 1";
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private Image tutorialImage;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button nextButton;
    [SerializeField] private TextMeshProUGUI buttonLabel;

    [SerializeField] private TutorialStep[] steps;

    private int _currentIndex = 0;

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
        if(nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextClicked);
        }
        // check log
        Debug.Log("TutorialController: Start() is running at scene: " + SceneManager.GetActiveScene().name);
        HideTutorial();
    }
    private void Update()
    {
        if(tutorialPanel.activeSelf && Time.timeScale != 0f)
        {
            Time.timeScale = 0f;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("TutorialController: OnSceneLoaded. Scene name loading: '" + scene.name + "' | Target Scene: '" + targetSceneName + "'");
        if (scene.name == targetSceneName)
        {
            Debug.Log("Match Scene Level 1! Calling ShowTutorial!");
            ShowTutorial();
        }
        else
        {
            Debug.Log("Not Match Scene Level 1! Calling HideTutorial!");
            HideTutorial();
        }
    }
    public void ShowTutorial()
    {
        if (steps == null || steps.Length == 0)
        {
            Debug.Log("Error: Steps is Empty");
            return;
        }

        Debug.Log("Show Tutorial!");
        tutorialPanel.SetActive(true);
        _currentIndex = 0;
        UpdateVisuals();
        Time.timeScale = 0f;
    }
    public void HideTutorial()
    {
        if(tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
        if(Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
    }
    private void OnNextClicked()
    {
        if(_currentIndex < steps.Length - 1)
        {
            _currentIndex++;
            UpdateVisuals();
        }
        else
        {
            StartGame();
        }
    }
    private void UpdateVisuals()
    {
        TutorialStep currentStep = steps[_currentIndex];
        if(tutorialImage != null && currentStep.Image != null)
        {
            tutorialImage.sprite = currentStep.Image;
            tutorialImage.preserveAspect = true;
        }
        if(descriptionText != null)
        {
            descriptionText.text = currentStep.Description;
        }
        if(buttonLabel != null)
        {
            bool isLastStep = (_currentIndex == steps.Length - 1);
            buttonLabel.text = isLastStep ? "START" : "NEXT";
        }
    }
    private void StartGame()
    {
        tutorialPanel.SetActive(false);
        Time.timeScale = 1f;
    }

}
