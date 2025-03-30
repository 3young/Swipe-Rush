using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

public class GemPoolManager : MonoBehaviour
{
    public static GemPoolManager instance;

    [Header("Pool Settings")]
    public Gem[] gemPrefabs;
    private Dictionary<Gem.GemType, Queue<Gem>> gemPools = new();
    public GameObject[] destroyEffectPrefabs;
    private Dictionary<string, Queue<GameObject>> effectPools = new();

    private void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(gameObject);

        foreach(var prefab in gemPrefabs)
        {
            if(!gemPools.ContainsKey(prefab.gemType))
            {
                gemPools[prefab.gemType] = new Queue<Gem>();
            }

            for(int i = 0; i < 10; i++)
            {
                Gem gem = Instantiate(prefab, transform);
                gem.gameObject.SetActive(false);
                gemPools[prefab.gemType].Enqueue(gem);
            }
        }

        foreach(var prefab in destroyEffectPrefabs)
        {
            string key = prefab.name;
            effectPools[key] = new Queue<GameObject>();

            for (int i = 0; i < 5; i++)
            {
                GameObject effect = Instantiate(prefab, transform);
                effect.SetActive(false);
                effectPools[prefab.name].Enqueue(effect);
            }
        }

    }

    public Gem GetGem(Gem.GemType type)
    {
        if(!gemPools.ContainsKey(type)) return null;

        var pool = gemPools[type];
        if (pool.Count == 0)
        {
            Gem prefab = gemPrefabs.First(g => g.gemType == type);
            pool.Enqueue(Instantiate(prefab, transform));
        }

        Gem gem = pool.Dequeue();
        gem.gameObject.SetActive(true);
        return gem;
    }

    public void ReturnGem(Gem gem)
    {
        gem.gameObject.SetActive(false);
        gemPools[gem.gemType].Enqueue(gem);
    }

    public GameObject GetEffect(GameObject prefab)
    {
        string key = prefab.name;
        if(!effectPools.ContainsKey(key))
        {
            effectPools[key] = new Queue<GameObject>();
        }

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

    public void ReturnEffect(GameObject effect)
    {
        string key = effect.name.Replace("(Clone)", "").Trim();
        effect.SetActive(false);
        effectPools[key].Enqueue(effect);
    }
}
