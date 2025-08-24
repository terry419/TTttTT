// PrefabProvider.cs (최종 수정안)

using UnityEngine;
using System.Collections.Generic;

// ▼▼▼ [1] 프리팹과 개수를 묶는 클래스를 여기에 추가합니다. ▼▼▼
[System.Serializable]
public class PreloadItem
{
    public GameObject prefab;
    [Tooltip("미리 생성해 둘 개수입니다.")]
    public int count;
}

public class PrefabProvider : MonoBehaviour
{
    // ▼▼▼ [2] 리스트의 타입을 List<GameObject>에서 List<PreloadItem>으로 변경합니다. ▼▼▼
    [Header("공용 프리팹 목록 (Damage Text 등)")]
    [SerializeField] private List<PreloadItem> commonPreloadItems;

    private readonly Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();

    void Awake()
    {
        ServiceLocator.Register<PrefabProvider>(this);
        
        // commonPreloadItems 리스트를 기반으로 딕셔너리를 채웁니다.
        if (commonPreloadItems != null)
        {
            foreach (var item in commonPreloadItems)
            {
                if (item.prefab != null && !prefabDictionary.ContainsKey(item.prefab.name))
                {
                    // GetPrefab을 위해 이름-프리팹 쌍을 저장합니다.
                    prefabDictionary.Add(item.prefab.name, item.prefab);
                }
            }
        }
        Debug.Log($"[{GetType().Name}] 공용 프리팹 딕셔너리 초기화 완료. 총 {prefabDictionary.Count}개의 프리팹이 등록되었습니다.");
    }

    // GetPrefab 함수는 기존과 동일하게 유지됩니다.
    public GameObject GetPrefab(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        prefabDictionary.TryGetValue(name, out GameObject prefab);
        if (prefab == null)
        {
            Debug.LogError($"[PrefabProvider] 프리팹 딕셔너리에서 '{name}'을(를) 찾을 수 없습니다.");
        }
        return prefab;
    }

    // ▼▼▼ [3] GameManager가 프리팹과 개수 정보를 함께 가져갈 수 있도록 새 함수를 추가합니다. ▼▼▼
    public List<PreloadItem> GetCommonPreloadItems() => commonPreloadItems;
}