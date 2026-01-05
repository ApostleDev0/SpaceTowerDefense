using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateInMovement : MonoBehaviour
{
    private Vector3 _previousPosition;
    private Vector3 _moveDirection;
    private Quaternion _targetRotation;
    [SerializeField] private float rotationSpeed = 300;

    [SerializeField] private float rotationOffset = 0f; 

    void Start()
    {
        _previousPosition = transform.position;
    }
    void Update()
    {
        _moveDirection = transform.position - _previousPosition;

        if(_moveDirection.sqrMagnitude > 0.00001f)
        {
            // basic direction (unity base is Y+)
            Quaternion baseRotation = Quaternion.LookRotation(Vector3.forward, _moveDirection);

            // ++ offset for boss
            _targetRotation = baseRotation * Quaternion.Euler(0, 0, rotationOffset);

            // rotate
            transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        _previousPosition = transform.position;
    }
}
