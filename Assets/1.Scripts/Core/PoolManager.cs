// 파일명: PoolManager.cs (리팩토링 완료)
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();
    
    // [추가] 현재 씬에 활성화된 모든 풀링 오브젝트를 추적하기 위한 HashSet
    private readonly HashSet<GameObject> activePooledObjects = new HashSet<GameObject>();

    void Awake()
    {
        Debug.Log($"[ 진단 ] PoolManager.Awake() 호출됨. (Frame: {Time.frameCount})");
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

        // [추가] 오브젝트를 꺼내갈 때, 활성 목록에 등록합니다.
        activePooledObjects.Add(obj);

        return obj;
    }

    public void Release(GameObject instance)
    {
        if (instance == null) return;
        
        Debug.Log($"[PoolManager] Release 요청: {instance.name} (ID: {instance.GetInstanceID()})");

        // [추가] 오브젝트를 반납할 때, 활성 목록에서 제거합니다.
        activePooledObjects.Remove(instance);

        PooledObjectInfo pooledInfo = instance.GetComponent<PooledObjectInfo>();
        if (pooledInfo == null || pooledInfo.originalPrefab == null)
        {
            Debug.LogWarning($"[PoolManager] Release 실패: {instance.name} (ID: {instance.GetInstanceID()}) PooledObjectInfo 없음. 즉시 파괴.");
            Destroy(instance);
            return;
        }

        GameObject originalPrefab = pooledInfo.originalPrefab;

        if (!poolDictionary.ContainsKey(originalPrefab))
        {
            poolDictionary[originalPrefab] = new Queue<GameObject>();
        }

        instance.SetActive(false);
        Debug.Log($"[PoolManager] {instance.name} (ID: {instance.GetInstanceID()}) 비활성화 완료.");
        instance.transform.SetParent(transform);
        poolDictionary[originalPrefab].Enqueue(instance);
    }

    // [추가] 활성화된 모든 풀링 오브젝트를 정리하는 새로운 함수
    public void ClearAllActiveObjects()
    {
        Debug.Log($"[PoolManager] 활성화된 모든 풀 오브젝트 ({activePooledObjects.Count}개)를 정리합니다.");
        
        // HashSet을 직접 순회하면서 요소를 제거하면 오류가 발생하므로, 리스트로 복사한 뒤 순회합니다.
        foreach (var obj in activePooledObjects.ToList())
        {
            Release(obj);
        }
        
        // 모든 객체가 Release를 통해 개별적으로 제거되지만, 만약을 위해 마지막에 Clear를 호출합니다.
        activePooledObjects.Clear();
    }

    /// <summary>
    /// 모든 풀링된 오브젝트(활성 및 비활성)를 즉시 파괴하고 풀을 초기화합니다.
    /// 씬 전환 등 풀의 모든 오브젝트를 강제로 정리해야 할 때 사용합니다.
    /// </summary>
    public void DestroyAllPooledObjects()
    {
        Debug.Log($"[PoolManager] 모든 풀링된 오브젝트를 파괴합니다. (활성: {activePooledObjects.Count}개, 비활성 풀: {poolDictionary.Sum(kv => kv.Value.Count)}개)");

        // 활성 오브젝트 먼저 파괴
        foreach (var obj in activePooledObjects.ToList()) // ToList()로 복사하여 순회 중 수정 가능하게 함
        {
            Destroy(obj);
        }
        activePooledObjects.Clear();

        // 비활성 풀 오브젝트 파괴
        foreach (var kvp in poolDictionary)
        {
            foreach (var obj in kvp.Value)
            {
                Destroy(obj);
            }
        }
        poolDictionary.Clear();
        Debug.Log("[PoolManager] 모든 풀링된 오브젝트 파괴 완료.");
    }
}