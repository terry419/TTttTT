using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class InventoryController : MonoBehaviour
{
    [Header("UI 요소")]
    [Tooltip("카드 선택 시 고정될 커서 UI 오브젝트")]
    [SerializeField] private GameObject fixedCursor;

    [Header("슬롯 참조")]
    [SerializeField] private List<CardSlot> equippedSlots;
    [SerializeField] private List<CardSlot> ownedSlots;

    [Header("능력치 텍스트 참조")]
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private TextMeshProUGUI moveSpeedText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI critRateText;
    [SerializeField] private TextMeshProUGUI critDamageText;

    private CardManager cardManager;
    private PlayerDataManager playerDataManager;
    private CardSlot firstSelectedSlot = null;
    private bool isSubscribed = false;

    void Awake()
    {
        foreach (var slot in equippedSlots.Concat(ownedSlots))
        {
            slot.OnSlotClicked += HandleSlotClick;
        }
        if (fixedCursor != null) fixedCursor.SetActive(false);
    }

    void OnEnable()
    {
        if (!isSubscribed)
        {
            PlayerDataManager.OnRunDataChanged += OnRunDataChanged;
            isSubscribed = true;
        }
        InitializeAndRefresh();
    }

    void OnDisable()
    {
        if (isSubscribed)
        {
            PlayerDataManager.OnRunDataChanged -= OnRunDataChanged;
            isSubscribed = false;
        }
    }

    private void OnRunDataChanged(RunDataChangeType changeType)
    {
        if (changeType == RunDataChangeType.Cards || changeType == RunDataChangeType.All)
        {
            UpdateAllSlots();
            UpdateStatsPanel();
        }
    }

    private void InitializeAndRefresh()
    {
        cardManager = ServiceLocator.Get<CardManager>();
        playerDataManager = ServiceLocator.Get<PlayerDataManager>();
        if (cardManager == null || playerDataManager == null) return;
        UpdateAllSlots();
        UpdateStatsPanel();
    }

    private void HandleSlotClick(CardSlot clickedSlot)
    {
        Debug.Log($"-- [인벤토리 상호작용] 슬롯 클릭: {clickedSlot.gameObject.name} --");

        if (clickedSlot.currentState == CardSlot.SlotState.Locked)
        {
            Debug.Log("[인벤토리] 잠긴 슬롯은 선택할 수 없습니다.");
            return;
        }

        if (firstSelectedSlot == null)
        {
            if (clickedSlot.currentState == CardSlot.SlotState.Empty)
            {
                Debug.Log("[인벤토리] 빈 슬롯은 첫 번째로 선택할 수 없습니다.");
                return;
            }
            firstSelectedSlot = clickedSlot;

            if (fixedCursor != null)
            {
                fixedCursor.SetActive(true);
                fixedCursor.transform.position = firstSelectedSlot.transform.position;
            }
            Debug.Log($"[인벤토리] 카드 선택됨 (Lock-in): {firstSelectedSlot.currentCard.CardData.basicInfo.cardName}");
        }
        else
        {
            if (firstSelectedSlot == clickedSlot)
            {
                CancelSelection();
                return;
            }

            if (clickedSlot.currentState == CardSlot.SlotState.Empty)
            {
                MoveToEmptySlot(clickedSlot);
            }
            else
            {
                SwapWithFilledSlot(clickedSlot);
            }

            FinalizeSelection();
        }
    }

    private void MoveToEmptySlot(CardSlot emptySlot)
    {
        CardInstance cardToMove = firstSelectedSlot.currentCard;
        Debug.Log($"[인벤토리] 이동 시도: '{cardToMove.CardData.basicInfo.cardName}' -> 빈 슬롯 '{emptySlot.gameObject.name}'");

        // 1. 원래 위치에서 카드를 데이터상으로만 제거합니다.
        cardManager.Unequip(cardToMove);

        // 2. 목표 위치가 장착 슬롯이라면, 해당 위치에 카드를 데이터상으로 추가합니다.
        bool isTargetEquip = equippedSlots.Contains(emptySlot);
        if (isTargetEquip)
        {
            int targetIndex = equippedSlots.IndexOf(emptySlot);
            cardManager.Equip(cardToMove, targetIndex);
        }

        // 3. 데이터 변경이 완료되었음을 모든 UI에 알립니다.
        playerDataManager.NotifyRunDataChanged(RunDataChangeType.Cards);
    }

    private void SwapWithFilledSlot(CardSlot targetSlot)
    {
        string cardAName = firstSelectedSlot.currentCard.CardData.basicInfo.cardName;
        string cardBName = targetSlot.currentCard.CardData.basicInfo.cardName;
        Debug.Log($"[인벤토리] 스왑 시도: '{cardAName}' <-> '{cardBName}'");

        cardManager.SwapCards(firstSelectedSlot.currentCard, targetSlot.currentCard);
    }

    private void CancelSelection()
    {
        if (firstSelectedSlot == null) return;
        if (fixedCursor != null) fixedCursor.SetActive(false);
        Debug.Log($"[인벤토리] 카드 선택 해제 (Unlock): {firstSelectedSlot.currentCard.CardData.basicInfo.cardName}");
        firstSelectedSlot = null;
    }

    private void FinalizeSelection()
    {
        if (fixedCursor != null) fixedCursor.SetActive(false);
        firstSelectedSlot = null;
    }

    private void UpdateAllSlots()
    {
        var runData = playerDataManager.CurrentRunData;
        if (runData == null) return;

        for (int i = 0; i < equippedSlots.Count; i++)
        {
            if (i < runData.equippedCards.Count) equippedSlots[i].Setup(runData.equippedCards[i]);
            else equippedSlots[i].Setup(null);
        }

        var ownedOnlyCards = runData.ownedCards.Except(runData.equippedCards).ToList();
        for (int i = 0; i < ownedSlots.Count; i++)
        {
            if (i >= cardManager.maxOwnedSlots - cardManager.maxEquipSlots)
            {
                ownedSlots[i].SetState(CardSlot.SlotState.Locked);
                continue;
            }

            if (i < ownedOnlyCards.Count) ownedSlots[i].Setup(ownedOnlyCards[i]);
            else ownedSlots[i].Setup(null);
        }
    }

    private void UpdateStatsPanel()
    {
        if (playerDataManager == null) return;

        var oldStats = new Dictionary<string, string>
        {
            { "공격력", attackText.text }, { "공격속도", attackSpeedText.text }, { "이동속도", moveSpeedText.text },
            { "체력", healthText.text }, { "치명타 확률", critRateText.text }, { "치명타 피해", critDamageText.text }
        };

        BaseStats previewStats = playerDataManager.CalculatePreviewStats();

        if (previewStats == null) return;

        string newAttack = $"{previewStats.baseDamage:F1}";
        string newAttackSpeed = $"{previewStats.baseAttackSpeed:F2}";
        string newMoveSpeed = $"{previewStats.baseMoveSpeed:F1}";
        string newHealth = $"{playerDataManager.CurrentRunData.currentHealth:F0} / {previewStats.baseHealth:F0}";
        string newCritRate = $"{previewStats.baseCritRate:F1}%";
        string newCritDamage = $"{previewStats.baseCritDamage:F0}%";

        attackText.text = newAttack;
        attackSpeedText.text = newAttackSpeed;
        moveSpeedText.text = newMoveSpeed;
        healthText.text = newHealth;
        critRateText.text = newCritRate;
        critDamageText.text = newCritDamage;

        StringBuilder logBuilder = new StringBuilder();
        if (oldStats["공격력"] != newAttack) logBuilder.AppendLine($"공격력: {oldStats["공격력"]} -> {newAttack}");
        if (oldStats["공격속도"] != newAttackSpeed) logBuilder.AppendLine($"공격속도: {oldStats["공격속도"]} -> {newAttackSpeed}");
        if (oldStats["이동속도"] != newMoveSpeed) logBuilder.AppendLine($"이동속도: {oldStats["이동속도"]} -> {newMoveSpeed}");
        if (oldStats["체력"] != newHealth) logBuilder.AppendLine($"체력: {oldStats["체력"]} -> {newHealth}");
        if (oldStats["치명타 확률"] != newCritRate) logBuilder.AppendLine($"치명타 확률: {oldStats["치명타 확률"]} -> {newCritRate}");
        if (oldStats["치명타 피해"] != newCritDamage) logBuilder.AppendLine($"치명타 피해: {oldStats["치명타 피해"]} -> {newCritDamage}");

        if (logBuilder.Length > 0)
        {
            logBuilder.Insert(0, "--- [스탯 패널 업데이트] ---\n");
            Debug.Log(logBuilder.ToString());
        }
    }
    public void OnBackButtonPressed()
    {
        Debug.Log("[인벤토리] 뒤로가기 버튼 눌림.");
        if (firstSelectedSlot != null)
        {
            // 선택된 카드가 있으면, 선택을 취소합니다.
            CancelSelection();
        }
        else
        {
            // 선택된 카드가 없으면, 인벤토리를 닫습니다.
            Debug.Log("[인벤토리] 선택된 카드가 없으므로 인벤토리를 닫습니다.");
            gameObject.SetActive(false);
        }
    }
}