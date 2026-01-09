using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyVisuals : MonoBehaviour
{
    [SerializeField] private GameObject deathVFXPrefab;

    public void HandleDeathEffect()
    {
        if (deathVFXPrefab != null)
        {
            Instantiate(deathVFXPrefab, transform.position, transform.rotation);
        }
    }
}
