using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [SerializeField] private float maxHealth = 10f;
    [SerializeField] private int damageToBase = 1;
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private float maxSpeed = 3f;
    [SerializeField] private int goldReward = 10;
    public float MaxHealth => maxHealth;
    public int DamageToBase => damageToBase;
    public float MinSpeed => minSpeed;
    public float MaxSpeed => maxSpeed;
    public int GoldReward => goldReward;

    private void OnValidate()
    {
        if (minSpeed > maxSpeed)
        {
            minSpeed = maxSpeed;
        }
        if (maxHealth < 1)
        {
            maxHealth = 1;
        }
    }
}
