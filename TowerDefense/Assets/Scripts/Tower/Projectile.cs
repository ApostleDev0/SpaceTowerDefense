using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private TowerData _data;
    private Vector3 _shootDirection;
    private float _projectileDuration;

    void Update()
    {
        if (_data == null)
        {
            return;
        }
        if (_projectileDuration <= 0)
        {
            gameObject.SetActive(false);
        }
        else
        {
            _projectileDuration -= Time.deltaTime;
            transform.position += _shootDirection * _data.ProjectileSpeed * Time.deltaTime;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (collision.TryGetComponent<Enemy>(out Enemy enemy))
            {
                enemy.TakeDamage(_data.Damage);
                gameObject.SetActive(false);
            }
        }
    }
    public void Shoot(TowerData data, Vector3 shootDirection)
    {
        _data = data;
        _shootDirection = shootDirection;
        _projectileDuration = _data.ProjectileDuration;

        transform.localScale = Vector3.one * _data.ProjectileSize;

        float angle = Mathf.Atan2(_shootDirection.y, _shootDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}
