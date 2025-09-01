using UnityEngine;
using TMPro; // TextMeshPro를 사용한다면 추가

[ExecuteAlways] // 에디터에서도 실시간으로 변경사항을 보기 위해 추가
public class CardLayoutController : MonoBehaviour
{
    [Header("UI Element References")]
    public RectTransform rarityImageRect;
    public RectTransform cardNameRect;
    public RectTransform cardIconImageRect;
    public RectTransform descriptionTextRect;

    [Header("Layout Ratios")]
    [Tooltip("Card Name의 세로 비율")]
    public float nameRatio = 1.0f;
    [Tooltip("Card Icon의 세로 비율")]
    public float iconRatio = 1.5f;
    [Tooltip("Description의 세로 비율")]
    public float descriptionRatio = 3.0f;

    [Header("Padding & Margins")]
    [Tooltip("RarityImage를 위해 확보할 상단 공간")]
    public float topPadding = 80f;
    [Tooltip("카드 하단 여백")]
    public float bottomPadding = 20f;
    [Tooltip("카드 좌우 여백")]
    public float horizontalPadding = 20f;

    // 카드 자신의 RectTransform
    private RectTransform selfRect;

    // 스크립트가 활성화되거나, 인스펙터에서 값이 변경될 때마다 호출
    private void OnEnable()
    {
        selfRect = GetComponent<RectTransform>();
        ApplyLayout();
    }

    // 에디터에서 값이 변경될 때마다 호출 (ExecuteAlways 속성 필요)
    private void OnValidate()
    {
        selfRect = GetComponent<RectTransform>();
        ApplyLayout();
    }

    // 카드의 크기가 변경될 때마다 자동으로 호출되는 Unity 메시지
    private void OnRectTransformDimensionsChange()
    {
        if (selfRect != null)
        {
            ApplyLayout();
        }
    }

    //  핵심 로직: 레이아웃을 계산하고 적용하는 함수
    public void ApplyLayout()
    {
        if (cardNameRect == null || cardIconImageRect == null || descriptionTextRect == null)
        {
            Debug.LogWarning("UI Element References are not set in CardLayoutController.");
            return;
        }

        // =================================================================
        // ## 1단계: 구조 레이아웃 - 오브젝트의 크기와 위치를 강제로 설정
        // =================================================================

        // 1. 전체 계산 영역 설정
        float totalAvailableHeight = selfRect.rect.height - topPadding - bottomPadding;
        if (totalAvailableHeight <= 0) return; // 높이가 0 이하면 계산 중지

        float totalAvailableWidth = selfRect.rect.width - (horizontalPadding * 2);
        if (totalAvailableWidth <= 0) return;

        // 2. 비율 계산
        float totalRatio = nameRatio + iconRatio + descriptionRatio;

        // 3. 각 요소의 높이를 비율에 따라 계산
        float nameHeight = (nameRatio / totalRatio) * totalAvailableHeight;
        float iconHeight = (iconRatio / totalRatio) * totalAvailableHeight;
        float descriptionHeight = (descriptionRatio / totalRatio) * totalAvailableHeight;

        // 4. 각 요소의 위치와 크기를 순차적으로 적용 (Top-Down 방식)
        float currentYPosition = -topPadding; // 시작 위치는 상단 패딩 바로 아래

        // CardName 위치 및 크기 설정
        SetRectTransform(cardNameRect, currentYPosition, nameHeight, totalAvailableWidth);
        currentYPosition -= nameHeight;

        // CardIconImage 위치 및 크기 설정
        SetRectTransform(cardIconImageRect, currentYPosition, iconHeight, totalAvailableWidth);
        currentYPosition -= iconHeight;

        // DescriptionText 위치 및 크기 설정
        SetRectTransform(descriptionTextRect, currentYPosition, descriptionHeight, totalAvailableWidth);

        // =================================================================
        // ## 2단계: 콘텐츠 렌더링 - TextMeshPro가 일하게 놔두기
        // =================================================================
        // 이 단계는 별도의 코드가 필요 없습니다.
        // 1단계에서 RectTransform의 크기가 이미 고정되었기 때문에,
        // 다음 프레임에서 UI가 렌더링될 때 TextMeshPro의 Auto Size 기능은
        // 이 고정된 상자 안에서 최적의 폰트 크기를 '찾아낼' 수밖에 없습니다.
        // 즉, 폰트 크기가 레이아웃을 결정하는 것이 아니라,
        // 레이아웃이 폰트 크기를 결정하게 되는 순서로 뒤바뀐 것입니다.
    }

    // RectTransform을 설정하는 도우미 함수
    private void SetRectTransform(RectTransform targetRect, float posY, float height, float width)
    {
        // 앵커를 Top-Stretch로 설정
        targetRect.anchorMin = new Vector2(0.5f, 1);
        targetRect.anchorMax = new Vector2(0.5f, 1);
        targetRect.pivot = new Vector2(0.5f, 1); // 기준점을 상단 중앙으로

        // 위치와 크기 설정
        targetRect.anchoredPosition = new Vector2(0, posY);
        targetRect.sizeDelta = new Vector2(width, height);
    }
}