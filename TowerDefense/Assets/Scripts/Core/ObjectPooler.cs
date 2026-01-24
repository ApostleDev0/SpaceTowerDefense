using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] private GameObject prefab;
    [SerializeField] private int poolSize = 10;
    [SerializeField] private EnemyType poolType;
    #endregion

    #region Private Fields
    private List<GameObject> _pooledObjects;
    #endregion

    #region Public Properties
    public EnemyType PoolType => poolType;
    #endregion

    private void Awake()
    {
        _pooledObjects = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            _pooledObjects.Add(obj);
        }
    }

    public GameObject GetInstance()
    {
        foreach (GameObject obj in _pooledObjects)
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }

        GameObject newObj = Instantiate(prefab);
        newObj.SetActive(false);
        newObj.transform.SetParent(transform);
        _pooledObjects.Add(newObj);
        return newObj;
    }

    public void ResetPool()
    {
        foreach (GameObject obj in _pooledObjects)
        {
            if (obj.activeInHierarchy)
            {
                obj.SetActive(false);
            }
        }
    }
}