using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveData
{
    public DialogueData openingDialogue;

    [System.Serializable]
    public class WaveGroup
    {
        public EnemyType enemyType;
        public int count;
        public float spawnInterval;
        public float initialDelay;
    }

    public List<WaveGroup> groups = new List<WaveGroup>();
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
