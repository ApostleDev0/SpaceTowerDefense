using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    public string levelName;
    public string sceneName;
    public int wavesToWin; 
    public int startingResources; 
    public int startingLives; 
    public Vector2 initialSpawnPosition;

    public List<WaveData> waves = new List<WaveData>();
}
