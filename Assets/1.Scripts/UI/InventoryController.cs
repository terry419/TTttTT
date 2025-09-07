using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

public class InventoryController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainPanel;

    [Header("Equipped Slots")]
    [SerializeField] private List<CardDisplay> equippedCardDisplays;
    [SerializeField] private List<GameObject> equippedEmptyVisuals;
    [SerializeField] private List<Button> equippedEmptySlotButtons;

    [Header("Owned Slots")]
    [SerializeField] private List<CardDisplay> ownedCardDisplays;
    [SerializeField] private List<GameObject> ownedEmptyVisuals;
    [SerializeField] private List<Button> ownedEmptySlotButtons;
    [SerializeField] private List<GameObject> ownedSlotLocks;

    [Header("Stats Texts")]
    [SerializeField] private TextMeshProUGUI attackValueText;
    [SerializeField] private TextMeshProUGUI healthValueText;
    [SerializeField] private TextMeshProUGUI attackSpeedValueText;
    [SerializeField] private TextMeshProUGUI critRateValueText;
    [SerializeField] private TextMeshProUGUI moveSpeedValueText;
    [SerializeField] private TextMeshProUGUI critDamageValueText;

    [Header("Buttons")]
    [SerializeField] private Button backButton;

    // ���� ���� ������ ���� ����
    private CardInstance lockedInCard;
    private (bool isEquipped, int index) lockedInSlotInfo;
    private bool isEditable;
    private CardManager cardManager;
    private CharacterStats playerStats;

    void Awake()
    {
        mainPanel.SetActive(false); // ���� �ÿ��� ��Ȱ��ȭ
    }

    private void OnEnable()
    {
        // �ڷΰ��� ��ư �̺�Ʈ ����
        backButton.onClick.AddListener(OnBackButtonClicked);

        // ��� ���� ��ư�� �̺�Ʈ ������ ����
        for (int i = 0; i < equippedCardDisplays.Count; i++)
        {
            int index = i; // Ŭ���� ���� ����
            equippedCardDisplays[i].selectButton.onClick.AddListener(() => OnSlotClicked(true, index));
            equippedEmptySlotButtons[i].onClick.AddListener(() => OnSlotClicked(true, index));
        }

        for (int i = 0; i < ownedCardDisplays.Count; i++)
        {
            int index = i;
            ownedCardDisplays[i].selectButton.onClick.AddListener(() => OnSlotClicked(false, index));
            ownedEmptySlotButtons[i].onClick.AddListener(() => OnSlotClicked(false, index));
        }
    }

    private void OnDisable()
    {
        // �̺�Ʈ ������ ���� (�޸� ���� ����)
        backButton.onClick.RemoveAllListeners();
        foreach (var display in equippedCardDisplays) display.selectButton.onClick.RemoveAllListeners();
        foreach (var button in equippedEmptySlotButtons) button.onClick.RemoveAllListeners();
        foreach (var display in ownedCardDisplays) display.selectButton.onClick.RemoveAllListeners();
        foreach (var button in ownedEmptySlotButtons) button.onClick.RemoveAllListeners();
    }

    public void Show(bool editable)
    {
        this.isEditable = editable;

        // �ʿ��� �Ŵ��� ���� ��������
        cardManager = ServiceLocator.Get<CardManager>();
        var playerController = ServiceLocator.Get<PlayerController>();
        if (playerController != null)
        {
            playerStats = playerController.GetComponent<CharacterStats>();
        }

        mainPanel.SetActive(true);
        RefreshAllUI();

        // ��Ŀ�� ���� (��: ù ��° ���� ����)
        if (equippedCardDisplays.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(equippedCardDisplays[0].gameObject);
        }
    }

    public void Hide()
    {
        // ���� ���� �ʱ�ȭ �� ����
        CancelLockIn();
        mainPanel.SetActive(false);
    }

    private void OnBackButtonClicked()
    {
        // TODO: Pause ���� CardReward ������ ���� �ٸ� ������ �ϵ��� ���� �ʿ�
        // ��: UIManager.ShowPanel("PauseMenu"); �Ǵ� cardRewardUIManager.HideInventory();
        Hide();
    }

    public void RefreshAllUI()
    {
        if (cardManager == null || playerStats == null) return;

        UpdateCardSlots();
        UpdateStatsUI();
        // TODO: �׺���̼� ������Ʈ ����
    }

    private void UpdateCardSlots()
    {
        // ���� ���� ������Ʈ
        for (int i = 0; i < equippedCardDisplays.Count; i++)
        {
            if (i < cardManager.equippedCards.Count)
            {
                equippedCardDisplays[i].gameObject.SetActive(true);
                equippedEmptyVisuals[i].SetActive(false);
                equippedCardDisplays[i].Setup(cardManager.equippedCards[i]);
                equippedCardDisplays[i].selectButton.interactable = isEditable;
            }
            else
            {
                equippedCardDisplays[i].gameObject.SetActive(false);
                equippedEmptyVisuals[i].SetActive(true);
                equippedEmptySlotButtons[i].interactable = isEditable;
            }
        }

        // ���� ���� ������Ʈ
        List<CardInstance> unequippedOwnedCards = cardManager.ownedCards.Except(cardManager.equippedCards).ToList();
        for (int i = 0; i < ownedCardDisplays.Count; i++)
        {
            bool isSlotUnlocked = i < (cardManager.maxOwnedSlots - cardManager.maxEquipSlots);
            ownedSlotLocks[i].SetActive(!isSlotUnlocked);

            if (isSlotUnlocked)
            {
                if (i < unequippedOwnedCards.Count)
                {
                    ownedCardDisplays[i].gameObject.SetActive(true);
                    ownedEmptyVisuals[i].SetActive(false);
                    ownedCardDisplays[i].Setup(unequippedOwnedCards[i]);
                    ownedCardDisplays[i].selectButton.interactable = isEditable;
                }
                else
                {
                    ownedCardDisplays[i].gameObject.SetActive(false);
                    ownedEmptyVisuals[i].SetActive(true);
                    ownedEmptySlotButtons[i].interactable = isEditable;
                }
            }
            else
            {
                ownedCardDisplays[i].gameObject.SetActive(false);
                ownedEmptyVisuals[i].SetActive(false);
            }
        }
    }

    private void UpdateStatsUI()
    {
        attackValueText.text = $"{playerStats.FinalDamageBonus:F1}%";
        healthValueText.text = $"{playerStats.FinalHealth:F0}";
        attackSpeedValueText.text = $"{playerStats.FinalAttackSpeed:F2}";
        critRateValueText.text = $"{playerStats.FinalCritRate:F1}%";
        moveSpeedValueText.text = $"{playerStats.FinalMoveSpeed:F2}";
        critDamageValueText.text = $"{playerStats.FinalCritDamage:F0}%";
    }

    private void OnSlotClicked(bool isEquipped, int index)
    {
        if (!isEditable) return;

        if (lockedInCard == null) // ù ��° ī�� ����
        {
            CardInstance cardToLock = null;
            if (isEquipped)
            {
                if (index < cardManager.equippedCards.Count)
                    cardToLock = cardManager.equippedCards[index];
            }
            else
            {
                List<CardInstance> unequipped = cardManager.ownedCards.Except(cardManager.equippedCards).ToList();
                if (index < unequipped.Count)
                    cardToLock = unequipped[index];
            }

            if (cardToLock != null)
            {
                lockedInCard = cardToLock;
                lockedInSlotInfo = (isEquipped, index);
                // TODO: �ð��� ���̶���Ʈ ȿ�� ����
                Debug.Log($"ī�� ����: {lockedInCard.CardData.basicInfo.cardName}");
            }
        }
        else // �� ��° ī��(�Ǵ� �� ����) ����
        {
            // TODO: CardManager�� ī�� ��ü �Լ��� ����� ȣ��
            Debug.Log($"��ü �õ�: {lockedInCard.CardData.basicInfo.cardName} �� (isEquipped: {isEquipped}, index: {index}) ����");
            // cardManager.SwapCards(lockedInSlotInfo, (isEquipped, index));

            CancelLockIn();
            RefreshAllUI(); // ��ü �� UI ���ΰ�ħ
        }
    }

    private void CancelLockIn()
    {
        if (lockedInCard != null)
        {
            // TODO: �ð��� ���̶���Ʈ ȿ�� ����
            Debug.Log("���� ���");
        }
        lockedInCard = null;
    }

    void Update()
    {
        // ESC Ű�� ���� ���
        if (isEditable && Input.GetKeyDown(KeyCode.Escape))
        {
            if (lockedInCard != null)
            {
                CancelLockIn();
            }
            else
            {
                OnBackButtonClicked();
            }
        }
    }
}