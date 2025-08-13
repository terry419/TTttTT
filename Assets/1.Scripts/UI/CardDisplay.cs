using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 카드 한 장의 UI 표시를 담당하는 컴포넌트입니다.
/// CardRewardController에 의해 생성되고 제어됩니다.
/// </summary>
public class CardDisplay : MonoBehaviour
{
    [Header("UI 요소 참조")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image cardIcon;
    [SerializeField] private Image rarityBorder; // 희귀도에 따라 색상이 변할 테두리
    [SerializeField] private Image highlightBorder; // 선택 시 활성화될 테두리
    [SerializeField] public Button selectButton; // 카드 전체를 감싸는 투명 버튼 (public으로 변경)

    private CardDataSO currentCard;

    void Awake()
    {
        // 시작 시 하이라이트는 비활성화
        SetHighlight(false);
    }

    /// <summary>
    /// 카드 데이터로 UI를 초기화하고 클릭 이벤트를 설정합니다.
    /// </summary>
    public void Setup(CardDataSO cardData)
    {
        currentCard = cardData;

        if (nameText != null) nameText.text = currentCard.cardName;
        if (descriptionText != null) descriptionText.text = currentCard.effectDescription;
        // if (cardIcon != null && currentCard.icon != null) cardIcon.sprite = currentCard.icon;
        if (rarityBorder != null) rarityBorder.color = GetRarityColor(currentCard.rarity);

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => {
                if (CardRewardController.Instance != null)
                {
                    CardRewardController.Instance.OnCardSelected(currentCard);
                }
            });
        }
    }

    /// <summary>
    /// 이 카드가 어떤 카드인지 반환합니다.
    /// </summary>
    public CardDataSO GetCurrentCard()
    {
        return currentCard;
    }

    /// <summary>
    /// 하이라이트 테두리의 활성화 상태를 설정합니다.
    /// </summary>
    public void SetHighlight(bool isSelected)
    {
        if (highlightBorder != null)
        {
            highlightBorder.gameObject.SetActive(isSelected);
        }
    }

    private Color GetRarityColor(CardRarity rarity)
    {
        switch (rarity)
        {
            case CardRarity.Common: return Color.gray;
            case CardRarity.Rare: return new Color(0.2f, 0.6f, 1f); // 파란색
            case CardRarity.Epic: return new Color(0.7f, 0.3f, 0.9f); // 보라색
            case CardRarity.Legendary: return new Color(1f, 0.8f, 0.2f); // 주황/금색
            default: return Color.white;
        }
    }
}
