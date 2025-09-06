#if UNITY_EDITOR // �� ��ũ��Ʈ�� Unity �����Ϳ����� �����ϵǰ� �۵��մϴ�.

using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

/// <summary>
/// Unity �����Ϳ��� ���� ���� �������� ��,
/// �ʼ� ������(_Managers, _GameplaySession ��)�� ������ �ڵ����� �������ִ� ����׿� Ŭ�����Դϴ�.
/// </summary>
public class EditorSceneInitializer : MonoBehaviour
{
    async void Awake()
    {
        // GameManager�� �̹� �ִ��� Ȯ���մϴ�. �ִٸ� Initializer ������ ���������� ������ ���Դϴ�.
        if (ServiceLocator.IsRegistered<GameManager>())
        {
            // �̹� �Ŵ������� �����Ƿ� �� ��ũ��Ʈ�� �ƹ��͵� �� �ʿ䰡 �����ϴ�.
            Destroy(gameObject);
            return;
        }

        // GameManager�� ���ٸ�, �� ���� ���� ������ ���̹Ƿ� �ʼ� �����յ��� �ε��մϴ�.
        Debug.LogWarning("�ʼ� �Ŵ����� ���� [EditorSceneInitializer]�� �ε带 �����մϴ�...");

        // Addressables�� ���� _Managers�� _GameplaySession �������� �����մϴ�.
        await Addressables.InstantiateAsync(PrefabKeys.Managers).Task;
        await Addressables.InstantiateAsync(PrefabKeys.GameplaySession).Task;

        Debug.Log("������ �׽�Ʈ�� ���� �ʼ� �Ŵ��� �ε尡 �Ϸ�Ǿ����ϴ�.");

        // ������ �������Ƿ� �����θ� �ı��մϴ�.
        Destroy(gameObject);
    }
}
#endif