// ���� ���: Assets/1.Scripts/UI/InventoryController.cs

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainPanel;

    [Header("Equipped Slots")]
    [SerializeField] private List<CardDisplay> equippedCardDisplays;
    [SerializeField] private List<Button> equippedEmptySlotButtons;

    [Header("Owned Slots")]
    [SerializeField] private List<CardDisplay> ownedCardDisplays;
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

    private CardInstance lockedInCard;
    private (bool isEquipped, int index) lockedInSlotInfo;
    private bool isEditable;

    private CardManager cardManager;
    private PlayerDataManager playerDataManager;

    void Awake()
    {
        mainPanel.SetActive(false);
    }

    private void OnEnable()
    {
        var pdm = ServiceLocator.Get<PlayerDataManager>();
        if (pdm != null)
        {
            this.playerDataManager = pdm;
            playerDataManager.OnInventoryChanged += RefreshAllUI;
            playerDataManager.OnStatsChanged += RefreshAllUI;
        }

        backButton.onClick.AddListener(OnBackButtonClicked);
        for (int i = 0; i < equippedCardDisplays.Count; i++)
        {
            int index = i;
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
        if (playerDataManager != null)
        {
            playerDataManager.OnInventoryChanged -= RefreshAllUI;
            playerDataManager.OnStatsChanged -= RefreshAllUI;
        }

        backButton.onClick.RemoveAllListeners();
        foreach (var display in equippedCardDisplays) display.selectButton.onClick.RemoveAllListeners();
        foreach (var button in equippedEmptySlotButtons) button.onClick.RemoveAllListeners();
        foreach (var display in ownedCardDisplays) display.selectButton.onClick.RemoveAllListeners();
        foreach (var button in ownedEmptySlotButtons) button.onClick.RemoveAllListeners();
    }

    public void Show(bool editable)
    {
        this.isEditable = editable;
        cardManager = ServiceLocator.Get<CardManager>();
        playerDataManager = ServiceLocator.Get<PlayerDataManager>();

        if (cardManager == null || playerDataManager == null)
        {
            Debug.LogError("[InventoryController] �ٽ� �Ŵ����� ã�� �� �����ϴ�!");
            return;
        }

        // mainPanel Ȱ��ȭ�� StartCoroutine���� ���� ȣ���ؾ� �մϴ�.
        mainPanel.SetActive(true);
        RefreshAllUI();
        StartCoroutine(SetupNavigationAndFocus());
    }

    public void Hide()
    {
        CancelLockIn();
        mainPanel.SetActive(false);
    }

    public void RefreshAllUI()
    {
        if (playerDataManager == null) return;
        UpdateCardSlots();
        UpdateStatsUI();
    }

    // [���� 2, 3, 5 �ذ�] ���� UI Ȱ��ȭ/��Ȱ��ȭ �� ������ ���� ���� ����
    private void UpdateCardSlots()
    {
        // ���� ���� ������Ʈ
        var equipped = playerDataManager.GetEquippedCards();
        for (int i = 0; i < equippedCardDisplays.Count; i++)
        {
            bool hasCard = i < equipped.Count;

            // ī�� UI�� �� ���� UI�� Ȱ��ȭ ���¸� ��Ȯ�� ����
            equippedCardDisplays[i].gameObject.SetActive(hasCard);
            equippedEmptySlotButtons[i].gameObject.SetActive(!hasCard);

            if (hasCard)
            {
                equippedCardDisplays[i].Setup(equipped[i]); // ī�� ������ ä���
                equippedCardDisplays[i].selectButton.interactable = isEditable;
            }
            else
            {
                equippedEmptySlotButtons[i].interactable = isEditable;
            }
        }

        // ���� ���� ������Ʈ
        var owned = playerDataManager.GetOwnedCards();
        var unequippedOwnedCards = owned.Except(equipped).ToList();
        for (int i = 0; i < ownedCardDisplays.Count; i++)
        {
            bool isSlotUnlocked = i < (cardManager.maxOwnedSlots - cardManager.maxEquipSlots);
            ownedSlotLocks[i].SetActive(!isSlotUnlocked);

            if (isSlotUnlocked)
            {
                bool hasCard = i < unequippedOwnedCards.Count;
                ownedCardDisplays[i].gameObject.SetActive(hasCard);
                ownedEmptySlotButtons[i].gameObject.SetActive(!hasCard);

                if (hasCard)
                {
                    ownedCardDisplays[i].Setup(unequippedOwnedCards[i]);
                    ownedCardDisplays[i].selectButton.interactable = isEditable;
                }
                else
                {
                    ownedEmptySlotButtons[i].interactable = isEditable;
                }
            }
            else
            {
                ownedCardDisplays[i].gameObject.SetActive(false);
                ownedEmptySlotButtons[i].gameObject.SetActive(false);
            }
        }
    }

    // [���� 4 �ذ�] PlayerDataManager���� ���� ������ �о������ ����
    private void UpdateStatsUI()
    {
        var data = playerDataManager.GetRuntimeData();
        if (data == null) return;

        attackValueText.text = $"{data.FinalDamageBonus:F1}%";
        healthValueText.text = $"{data.FinalHealth:F0}";
        attackSpeedValueText.text = $"{data.FinalAttackSpeed:F2}";
        critRateValueText.text = $"{data.FinalCritRate:F1}%";
        moveSpeedValueText.text = $"{data.FinalMoveSpeed:F2}";
        critDamageValueText.text = $"{data.FinalCritDamage:F0}%";
    }

    // [���� 1 �ذ�] �ڷ�ƾ�� ���� UI�� Ȱ��ȭ�� �� ��Ŀ���� �����ϵ��� ����
    private IEnumerator SetupNavigationAndFocus()
    {
        yield return null; // UI�� ������ �׷��� ������ �� ������ ���

        //SetupNavigation(); // �׺���̼� ���� ���� (�ʿ�� �ּ� ����)

        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame(); // �� �������� ��Ŀ�� ������ ���� ������ ������ ���

        if (backButton != null && backButton.interactable)
        {
            EventSystem.current.SetSelectedGameObject(backButton.gameObject);
        }
    }

    // --- ������ �޼��� (���� ���� ����) ---
    // (OnSlotClicked, OnBackButtonClicked ��)
    private void OnSlotClicked(bool isEquippedSlot, int slotIndex)
    {
        if (!isEditable) return;
        CardInstance clickedCard = null;
        if (isEquippedSlot)
        {
            var equipped = playerDataManager.GetEquippedCards();
            if (slotIndex < equipped.Count) clickedCard = equipped[slotIndex];
        }
        else
        {
            var unequipped = playerDataManager.GetOwnedCards().Except(playerDataManager.GetEquippedCards()).ToList();
            if (slotIndex < unequipped.Count) clickedCard = unequipped[slotIndex];
        }

        if (lockedInCard == null)
        {
            if (clickedCard != null)
            {
                lockedInCard = clickedCard;
                lockedInSlotInfo = (isEquippedSlot, slotIndex);
                // TODO: ���̶���Ʈ UI ����
            }
        }
        else
        {
            if (lockedInCard == clickedCard)
            {
                CancelLockIn();
                return;
            }

            if (clickedCard != null)
            {
                cardManager.SwapCards(lockedInCard, clickedCard);
            }
            else if (isEquippedSlot)
            {
                cardManager.MoveCardToEmptyEquipSlot(lockedInCard, slotIndex);
            }
            else
            {
                if (lockedInSlotInfo.isEquipped)
                {
                    cardManager.Unequip(lockedInCard);
                }
            }
            CancelLockIn();
        }
    }

    private void CancelLockIn()
    {
        lockedInCard = null;
        // TODO: ���̶���Ʈ UI ����
    }

    void Update()
    {
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

    private void OnBackButtonClicked()
    {
        // CardReward �������� CardRewardUIManager�� �ٽ� Ȱ��ȭ
        ServiceLocator.Get<CardRewardUIManager>()?.Show();
        Hide();
    }
}