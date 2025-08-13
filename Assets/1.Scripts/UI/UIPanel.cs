// --- ���ϸ�: UIPanel.cs (���: D:\����ȭ\Unity\9th\Assets\1.Scripts\UI\UIPanel.cs) ---

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

        if (UIManager.Instance != null)
        {
            UIManager.Instance.RegisterPanel(panelName, gameObject);
        }
    }

    void OnDestroy()
    {
        if (UIManager.Instance != null && !string.IsNullOrEmpty(panelName))
        {
            UIManager.Instance.UnregisterPanel(panelName);
        }
    }
}