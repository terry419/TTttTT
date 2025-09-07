// 파일 경로: Assets/1.Scripts/UI/InventoryController.cs

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
            Debug.LogError("[InventoryController] 핵심 매니저를 찾을 수 없습니다!");
            return;
        }

        // mainPanel 활성화를 StartCoroutine보다 먼저 호출해야 합니다.
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

    // [문제 2, 3, 5 해결] 슬롯 UI 활성화/비활성화 및 데이터 설정 로직 수정
    private void UpdateCardSlots()
    {
        // 장착 슬롯 업데이트
        var equipped = playerDataManager.GetEquippedCards();
        for (int i = 0; i < equippedCardDisplays.Count; i++)
        {
            bool hasCard = i < equipped.Count;

            // 카드 UI와 빈 슬롯 UI의 활성화 상태를 명확히 제어
            equippedCardDisplays[i].gameObject.SetActive(hasCard);
            equippedEmptySlotButtons[i].gameObject.SetActive(!hasCard);

            if (hasCard)
            {
                equippedCardDisplays[i].Setup(equipped[i]); // 카드 데이터 채우기
                equippedCardDisplays[i].selectButton.interactable = isEditable;
            }
            else
            {
                equippedEmptySlotButtons[i].interactable = isEditable;
            }
        }

        // 소유 슬롯 업데이트
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

    // [문제 4 해결] PlayerDataManager에서 직접 스탯을 읽어오도록 수정
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

    // [문제 1 해결] 코루틴을 통해 UI가 활성화된 후 포커스를 설정하도록 보장
    private IEnumerator SetupNavigationAndFocus()
    {
        yield return null; // UI가 완전히 그려질 때까지 한 프레임 대기

        //SetupNavigation(); // 네비게이션 설정 로직 (필요시 주석 해제)

        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame(); // 더 안정적인 포커스 설정을 위해 프레임 끝까지 대기

        if (backButton != null && backButton.interactable)
        {
            EventSystem.current.SetSelectedGameObject(backButton.gameObject);
        }
    }

    // --- 나머지 메서드 (기존 로직 유지) ---
    // (OnSlotClicked, OnBackButtonClicked 등)
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
                // TODO: 하이라이트 UI 적용
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
        // TODO: 하이라이트 UI 제거
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
        // CardReward 씬에서는 CardRewardUIManager를 다시 활성화
        ServiceLocator.Get<CardRewardUIManager>()?.Show();
        Hide();
    }
}