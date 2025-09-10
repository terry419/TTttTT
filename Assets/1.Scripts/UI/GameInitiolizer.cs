// ./TTttTT/Assets/1.Scripts/UI/GameInitiolizer.cs
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
        Debug.Log("[GameInitializer] 필수 데이터 로드 시작합니다...");

        // 1. Addressables 시스템 초기화
        yield return Addressables.InitializeAsync();

        // 2. [수정] 애플리케이션 생명주기를 갖는 _Managers 프리팹만 생성합니다.
        yield return Addressables.InstantiateAsync(PrefabKeys.Managers);
        Debug.Log("[GameInitializer] 애플리케이션 매니저(_Managers) 생성 완료.");

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