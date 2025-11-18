using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public float lives;
    public float speed;
    public int damage;
    public float resourceReward;
}
