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
        // 0. (선택) 로딩 스피너 또는 로딩 화면 UI 표시하는 코드를 넣을 수 있습니다.
        Debug.Log("[GameInitializer] 필수 데이터 로드 시작합니다...");

        // 1. Addressables 시스템 초기화 (최우선)
        yield return Addressables.InitializeAsync();

        // 2. 매니저 인스턴스들을 비동기로 인스턴스화합니다.
        yield return Addressables.InstantiateAsync(PrefabKeys.Managers);
        yield return Addressables.InstantiateAsync(PrefabKeys.GameplaySession);
        Debug.Log("[GameInitializer] 매니저 인스턴스 생성 완료.");

        // 3. DataManager의 데이터 로드를 기다립니다.
        var dataManager = ServiceLocator.Get<DataManager>();
        if (dataManager != null)
        {
            yield return dataManager.LoadAllDataAsync();
        }

        // 4. 모든 로드 후 MainMenu로 이동합니다.
        Debug.Log("[GameInitializer] 모든 로드 완료. 메인 메뉴로 이동합니다.");
        SceneManager.LoadScene(SceneNames.MainMenu);
    }
}
