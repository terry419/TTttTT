using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

[System.Serializable]
public class PreloadItem
{
    public AssetReferenceGameObject prefabRef; // AssetReferenceGameObject로 교체
    [Tooltip("미리 생성해 둘 개수입니다.")]
    public int count;
}

public class PrefabProvider : MonoBehaviour
{
    [Header("공용 프리팹 목록 (Damage Text 등)")]
    [SerializeField] private List<PreloadItem> commonPreloadItems;

    [Header("특수 효과 프리팹")]
    [Tooltip("크리티컬 히트 시 생성될 이펙트 프리팹입니다.")]
    public AssetReferenceGameObject critEffectPrefabRef; // AssetReference로 변경

    // 딕셔너리도 AssetReferenceGameObject를 저장하도록 변경
    private readonly Dictionary<string, AssetReferenceGameObject> prefabDictionary = new Dictionary<string, AssetReferenceGameObject>();

    void Awake()
    {
        ServiceLocator.Register<PrefabProvider>(this);
        
        if (commonPreloadItems != null)
        {
            foreach (var item in commonPreloadItems)
            {
                // AssetReference가 유효한지, 그리고 키가 중복되지 않는지 확인합니다.
                if (item.prefabRef != null && item.prefabRef.RuntimeKeyIsValid() && !prefabDictionary.ContainsKey(item.prefabRef.AssetGUID))
                {
                    // 이름 대신 고유한 AssetGUID를 키로 사용합니다.
                    prefabDictionary.Add(item.prefabRef.AssetGUID, item.prefabRef);
                }
            }
        }
        Debug.Log($"[{GetType().Name}] 공용 프리팹 딕셔너리 초기화 완료. 총 {prefabDictionary.Count}개의 프리팹이 등록되었습니다.");
    }

    // 키(GUID)를 받아 AssetReferenceGameObject를 반환하는 함수
    public AssetReferenceGameObject GetPrefabRef(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        prefabDictionary.TryGetValue(key, out AssetReferenceGameObject prefabRef);
        if (prefabRef == null || !prefabRef.RuntimeKeyIsValid())
        {
            Debug.LogWarning($"[PrefabProvider] 프리팹 딕셔너리에서 '{key}'을(를) 찾을 수 없거나 유효하지 않습니다.");
        }
        return prefabRef;
    }
    
    public List<PreloadItem> GetCommonPreloadItems() => commonPreloadItems;
}
