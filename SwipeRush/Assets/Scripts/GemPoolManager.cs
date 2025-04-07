using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 보석과 이펙트의 오브젝트 풀을 관리하는 싱글톤 클래스
/// ObjectPool<T>를 활용하여 중복 제거
/// </summary>
public class GemPoolManager : MonoBehaviour
{
    public static GemPoolManager instance;

    [Header("Pool Settings")]
    public Gem[] gemPrefabs;
    public GameObject[] destroyEffectPrefabs;

    private Dictionary<Gem.GemType, ObjectPool<Gem>> gemPools = new();
    private Dictionary<string, ObjectPool<GameObject>> effectPools = new();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        foreach (var prefab in gemPrefabs)
        {
            gemPools[prefab.gemType] = new ObjectPool<Gem>(prefab, 10, transform);
        }

        foreach (var prefab in destroyEffectPrefabs)
        {
            effectPools[prefab.name] = new ObjectPool<GameObject>(prefab, 5, transform);
        }
    }

    public Gem GetGem(Gem.GemType type)
    {
        if (!gemPools.ContainsKey(type)) return null;
        return gemPools[type].Get();
    }

    public void ReturnGem(Gem gem)
    {
        gemPools[gem.gemType].ReturnToPool(gem);
    }

    public GameObject GetEffect(GameObject prefab)
    {
        string key = prefab.name;
        if (!effectPools.ContainsKey(key))
        {
            effectPools[key] = new ObjectPool<GameObject>(prefab, 1, transform);
        }

        return effectPools[key].Get();
    }

    public void ReturnEffect(GameObject effect)
    {
        string key = effect.name.Replace("(Clone)", "").Trim();
        if (effectPools.ContainsKey(key))
        {
            effectPools[key].ReturnToPool(effect);
        }
    }
}
