// 파일 경로: Assets/1/Scripts/UI/ScrollToSelectedElement.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

[RequireComponent(typeof(ScrollRect))]
public class ScrollToSelectedElement : MonoBehaviour
{
    [Header("스크롤 설정")]
    [Tooltip("자동 스크롤이 움직이는 속도입니다.")]
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

        // 스크롤이 불가능하면 (콘텐츠가 뷰포트보다 작으면) 로직을 실행하지 않습니다.
        if (contentPanel.rect.height <= scrollRect.viewport.rect.height)
        {
            return;
        }

        // 선택된 카드의 줄(row) 인덱스를 계산합니다.
        int selectedIndex = selected.transform.GetSiblingIndex();
        int columnCount = gridLayout.constraintCount;
        int rowIndex = selectedIndex / columnCount; // 0 = 첫째 줄, 1 = 둘째 줄, ...

        // 전체 줄의 개수를 계산합니다.
        int childCount = contentPanel.childCount;
        int totalRows = Mathf.CeilToInt((float)childCount / columnCount);

        float targetNormalizedPosition = 1.0f; // 기본값은 맨 위

        // [사용자님 논리 적용] 줄 개수에 따라 스크롤 위치를 다르게 계산합니다.
        if (totalRows == 2)
        {
            // 2줄일 경우: 첫 줄은 맨 위(1.0), 둘째 줄은 맨 아래(0.0)
            targetNormalizedPosition = (rowIndex == 0) ? 1.0f : 0.0f;
        }
        else if (totalRows > 2) // 3줄 이상일 경우
        {
            // 첫 줄은 맨 위(1.0), 마지막 줄은 맨 아래(0.0), 중간 줄들은 그 사이 값
            targetNormalizedPosition = 1.0f - ((float)rowIndex / (totalRows - 1));
        }

        // 스크롤바의 위치를 부드럽게 이동시킵니다.
        scrollRect.verticalNormalizedPosition = Mathf.Lerp(scrollRect.verticalNormalizedPosition, targetNormalizedPosition, Time.unscaledDeltaTime * scrollSpeed);
    }
}