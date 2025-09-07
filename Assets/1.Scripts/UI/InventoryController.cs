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
        this.onBackAction = onBack; // 전달받은 뒤로가기 동작을 저장

        cardManager = ServiceLocator.Get<CardManager>();
        playerDataManager = ServiceLocator.Get<PlayerDataManager>();

        if (cardManager == null || playerDataManager == null)
        {
            Debug.LogError("[InventoryController] 필수 매니저를 찾을 수 없습니다!");
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

        SetupNavigation(); // 네비게이션 설정 로직 (필요시 주석 해제)

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

        // --- 첫 번째 카드 선택 (Lock-in) ---
        if (lockedInCard == null)
        {
            if (clickedCard != null) // 카드가 있는 슬롯을 처음 클릭했다면
            {
                lockedInCard = clickedCard;
                lockedInSlotInfo = (isEquippedSlot, slotIndex);
                // TODO: 선택된 카드 UI에 하이라이트 효과 추가
                Debug.Log($"[Inventory] Lock-in: {lockedInCard.CardData.basicInfo.cardName}");
            }
            // 빈 슬롯을 처음 클릭한 경우는 아무것도 하지 않음
        }
        // --- 두 번째 슬롯 선택 (교체 또는 이동) ---
        else
        {
            // 1. 락인된 카드와 다른 '카드가 있는 슬롯'을 클릭한 경우 -> Swap
            if (clickedCard != null && lockedInCard != clickedCard)
            {
                Debug.Log($"[Inventory] Swap: {lockedInCard.CardData.basicInfo.cardName} <-> {clickedCard.CardData.basicInfo.cardName}");
                cardManager.SwapCards(lockedInCard, clickedCard);
            }
            // 2. '빈 장착 슬롯'을 클릭한 경우 -> Move
            else if (clickedCard == null && isEquippedSlot)
            {
                Debug.Log($"[Inventory] Move: {lockedInCard.CardData.basicInfo.cardName} -> Equipped Slot {slotIndex}");
                cardManager.MoveCardToEmptyEquipSlot(lockedInCard, slotIndex);
            }
            // 3. 락인된 카드와 '같은 카드'를 다시 클릭하거나, 그 외의 경우 -> Lock-in 취소
            else
            {
                Debug.Log($"[Inventory] Lock-in Canceled.");
            }

            // 어떤 경우든 작업 후에는 락인을 해제합니다.
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
        Hide();
        onBackAction?.Invoke(); // 저장해둔 뒤로가기 동작을 실행
    }

    private void SetupNavigation()
    {
        // 모든 상호작용 가능한 UI 요소를 리스트에 담습니다.
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

            // 간단한 좌우 순환 내비게이션 설정
            nav.selectOnLeft = selectables[(i - 1 + selectables.Count) % selectables.Count];
            nav.selectOnRight = selectables[(i + 1) % selectables.Count];

            // TODO: 필요하다면 더 정교한 상하좌우 그리드 네비게이션을 구현할 수 있습니다.
            // 현재는 간단한 좌우 순환으로 포커스 이탈을 막는 데 중점을 둡니다.

            currentSelectable.navigation = nav;
        }
    }
}