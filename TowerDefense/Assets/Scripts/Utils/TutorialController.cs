using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class TutorialStep
{
    public Sprite illustration;
    [TextArea(3, 5)]
    public string content;
}
public class TutorialController : MonoBehaviour
{
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private Image displayImage;
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI buttonLabel;

    [SerializeField] private TutorialStep[] steps;

    private int _currentIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        if (actionButton != null)
        {
            actionButton.onClick.AddListener(OnNextClicked);
        }

        ShowTutorial();
    }
    public void ShowTutorial()
    {
        if (steps == null || steps.Length == 0) return;

        tutorialPanel.SetActive(true);
        _currentIndex = 0;
        UpdateUI();

        Time.timeScale = 0f;
    }
    private void OnNextClicked()
    {
        if (_currentIndex < steps.Length - 1)
        {
            _currentIndex++;
            UpdateUI();
        }
        else
        {
            StartGame();
        }
    }
    private void UpdateUI()
    {
        TutorialStep currentStep = steps[_currentIndex];

        if (currentStep.illustration != null)
        {
            displayImage.sprite = currentStep.illustration;
            displayImage.preserveAspect = true;
        }

        if (displayText != null)
        {
            displayText.text = currentStep.content;
        }

        if (buttonLabel != null)
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
