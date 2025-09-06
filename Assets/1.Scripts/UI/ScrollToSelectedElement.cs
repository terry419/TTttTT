// ���� ���: Assets/1/Scripts/UI/ScrollToSelectedElement.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

[RequireComponent(typeof(ScrollRect))]
public class ScrollToSelectedElement : MonoBehaviour
{
    [Header("��ũ�� ����")]
    [Tooltip("�ڵ� ��ũ���� �����̴� �ӵ��Դϴ�.")]
    public float scrollSpeed = 20f;

    private ScrollRect scrollRect;
    private RectTransform contentPanel;
    private GridLayoutGroup gridLayout;

    void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        if (scrollRect != null)
        {
            contentPanel = scrollRect.content;
            if (contentPanel != null)
            {
                gridLayout = contentPanel.GetComponent<GridLayoutGroup>();
            }
        }
    }

    void LateUpdate()
    {
        GameObject selected = EventSystem.current.currentSelectedGameObject;

        if (selected == null || !selected.transform.IsChildOf(contentPanel) || gridLayout == null)
        {
            return;
        }

        // ��ũ���� �Ұ����ϸ� (�������� ����Ʈ���� ������) ������ �������� �ʽ��ϴ�.
        if (contentPanel.rect.height <= scrollRect.viewport.rect.height)
        {
            return;
        }

        // ���õ� ī���� ��(row) �ε����� ����մϴ�.
        int selectedIndex = selected.transform.GetSiblingIndex();
        int columnCount = gridLayout.constraintCount;
        int rowIndex = selectedIndex / columnCount; // 0 = ù° ��, 1 = ��° ��, ...

        // ��ü ���� ������ ����մϴ�.
        int childCount = contentPanel.childCount;
        int totalRows = Mathf.CeilToInt((float)childCount / columnCount);

        float targetNormalizedPosition = 1.0f; // �⺻���� �� ��

        // [����ڴ� �� ����] �� ������ ���� ��ũ�� ��ġ�� �ٸ��� ����մϴ�.
        if (totalRows == 2)
        {
            // 2���� ���: ù ���� �� ��(1.0), ��° ���� �� �Ʒ�(0.0)
            targetNormalizedPosition = (rowIndex == 0) ? 1.0f : 0.0f;
        }
        else if (totalRows > 2) // 3�� �̻��� ���
        {
            // ù ���� �� ��(1.0), ������ ���� �� �Ʒ�(0.0), �߰� �ٵ��� �� ���� ��
            targetNormalizedPosition = 1.0f - ((float)rowIndex / (totalRows - 1));
        }

        // ��ũ�ѹ��� ��ġ�� �ε巴�� �̵���ŵ�ϴ�.
        scrollRect.verticalNormalizedPosition = Mathf.Lerp(scrollRect.verticalNormalizedPosition, targetNormalizedPosition, Time.unscaledDeltaTime * scrollSpeed);
    }
}