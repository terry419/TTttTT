using UnityEngine;

/// <summary>
/// 풀링된 오브젝트의 원본 프리팹 정보를 저장하는 컴포넌트입니다.
/// PoolManager가 오브젝트를 풀로 반환할 때 어떤 풀에 속하는지 식별하는 데 사용됩니다.
/// </summary>
public class PooledObjectInfo : MonoBehaviour
{
    public GameObject originalPrefab; // 이 오브젝트의 원본 프리팹

    /// <summary>
    /// 오브젝트 정보를 초기화합니다.
    /// </summary>
    /// <param name="prefab">이 오브젝트의 원본 프리팹</param>
    public void Initialize(GameObject prefab)
    {
        originalPrefab = prefab;
    }
}
