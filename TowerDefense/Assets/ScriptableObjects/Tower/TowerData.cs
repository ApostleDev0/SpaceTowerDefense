using UnityEngine;

[CreateAssetMenu(fileName = "TowerData", menuName = "Scriptable Objects/TowerData")]
public class TowerData : ScriptableObject
{
    [SerializeField] private string displayLevel = "Level 1";
    [SerializeField] private Sprite sprite;
    [SerializeField] private GameObject prefab;

    [SerializeField] private float range = 3f;
    [SerializeField] private float shootInterval = 1f;
    [SerializeField] private float damage = 10f;

    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileDuration = 2f;
    [SerializeField] private float projectileSize = 1f;

    [SerializeField] private int cost = 100;
    [SerializeField] private int sellPrice = 50;
    [SerializeField] private int upgradeCost = 150;
    [SerializeField] private TowerData nextLevelData;


    public string DisplayLevel => displayLevel;
    public Sprite Sprite => sprite;
    public GameObject Prefab => prefab;

    public float Range => range;
    public float ShootInterval => shootInterval;
    public float Damage => damage;

    public float ProjectileSpeed => projectileSpeed;
    public float ProjectileDuration => projectileDuration;
    public float ProjectileSize => projectileSize;

    public int Cost => cost;
    public int SellPrice => sellPrice;

    public int UpgradeCost => upgradeCost;
    public TowerData NextLevelData => nextLevelData;

}
