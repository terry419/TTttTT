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
    private LocalizeStringEvent nameLocalizeEvent;
    private LocalizeStringEvent descriptionLocalizeEvent;


    void Awake()
    {
        if (nameText != null)
        {
            nameLocalizeEvent = nameText.GetComponent<LocalizeStringEvent>();
            if (nameLocalizeEvent == null)
            {
                Debug.LogError($"[CardDisplay - {gameObject.name}] 'NameText' ������Ʈ�� Localize String Event ������Ʈ�� �����ϴ�! �������� Ȯ�����ּ���.");
            }
            else
            {
                Debug.Log($"[CardDisplay - {gameObject.name}] Awake: Localize String Event ������Ʈ�� ���������� ã�ҽ��ϴ�.");
            }
        }

        if (descriptionText != null)
        {
            descriptionLocalizeEvent = descriptionText.GetComponent<LocalizeStringEvent>();
            if (descriptionLocalizeEvent == null)
            {
                // �� ����� �α� 1: ������Ʈ ���� Ȯ��
                Debug.LogError($"[CardDisplay - {gameObject.name}] 'DescriptionText' ������Ʈ�� Localize String Event ������Ʈ�� �����ϴ�! �������� Ȯ�����ּ���.");
            }
        }
    }

    public void Setup(CardInstance cardInstance)
    {
        if (cardInstance == null)
        {
            Debug.LogError("[CardDisplay] Setup ����: cardInstance�� null�Դϴ�.");
            return;
        }
        CurrentCardInstance = cardInstance;
        NewCardDataSO cardData = cardInstance.CardData;
        CurrentCard = cardData;

        Debug.Log($"[CardDisplay - {gameObject.name}] Setup ȣ���. ī��: {cardData.name}");

        // --- �̸� UI ���� �κ� ���� ---
        if (nameLocalizeEvent != null)
        {
            var localizedString = cardData.basicInfo.cardName;
            Debug.Log($"[CardDisplay] ���ö���¡ ������ ���� �õ�. Table: '{localizedString.TableReference}', Key: '{localizedString.TableEntryReference}'");

            nameLocalizeEvent.StringReference = localizedString;
        }

        if (descriptionLocalizeEvent != null)
        {
            // �� ����� �α� 2: LocalizedString ���� Ȯ��
            var localizedString = cardData.basicInfo.effectDescription;
            Debug.Log($"[CardDisplay] ����(Description) ���ö���¡ ������ ���� �õ�. Table: '{localizedString.TableReference}', Key: '{localizedString.TableEntryReference}'");

            descriptionLocalizeEvent.StringReference = localizedString;
        }

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