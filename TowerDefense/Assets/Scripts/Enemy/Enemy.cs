using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    #region Events
    public static event Action<EnemyData> OnEnemyReachedEnd;
    public static event Action<Enemy> OnEnemyDestroyed;
    #endregion

    #region Serialized Fields
    [SerializeField] private EnemyData data;
    [SerializeField] private Transform healthBar;
    #endregion

    #region Public Properties
    public EnemyData Data => data;
    #endregion

    #region Private Fields
    private Path _currentPath;
    private FlashDamage _flashEffect;
    private EnemyVisuals _enemyVisuals;

    private Vector3 _targetPosition;
    private Vector3 _healthBarOriginalScale;
    private Vector3 _offset;

    private int _currentWaypointIndex;
    private float _currentHealth;
    private float _maxHealth;
    private float _speed;
    private bool _hasBeenCounted = false;
    #endregion

    private void Awake()
    {
        _flashEffect = GetComponent<FlashDamage>();
        _enemyVisuals = GetComponentInChildren<EnemyVisuals>();
        if (healthBar != null)
        {
            _healthBarOriginalScale = healthBar.localScale;
        }
    }
    private void Update()
    {
        // check enemy die or reach end will not calculate
        if(_hasBeenCounted)
        {
            return;
        }
        if (_currentPath == null)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _speed * Time.deltaTime);
        float sqrDistance = (transform.position - _targetPosition).sqrMagnitude;
        if (sqrDistance < 0.01f)
        {
            if (_currentWaypointIndex < _currentPath.WaypointCount - 1)
            {
                _currentWaypointIndex++;
                _targetPosition = _currentPath.GetPosition(_currentWaypointIndex) + _offset;
            }
            else
            {
                ReachEnd();
            }
        }
    }

    //====PUBLIC
    public void Initialize(Path path, float healthMultiplier = 1f)
    {
        _currentPath = path;
        _currentWaypointIndex = 0;
        _hasBeenCounted = false;

        // reset stats
        _maxHealth = data.maxHealth * healthMultiplier;
        _currentHealth = _maxHealth;
        _speed = UnityEngine.Random.Range(data.minSpeed, data.maxSpeed);

        // calculate random position for movement
        float offsetX = UnityEngine.Random.Range(-0.5f, 0.5f);
        float offsetY = UnityEngine.Random.Range(-0.5f, 0.5f);
        _offset = new Vector3(offsetX, offsetY, 0);

        // set first position
        if (_currentPath != null && _currentPath.WaypointCount > 0)
        {
            transform.position = _currentPath.GetPosition(0) + _offset;
            // Target next point
            if (_currentPath.WaypointCount > 1)
            {
                _currentWaypointIndex = 1;
                _targetPosition = _currentPath.GetPosition(1) + _offset;
            }
            else
            {
                _targetPosition = transform.position;
            }
        }
        UpdateHealthBar();
        gameObject.SetActive(true);
    }

    public void TakeDamage(float damage)
    {
        if(_hasBeenCounted) 
        { 
            return; 
        }
        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0);

        // effect red flash
        if (_flashEffect != null)
        {
            _flashEffect.Flash();
        }
        UpdateHealthBar();
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    //====PRIVATE
    private void UpdateHealthBar()
    {
        if (healthBar == null)
        {
            return;
        }

        float healthPercent = Mathf.Clamp01(_currentHealth / _maxHealth);
        Vector3 newScale = _healthBarOriginalScale;
        newScale.x = _healthBarOriginalScale.x * healthPercent;
        healthBar.localScale = newScale;
    }
    private void ReachEnd()
    {
        _hasBeenCounted = true;
        OnEnemyReachedEnd?.Invoke(data);
        gameObject.SetActive(false);
    }
    private void Die()
    {
        _hasBeenCounted = true;

        // effect explode (VFX)
        if (_enemyVisuals != null)
        {
            _enemyVisuals.HandleDeathEffect();
        }

        // sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyDestroyed();
        }
        OnEnemyDestroyed?.Invoke(this);
        gameObject.SetActive(false);
    }
}
