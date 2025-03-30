using UnityEngine;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

/// <summary>
/// 제네릭 타입을 지원하는 오브젝트 풀 클래스
/// Component를 상속하는 모든 타입에 사용 가능
/// </summary>
/// <typeparam name="T">풀링할 컴포넌트 타입</typeparam>
public class ObjectPool<T> where T : Component
{
    private readonly T prefab;             // 풀링할 프리팹
    private readonly Queue<T> pool = new Queue<T>(); // 풀
    private readonly Transform parent;     // 오브젝트들의 부모 Transform

    /// <summary>
    /// 오브젝트 풀 초기화
    /// </summary>
    /// <param name="prefab">풀링할 프리팹</param>
    /// <param name="initialSize">초기 풀 크기</param>
    /// <param name="parent">오브젝트들의 부모 Transform (선택적)</param>
    public ObjectPool(T prefab, int initialSize, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;

        for(int i = 0; i < initialSize; i++)
        {
            AddObject();
        }
    }

    /// <summary>
    /// 풀에서 오브젝트 가져옴
    /// </summary>
    /// <returns>활성화된 컴포넌트</returns>
    public T Get()
    {
        if(pool.Count == 0)
        {
            AddObject();
        }

        T obj = pool.Dequeue();
        obj.gameObject.SetActive(true);
        return obj;
    }

    /// <summary>
    /// 사용이 끝난 오브젝트를 풀로 반환
    /// </summary>
    /// <param name="obj">반환할 컴포넌트</param>
    public void ReturnToPool(T obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }

    /// <summary>
    /// 풀에 새 오브젝트 추가
    /// </summary>
    private void AddObject()
    {
        T obj = Object.Instantiate(prefab, parent);
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}
