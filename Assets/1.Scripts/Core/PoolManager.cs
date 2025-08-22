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
        // [디버그] PoolManager가 깨어났음을 알립니다.
        Debug.Log($"[ 진단 ] PoolManager.Awake() 호출됨. (Frame: {Time.frameCount})");

        // ▼▼▼ 기존의 싱글톤 코드를 이 한 줄로 대체합니다. ▼▼▼
        ServiceLocator.Register<PoolManager>(this);

        // 씬이 바뀌어도 파괴되지 않도록 설정합니다.
        DontDestroyOnLoad(gameObject);
    }

    public void Preload(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0)
        {
            Debug.LogWarning("[PoolManager] Preload 실패: 프리팹이 null이거나 수량이 0 이하입니다.");
            return;
        }

        Debug.Log($"[ 진단-Preload ] 프리팹 '{prefab.name}' (ID: {prefab.GetInstanceID()}) {count}개 미리 생성 요청됨.");

        if (!poolDictionary.ContainsKey(prefab))
        {
            poolDictionary[prefab] = new Queue<GameObject>();
        }

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            // PooledObjectInfo가 이미 프리팹에 붙어있을 수 있으므로, 없으면 추가
            if (!obj.TryGetComponent<PooledObjectInfo>(out var pooledInfo))
            {
                pooledInfo = obj.AddComponent<PooledObjectInfo>();
            }
            pooledInfo.Initialize(prefab);
            poolDictionary[prefab].Enqueue(obj);
        }
    }

    public GameObject Get(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("[PoolManager] Get 실패: 요청한 프리팹이 null입니다.");
            return null;
        }

        if (!poolDictionary.ContainsKey(prefab) || poolDictionary[prefab].Count == 0)
        {
            Debug.LogWarning($"[PoolManager] {prefab.name} 풀이 비어있어 새로 생성합니다. Preload가 정상적으로 작동했는지 확인해보세요.");
            GameObject newObj = Instantiate(prefab);
            if (!newObj.TryGetComponent<PooledObjectInfo>(out var pooledInfo))
            {
                pooledInfo = newObj.AddComponent<PooledObjectInfo>();
            }
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
            // 풀링된 오브젝트가 아닐 경우 그냥 파괴
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