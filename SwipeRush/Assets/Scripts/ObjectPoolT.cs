using UnityEngine;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

public class ObjectPool<T> where T : Component
{
    private readonly T prefab;
    private readonly Queue<T> pool = new Queue<T>();
    private readonly Transform parent;

    public ObjectPool(T prefab, int initialSize, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;

        for(int i = 0; i < initialSize; i++)
        {
            AddObject();
        }
    }

    public T Get()
    {
        if(pool.Count ==0)
        {
            AddObject();
        }

        T obj = pool.Dequeue();
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void ReturnToPool(T obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }

    private void AddObject()
    {
        T obj = Object.Instantiate(prefab, parent);
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}
