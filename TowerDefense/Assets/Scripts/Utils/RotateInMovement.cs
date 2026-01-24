using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateInMovement : MonoBehaviour
{
    #region
    [SerializeField] private float rotationSpeed = 600f;
    [SerializeField] private float rotationOffset = 0f;
    #endregion

    #region Private Fields
    private Vector3 _previousPosition;
    private Vector3 _moveDirection;
    #endregion

    private void OnEnable()
    {
        // reset old position
        _previousPosition = transform.position;
    }
    private void Update()
    {
        _moveDirection = transform.position - _previousPosition;

        if (_moveDirection.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, _moveDirection);
            if (rotationOffset != 0)
            {
                targetRotation *= Quaternion.Euler(0, 0, rotationOffset);
            }
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        _previousPosition = transform.position;
    }
}
