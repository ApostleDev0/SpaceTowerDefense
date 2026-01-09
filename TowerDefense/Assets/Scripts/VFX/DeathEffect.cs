using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathEffect : MonoBehaviour
{
    [SerializeField] private float delay = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject,delay);
    }
}
