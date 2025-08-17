// CardDisplay.cs (최종 수정본)
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

[System.Serializable]
public class CardSelectedEvent : UnityEvent<CardDataSO> { }

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

    // --- 아래 함수를 새로 추가하여 이전 스크립트와의 호환성 문제를 해결합니다. ---
    public CardDataSO GetCurrentCard()
    {
        return currentCard;
    }
    // --------------------------------------------------------------------

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
            selectButton.onClick.AddListener(() => OnCardSelected.Invoke(currentCard));
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