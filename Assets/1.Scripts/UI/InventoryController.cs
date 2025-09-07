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
    private CanvasGroup canvasGroup;
    private Action onBackAction;

    void Awake()
    {
        mainPanel.SetActive(false);
        canvasGroup = mainPanel.GetComponent<CanvasGroup>(); 
        if (canvasGroup == null)
        {
            canvasGroup = mainPanel.AddComponent<CanvasGroup>(); 
        }
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

public void Show(bool editable, Action onBack)
    {
        this.isEditable = editable;
        this.onBackAction = onBack; // ���޹��� �ڷΰ��� ������ ����

        cardManager = ServiceLocator.Get<CardManager>();
        playerDataManager = ServiceLocator.Get<PlayerDataManager>();

        if (cardManager == null || playerDataManager == null)
        {
            Debug.LogError("[InventoryController] �ʼ� �Ŵ����� ã�� �� �����ϴ�!");
            return;
        }

        mainPanel.SetActive(true);
        RefreshAllUI();
        StartCoroutine(SetupNavigationAndFocus());
    }

    public void Hide()
    {
        CancelLockIn();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false; 
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

        SetupNavigation(); // �׺���̼� ���� ���� (�ʿ�� �ּ� ����)

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

        // --- ù ��° ī�� ���� (Lock-in) ---
        if (lockedInCard == null)
        {
            if (clickedCard != null) // ī�尡 �ִ� ������ ó�� Ŭ���ߴٸ�
            {
                lockedInCard = clickedCard;
                lockedInSlotInfo = (isEquippedSlot, slotIndex);
                // TODO: ���õ� ī�� UI�� ���̶���Ʈ ȿ�� �߰�
                Debug.Log($"[Inventory] Lock-in: {lockedInCard.CardData.basicInfo.cardName}");
            }
            // �� ������ ó�� Ŭ���� ���� �ƹ��͵� ���� ����
        }
        // --- �� ��° ���� ���� (��ü �Ǵ� �̵�) ---
        else
        {
            // 1. ���ε� ī��� �ٸ� 'ī�尡 �ִ� ����'�� Ŭ���� ��� -> Swap
            if (clickedCard != null && lockedInCard != clickedCard)
            {
                Debug.Log($"[Inventory] Swap: {lockedInCard.CardData.basicInfo.cardName} <-> {clickedCard.CardData.basicInfo.cardName}");
                cardManager.SwapCards(lockedInCard, clickedCard);
            }
            // 2. '�� ���� ����'�� Ŭ���� ��� -> Move
            else if (clickedCard == null && isEquippedSlot)
            {
                Debug.Log($"[Inventory] Move: {lockedInCard.CardData.basicInfo.cardName} -> Equipped Slot {slotIndex}");
                cardManager.MoveCardToEmptyEquipSlot(lockedInCard, slotIndex);
            }
            // 3. ���ε� ī��� '���� ī��'�� �ٽ� Ŭ���ϰų�, �� ���� ��� -> Lock-in ���
            else
            {
                Debug.Log($"[Inventory] Lock-in Canceled.");
            }

            // � ���� �۾� �Ŀ��� ������ �����մϴ�.
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
        Hide();
        onBackAction?.Invoke(); // �����ص� �ڷΰ��� ������ ����
    }

    private void SetupNavigation()
    {
        // ��� ��ȣ�ۿ� ������ UI ��Ҹ� ����Ʈ�� ����ϴ�.
        var selectables = new List<Selectable>();
        selectables.AddRange(equippedCardDisplays.Where(d => d.gameObject.activeSelf).Select(d => d.selectButton));
        selectables.AddRange(equippedEmptySlotButtons.Where(b => b.gameObject.activeSelf));
        selectables.AddRange(ownedCardDisplays.Where(d => d.gameObject.activeSelf).Select(d => d.selectButton));
        selectables.AddRange(ownedEmptySlotButtons.Where(b => b.gameObject.activeSelf));

        if (backButton.interactable)
        {
            selectables.Add(backButton);
        }

        if (selectables.Count <= 1) return;

        for (int i = 0; i < selectables.Count; i++)
        {
            var currentSelectable = selectables[i];
            Navigation nav = currentSelectable.navigation;
            nav.mode = Navigation.Mode.Explicit;

            // ������ �¿� ��ȯ ������̼� ����
            nav.selectOnLeft = selectables[(i - 1 + selectables.Count) % selectables.Count];
            nav.selectOnRight = selectables[(i + 1) % selectables.Count];

            // TODO: �ʿ��ϴٸ� �� ������ �����¿� �׸��� �׺���̼��� ������ �� �ֽ��ϴ�.
            // ����� ������ �¿� ��ȯ���� ��Ŀ�� ��Ż�� ���� �� ������ �Ӵϴ�.

            currentSelectable.navigation = nav;
        }
    }
}