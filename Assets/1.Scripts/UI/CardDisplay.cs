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
    /// ī�� �ν��Ͻ� ������ UI�� �����մϴ�. (�̸��� ���� ǥ�� ����)
    /// </summary>
    public void Setup(CardInstance cardInstance)
    {
        // ���� Setup �޼��带 ���� ȣ���Ͽ� ������, ���� �� ���� ������ �����մϴ�.
        Setup(cardInstance.CardData);

        // �̸� �ؽ�Ʈ�� ������ �����Ͽ� ����ϴ�.
        if (nameText != null)
        {
            // EnhancementLevel�� 0���� �����ϹǷ�, ����ڿ��� ������ ���� +1�� ���ݴϴ� (Lv.1, Lv.2...)
            nameText.text = $"{cardInstance.CardData.basicInfo.cardName} (Lv.{cardInstance.EnhancementLevel + 1})";
        }
    }

}