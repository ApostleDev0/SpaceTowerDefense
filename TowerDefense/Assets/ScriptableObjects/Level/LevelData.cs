using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    public string levelName; // match scene name game
    public int wavesToWin; // condition to win
    public int startingResources; // amount golds to start
    public int startingLives; // amount lives to start

    public Vector2 initialSpawnPosition;
}
