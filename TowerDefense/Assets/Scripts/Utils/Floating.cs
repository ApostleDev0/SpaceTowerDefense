using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floating : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] private float moveAmplitude = 0.5f;
    [SerializeField] private float moveSpeed = 1f;
    #endregion

    #region Private
    private Vector3 _startLocalPosition;
    private Vector2 _randomSeed;
    #endregion

    void Start()
    {
        // save base position
        _startLocalPosition = transform.localPosition;

        // random seed for random floating
        _randomSeed = new Vector2(Random.Range(0f, 100f), Random.Range(0f, 100f));
    }
    void Update()
    {
        // calculate time
        float time = Time.time * moveSpeed;

        // apply Perlin Noise make random movement 
        // Noise return 0..1 -> -0.5 to -0.5..0.5 -> time 2 back to -1..1
        float x = (Mathf.PerlinNoise(time + _randomSeed.x, 0f) - 0.5f) * 2f;
        float y = (Mathf.PerlinNoise(0f, time + _randomSeed.y) - 0.5f) * 2f;

        // apply new position
        Vector3 offset = new Vector3(x, y, 0) * moveAmplitude;
        transform.localPosition = _startLocalPosition + offset;

    }
}
