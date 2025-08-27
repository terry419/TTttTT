using UnityEngine.AddressableAssets;
using System.Collections.Generic;

/// <summary>
/// 이 인터페이스를 구현하는 객체는 GameManager의 프리로드 시스템에
/// 미리 로드해야 할 Addressable 프리팹 목록을 제공할 수 있습니다.
/// </summary>
public interface IPreloadable
{
    /// <summary>
    /// 프리로드할 대상 프리팹의 AssetReferenceGameObject 목록을 반환합니다.
    /// </summary>
    IEnumerable<AssetReferenceGameObject> GetPrefabsToPreload();
}