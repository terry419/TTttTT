using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// �κ��丮 UI�� ��� ����(���� ������Ʈ, ���� ǥ��, ī�� ��ȯ)�� �Ѱ��ϴ� ��Ʈ�ѷ��Դϴ�.
/// [3�ܰ� ����] ���� CharacterStats�� ���� �������� �ʰ�, PlayerDataManager�� �̸����� ����� ����մϴ�.
/// </summary>
public class InventoryController : MonoBehaviour
{
    [Header("UI ������")]
    [Tooltip("���Կ� ������ ī�� UI �������Դϴ�.")]
    [SerializeField] private GameObject cardDisplayPrefab; // [�ű�] ������ ������ ����

    [Header("���� ����")]
    [SerializeField] private List<CardSlot> equippedSlots;
    [SerializeField] private List<CardSlot> ownedSlots;

    [Header("�ɷ�ġ �ؽ�Ʈ ����")]
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
            Debug.LogError("[InventoryController] �ʼ� �Ŵ��� �Ǵ� ī�� �������� ������� �ʾҽ��ϴ�!");
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

        // [�ٽ� ����] PlayerDataManager�� �̸����� �Լ��� ȣ���Ͽ� ���� ������ �����ɴϴ�.
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

        // �̸����� �������� UI �ؽ�Ʈ�� ������Ʈ�մϴ�.
        attackText.text = $"{previewStats.baseDamage:F1}";
        attackSpeedText.text = $"{previewStats.baseAttackSpeed:F2}";
        moveSpeedText.text = $"{previewStats.baseMoveSpeed:F1}";
        // ���� ü���� PlayerRunData���� ���� ��������, �ִ� ü���� �̸����� ������ ����մϴ�.
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

            // cardB�� null�� �� �ִ� �� ���԰��� ��ȯ�� ó���մϴ�.
            cardManager.SwapCards(firstSelectedSlot.currentCard, clickedSlot.currentCard);

            firstSelectedSlot.SetHighlight(false);
            firstSelectedSlot = null;

            // ī�� ���°� ����Ǿ����Ƿ�, ��� UI�� ��� ���ΰ�ħ�մϴ�.
            UpdateAllSlots();
            UpdateStatsPanel(); // [�߿�] ���� �гε� �Բ� ���ΰ�ħ
        }
    }
}