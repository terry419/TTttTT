// CardDisplay.cs - 수정된 최종본

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

[System.Serializable]
public class CardSelectedEvent : UnityEvent<CardDisplay> { }

public class CardDisplay : MonoBehaviour
{
    [Header("--- UI 컴포넌트 ---")]
    [Header("상단")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image attributeIcon;

    [Header("중앙")]
    [SerializeField] private Image illustrationImage;

    [Header("하단")]
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("기타 UI")]
    [SerializeField] private Image highlightBorder; // 키보드/패드 선택 테두리
    [SerializeField] private Image lockInBorder;    // 첫번째 선택(락인) 테두리
    public Button selectButton;

    [Header("--- 이벤트 ---")]
    public CardSelectedEvent OnCardSelected;

    public NewCardDataSO CurrentCard { get; private set; }
    public CardInstance CurrentCardInstance { get; private set; }

    private void Awake()
    {
        // 시작 시 모든 테두리 비활성화
        SetHighlight(false);
        SetLockIn(false);
    }

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
            // Highlight Border 색상 설정 (등급별)
            if (highlightBorder != null)
            {
                highlightBorder.color = uiDb.GetRarityColor(cardData.basicInfo.rarity);
            }

            // 속성 아이콘 설정 (CardType 기반)
            if (attributeIcon != null)
            {
                attributeIcon.sprite = uiDb.GetAttributeSprite(cardData.basicInfo.type);
                attributeIcon.enabled = attributeIcon.sprite != null;
            }
        }

        // 카드 일러스트 (null 체크)
        if (illustrationImage != null)
        {
            illustrationImage.sprite = cardData.basicInfo.cardIllustration;
            illustrationImage.enabled = illustrationImage.sprite != null;
        }

        // 버튼 리스너 설정
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

    public void SetLockIn(bool isLockedIn)
    {
        if (lockInBorder != null)
        {
            lockInBorder.gameObject.SetActive(isLockedIn);
        }
    }
}
