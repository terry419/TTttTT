// --- 파일명: PoolManager.cs (최종 수정본) ---
// 경로: Assets/1.Scripts/Core/PoolManager.cs
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();

    void Awake()
    {
        Debug.Log($"[ 진단 1단계 ] PoolManager.Awake() 호출됨. (Frame: {Time.frameCount})");

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// DataManager로부터 Preload 요청을 받아 처리합니다.
    /// </summary>
    
















    public void Preload(GameObject prefab, int count)
    {
        Debug.Log($"[ 진단 3단계-Preload ] 프리팹 '{prefab.name}' (ID: {prefab.GetInstanceID()}) {count}개 미리 생성 요청됨.");

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
        Debug.Log($"[ 진단 3단계-Get ] 프리팹 '{prefab.name}' (ID: {prefab.GetInstanceID()}) 요청됨.");

        if (prefab == null) return null;

        if (!poolDictionary.ContainsKey(prefab) || poolDictionary[prefab].Count == 0)
        {
            // 풀이 비어있으면 새로 생성 (경고 메시지는 유지)
            Debug.LogWarning($"[PoolManager] {prefab.name} 풀이 비어있어 새로 생성합니다. Preload 수량을 늘리는 것을 고려해보세요.");
            GameObject newObj = Instantiate(prefab);
            PooledObjectInfo pooledInfo = newObj.AddComponent<PooledObjectInfo>();
            pooledInfo.Initialize(prefab);
            return newObj;
        }

        GameObject obj = poolDictionary[prefab].Dequeue();
        obj.SetActive(true);
        return obj;
    }

    public void Release(GameObject instance)
    {
        if (instance == null) return;

        PooledObjectInfo pooledInfo = instance.GetComponent<PooledObjectInfo>();
        if (pooledInfo == null || pooledInfo.originalPrefab == null)
        {
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
