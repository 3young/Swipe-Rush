using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 제네릭 타입을 지원하는 오브젝트 풀 클래스
/// UnityEngine.Object를 상속하는 모든 타입에 사용 가능
/// </summary>
/// <typeparam name="T">풀링할 오브젝트 타입</typeparam>
public class ObjectPool<T> where T : UnityEngine.Object
{
    private readonly T prefab;
    private readonly Queue<T> pool = new Queue<T>();
    private readonly Transform parent;

    public ObjectPool(T prefab, int initialSize, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;

        for (int i = 0; i < initialSize; i++)
        {
            AddObject();
        }
    }

    public T Get()
    {
        if (pool.Count == 0)
        {
            AddObject();
        }

        T obj = pool.Dequeue();
        if (obj is GameObject go)
        {
            go.SetActive(true);
        }
        else if (obj is Component comp)
        {
            comp.gameObject.SetActive(true);
        }
        return obj;
    }

    public void ReturnToPool(T obj)
    {
        if (obj is GameObject go)
        {
            go.SetActive(false);
        }
        else if (obj is Component comp)
        {
            comp.gameObject.SetActive(false);
        }
        pool.Enqueue(obj);
    }

    private void AddObject()
    {
        T obj = Object.Instantiate(prefab, parent);

        if (obj is GameObject go)
        {
            go.SetActive(false);
        }
        else if (obj is Component comp)
        {
            comp.gameObject.SetActive(false);
        }
        pool.Enqueue(obj);
    }
}
