using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocalizationInitializer : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(InitializeLocale());
    }

    IEnumerator InitializeLocale()
    {
        // 로컬라이제이션 시스템의 초기화 작업이 끝날 때까지 기다립니다.
        yield return LocalizationSettings.InitializationOperation;

        // 초기화가 완료되면, 사용 가능한 언어 목록의 첫 번째 언어를 선택합니다.
        // 보통 여기에 기본 언어(예: 영어)를 두는 것이 좋습니다.
        if (LocalizationSettings.AvailableLocales.Locales.Count > 0)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
        }
    }
}