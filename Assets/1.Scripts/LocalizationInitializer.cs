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
        // ���ö������̼� �ý����� �ʱ�ȭ �۾��� ���� ������ ��ٸ��ϴ�.
        yield return LocalizationSettings.InitializationOperation;

        // �ʱ�ȭ�� �Ϸ�Ǹ�, ��� ������ ��� ����� ù ��° �� �����մϴ�.
        // ���� ���⿡ �⺻ ���(��: ����)�� �δ� ���� �����ϴ�.
        if (LocalizationSettings.AvailableLocales.Locales.Count > 0)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
        }
    }
}