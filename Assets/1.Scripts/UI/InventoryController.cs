using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 인벤토리 UI의 모든 동작(슬롯 업데이트, 스탯 표시, 카드 교환)을 총괄하는 컨트롤러입니다.
/// [3단계 수정] 이제 CharacterStats에 직접 의존하지 않고, PlayerDataManager의 미리보기 기능을 사용합니다.
/// </summary>
public class InventoryController : MonoBehaviour
{
    [Header("UI 프리팹")]
    [Tooltip("슬롯에 생성될 카드 UI 프리팹입니다.")]
    [SerializeField] private GameObject cardDisplayPrefab; // [신규] 생성할 프리팹 참조

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

    void Awake()
    {
        foreach (var slot in equippedSlots.Concat(ownedSlots))
        {
            slot.OnSlotClicked += HandleSlotClick;
        }
    }

    void OnEnable()
    {
        InitializeAndRefresh();
    }

    private void InitializeAndRefresh()
    {
        cardManager = ServiceLocator.Get<CardManager>();
        playerDataManager = ServiceLocator.Get<PlayerDataManager>();

        if (cardManager == null || playerDataManager == null || cardDisplayPrefab == null)
        {
            Debug.LogError("[InventoryController] 필수 매니저 또는 카드 프리팹이 연결되지 않았습니다!");
            return;
        }

        UpdateAllSlots();
        UpdateStatsPanel();
    }

    private void UpdateAllSlots()
    {
        var runData = playerDataManager.CurrentRunData;
        if (runData == null) return;

        for (int i = 0; i < equippedSlots.Count; i++)
        {
            if (i < runData.equippedCards.Count)
            {
                equippedSlots[i].Setup(runData.equippedCards[i], cardDisplayPrefab);
            }
            else
            {
                equippedSlots[i].Setup(null, cardDisplayPrefab);
            }
        }

        var ownedOnlyCards = runData.ownedCards.Except(runData.equippedCards).ToList();
        for (int i = 0; i < ownedSlots.Count; i++)
        {
            if (i >= cardManager.maxOwnedSlots - cardManager.maxEquipSlots)
            {
                ownedSlots[i].SetState(CardSlot.SlotState.Locked);
                continue;
            }

            if (i < ownedOnlyCards.Count)
            {
                ownedSlots[i].Setup(ownedOnlyCards[i], cardDisplayPrefab);
            }
            else
            {
                ownedSlots[i].Setup(null, cardDisplayPrefab);
            }
        }
    }
    private void UpdateStatsPanel()
    {
        if (playerDataManager == null) return;

        // [핵심 수정] PlayerDataManager의 미리보기 함수를 호출하여 최종 스탯을 가져옵니다.
        BaseStats previewStats = playerDataManager.CalculatePreviewStats();

        if (previewStats == null)
        {
            attackText.text = "N/A";
            attackSpeedText.text = "N/A";
            moveSpeedText.text = "N/A";
            healthText.text = "N/A";
            critRateText.text = "N/A";
            critDamageText.text = "N/A";
            return;
        }

        // 미리보기 스탯으로 UI 텍스트를 업데이트합니다.
        attackText.text = $"{previewStats.baseDamage:F1}";
        attackSpeedText.text = $"{previewStats.baseAttackSpeed:F2}";
        moveSpeedText.text = $"{previewStats.baseMoveSpeed:F1}";
        // 현재 체력은 PlayerRunData에서 직접 가져오고, 최대 체력은 미리보기 스탯을 사용합니다.
        healthText.text = $"{playerDataManager.CurrentRunData.currentHealth:F0} / {previewStats.baseHealth:F0}";
        critRateText.text = $"{previewStats.baseCritRate:F1}%";
        critDamageText.text = $"{previewStats.baseCritDamage:F0}%";
    }

    private void HandleSlotClick(CardSlot clickedSlot)
    {
        if (firstSelectedSlot == null)
        {
            if (clickedSlot.currentState == CardSlot.SlotState.Empty) return;

            firstSelectedSlot = clickedSlot;
            firstSelectedSlot.SetHighlight(true);
        }
        else
        {
            if (firstSelectedSlot == clickedSlot)
            {
                firstSelectedSlot.SetHighlight(false);
                firstSelectedSlot = null;
                return;
            }

            // cardB가 null일 수 있는 빈 슬롯과의 교환도 처리합니다.
            cardManager.SwapCards(firstSelectedSlot.currentCard, clickedSlot.currentCard);

            firstSelectedSlot.SetHighlight(false);
            firstSelectedSlot = null;

            // 카드 상태가 변경되었으므로, 모든 UI를 즉시 새로고침합니다.
            UpdateAllSlots();
            UpdateStatsPanel(); // [중요] 스탯 패널도 함께 새로고침
        }
    }
}