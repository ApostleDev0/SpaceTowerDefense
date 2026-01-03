using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public float maxHealth;
    public float minSpeed;
    public float maxSpeed;
    public int damageToBase;
    public int goldReward;
}
