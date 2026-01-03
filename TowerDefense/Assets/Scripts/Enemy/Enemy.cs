using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyData data;
    public EnemyData Data => data;

    public static event Action<EnemyData> OnEnemyReachedEnd;
    public static event Action<Enemy> OnEnemyDestroyed;

    private Path _currentPath;
    private Vector3 _targetPosition;
    private int _currentWaypoints;
    private Vector3 _offset;

    private float _lives;
    private float _maxLives;
    private float speed;

    [SerializeField] private Transform healthBar;
    private Vector3 _healthBarOriginalScale;

    private bool _hasBeenCounted = false;

    private void Awake()
    {
        _healthBarOriginalScale = transform.localScale;
    }

    private void OnEnable()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(_hasBeenCounted)
        {
            return;
        }

        // move toward to target position
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, speed * Time.deltaTime);

        // set new target position
        float relativeDistance = (transform.position - _targetPosition).magnitude;
        if(relativeDistance < 0.1f)
        {
            if(_currentWaypoints < _currentPath.Waypoints.Length - 1)
            {
                _currentWaypoints++;
                _targetPosition = _currentPath.GetPosition(_currentWaypoints) + _offset;
            }
            else // reached last waypoint
            {
                _hasBeenCounted = true;
                OnEnemyReachedEnd?.Invoke(data);
                gameObject.SetActive(false);
            }
        }
    }
    public void TakeDamage(float damage)
    {
        if(_hasBeenCounted)
        {
            return;
        }

        _lives -= damage;
        _lives = Mathf.Max(_lives, 0);
        UpdateHealthBar();

        if(_lives <=0)
        {
            AudioManager.Instance.PlayEnemyDestroyed();
            _hasBeenCounted = true;
            OnEnemyDestroyed?.Invoke(this);
            gameObject.SetActive(false);
        }
    }
    private void UpdateHealthBar()
    {
        float healthPercent = _lives / _maxLives;
        Vector3 scale = _healthBarOriginalScale;
        scale.x = _healthBarOriginalScale.x * healthPercent;
        healthBar.localScale = scale;
    }
    public void Initialized(Path path, float healthMultiplier)
    {
        _currentPath = path;
        _currentWaypoints = 0;
        _targetPosition = _currentPath.GetPosition(_currentWaypoints) + _offset;
        _hasBeenCounted = false;
        _maxLives = data.maxHealth * healthMultiplier;
        _lives = _maxLives;
        UpdateHealthBar();
        speed = UnityEngine.Random.Range(data.minSpeed, data.maxSpeed);
        float offsetX = UnityEngine.Random.Range(-0.5f, 0.5f);
        float offsetY = UnityEngine.Random.Range(-0.5f, 0.5f);
        _offset = new Vector2(offsetX, offsetY);
    }
}
