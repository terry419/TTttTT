// 경로: Assets/1/Scripts/UI/AutoFocusScroller.cs

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AutoFocusScroller : MonoBehaviour
{
    [Header("제어 대상")]
    [SerializeField] private ScrollRect mapScrollRect;
    [SerializeField] private MapView mapView;

    [Header("스크롤 설정")]
    [SerializeField] private float edgeScrollSpeed = 1.0f;
    [SerializeField] private float scrollBoundaryOffset = 50f;

    private Vector3[] viewportCorners = new Vector3[4];

    void Update()
    {
        if (!gameObject.activeInHierarchy) return;

        GameObject rawSelectedObject = EventSystem.current.currentSelectedGameObject;
        if (rawSelectedObject == null) return;


        GameObject closestNode = mapView.FindClosestNodeTo(rawSelectedObject.transform.position);
        if (closestNode == null) return;


        mapScrollRect.viewport.GetWorldCorners(viewportCorners);
        float viewportTopY = viewportCorners[1].y;
        float viewportBottomY = viewportCorners[0].y;

        float nodeY = closestNode.transform.position.y;

        float topBoundary = viewportTopY - scrollBoundaryOffset;
        float bottomBoundary = viewportBottomY + scrollBoundaryOffset;
        
        bool shouldScrollUp = nodeY > topBoundary;
        bool shouldScrollDown = nodeY < bottomBoundary;

        if (shouldScrollUp)
        {
            // --- [디버그 DELTA: 조건 발동] ---
            Scroll(1);
        }
        else if (shouldScrollDown)
        {
            // --- [디버그 DELTA: 조건 발동] ---
            Scroll(-1);
        }
    }

    private void Scroll(int direction)
    {
        float currentScrollPos = mapScrollRect.verticalNormalizedPosition;
        float newScrollPos = currentScrollPos + (direction * edgeScrollSpeed * Time.unscaledDeltaTime);
        newScrollPos = Mathf.Clamp01(newScrollPos);

        mapScrollRect.verticalNormalizedPosition = newScrollPos;
    }
}