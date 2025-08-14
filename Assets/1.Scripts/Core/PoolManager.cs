// --- 파일명: PoolManager.cs ---

using System.Collections.Generic;
using UnityEngine;
// [수정] 불필요한 using 구문 제거
// using static UnityEngine.RuleTile.TilingRuleOutput; 

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Preload(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0) return;

        if (!poolDictionary.ContainsKey(prefab))
        {
            poolDictionary[prefab] = new Queue<GameObject>();
        }

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            PooledObjectInfo pooledInfo = obj.AddComponent<PooledObjectInfo>();
            pooledInfo.Initialize(prefab);
            poolDictionary[prefab].Enqueue(obj);
        }
        Debug.Log($"[PoolManager] {prefab.name}을(를) {count}개 미리 생성했습니다.");
    }

    public GameObject Get(GameObject prefab)
    {
        if (prefab == null) return null;

        if (!poolDictionary.ContainsKey(prefab))
        {
            poolDictionary[prefab] = new Queue<GameObject>();
        }

        Queue<GameObject> objectQueue = poolDictionary[prefab];

        if (objectQueue.Count > 0)
        {
            GameObject obj = objectQueue.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            GameObject newObj = Instantiate(prefab);
            PooledObjectInfo pooledInfo = newObj.AddComponent<PooledObjectInfo>();
            pooledInfo.Initialize(prefab);
            Debug.LogWarning($"[PoolManager] {prefab.name} 풀이 비어있어 새로 생성합니다. Preload 수량을 늘리는 것을 고려해보세요.");
            return newObj;
        }
    }

    public void Release(GameObject instance)
    {
        if (instance == null) return;

        PooledObjectInfo pooledInfo = instance.GetComponent<PooledObjectInfo>();
        if (pooledInfo == null || pooledInfo.originalPrefab == null)
        {
            Debug.LogError($"[PoolManager] {instance.name}에 PooledObjectInfo가 없거나 originalPrefab이 설정되지 않았습니다. 풀에 반환할 수 없습니다.");
            Destroy(instance);
            return;
        }

        GameObject originalPrefab = pooledInfo.originalPrefab;

        if (!poolDictionary.ContainsKey(originalPrefab))
        {
            poolDictionary[originalPrefab] = new Queue<GameObject>();
        }

        instance.SetActive(false);
        instance.transform.SetParent(transform);
        poolDictionary[originalPrefab].Enqueue(instance);
    }
}