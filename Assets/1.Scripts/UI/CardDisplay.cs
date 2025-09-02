using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

[System.Serializable]
public class CardSelectedEvent : UnityEvent<CardDisplay> { }

public class CardDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image cardIconImage;
    [SerializeField] private Image rarityImage;
    [SerializeField] private Image highlightBorder;
    public Button selectButton;
    public CardSelectedEvent OnCardSelected;

    public NewCardDataSO CurrentCard { get; private set; }

    public void Setup(NewCardDataSO cardData)
    {
        CurrentCard = cardData;
        if (nameText != null) nameText.text = cardData.basicInfo.cardName;
        if (descriptionText != null) descriptionText.text = cardData.basicInfo.effectDescription;
        if (cardIconImage != null) cardIconImage.sprite = cardData.basicInfo.cardIcon;
        if (rarityImage != null)
        {
            rarityImage.sprite = ServiceLocator.Get<UIGraphicsDB>().GetRaritySprite(cardData.basicInfo.rarity);
        }
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
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

    /// <summary>
    /// 카드 인스턴스 정보로 UI를 설정합니다. (이름에 레벨 표시 포함)
    /// </summary>
    public void Setup(CardInstance cardInstance)
    {
        // 기존 Setup 메서드를 먼저 호출하여 아이콘, 설명 등 공통 정보를 설정합니다.
        Setup(cardInstance.CardData);

        // 이름 텍스트만 레벨을 포함하여 덮어씁니다.
        if (nameText != null)
        {
            // EnhancementLevel은 0부터 시작하므로, 사용자에게 보여줄 때는 +1을 해줍니다 (Lv.1, Lv.2...)
            nameText.text = $"{cardInstance.CardData.basicInfo.cardName} (Lv.{cardInstance.EnhancementLevel + 1})";
        }
    }

}