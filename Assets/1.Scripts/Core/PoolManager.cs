using Cysharp.Threading.Tasks; // UniTask 사용
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PoolManager : MonoBehaviour
{
    private readonly Dictionary<string, Queue<GameObject>> _poolDictionary = new Dictionary<string, Queue<GameObject>>();
    private readonly HashSet<GameObject> _activePooledObjects = new HashSet<GameObject>();
    private readonly Dictionary<string, GameObject> _prefabTemplates = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, SemaphoreSlim> _loadingSemaphores = new Dictionary<string, SemaphoreSlim>();

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        var gameManager = ServiceLocator.Get<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnBeforeStateChange += HandleGameStateChange;
        }
        else
        {
            // Start에서도 GameManager를 찾지 못한다면 더 큰 문제이므로 에러 로그를 남깁니다.
            Debug.LogError("[PoolManager] GameManager를 찾을 수 없어 상태 변경 이벤트를 구독할 수 없습니다.");
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (ServiceLocator.IsRegistered<GameManager>())
        {
            ServiceLocator.Get<GameManager>().OnBeforeStateChange -= HandleGameStateChange;
        }
    }

    private void HandleGameStateChange(GameManager.GameState from, GameManager.GameState to)
    {
        // 이전 상태가 Gameplay였고, 다음 상태가 Gameplay가 아니라면 모든 풀을 정리합니다.
        if (from == GameManager.GameState.Gameplay && to != GameManager.GameState.Gameplay)
        {
            Debug.Log($"[PoolManager] 게임 플레이 상태({from})를 벗어나므로 모든 풀 오브젝트를 정리합니다.");
            ClearAndDestroyEntirePool();
        }
    }

    // 새 씬이 로드될 때 풀을 정리하여 MissingReferenceException을 방지합니다.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }

    public async UniTask<GameObject> GetAsync(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;

        if (!_loadingSemaphores.ContainsKey(key))
        {
            _loadingSemaphores[key] = new SemaphoreSlim(1, 1);
        }
        await _loadingSemaphores[key].WaitAsync();

        try
        {
            if (_poolDictionary.TryGetValue(key, out var queue) && queue.Count > 0)
            {
                GameObject obj = queue.Dequeue();

                if (obj == null)
                {
                    // null이면 새 오브젝트 생성 로직으로 넘어감
                }
                else
                {
                    obj.SetActive(true);
                    _activePooledObjects.Add(obj);
                    return obj;
                }
            }

            if (!_prefabTemplates.ContainsKey(key) || _prefabTemplates[key] == null)
            {
                var resourceManager = ServiceLocator.Get<ResourceManager>();
                _prefabTemplates[key] = await resourceManager.LoadAsync<GameObject>(key);
            }

            if (_prefabTemplates[key] == null)
            {
                Debug.LogError($"[PoolManager] 키 '{key}'에 해당하는 프리팹을 로드할 수 없습니다.");
                return null;
            }

            GameObject newObj = Instantiate(_prefabTemplates[key]);
            newObj.AddComponent<PooledObjectInfo>().Initialize(key);
            _activePooledObjects.Add(newObj);
            return newObj;
        }
        finally
        {
            _loadingSemaphores[key].Release();
        }
    }

    public async UniTask Preload(string key, int count)
    {
        if (string.IsNullOrEmpty(key) || count <= 0) return;
        if (!_poolDictionary.ContainsKey(key)) _poolDictionary[key] = new Queue<GameObject>();

        var resourceManager = ServiceLocator.Get<ResourceManager>();
        GameObject prefab = await resourceManager.LoadAsync<GameObject>(key);

        if (prefab == null) return;

        _prefabTemplates[key] = prefab;

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            obj.AddComponent<PooledObjectInfo>().Initialize(key);
            _poolDictionary[key].Enqueue(obj);
        }
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
    public void ReturnAllActiveObjectsToPool()
    {
        foreach (var obj in _activePooledObjects.ToList())
        {
            Release(obj);
        }
        _activePooledObjects.Clear();
    }

    public void ClearAndDestroyEntirePool()
    {
        var resourceManager = ServiceLocator.Get<ResourceManager>();
        foreach (var obj in _activePooledObjects) { if(obj != null) Destroy(obj); }
        _activePooledObjects.Clear();

        foreach (var queue in _poolDictionary.Values)
        {
            foreach (var obj in queue) { if(obj != null) Destroy(obj); }
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

