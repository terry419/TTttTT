using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocalizationInitializer : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(InitializeLocaleWithDebug());
    }

    IEnumerator InitializeLocaleWithDebug()
    {
        // 1. 초기화 시작 직전에 로그를 출력합니다.
        Debug.Log("[디버그 1] Localization 초기화를 시작합니다.");

        // Localization 시스템이 내부적으로 준비를 마칠 때까지 기다립니다.
        yield return LocalizationSettings.InitializationOperation;

        // 2. 초기화가 끝난 직후에 로그를 출력합니다.
        // 만약 콘솔에 [디버그 1]은 뜨는데 이 로그가 안 뜬다면, 초기화 과정 자체에서 멈춘 것입니다.
        Debug.Log("[디버그 2] Localization 초기화 작업이 완료되었습니다.");

        // 3. 현재 설정된 언어가 무엇인지 확인합니다.
        if (LocalizationSettings.SelectedLocale != null)
        {
            Debug.Log($"[디버그 3] 현재 선택된 언어: {LocalizationSettings.SelectedLocale.LocaleName}");
        }
        else
        {
            Debug.LogError("[디버그 3-에러] 초기화 후 선택된 언어가 없습니다! (null)");
        }
    }
}