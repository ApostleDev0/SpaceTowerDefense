using UnityEngine;

[CreateAssetMenu(fileName = "TowerData", menuName = "Scriptable Objects/TowerData")]
public class TowerData : ScriptableObject
{
    // current tower data
    public float range;
    public float shootInterval;
    public float projectileSpeed;
    public float projectileDuration;
    public float projectileSize;
    public float damage;

    public int cost;
    public Sprite sprite;
    public GameObject prefab;

    // next level tower data
    public int upgradeCost;
    public int sellPrice;
    public TowerData nextLevelData;

}
