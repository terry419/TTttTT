// --- 파일명: PrefabProvider.cs (수정됨) ---
// 경로: Assets/1.Scripts/Gameplay/PrefabProvider.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gameplay에 필요한 모든 프리팹의 데이터베이스 역할을 합니다.
/// 이 컴포넌트를 GameplaySystems 오브젝트에 추가하면, 씬 내 다른 시스템들이
/// 이름(string)으로 원하는 프리팹을 얻을 수 있습니다.
/// </summary>
public class PrefabProvider : MonoBehaviour
{
    // public static PrefabProvider Instance { get; private set; } // ServiceLocator 사용으로 대체

    [Header("프리팹 목록")]
    [SerializeField] private List<GameObject> monsterPrefabs;
    [SerializeField] private List<GameObject> bulletPrefabs;
    [SerializeField] private List<GameObject> vfxPrefabs;

    private readonly Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();

    void Awake()
    {
        Debug.Log($"[생명주기] {GetType().Name} (ID: {gameObject.GetInstanceID()}) - Awake() 시작. (프레임: {Time.frameCount})");
        // --- [ServiceLocator] PrefabProvider 등록 ---
        // 이 매니저의 인스턴스를 ServiceLocator에 등록하여, 다른 곳에서 쉽게 찾아 쓸 수 있도록 합니다.
        Debug.Log($"[{GetType().Name}] ServiceLocator에 등록을 시도합니다.");
        ServiceLocator.Register<PrefabProvider>(this);
        DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 파괴되지 않도록 설정
        Debug.Log($"[{GetType().Name}] ServiceLocator 등록 및 DontDestroyOnLoad 설정 완료.");

        // 모든 프리팹 리스트를 하나의 딕셔너리에 통합하여
        // 이름으로 빠르게 검색할 수 있도록 준비합니다.
        Debug.Log($"[{GetType().Name}] 프리팹 딕셔너리 초기화를 시작합니다.");
        PopulatePrefabDict(monsterPrefabs);
        PopulatePrefabDict(bulletPrefabs);
        PopulatePrefabDict(vfxPrefabs);
        Debug.Log($"[{GetType().Name}] 프리팹 딕셔너리 초기화 완료. 총 {prefabDictionary.Count}개의 프리팹이 등록되었습니다.");
    }

    private void PopulatePrefabDict(List<GameObject> prefabList)
    {
        if (prefabList == null) return;
        foreach (var prefab in prefabList)
        {
            if (prefab != null && !prefabDictionary.ContainsKey(prefab.name))
            {
                prefabDictionary.Add(prefab.name, prefab);
            }
        }
    }

    /// <summary>
    /// 이름으로 프리팹을 찾아 반환합니다.
    /// </summary>
    /// <param name="name">찾을 프리팹의 이름</param>
    /// <returns>찾은 프리팹. 없으면 null을 반환합니다.</returns>
    public GameObject GetPrefab(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;

        prefabDictionary.TryGetValue(name, out GameObject prefab);
        if (prefab == null)
        {
            // 로그 메시지의 깨진 문자 수정
            Debug.LogError($"[PrefabProvider] 프리팹 딕셔너리에서 '{name}'을(를) 찾을 수 없습니다. Inspector의 프리팹 목록에 등록되었는지 확인하세요.");
        }
        return prefab;
    }

    private void OnDestroy()
    {
        Debug.Log($"[생명주기] {GetType().Name} (ID: {gameObject.GetInstanceID()}) - OnDestroy() 시작. (프레임: {Time.frameCount})");
    }
}