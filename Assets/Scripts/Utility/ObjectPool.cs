using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<PoolObject> where PoolObject : MonoBehaviour
{
    /* This class is a generic object pool.
     * It can be used to pool any type of object that inherits from MonoBehaviour.
     * Unfortunately, it can't be used to pool objects those are game objects without a MonoBehaviour.
     */
    private PoolObject prefab;
    private List<PoolObject> pool = new List<PoolObject>();
    private GameObject parent;

    public ObjectPool(string path, int initialPoolSize = 5)
    {
        this.prefab = Resources.Load<PoolObject>(path);
        parent = new GameObject("Object Pool " + prefab.name);
        AddObjectsToPool(initialPoolSize);
    }
    public ObjectPool(PoolObject prefab, int initialPoolSize = 5)
    {
        this.prefab = prefab.GetComponent<PoolObject>();
        parent = new GameObject("Object Pool " + prefab.name);
        AddObjectsToPool(initialPoolSize);
    }

    private void AddObjectToPool()
    {
        var newObject = GameObject.Instantiate(prefab);
        newObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        newObject.transform.localScale = Vector3.one;
        newObject.transform.SetParent(parent.transform);
        newObject.gameObject.SetActive(false);
        pool.Add(newObject);
    }

    private void AddObjectsToPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            AddObjectToPool();
        }
    }

    public PoolObject Get()
    {
        // If the pool is empty or all objects are active, add more objects to the pool
        if (pool.TrueForAll(obj => obj!= null && obj.gameObject.activeSelf)) AddObjectsToPool(200);

        PoolObject objectFromPool = null;

        // Find the first inactive object in the pool
        for (int i = 0; i < pool.Count; i++)
        {
            if(pool[i] == null) continue;
            if (!pool[i].gameObject.activeSelf)
            {
                objectFromPool = pool[i];
                break;
            }
        }

        // Set the object active and return it
        if (objectFromPool != null)
        {
            objectFromPool.gameObject.SetActive(true);
        }

        return objectFromPool;
    }

    public void Return(PoolObject objectToReturn)
    {
        objectToReturn.gameObject.SetActive(false);
        objectToReturn.transform.SetParent(parent.transform);
    }
}
