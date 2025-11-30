using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateInMovement : MonoBehaviour
{
    private Vector3 _previousPosition;
    private Vector3 _moveDirection;
    private Quaternion _targetRotation;
    [SerializeField] private float rotationSpeed = 300;

    void Start()
    {
        _previousPosition = transform.position;
    }
    void Update()
    {
        _moveDirection = transform.position - _previousPosition;
        _targetRotation = Quaternion.LookRotation(Vector3.forward, _moveDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, rotationSpeed * Time.deltaTime);
        _previousPosition = transform.position;
    }
}
