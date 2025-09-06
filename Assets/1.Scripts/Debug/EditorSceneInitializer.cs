#if UNITY_EDITOR // 이 스크립트는 Unity 에디터에서만 컴파일되고 작동합니다.

using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

/// <summary>
/// Unity 에디터에서 씬을 직접 실행했을 때,
/// 필수 관리자(_Managers, _GameplaySession 등)가 없으면 자동으로 생성해주는 디버그용 클래스입니다.
/// </summary>
public class EditorSceneInitializer : MonoBehaviour
{
    async void Awake()
    {
        // GameManager가 이미 있는지 확인합니다. 있다면 Initializer 씬부터 정상적으로 시작한 것입니다.
        if (ServiceLocator.IsRegistered<GameManager>())
        {
            // 이미 매니저들이 있으므로 이 스크립트는 아무것도 할 필요가 없습니다.
            Destroy(gameObject);
            return;
        }

        // GameManager가 없다면, 이 씬을 직접 실행한 것이므로 필수 프리팹들을 로드합니다.
        Debug.LogWarning("필수 매니저가 없어 [EditorSceneInitializer]가 로드를 시작합니다...");

        // Addressables를 통해 _Managers와 _GameplaySession 프리팹을 생성합니다.
        await Addressables.InstantiateAsync(PrefabKeys.Managers).Task;
        await Addressables.InstantiateAsync(PrefabKeys.GameplaySession).Task;

        Debug.Log("에디터 테스트를 위한 필수 매니저 로드가 완료되었습니다.");

        // 역할을 다했으므로 스스로를 파괴합니다.
        Destroy(gameObject);
    }
}
#endif