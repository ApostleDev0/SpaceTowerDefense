using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text resourcesText;

    [SerializeField] private GameObject towerPanel;
    [SerializeField] private GameObject towerCardPrefab;
    [SerializeField] private Transform cardsContainer;

    [SerializeField] private TowerData[] towers;
    private List<GameObject> activeCards = new List<GameObject>();

    private Platform _currentPlatform;

    private void OnEnable()
    {
        Spawner.OnWaveChanged += UpdateWaveText;
        GameManager.OnLivesChanged += UpdateLivesText;
        GameManager.OnResourcesChanged += UpdateResourcesText;
        Platform.OnPLatformClicked += HandlePLatformClicked;
        TowerCard.OnTowerSelected += HandleTowerSelected;
    }
    private void OnDisable()
    {
        Spawner.OnWaveChanged -= UpdateWaveText;
        GameManager.OnLivesChanged -= UpdateLivesText;
        GameManager.OnResourcesChanged -= UpdateResourcesText;
        Platform.OnPLatformClicked -= HandlePLatformClicked;
        TowerCard.OnTowerSelected -= HandleTowerSelected;
    }
    
    private void UpdateWaveText(int currentWave)
    {
        waveText.text = $"Wave: {currentWave + 1}";
    }
    private void UpdateLivesText(int currentLives)
    {
        livesText.text = $"Lives: {currentLives}";
    }
    private void UpdateResourcesText(int currentResources)
    {
        resourcesText.text = $"Golds: {currentResources}";
    }
    private void HandlePLatformClicked(Platform platform)
    {
        _currentPlatform = platform;
        ShowTowerPanel();
    }
    private void ShowTowerPanel()
    {
        towerPanel.SetActive(true);
        GameManager.Instance.SetTimeScale(0f);
        PopulateTowerCards();
    }
    public void HideTowerPanel()
    {
        towerPanel.SetActive(false);
        GameManager.Instance.SetTimeScale(1f);
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
        _currentPlatform.PlaceTower(towerData);
        HideTowerPanel();
    }

}
