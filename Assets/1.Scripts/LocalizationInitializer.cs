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
        // 1. �ʱ�ȭ ���� ������ �α׸� ����մϴ�.
        Debug.Log("[����� 1] Localization �ʱ�ȭ�� �����մϴ�.");

        // Localization �ý����� ���������� �غ� ��ĥ ������ ��ٸ��ϴ�.
        yield return LocalizationSettings.InitializationOperation;

        // 2. �ʱ�ȭ�� ���� ���Ŀ� �α׸� ����մϴ�.
        // ���� �ֿܼ� [����� 1]�� �ߴµ� �� �αװ� �� ��ٸ�, �ʱ�ȭ ���� ��ü���� ���� ���Դϴ�.
        Debug.Log("[����� 2] Localization �ʱ�ȭ �۾��� �Ϸ�Ǿ����ϴ�.");

        // 3. ���� ������ �� �������� Ȯ���մϴ�.
        if (LocalizationSettings.SelectedLocale != null)
        {
            Debug.Log($"[����� 3] ���� ���õ� ���: {LocalizationSettings.SelectedLocale.LocaleName}");
        }
        else
        {
            Debug.LogError("[����� 3-����] �ʱ�ȭ �� ���õ� �� �����ϴ�! (null)");
        }
    }
}