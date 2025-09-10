#if UNITY_EDITOR // �� ��ũ��Ʈ�� Unity �����Ϳ����� �����ϵ˴ϴ�.

using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

/// <summary>
/// Unity �����Ϳ��� ���� ���� ������ ��,
/// �ʼ� �Ŵ���(_Managers, _GameplaySession)���� �ڵ����� �ε����ִ� �׽�Ʈ�� Ŭ�����Դϴ�.
/// </summary>
public class EditorSceneInitializer : MonoBehaviour
{
    async void Awake()
    {
        // GameManager�� �̹� �ִ��� Ȯ���մϴ�. �ִٸ� Initializer�� �ʿ� ���� ��Ȳ�Դϴ�.
        if (ServiceLocator.IsRegistered<GameManager>())
        {
            Destroy(gameObject);
            return;
        }

        Debug.LogWarning("�ʼ� �Ŵ����� ���� [EditorSceneInitializer]�� �ε带 �����մϴ�...");

        // [�����丵] �׽�Ʈ ȯ�濡���� �� �������� ��� �����ؾ� �մϴ�.
        // 1. ���ø����̼� �����ֱ� �Ŵ����� ���� �����մϴ�.
        await Addressables.InstantiateAsync(PrefabKeys.Managers).Task;
        // 2. ���� ���� �����ֱ� �Ŵ����� �����մϴ�.
        await Addressables.InstantiateAsync(PrefabKeys.GameplaySession).Task;

        Debug.Log("�׽�Ʈ�� ���� �ʼ� �Ŵ��� �ε尡 �Ϸ�Ǿ����ϴ�.");

        Destroy(gameObject);
    }
}
#endif