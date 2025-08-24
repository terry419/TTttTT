// 파일명: GameInitializer.cs
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class GameInitializer : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(LoadEssentialDataAndProceed());
    }

    private IEnumerator LoadEssentialDataAndProceed()
    {
        // 0. (선택사항) 여기에 로딩 화면 UI를 표시하는 코드를 넣을 수 있습니다.
        Debug.Log("[GameInitializer] 필수 데이터 로딩을 시작합니다...");

        // 1. Addressables 시스템 초기화 (최초 한 번만 실행)
        yield return Addressables.InitializeAsync();

        // 2. 핵심 매니저 프리팹들을 비동기적으로 인스턴스화합니다.
        yield return Addressables.InstantiateAsync(PrefabKeys.Managers);
        yield return Addressables.InstantiateAsync(PrefabKeys.GameplaySession);

        Debug.Log("[GameInitializer] 핵심 매니저 생성 완료.");

        // 3. DataManager의 모든 데이터 로딩이 끝날 때까지 기다립니다.
        var dataManager = ServiceLocator.Get<DataManager>();
        if (dataManager != null)
        {
            yield return dataManager.LoadAllDataAsync();
        }

        // 4. 모든 로딩이 끝나면 MainMenu 씬으로 이동합니다.
        Debug.Log("[GameInitializer] 모든 데이터 로딩 완료. 메인 메뉴로 이동합니다.");
        SceneManager.LoadScene(SceneNames.MainMenu);
    }
}