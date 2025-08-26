// ϸ: GameInitializer.cs
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
        // 0. (û) ⿡ ε ȭ UI ǥϴ ڵ带   ֽϴ.
        Debug.Log("[GameInitializer] ʼ  ε մϴ...");

        // 1. Addressables ý ʱȭ (   )
        yield return Addressables.InitializeAsync();

        // 2. ٽ Ŵ յ 񵿱 νϽȭմϴ.
        yield return Addressables.InstantiateAsync(PrefabKeys.Managers);
        yield return Addressables.InstantiateAsync(PrefabKeys.GameplaySession);

        Debug.Log("[GameInitializer] ٽ Ŵ  Ϸ.");

        // 3. DataManager   ε   ٸϴ.
        var dataManager = ServiceLocator.Get<DataManager>();
        if (dataManager != null)
        {
            yield return dataManager.LoadAllDataAsync();
        }

        // 4.  ε  MainMenu  ̵մϴ.
        Debug.Log("[GameInitializer]   ε Ϸ.  ޴ ̵մϴ.");
        SceneManager.LoadScene(SceneNames.MainMenu);
    }
}