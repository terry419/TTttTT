using UnityEngine;

/// <summary>
/// UIManager�� �г��� �ڵ����� ����ϴ� ���� ��ũ��Ʈ�Դϴ�.
/// �� ��ũ��Ʈ�� �� UI �г��� ��Ʈ GameObject�� �߰��ϼ���.
/// </summary>
public class UIPanel : MonoBehaviour
{
    // UIManager���� �г��� �ĺ��� �̸�. Inspector���� �����մϴ�.
    public string panelName;

    void Awake()
    {
        if (string.IsNullOrEmpty(panelName))
        {
            Debug.LogWarning($"UIPanel ��ũ��Ʈ�� ������ '{gameObject.name}' ������Ʈ�� 'panelName'�� �������� �ʾҽ��ϴ�. GameObject �̸��� ����մϴ�.");
            panelName = gameObject.name;
        }

        // --- [����] ServiceLocator�� ���� UIManager �ν��Ͻ��� �����ɴϴ�. ---
        var uiManager = ServiceLocator.Get<UIManager>();
        if (uiManager != null)
        {
            uiManager.RegisterPanel(panelName, gameObject);
        }
    }

    void OnDestroy()
    {
        // --- [����] ServiceLocator�� ���� UIManager �ν��Ͻ��� �����ɴϴ�. ---
        var uiManager = ServiceLocator.Get<UIManager>();
        if (uiManager != null && !string.IsNullOrEmpty(panelName))
        {
            uiManager.UnregisterPanel(panelName);
        }
    }
}