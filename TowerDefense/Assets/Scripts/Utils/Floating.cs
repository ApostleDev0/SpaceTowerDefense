using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floating : MonoBehaviour
{
    const float MOVE_AMPLITUDE = 0.1f;
    const float MOVE_SPEED = 0.5f;

    private Vector3 _startLocalPostion;
    private Vector3 _sead;

    void Start()
    {
        _startLocalPostion = transform.localPosition;
        _sead = new Vector3(Random.value * 100f, Random.value * 100f, 0f);
    }
    void Update()
    {
        float t = Time.time * MOVE_SPEED;
        float offsetX = (Mathf.PerlinNoise(t +_sead.x,0) - 0.5f) * 2f * MOVE_AMPLITUDE;
        float offsetY = (Mathf.PerlinNoise(0, t - _sead.y) - 0.5f) * 2f * MOVE_AMPLITUDE;
        transform.localPosition = _startLocalPostion + new Vector3(offsetX, offsetY,0);

    }
}
