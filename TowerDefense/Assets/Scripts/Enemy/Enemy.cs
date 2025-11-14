using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyData data;
    private Path _currentPath;

    private Vector3 _targetPosition;
    private int _currentWaypoints;

    private void Awake()
    {
        _currentPath = GameObject.Find("Path1").GetComponent<Path>();
    }

    private void OnEnable()
    {
        _currentWaypoints = 0;
        _targetPosition = _currentPath.GetPosition(_currentWaypoints);
    }

    // Update is called once per frame
    void Update()
    {
        // move toward to target position
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, data.speed * Time.deltaTime);

        // set new target position
        float relativeDistance = (transform.position - _targetPosition).magnitude;
        if(relativeDistance < 0.1f)
        {
            if(_currentWaypoints < _currentPath.Waypoints.Length - 1)
            {
                _currentWaypoints++;
                _targetPosition = _currentPath.GetPosition(_currentWaypoints);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
