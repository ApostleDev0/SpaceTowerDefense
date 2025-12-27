using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [SerializeField] private TowerData data;
    [SerializeField] private GameObject rangeIndicator;

    private CircleCollider2D _circleCollider;
    private List<Enemy> _enemiesInRange;
    private ObjectPooler _projectilePool;
    private float _shootTimer;

    public TowerData GetData()
    {
        return data;
    }
    public void ToggleRange(bool status)
    {
        if(rangeIndicator != null)
        {
            rangeIndicator.SetActive(status);
            if(status)
            {
                // reset position of range circle 
                rangeIndicator.transform.localPosition = Vector3.zero;
                float diameter = data.range * 2f;
                rangeIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);
            }
        }
    }
    private void Awake()
    {
        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(false);
        }
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
        _circleCollider.radius = data.range;
        _enemiesInRange = new List<Enemy>();
        _projectilePool = GetComponent<ObjectPooler>();
        _shootTimer = data.shootInterval;
    }
    private void Update()
    {
        _shootTimer -= Time.deltaTime;
        if( _shootTimer <= 0 )
        {
            _shootTimer = data.shootInterval;
            Shoot();
        }
    }
    private void OnDrawGizmos()
    {
        if(data != null)
        {
            Gizmos.DrawWireSphere(transform.position,data.range);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            _enemiesInRange.Add(enemy);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if(_enemiesInRange.Contains(enemy))
            {
                _enemiesInRange.Remove(enemy);
            }
        }
    }
    private void Shoot()
    {
        _enemiesInRange.RemoveAll(enemy => enemy == null || !enemy.gameObject.activeInHierarchy);

        if(_enemiesInRange.Count > 0)
        {
            GameObject projectile = _projectilePool.GetPooledObjected();
            if(projectile != null )
            {
                projectile.transform.position = transform.position;
                projectile.SetActive(true);
                Vector2 _shootDirection = (_enemiesInRange[0].transform.position - transform.position).normalized;
                projectile.GetComponent<Projectile>().Shoot(data, _shootDirection);
            }
        }
    }
    private void HandleEnemyDestroyed(Enemy enemy)
    {
        _enemiesInRange.Remove(enemy);
    }
}
