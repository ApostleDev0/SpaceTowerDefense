using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int poolSize = 5;

    private List<GameObject> _pool;

    // Start is called before the first frame update
    void Awake()
    {
        // create pool
        _pool = new List<GameObject>();
        for(int i = 0; i< poolSize; i++)
        {
            CreateNewObject();
        }
    }

    private GameObject CreateNewObject()
    {
        GameObject obj = Instantiate(prefab,transform);
        obj.SetActive(false);
        _pool.Add(obj);
        return obj;
    }

    public GameObject GetPooledObjected()
    {
        foreach(GameObject obj in _pool)
        {
            if(!obj.activeSelf)
            {
                return obj;
            }
        }
        return CreateNewObject();
    }
    public void ResetPool()
    {
        foreach (GameObject obj in _pool)
        {
            obj.SetActive(false);
        }
    }
}
