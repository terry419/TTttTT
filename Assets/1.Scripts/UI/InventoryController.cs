using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using UnityEngine.EventSystems;

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

    private InventoryManager inventoryManager;

    void Awake()
    {
        foreach (var slot in equippedSlots.Concat(ownedSlots))
        {
            slot.OnSlotClicked += HandleSlotClick;
        }
        if (fixedCursor != null) fixedCursor.SetActive(false);
    }

    void Start()
    {
        inventoryManager = GetComponentInParent<InventoryManager>();
        if (inventoryManager == null)
        {
            Debug.LogError("[InventoryController] 부모 오브젝트에서 InventoryManager를 찾을 수 없습니다!");
        }
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
        Debug.Log($"-- [인벤토리 상호작용] 슬롯 클릭: {clickedSlot.gameObject.name}, 상태: {clickedSlot.currentState} --");

        if (clickedSlot.currentState == CardSlot.SlotState.Locked)
        {
            Debug.Log("[인벤토리] 잠긴 슬롯은 선택할 수 없습니다.");
            return;
        }

        if (firstSelectedSlot == null)
        {
            firstSelectedSlot = clickedSlot;
            if (fixedCursor != null)
            {
                fixedCursor.SetActive(true);
                fixedCursor.transform.position = firstSelectedSlot.transform.position;
            }
            string cardName = (firstSelectedSlot.currentState == CardSlot.SlotState.Empty) ? "빈 슬롯" : firstSelectedSlot.currentCard.CardData.basicInfo.cardName;
            Debug.Log($"[인벤토리] 1차 선택(Lock-in): {cardName}");
        }
        else
        {
            Debug.Log($"[인벤토리] 2차 선택: {clickedSlot.gameObject.name}");

            if (firstSelectedSlot.currentState == CardSlot.SlotState.Empty && clickedSlot.currentState == CardSlot.SlotState.Empty)
            {
                Debug.Log("[인벤토리] 빈 슬롯끼리는 상호작용할 수 없습니다. 선택을 취소합니다.");
                CancelSelection();
                return; 
            }

            if (firstSelectedSlot == clickedSlot)
            {
                CancelSelection();
                return;
            }

            CardSlot sourceSlot = null;
            CardSlot targetSlot = null;

            if (firstSelectedSlot.currentState != CardSlot.SlotState.Empty)
            {
                sourceSlot = firstSelectedSlot;
                targetSlot = clickedSlot;
            }
            else if (clickedSlot.currentState != CardSlot.SlotState.Empty)
            {
                sourceSlot = clickedSlot;
                targetSlot = firstSelectedSlot;
            }
            else
            {
                Debug.Log("[인벤토리] 빈 슬롯끼리는 상호작용할 수 없습니다. 선택을 취소합니다.");
                CancelSelection();
                return;
            }

            Debug.Log($"[인벤토리] Source: '{sourceSlot.gameObject.name}', Target: '{targetSlot.gameObject.name}'로 결정됨");

            if (targetSlot.currentState == CardSlot.SlotState.Empty)
            {
                Debug.Log($"[인벤토리] Target이 비어있으므로 'MoveToEmptySlot' 호출");
                firstSelectedSlot = sourceSlot;
                MoveToEmptySlot(targetSlot);
            }
            else
            {
                Debug.Log($"[인벤토리] Target에 카드가 있으므로 'SwapWithFilledSlot' 호출");
                firstSelectedSlot = sourceSlot;
                SwapWithFilledSlot(targetSlot);
            }

            FinalizeSelection();
        }
    }

    private void MoveToEmptySlot(CardSlot emptySlot)
    {
        CardInstance cardToMove = firstSelectedSlot.currentCard;
        Debug.Log($"[인벤토리] 이동 시도: '{cardToMove.CardData.basicInfo.cardName}' ({firstSelectedSlot.gameObject.name}) -> 빈 슬롯 '{emptySlot.gameObject.name}'");

        bool isSourceEquipped = equippedSlots.Contains(firstSelectedSlot);
        Debug.Log($"[인벤토리] Source는 장착 슬롯인가? {isSourceEquipped}");

        // 장착 슬롯에 있던 카드라면 Unequip을 먼저 호출해야 함
        if (isSourceEquipped)
        {
            cardManager.Unequip(cardToMove);
        }

        bool isTargetEquip = equippedSlots.Contains(emptySlot);
        Debug.Log($"[인벤토리] Target은 장착 슬롯인가? {isTargetEquip}");

        if (isTargetEquip)
        {
            int targetIndex = equippedSlots.IndexOf(emptySlot);
            Debug.Log($"[인벤토리] Target이 장착 슬롯이므로 Equip 호출 (인덱스: {targetIndex})");
            cardManager.Equip(cardToMove, targetIndex);
        }

        Debug.Log("[인벤토리] 데이터 변경 완료. UI 새로고침(NotifyRunDataChanged) 요청.");
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

        string cardName = firstSelectedSlot.currentCard != null
        ? firstSelectedSlot.currentCard.CardData.basicInfo.cardName
        : "빈 슬롯";

        Debug.Log($"[인벤토리] 카드 선택 해제 (Unlock): {cardName}");
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
            // cardManager.MaxInventorySlots 개수만큼만 슬롯을 활성화(Empty)하고 나머지는 잠금(Locked) 처리합니다.
            if (i < cardManager.MaxInventorySlots)
            {
                if (i < ownedOnlyCards.Count)
                {
                    ownedSlots[i].Setup(ownedOnlyCards[i]); // 카드가 있으면 채움
                }
                else
                {
                    ownedSlots[i].Setup(null); // 카드가 없으면 빈 슬롯으로
                }
            }
            else
            {
                ownedSlots[i].SetState(CardSlot.SlotState.Locked); // 한도를 초과하면 잠금
            }
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
            Debug.Log("[인벤토리] 선택된 카드가 있어 선택을 취소합니다.");
            CancelSelection();
        }
        else
        {
            Debug.Log("[인벤토리] 선택된 카드가 없으므로 인벤토리를 닫습니다. InventoryManager.Close() 호출 시도.");
            if (inventoryManager != null)
            {
                inventoryManager.Close();
            }
            else
            {
                Debug.LogWarning("[인벤토리] InventoryManager가 없어 gameObject를 직접 비활성화합니다. 콜백이 실행되지 않습니다.");
                gameObject.SetActive(false);
            }
        }
    }
}