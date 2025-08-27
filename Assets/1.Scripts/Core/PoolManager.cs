using Cysharp.Threading.Tasks; // UniTask 사용
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    private readonly Dictionary<string, Queue<GameObject>> _poolDictionary = new Dictionary<string, Queue<GameObject>>();
    private readonly HashSet<GameObject> _activePooledObjects = new HashSet<GameObject>();

    // [추가] 제안해주신 '키-프리팹 템플릿' 매핑 딕셔너리
    private readonly Dictionary<string, GameObject> _prefabTemplates = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (!ServiceLocator.IsRegistered<PoolManager>())
        {
            ServiceLocator.Register<PoolManager>(this);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async UniTask Preload(string key, int count)
    {
        if (string.IsNullOrEmpty(key) || count <= 0) return;
        if (!_poolDictionary.ContainsKey(key)) _poolDictionary[key] = new Queue<GameObject>();

        var resourceManager = ServiceLocator.Get<ResourceManager>();
        GameObject prefab = await resourceManager.LoadAsync<GameObject>(key);

        if (prefab == null) return;

        _prefabTemplates[key] = prefab; // 템플릿 캐싱

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            obj.AddComponent<PooledObjectInfo>().Initialize(key);
            _poolDictionary[key].Enqueue(obj);
        }
    }

    public async UniTask<GameObject> GetAsync(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;

        if (_poolDictionary.TryGetValue(key, out var queue) && queue.Count > 0)
        {
            GameObject obj = queue.Dequeue();
            obj.SetActive(true);
            _activePooledObjects.Add(obj);
            return obj;
        }

        // 폴백 로딩: 템플릿이 캐싱되어 있으면 바로 사용, 없으면 로드 후 사용
        if (!_prefabTemplates.ContainsKey(key))
        {
            var resourceManager = ServiceLocator.Get<ResourceManager>();
            _prefabTemplates[key] = await resourceManager.LoadAsync<GameObject>(key);
        }

        if (_prefabTemplates[key] == null) return null;

        GameObject newObj = Instantiate(_prefabTemplates[key]);
        newObj.AddComponent<PooledObjectInfo>().Initialize(key);
        _activePooledObjects.Add(newObj);
        return newObj;
    }

    public void Release(GameObject instance)
    {
        if (instance == null) return;

        _activePooledObjects.Remove(instance);

        PooledObjectInfo pooledInfo = instance.GetComponent<PooledObjectInfo>();
        if (pooledInfo == null || string.IsNullOrEmpty(pooledInfo.AssetKey))
        {
            Destroy(instance);
            return;
        }

        string key = pooledInfo.AssetKey;
        if (!_poolDictionary.ContainsKey(key)) _poolDictionary[key] = new Queue<GameObject>();

        instance.SetActive(false);
        instance.transform.SetParent(transform);
        _poolDictionary[key].Enqueue(instance);
    }

    public void ClearAllActiveObjects()
    {
        foreach (var obj in _activePooledObjects.ToList())
        {
            Release(obj);
        }
        _activePooledObjects.Clear();
    }

    public void DestroyAllPooledObjects()
    {
        var resourceManager = ServiceLocator.Get<ResourceManager>();
        foreach (var obj in _activePooledObjects) { Destroy(obj); }
        _activePooledObjects.Clear();

        foreach (var queue in _poolDictionary.Values)
        {
            foreach (var obj in queue) { Destroy(obj); }
        }
        _poolDictionary.Clear();

        // 모든 템플릿 에셋의 참조 카운트도 해제
        foreach (var key in _prefabTemplates.Keys)
        {
            resourceManager.Release(key);
        }
        _prefabTemplates.Clear();
    }
}
