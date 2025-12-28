using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveData", menuName = "Scriptable Objects/WaveData")]
public class WaveData : ScriptableObject
{
    [System.Serializable]
    public class WaveGroup
    {
        public EnemyType enemyType;
        public int count;
        public float spawnInterval;
        public float initialDelay;
    }

    public List<WaveGroup> groups;
    public int GetTotalEnemyCount()
    {
        int total = 0;
        if(groups != null)
        {
            foreach(var group in groups )
            {
                total += group.count;
            }
        }
        return total;
    }
}
