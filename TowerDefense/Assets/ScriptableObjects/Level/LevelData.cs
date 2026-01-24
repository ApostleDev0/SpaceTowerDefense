using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    [SerializeField] private string levelName;
    [SerializeField] private string sceneName;

    [SerializeField] private int startingResources = 500;
    [SerializeField] private int startingLives = 20;

    [SerializeField] private Vector2 initialSpawnPosition;
    [SerializeField] private List<WaveData> waves = new List<WaveData>();


    public string LevelName => levelName;
    public string SceneName => sceneName;

    public int StartingResources => startingResources;
    public int StartingLives => startingLives;
    public Vector2 InitialSpawnPosition => initialSpawnPosition;

    // return wave list for spawner read
    public List<WaveData> Waves => waves;

    // auto calculate 
    public int TotalWaves => waves != null ? waves.Count : 0;
}
