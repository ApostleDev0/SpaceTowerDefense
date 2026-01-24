using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] private GameObject prefab;
    [SerializeField] private int initialPoolSize = 5;
    #endregion

    #region Private Fields
    private List<GameObject> _pool;
    #endregion

    // Start is called before the first frame update
    void Awake()
    {
        // check prefab
        if(prefab == null)
        {
            Debug.LogError($"ObjectPooler on {gameObject.name} is missing Prefab!");
            return;
        }

        // create pool
        _pool = new List<GameObject>();
        for(int i = 0; i< initialPoolSize; i++)
        {
            CreateNewInstance();
        }
    }

    //====PUBLIC
    public GameObject GetInstance()
    {
        // check inactive object
        foreach(GameObject obj in _pool)
        {
            if(!obj.activeSelf)
            {
                return obj;
            }
        }
        // add new object if full
        return CreateNewInstance();
    }
    public void ResetPool()
    {
        foreach (GameObject obj in _pool)
        {
            obj.SetActive(false);
        }
    }

    //====PRIVATE
    private GameObject CreateNewInstance()
    {
        GameObject obj = Instantiate(prefab,transform);
        obj.SetActive(false);
        _pool.Add(obj);
        return obj;
    }
}
