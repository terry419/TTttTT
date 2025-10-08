// TTttTT/Assets/1.Scripts/UI/CardDisplay.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.Localization.Components;

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
    private LocalizeStringEvent nameLocalizeEvent;
    private LocalizeStringEvent descriptionLocalizeEvent;


    void Awake()
    {
        if (nameText != null)
        {
            nameLocalizeEvent = nameText.GetComponent<LocalizeStringEvent>();
            if (nameLocalizeEvent == null)
            {
                Debug.LogError($"[CardDisplay - {gameObject.name}] 'NameText' 오브젝트에 Localize String Event 컴포넌트가 없습니다! 프리팹을 확인해주세요.");
            }
            else
            {
                Debug.Log($"[CardDisplay - {gameObject.name}] Awake: Localize String Event 컴포넌트를 성공적으로 찾았습니다.");
            }
        }

        if (descriptionText != null)
        {
            descriptionLocalizeEvent = descriptionText.GetComponent<LocalizeStringEvent>();
            if (descriptionLocalizeEvent == null)
            {
                // ★ 디버그 로그 1: 컴포넌트 누락 확인
                Debug.LogError($"[CardDisplay - {gameObject.name}] 'DescriptionText' 오브젝트에 Localize String Event 컴포넌트가 없습니다! 프리팹을 확인해주세요.");
            }
        }
    }

    public void Setup(CardInstance cardInstance)
    {
        if (cardInstance == null)
        {
            Debug.LogError("[CardDisplay] Setup 실패: cardInstance가 null입니다.");
            return;
        }
        CurrentCardInstance = cardInstance;
        NewCardDataSO cardData = cardInstance.CardData;
        CurrentCard = cardData;

        Debug.Log($"[CardDisplay - {gameObject.name}] Setup 호출됨. 카드: {cardData.name}");

        // --- 이름 UI 설정 부분 수정 ---
        if (nameLocalizeEvent != null)
        {
            var localizedString = cardData.basicInfo.cardName;
            Debug.Log($"[CardDisplay] 로컬라이징 데이터 설정 시도. Table: '{localizedString.TableReference}', Key: '{localizedString.TableEntryReference}'");

            nameLocalizeEvent.StringReference = localizedString;
        }

        if (descriptionLocalizeEvent != null)
        {
            // ★ 디버그 로그 2: LocalizedString 정보 확인
            var localizedString = cardData.basicInfo.effectDescription;
            Debug.Log($"[CardDisplay] 설명(Description) 로컬라이징 데이터 설정 시도. Table: '{localizedString.TableReference}', Key: '{localizedString.TableEntryReference}'");

            descriptionLocalizeEvent.StringReference = localizedString;
        }

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