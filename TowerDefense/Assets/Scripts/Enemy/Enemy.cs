using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyData data;
    public static event Action<EnemyData> OnEnemyReachedEnd;
    public static event Action<Enemy> OnEnemyDestroyed;

    private Path _currentPath;

    private Vector3 _targetPosition;
    private int _currentWaypoints;
    private float _lives;

    private void Awake()
    {
        _currentPath = GameObject.Find("Path1").GetComponent<Path>();
    }

    private void OnEnable()
    {
        _currentWaypoints = 0;
        _targetPosition = _currentPath.GetPosition(_currentWaypoints);
        _lives = data.lives;
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
            else // reached last waypoint
            {
                OnEnemyReachedEnd?.Invoke(data);
                gameObject.SetActive(false);
            }
        }
    }
    public void TakeDamage(float damage)
    {
        _lives -= damage;
        _lives = Mathf.Max(_lives, 0);
        if(_lives <=0)
        {
            OnEnemyDestroyed?.Invoke(this);
            gameObject.SetActive(false);
        }
    }
}
