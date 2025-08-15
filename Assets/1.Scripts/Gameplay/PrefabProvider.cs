// --- 파일명: PrefabProvider.cs (신규 생성) ---
// 경로: Assets/1.Scripts/Gameplay/PrefabProvider.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gameplay 씬에서 사용되는 모든 프리팹의 마스터 데이터베이스 역할을 합니다.
/// 이 컴포넌트는 GameplaySystems 오브젝트에 존재하며, 씬 내의 다른 시스템들에게
/// 이름(string)을 기반으로 프리팹을 제공합니다.
/// </summary>
public class PrefabProvider : MonoBehaviour
{
    public static PrefabProvider Instance { get; private set; }

    [Header("게임플레이 프리팹 목록")]
    [SerializeField] private List<GameObject> monsterPrefabs;
    [SerializeField] private List<GameObject> bulletPrefabs;
    [SerializeField] private List<GameObject> vfxPrefabs;

    private readonly Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 게임 시작 시 모든 프리팹 리스트를 하나의 딕셔너리로 통합하여
        // 빠르고 쉽게 검색할 수 있도록 준비합니다.
        PopulatePrefabDict(monsterPrefabs);
        PopulatePrefabDict(bulletPrefabs);
        PopulatePrefabDict(vfxPrefabs);
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
            Debug.LogError($"[PrefabProvider] 프리팹 딕셔너리에서 '{name}'을(를) 찾을 수 없습니다. Inspector의 프리팹 목록에 등록되었는지 확인하세요.");
        }
        return prefab;
    }
}
