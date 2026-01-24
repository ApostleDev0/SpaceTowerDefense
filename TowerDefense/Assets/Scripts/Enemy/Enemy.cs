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

    #region Public Fields
    public EnemyData Data => data;
    #endregion

    #region Private Fields
    private Path _currentPath;
    private FlashDamage _flashEffect;
    private EnemyVisuals _enemyVisuals;

    private Vector3 _targetPosition;
    private Vector3 _healthBarOriginalScale;
    private Vector3 _offset;

    private int _currentWaypoints;
    private float _lives;
    private float _maxLives;
    private float speed;
    private bool _hasBeenCounted = false;
    #endregion

    private void Awake()
    {
        _healthBarOriginalScale = transform.localScale;
        _flashEffect = GetComponent<FlashDamage>();
        _enemyVisuals = GetComponentInChildren<EnemyVisuals>();
    }
    void Update()
    {
        if(_hasBeenCounted)
        {
            return;
        }
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, speed * Time.deltaTime);
        float relativeDistance = (transform.position - _targetPosition).magnitude;
        if(relativeDistance < 0.1f)
        {
            if(_currentWaypoints < _currentPath.WaypointCount - 1)
            {
                _currentWaypoints++;
                _targetPosition = _currentPath.GetPosition(_currentWaypoints) + _offset;
            }
            else
            {
                _hasBeenCounted = true;
                OnEnemyReachedEnd?.Invoke(data);
                gameObject.SetActive(false);
            }
        }
    }

    //====PUBLIC
    public void TakeDamage(float damage)
    {
        if(_hasBeenCounted) { return; }
        _lives -= damage;
        _lives = Mathf.Max(_lives, 0);
        if (_flashEffect != null)
        {
            _flashEffect.Flash();
        }
        UpdateHealthBar();
        if(_lives <=0)
        {
            if (_enemyVisuals != null)
            {
                _enemyVisuals.HandleDeathEffect();
            }
            AudioManager.Instance.PlayEnemyDestroyed();
            _hasBeenCounted = true;
            OnEnemyDestroyed?.Invoke(this);
            gameObject.SetActive(false);
        }
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

    //====PRIVATE
    private void UpdateHealthBar()
    {
        float healthPercent = _lives / _maxLives;
        Vector3 scale = _healthBarOriginalScale;
        scale.x = _healthBarOriginalScale.x * healthPercent;
        healthBar.localScale = scale;
    }
    
}
