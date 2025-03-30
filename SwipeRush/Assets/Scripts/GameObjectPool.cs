using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 오브젝트 풀링을 구현한 클래스
/// 오브젝트의 빈번한 생성과 파괴를 방지하여 성능 최적화
/// </summary>
public class GameObjectPool
{
    private readonly GameObject prefab;    // 풀링할 게임 오브젝트 프리팹
    private readonly Queue<GameObject> pool = new Queue<GameObject>(); // 풀
    private readonly Transform parent;     // 오브젝트들의 부모 Transform

    /// <summary>
    /// 오브젝트 풀을 초기화
    /// </summary>
    /// <param name="prefab">풀링할 게임 오브젝트 프리팹</param>
    /// <param name="initialSize">초기 풀 크기</param>
    /// <param name="parent">오브젝트들의 부모 Transform (선택적)</param>
    public GameObjectPool(GameObject prefab, int initialSize, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;
        
        // 초기 오브젝트 생성
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Object.Instantiate(prefab, parent);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    /// <summary>
    /// 풀에서 오브젝트를 가져옴
    /// </summary>
    /// <returns>활성화된 게임 오브젝트</returns>
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

    /// <summary>
    /// 사용이 끝난 오브젝트를 풀로 반환
    /// </summary>
    /// <param name="obj">반환할 게임 오브젝트</param>
    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }

    /// <summary>
    /// 풀에 새 오브젝트를 추가
    /// </summary>
    private void AddObject()
    {
        GameObject obj = Object.Instantiate(prefab, parent);
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
