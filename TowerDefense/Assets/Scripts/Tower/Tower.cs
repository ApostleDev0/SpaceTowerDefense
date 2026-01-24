using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] private TowerData data;
    [SerializeField] private GameObject rangeIndicator;
    #endregion

    #region Private Fields
    private CircleCollider2D _circleCollider;
    private List<Enemy> _enemiesInRange;
    private ObjectPooler _projectilePool;
    private float _shootTimer;
    #endregion

    private void Awake()
    {
        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(false);
        }
        _enemiesInRange = new List<Enemy>();
    }
    private void OnEnable()
    {
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
    }
    private void OnDisable()
    {
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
    }
    private void Start()
    {
        _circleCollider = GetComponent<CircleCollider2D>();
        if(_circleCollider != null && data != null)
        {
            _circleCollider.radius = data.Range;
        }

        _projectilePool = GetComponent<ObjectPooler>();
        if(data != null)
        {
            _shootTimer = data.ShootInterval;

        }
    }
    private void Update()
    {
        if (data == null)
        {
            return;
        }
        _shootTimer -= Time.deltaTime;

        if( _shootTimer <= 0 )
        {
            _shootTimer = data.ShootInterval;
            Shoot();
        }
    }

    //====PUBLIC
    public TowerData GetData() => data;
    public void ToggleRange(bool status)
    {
        if(rangeIndicator != null)
        {
            rangeIndicator.SetActive(status);
            if (status && data != null)
            {
                rangeIndicator.transform.localPosition = Vector3.zero;
                float diameter = data.Range * 2f;
                rangeIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);
            }
        }
    }

    //====PRIVATE
    private void Shoot()
    {
        // get valid target
        Enemy target = GetTarget();

        // shoot when target
        if (target != null)
        {
            GameObject projectileObj = _projectilePool.GetInstance();

            if (projectileObj != null)
            {
                projectileObj.transform.position = transform.position;
                projectileObj.SetActive(true);

                // calculate direction to shoot
                Vector3 direction = (target.transform.position - transform.position).normalized;

                // get projectile and shoot
                if (projectileObj.TryGetComponent<Projectile>(out Projectile projectileScript))
                {
                    projectileScript.Shoot(data, direction);
                }
            }
        }
    }
    private Enemy GetTarget()
    {
        // clear list at first
        while (_enemiesInRange.Count > 0)
        {
            Enemy potentialTarget = _enemiesInRange[0];

            // check target null or pooled
            if (potentialTarget == null || !potentialTarget.gameObject.activeInHierarchy)
            {
                _enemiesInRange.RemoveAt(0);
            }
            else
            {
                // select target alive
                return potentialTarget;
            }
        }
        return null;
    }

    //====TRIGGER & EVENTS
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Enemy"))
        {
            if (collision.TryGetComponent<Enemy>(out Enemy enemy))
            {
                _enemiesInRange.Add(enemy);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Enemy"))
        {
            if (collision.TryGetComponent<Enemy>(out Enemy enemy))
            {
                if (_enemiesInRange.Contains(enemy))
                {
                    _enemiesInRange.Remove(enemy);
                }
            }
        }
    }
    private void HandleEnemyDestroyed(Enemy enemy)
    {
        if (_enemiesInRange.Contains(enemy))
        {
            _enemiesInRange.Remove(enemy);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (data != null)
        {
            // color for range
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, data.Range);
        }
    }
#endif
}
