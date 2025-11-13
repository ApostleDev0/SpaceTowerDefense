using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Path currentPath;

    private Vector3 _targetPosition;
    private int _currentWaypoints;

    private void Awake()
    {
        currentPath = GameObject.Find("Path1").GetComponent<Path>();
    }

    private void OnEnable()
    {
        _currentWaypoints = 0;
        _targetPosition = currentPath.GetPosition(_currentWaypoints);
    }

    // Update is called once per frame
    void Update()
    {
        // move toward to target position
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, moveSpeed * Time.deltaTime);

        // set new target position
        float relativeDistance = (transform.position - _targetPosition).magnitude;
        if(relativeDistance < 0.1f)
        {
            if(_currentWaypoints < currentPath.Waypoints.Length - 1)
            {
                _currentWaypoints++;
                _targetPosition = currentPath.GetPosition(_currentWaypoints);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
