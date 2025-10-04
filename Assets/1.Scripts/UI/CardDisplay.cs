// TTttTT/Assets/1.Scripts/UI/CardDisplay.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

[System.Serializable]
public class CardSelectedEvent : UnityEvent<CardDisplay> { }

public class CardDisplay : MonoBehaviour
{
    [Header("--- UI 요소 연결 ---")]
    [Header("상단 정보")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image attributeIcon;

    [Header("중앙 정보")]
    [SerializeField] private Image illustrationImage;

    [Header("하단 정보")]
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("기타 UI")]
    // [SerializeField] private Image rarityImage; 
    [SerializeField] private Image highlightBorder;
    public Button selectButton;

    [Header("--- 이벤트 ---")]
    public CardSelectedEvent OnCardSelected;

    public NewCardDataSO CurrentCard { get; private set; }
    public CardInstance CurrentCardInstance { get; private set; }

    public void Setup(CardInstance cardInstance)
    {
        if (cardInstance == null) return;

        CurrentCardInstance = cardInstance;
        NewCardDataSO cardData = cardInstance.CardData;
        CurrentCard = cardData;

        // --- 각 UI 요소에 데이터 할당 ---
        if (nameText != null)
            nameText.text = cardData.basicInfo.cardName;

        if (descriptionText != null)
            descriptionText.text = cardData.basicInfo.effectDescription;

        if (levelText != null)
            levelText.text = $"Lv.{cardInstance.EnhancementLevel + 1}";

        var uiDb = ServiceLocator.Get<UIGraphicsDB>();
        if (uiDb != null)
        {
            // Highlight Border 색상 설정 (등급 기준)
            if (highlightBorder != null)
            {
                highlightBorder.color = uiDb.GetRarityColor(cardData.basicInfo.rarity);
            }

            // 속성 아이콘 설정 (CardType 기준)
            if (attributeIcon != null)
            {
                attributeIcon.sprite = uiDb.GetAttributeSprite(cardData.basicInfo.type);
                attributeIcon.enabled = attributeIcon.sprite != null;
            }
        }

        // 메인 일러스트 (null 체크)
        if (illustrationImage != null)
        {
            illustrationImage.sprite = cardData.basicInfo.cardIllustration;
            illustrationImage.enabled = illustrationImage.sprite != null;
        }

        // 버튼 리스너 연결
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