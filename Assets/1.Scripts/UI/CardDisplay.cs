// CardDisplay.cs (최종 수정본)
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

// [수정] 이벤트가 CardDataSO 대신 CardDisplay 자신을 전달하도록 변경합니다.
[System.Serializable]
public class CardSelectedEvent : UnityEvent<CardDisplay> { }

public class CardDisplay : MonoBehaviour
{
    [Header("UI 요소 참조")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image cardBackgroundImage;
    [SerializeField] private Image cardIconImage;
    [SerializeField] private Image rarityImage;
    [SerializeField] private Image highlightBorder;

    public Button selectButton;
    public CardSelectedEvent OnCardSelected;

    private CardDataSO currentCard;
    public CardDataSO CurrentCard => currentCard;

    public CardDataSO GetCurrentCard()
    {
        return currentCard;
    }

    public void Setup(CardDataSO cardData)
    {
        currentCard = cardData;

        if (nameText != null) nameText.text = cardData.cardName;
        if (descriptionText != null) descriptionText.text = cardData.effectDescription;
        if (cardIconImage != null && cardData.cardIcon != null)
        {
            cardIconImage.sprite = cardData.cardIcon;
        }
        if (rarityImage != null)
        {
            rarityImage.sprite = UIGraphicsDB.Instance.GetRaritySprite(cardData.rarity);
        }
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            // [수정] 이벤트 발생 시 카드 데이터(currentCard) 대신 CardDisplay 컴포넌트(this)를 전달합니다.
            selectButton.onClick.AddListener(() => OnCardSelected.Invoke(this));
        }
    }

    public void SetHighlight(bool isSelected)
    {
        if (highlightBorder != null)
        {
            highlightBorder.gameObject.SetActive(isSelected);
        }
    }
}