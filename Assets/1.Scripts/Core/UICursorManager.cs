// --- ���ϸ�: UICursorManager.cs (���� ����) ---
using UnityEngine;
using UnityEngine.EventSystems; // EventSystem ����� ���� �ʼ�

/// <summary>
/// EventSystem�� ���� ������ UI ��Ҹ� �����Ͽ�
/// Ŀ�� ��������Ʈ�� ��ġ�� �ǽð����� ������Ʈ�ϴ� �߾� �������Դϴ�.
/// �� ��ũ��Ʈ�� DontDestroyOnLoad�� �����Ǿ� ��� ������ �����մϴ�.
/// </summary>
public class UICursorManager : MonoBehaviour
{
    public static UICursorManager Instance { get; private set; }

    // ���� ������ ������ Ŀ�� ��������Ʈ�� RectTransform
    private RectTransform activeCursor;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // ������ Ŀ���� ��ϵǾ� ���� ������ �ƹ��͵� ���� ����
        if (activeCursor == null)
        {
            return;
        }

        // EventSystem�� ���� �����ϰ� �ִ� ���ӿ�����Ʈ�� �����ɴϴ�.
        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;

        // ���õ� ������Ʈ�� �ִٸ�
        if (selectedObject != null)
        {
            // Ŀ�� ��������Ʈ�� Ȱ��ȭ�ϰ�, ���õ� ������Ʈ�� ��ġ�� �̵���ŵ�ϴ�.
            activeCursor.gameObject.SetActive(true);
            activeCursor.position = selectedObject.transform.position;
        }
        else
        {
            // ���õ� ������Ʈ�� �ƹ��͵� ���ٸ� Ŀ���� ����ϴ�.
            activeCursor.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// ���ο� ���� �ε�� ��, �ش� ���� UI �Ŵ����� �� �Լ��� ȣ���Ͽ�
    /// ������ Ŀ�� ��������Ʈ�� ����մϴ�.
    /// </summary>
    /// <param name="cursorRectTransform">���ο� ���� Ŀ�� RectTransform</param>
    public void RegisterCursor(RectTransform cursorRectTransform)
    {
        this.activeCursor = cursorRectTransform;
        if (this.activeCursor != null)
        {
            Debug.Log($"[UICursorManager] ���ο� Ŀ�� ���: {activeCursor.name}");
        }
    }

    /// <summary>
    /// ���� ���� �� Ŀ�� ����� �����մϴ�.
    /// </summary>
    public void UnregisterCursor()
    {
        this.activeCursor = null;
    }
}