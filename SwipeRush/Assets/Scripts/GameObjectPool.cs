using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool
{
    private readonly GameObject prefab;
    private readonly Queue<GameObject> pool = new Queue<GameObject>();
    private readonly Transform parent;

    public GameObjectPool(GameObject prefab, int initialSize, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Object.Instantiate(prefab, parent);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject Get()
    {
        if (pool.Count == 0)
        {
            AddObject();
        }
        GameObject obj = pool.Dequeue();
        obj.SetActive(true);
        return obj;
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }

    private void AddObject()
    {
        GameObject obj = Object.Instantiate(prefab, parent);
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
