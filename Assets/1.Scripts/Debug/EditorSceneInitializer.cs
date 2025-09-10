#if UNITY_EDITOR // 이 스크립트는 Unity 에디터에서만 컴파일됩니다.

using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

/// <summary>
/// Unity 에디터에서 씬을 직접 실행할 때,
/// 필수 매니저(_Managers, _GameplaySession)들을 자동으로 로드해주는 테스트용 클래스입니다.
/// </summary>
public class EditorSceneInitializer : MonoBehaviour
{
    async void Awake()
    {
        // GameManager가 이미 있는지 확인합니다. 있다면 Initializer는 필요 없는 상황입니다.
        if (ServiceLocator.IsRegistered<GameManager>())
        {
            Destroy(gameObject);
            return;
        }

        Debug.LogWarning("필수 매니저가 없어 [EditorSceneInitializer]가 로드를 시작합니다...");

        // [리팩토링] 테스트 환경에서는 두 프리팹을 모두 생성해야 합니다.
        // 1. 애플리케이션 생명주기 매니저를 먼저 생성합니다.
        await Addressables.InstantiateAsync(PrefabKeys.Managers).Task;
        // 2. 게임 세션 생명주기 매니저를 생성합니다.
        await Addressables.InstantiateAsync(PrefabKeys.GameplaySession).Task;

        Debug.Log("테스트를 위한 필수 매니저 로드가 완료되었습니다.");

        Destroy(gameObject);
    }
}
#endif