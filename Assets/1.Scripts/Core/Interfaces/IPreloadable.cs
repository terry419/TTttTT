using UnityEngine.AddressableAssets;
using System.Collections.Generic;

/// <summary>
/// �� �������̽��� �����ϴ� ��ü�� GameManager�� �����ε� �ý��ۿ�
/// �̸� �ε��ؾ� �� Addressable ������ ����� ������ �� �ֽ��ϴ�.
/// </summary>
public interface IPreloadable
{
    /// <summary>
    /// �����ε��� ��� �������� AssetReferenceGameObject ����� ��ȯ�մϴ�.
    /// </summary>
    IEnumerable<AssetReferenceGameObject> GetPrefabsToPreload();
}