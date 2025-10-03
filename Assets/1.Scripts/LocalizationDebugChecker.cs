using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

public class LocalizationDebugChecker : MonoBehaviour
{
    void Start()
    {
        // �� ��ũ��Ʈ�� �پ��ִ� ���� ������Ʈ���� LocalizeStringEvent ������Ʈ�� ã���ϴ�.
        var localizeEvent = GetComponent<LocalizeStringEvent>();
        if (localizeEvent != null)
        {
            // [����� 4] LocalizeStringEvent�� � ���̺�� Ű�� �����ϰ� �ִ��� Ȯ���մϴ�.
            string tableName = localizeEvent.StringReference.TableReference;
            string entryName = localizeEvent.StringReference.TableEntryReference;
            Debug.Log($"[����� 4] '{gameObject.name}' ������Ʈ�� LocalizeStringEvent�� �����ϴ� Ű: [���̺�: {tableName}], [Ű: {entryName}]");

            // [����� 5] �� ������Ʈ�� ���ڿ��� '����'�� ������ OnStringChanged �Լ��� �����ϵ��� ����մϴ�.
            localizeEvent.OnUpdateString.AddListener(OnStringChanged);
            Debug.Log($"[����� 5] '{gameObject.name}' ������Ʈ�� ���ڿ� ���� �����ڸ� ���������� ����߽��ϴ�.");
        }
        else
        {
            Debug.LogError($"[����� 4-����] '{gameObject.name}' ������Ʈ���� LocalizeStringEvent ������Ʈ�� ã�� �� �����ϴ�.");
        }
    }

    // ���ڿ��� ���������� ����Ǿ� UI�� ����� �� �� �Լ��� ȣ��˴ϴ�.
    void OnStringChanged(string newValue)
    {
        // [����� 6] ���� �� �αװ� ���δٸ�, ��� ������ �����߰� ���������� UI�� ������Ʈ �Ǿ��ٴ� ���Դϴ�.
        Debug.Log($"[����� 6] ���ڿ��� ���������� ����Ǿ����ϴ�! ���� ����� ��: '{newValue}'");
    }
}