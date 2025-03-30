using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 보석과 이펙트의 오브젝트 풀을 관리하는 싱글톤 클래스
/// 게임 내 모든 보석과 이펙트의 생성 및 재사용을 처리
/// </summary>
public class GemPoolManager : MonoBehaviour
{
    /// <summary>싱글톤 인스턴스</summary>
    public static GemPoolManager instance;

    [Header("Pool Settings")]
    /// <summary>보석 프리팹 배열</summary>
    public Gem[] gemPrefabs;
    private Dictionary<Gem.GemType, Queue<Gem>> gemPools = new();
    
    /// <summary>파괴 이펙트 프리팹 배열</summary>
    public GameObject[] destroyEffectPrefabs;
    private Dictionary<string, Queue<GameObject>> effectPools = new();

    /// <summary>
    /// 싱글톤 인스턴스를 초기화하고 풀을 생성
    /// </summary>
    private void Awake()
    {
        // 싱글톤 패턴 적용
        if(instance == null) instance = this;
        else Destroy(gameObject);

        // 보석 풀 초기화
        foreach(var prefab in gemPrefabs)
        {
            if(!gemPools.ContainsKey(prefab.gemType))
            {
                gemPools[prefab.gemType] = new Queue<Gem>();
            }

            // 초기 풀 사이즈만큼 보석 생성
            for(int i = 0; i < 10; i++)
            {
                Gem gem = Instantiate(prefab, transform);
                gem.gameObject.SetActive(false);
                gemPools[prefab.gemType].Enqueue(gem);
            }
        }

        // 이펙트 풀 초기화
        foreach(var prefab in destroyEffectPrefabs)
        {
            string key = prefab.name;
            effectPools[key] = new Queue<GameObject>();

            // 초기 풀 사이즈만큼 이펙트 생성
            for (int i = 0; i < 5; i++)
            {
                GameObject effect = Instantiate(prefab, transform);
                effect.SetActive(false);
                effectPools[prefab.name].Enqueue(effect);
            }
        }
    }

    /// <summary>
    /// 지정된 타입의 보석을 풀에서 가져옴
    /// </summary>
    /// <param name="type">보석 타입</param>
    /// <returns>활성화된 보석</returns>
    public Gem GetGem(Gem.GemType type)
    {
        if(!gemPools.ContainsKey(type)) return null;

        var pool = gemPools[type];
        if (pool.Count == 0)
        {
            // 풀이 비어있으면 새 보석 생성
            Gem prefab = gemPrefabs.First(g => g.gemType == type);
            pool.Enqueue(Instantiate(prefab, transform));
        }

        Gem gem = pool.Dequeue();
        gem.gameObject.SetActive(true);
        return gem;
    }

    /// <summary>
    /// 사용이 끝난 보석을 풀로 반환
    /// </summary>
    /// <param name="gem">반환할 보석</param>
    public void ReturnGem(Gem gem)
    {
        gem.gameObject.SetActive(false);
        gemPools[gem.gemType].Enqueue(gem);
    }

    /// <summary>
    /// 지정된 이펙트를 풀에서 가져옴
    /// </summary>
    /// <param name="prefab">이펙트 프리팹</param>
    /// <returns>활성화된 이펙트</returns>
    public GameObject GetEffect(GameObject prefab)
    {
        string key = prefab.name;
        if(!effectPools.ContainsKey(key))
        {
            effectPools[key] = new Queue<GameObject>();
        }

        // 풀이 비어있으면 새 이펙트 생성
        if(effectPools[key].Count == 0)
        {
            GameObject newEffect = Instantiate(prefab, transform);
            newEffect.SetActive(false);
            effectPools[key].Enqueue(newEffect);
        }

        GameObject effect = effectPools[key].Dequeue();
        effect.SetActive(true);
        return effect;
    }

    /// <summary>
    /// 사용이 끝난 이펙트를 풀로 반환
    /// </summary>
    /// <param name="effect">반환할 이펙트</param>
    public void ReturnEffect(GameObject effect)
    {
        string key = effect.name.Replace("(Clone)", "").Trim();
        effect.SetActive(false);
        effectPools[key].Enqueue(effect);
    }
}
