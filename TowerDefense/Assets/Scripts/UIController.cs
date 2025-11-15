using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private TMP_Text waveText;

    private void OnEnable()
    {
        Spawner.OnWaveChanged += UpdateWaveText;
    }
    private void OnDisable()
    {
        Spawner.OnWaveChanged -= UpdateWaveText;
    }
    
    private void UpdateWaveText(int currentWave)
    {
        waveText.text = $"Wave: {currentWave + 1}";
    }
}
