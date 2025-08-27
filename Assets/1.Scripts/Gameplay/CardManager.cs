using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [Header("v8.0 카드 목록")]
    public List<NewCardDataSO> ownedCards = new List<NewCardDataSO>();
    public List<NewCardDataSO> equippedCards = new List<NewCardDataSO>();

    [Header("슬롯 설정")]
    public int maxOwnedSlots = 7;
    public int maxEquipSlots = 5;

    [Header("실시간 카드 상태")]
    public NewCardDataSO activeCard;

    private CharacterStats playerStats;

    void Awake()
    {
        if (!ServiceLocator.IsRegistered<CardManager>())
        {
            ServiceLocator.Register<CardManager>(this);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable() { RoundManager.OnRoundEnded += HandleRoundEnd; }
    void OnDisable() { RoundManager.OnRoundEnded -= HandleRoundEnd; }
    private void HandleRoundEnd(bool success) { CancelInvoke(nameof(SelectActiveCard)); }

    public void LinkToNewPlayer(CharacterStats newPlayerStats)
    {
        playerStats = newPlayerStats;
        RecalculateCardStats();
    }

    public void AddCard(NewCardDataSO newCard)
    {
        if (ownedCards.Count >= maxOwnedSlots) return;
        ownedCards.Add(newCard);
    }

    public bool Equip(NewCardDataSO card)
    {
        if (equippedCards.Count >= maxEquipSlots || !ownedCards.Contains(card) || equippedCards.Contains(card))
            return false;

        equippedCards.Add(card);
        if (playerStats != null)
        {
            playerStats.AddModifier(StatType.Attack, new StatModifier(card.statModifiers.damageMultiplier, card));
            playerStats.AddModifier(StatType.AttackSpeed, new StatModifier(card.statModifiers.attackSpeedMultiplier, card));
            playerStats.AddModifier(StatType.MoveSpeed, new StatModifier(card.statModifiers.moveSpeedMultiplier, card));
            playerStats.AddModifier(StatType.Health, new StatModifier(card.statModifiers.healthMultiplier, card));
            playerStats.AddModifier(StatType.CritRate, new StatModifier(card.statModifiers.critRateMultiplier, card));
            playerStats.AddModifier(StatType.CritMultiplier, new StatModifier(card.statModifiers.critDamageMultiplier, card));
        }
        return true;
    }

    public bool Unequip(NewCardDataSO card)
    {
        bool removed = equippedCards.Remove(card);
        if (removed && playerStats != null)
        {
            playerStats.RemoveModifiersFromSource(card);
        }
        return removed;
    }

    private void RecalculateCardStats()
    {
        if (playerStats == null) return;

        var allOwned = new List<NewCardDataSO>(ownedCards);
        foreach (var card in allOwned) playerStats.RemoveModifiersFromSource(card);

        var currentEquipped = new List<NewCardDataSO>(equippedCards);
        equippedCards.Clear();
        foreach (var card in currentEquipped) Equip(card);
    }

    public void StartCardSelectionLoop()
    {
        CancelInvoke(nameof(SelectActiveCard));
        float interval = (playerStats != null) ? playerStats.cardSelectionInterval : 10f;
        InvokeRepeating(nameof(SelectActiveCard), 0f, interval);
        SelectActiveCard();
    }

    private void SelectActiveCard()
    {
        if (equippedCards.Count > 0)
        {
            var selectable = equippedCards.Where(c => c.selectionWeight > 0).ToList();
            if (selectable.Count == 0)
            {
                activeCard = equippedCards[0];
            }
            else
            {
                float totalWeight = selectable.Sum(card => card.selectionWeight);
                float randomPoint = Random.Range(0, totalWeight);
                float currentWeightSum = 0f;
                foreach (var card in selectable)
                {
                    currentWeightSum += card.selectionWeight;
                    if (randomPoint <= currentWeightSum)
                    {
                        activeCard = card;
                        break;
                    }
                }
            }
            if (activeCard != null) Debug.Log($"[CardManager] 활성 카드 선택됨: {activeCard.basicInfo.cardName}");
        }
        else
        {
            activeCard = null;
        }
    }

    public void ClearAndResetDeck()
    {
        ownedCards.Clear();
        equippedCards.Clear();
        activeCard = null;
    }
}