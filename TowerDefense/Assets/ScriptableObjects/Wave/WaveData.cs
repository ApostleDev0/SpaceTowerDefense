using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveData
{
    [SerializeField] private DialogueData openingDialogue;
    [SerializeField] private List<WaveGroup> groups = new List<WaveGroup>();

    public DialogueData OpeningDialogue => openingDialogue;
    public List<WaveGroup> Groups => groups;

    public int TotalEnemyCount
    {
        get
        {
            int total = 0;
            if (groups != null)
            {
                foreach (var group in groups) total += group.Count;
            }
            return total;
        }
    }

    [System.Serializable]
    public class WaveGroup
    {
        [SerializeField] private EnemyType enemyType;

        [SerializeField] private int count = 5;
        [SerializeField] private float spawnInterval = 1f;
        [SerializeField] private float initialDelay = 0f;

        // Property public for Spawner read
        public EnemyType Type => enemyType;
        public int Count => count;
        public float SpawnInterval => spawnInterval;
        public float InitialDelay => initialDelay;
    }
}
