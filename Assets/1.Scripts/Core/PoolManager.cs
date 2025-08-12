using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

     /// <summary>
      /// 게임 오브젝트 풀링을 관리하는 고성능 싱글톤 클래스입니다.
      /// 총알, 몬스터, 이펙트 등 반복적으로 생성되고 파괴되는 오브젝트를 재사용하여
      /// 게임의 성능 저하(가비지 컬렉션 등)를 방지하는 핵심적인 역할을 수행합니다.
      /// </summary>
      public class PoolManager : MonoBehaviour
     {
         public static PoolManager Instance { get; private set; }
    
         // 프리팹을 키로, 해당 프리팹으로 생성된 오브젝트들의 큐를 값으로 가지는 딕셔너리입니다.
         // 이를 통해 여러 종류의 오브젝트 풀을 동시에 관리할 수 있습니다.
         private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject,
       Queue<GameObject>>();
    
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
    
         /// <summary>
         /// 게임 시작 전에 지정된 프리팹을 원하는 수량만큼 미리 생성하여 풀에 준비시킵니다.
         /// 이를 통해 게임 플레이 중의 첫 생성 비용을 없앨 수 있습니다.
         /// </summary>
         /// <param name="prefab">미리 생성할 오브젝트의 프리팹</param>
         /// <param name="count">생성할 수량</param>
         public void Preload(GameObject prefab, int count)
         {
             if (prefab == null || count <= 0) return;
    
             // 해당 프리팹에 대한 풀이 없으면 새로 생성합니다.
             if (!poolDictionary.ContainsKey(prefab))
                 {
                     poolDictionary[prefab] = new Queue<GameObject>();
                 }
    
             for (int i = 0; i < count; i++)
                 {
                     GameObject obj = Instantiate(prefab, transform); // 풀 매니저 하위에 생성하여 관리
                     obj.SetActive(false); // 비활성화 상태로 준비
                     // PooledObjectInfo 컴포넌트를 추가하고 원본 프리팹을 설정합니다.
                 PooledObjectInfo pooledInfo = obj.AddComponent<PooledObjectInfo>();
                     pooledInfo.Initialize(prefab);
                     poolDictionary[prefab].Enqueue(obj);
                 }
             Debug.Log($"[PoolManager] {prefab.name}을(를) {count}개 미리 생성했습니다.");
         }

         /// <summary>
         /// 풀에서 사용 가능한 오브젝트를 가져옵니다. 만약 풀에 오브젝트가 없다면 새로 생성합니다.
         /// </summary>
         /// <param name="prefab">가져올 오브젝트의 프리팹</param>
         /// <returns>활성화된 게임 오브젝트</returns>
         public GameObject Get(GameObject prefab)
         {
             if (prefab == null) return null;
    
             // 해당 프리팹에 대한 풀이 없으면 새로 생성합니다.
             if (!poolDictionary.ContainsKey(prefab))
                 {
                     poolDictionary[prefab] = new Queue<GameObject>();
                 }
    
             Queue<GameObject> objectQueue = poolDictionary[prefab];
    
             // 풀에 사용 가능한 오브젝트가 있으면 그것을 사용합니다.
             if (objectQueue.Count > 0)
                 {
                     GameObject obj = objectQueue.Dequeue();
                     obj.SetActive(true);
                     return obj;
                 }
             // 풀에 오브젝트가 없다면 새로 생성하여 반환합니다.
             else
                 {
                     GameObject newObj = Instantiate(prefab);
                     // 새로 생성된 오브젝트에도 PooledObjectInfo 컴포넌트를 추가하고 원본 프리팹을 설정합니다.
                 PooledObjectInfo pooledInfo = newObj.AddComponent<PooledObjectInfo>();
                     pooledInfo.Initialize(prefab);
                     Debug.LogWarning($"[PoolManager] {prefab.name} 풀이 비어있어 새로 생성합니다. Preload 수량을 늘리는 것을 고려해보세요.");
    
                     return newObj;
                 }
         }

         /// <summary>
         /// 사용이 끝난 오브젝트를 비활성화하고 풀에 반환합니다.
         /// </summary>
         /// <param name="instance">반환할 오브젝트 인스턴스</param>
         public void Release(GameObject instance)
         {
             if (instance == null) return;
    
            // PooledObjectInfo 컴포넌트를 통해 원본 프리팹을 가져옵니다.
            PooledObjectInfo pooledInfo = instance.GetComponent<PooledObjectInfo>();
            if (pooledInfo == null || pooledInfo.originalPrefab == null)
                 {
                     Debug.LogError($"[PoolManager] {instance.name}에 PooledObjectInfo가 없거나 originalPrefab이 설정되지 않았습니다.풀에 반환할 수 없습니다.");
     
                     Destroy(instance); // 풀링할 수 없으므로 파괴합니다.
                     return;
                 }
    
            GameObject originalPrefab = pooledInfo.originalPrefab;
    
            // 해당 프리팹에 대한 풀이 없다면, 풀을 생성하고 오브젝트를 추가합니다.
            if (!poolDictionary.ContainsKey(originalPrefab))
                 {
                     poolDictionary[originalPrefab] = new Queue<GameObject>();
                 }
    
            instance.SetActive(false);
             instance.transform.SetParent(transform); // 다시 풀 매니저 하위로 이동
             poolDictionary[originalPrefab].Enqueue(instance);
        }
 }