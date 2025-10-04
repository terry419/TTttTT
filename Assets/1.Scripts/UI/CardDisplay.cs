// TTttTT/Assets/1.Scripts/UI/CardDisplay.cs
using UnityEngine.Localization;
using UnityEngine.Localization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

[System.Serializable]
public class CardSelectedEvent : UnityEvent<CardDisplay> { }

public class CardDisplay : MonoBehaviour
{
    [Header("--- UI ��� ���� ---")]
    [Header("��� ����")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image attributeIcon;

    [Header("�߾� ����")]
    [SerializeField] private Image illustrationImage;

    [Header("�ϴ� ����")]
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("��Ÿ UI")]
    // [SerializeField] private Image rarityImage; 
    [SerializeField] private Image highlightBorder;
    public Button selectButton;

    [Header("--- �̺�Ʈ ---")]
    public CardSelectedEvent OnCardSelected;

    public NewCardDataSO CurrentCard { get; private set; }
    public CardInstance CurrentCardInstance { get; private set; }

    public void Setup(CardInstance cardInstance)
    {
        if (cardInstance == null) return;

        CurrentCardInstance = cardInstance;
        NewCardDataSO cardData = cardInstance.CardData;
        CurrentCard = cardData;

        // --- �� UI ��ҿ� ������ �Ҵ� ---
        if (nameText != null)
            nameText.text = cardData.basicInfo.cardName.ToString();

        if (descriptionText != null)
            descriptionText.text = cardData.basicInfo.effectDescription.ToString();      

        if (levelText != null)
            levelText.text = $"Lv.{cardInstance.EnhancementLevel + 1}";

        var uiDb = ServiceLocator.Get<UIGraphicsDB>();
        if (uiDb != null)
        {
            // Highlight Border ���� ���� (��� ����)
            if (highlightBorder != null)
            {
                highlightBorder.color = uiDb.GetRarityColor(cardData.basicInfo.rarity);
            }

            // �Ӽ� ������ ���� (CardType ����)
            if (attributeIcon != null)
            {
                attributeIcon.sprite = uiDb.GetAttributeSprite(cardData.basicInfo.type);
                attributeIcon.enabled = attributeIcon.sprite != null;
            }
        }

        // ���� �Ϸ���Ʈ (null üũ)
        if (illustrationImage != null)
        {
            illustrationImage.sprite = cardData.basicInfo.cardIllustration;
            illustrationImage.enabled = illustrationImage.sprite != null;
        }

        // ��ư ������ ����
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
}