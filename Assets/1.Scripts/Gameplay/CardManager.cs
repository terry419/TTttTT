using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

/// <summary>
/// 카드의 '동작' (획득, 장착, 합성, 사용)을 관리합니다.
/// [2단계 리팩토링] 이제 카드 '데이터'(목록)는 PlayerDataManager가 소유합니다.
/// </summary>
public class CardManager : MonoBehaviour
{
    [Header("슬롯 설정")]
    public int maxOwnedSlots = 7;
    public int maxEquipSlots = 5;

    [Header("실시간 카드 상태")]
    public CardInstance activeCard;

    private CharacterStats playerStats;
    private CancellationTokenSource _cardSelectionCts;

    // PlayerDataManager의 데이터를 사용하기 위한 참조 속성
    private PlayerDataManager _playerDataManager;
    private PlayerDataManager PlayerDataManager
    {
        get
        {
            if (_playerDataManager == null) _playerDataManager = ServiceLocator.Get<PlayerDataManager>();
            return _playerDataManager;
        }
    }

    void Awake()
    {
        if (!ServiceLocator.IsRegistered<CardManager>()) ServiceLocator.Register<CardManager>(this);
        else Destroy(gameObject);
    }

    void OnEnable() { RoundManager.OnRoundEnded += HandleRoundEnd; }
    void OnDisable()
    {
        RoundManager.OnRoundEnded -= HandleRoundEnd;
        StopCardSelectionLoop();
    }

    private void HandleRoundEnd(bool success)
    {
        StopCardSelectionLoop();
    }

    public void LinkToNewPlayer(CharacterStats newPlayerStats)
    {
        playerStats = newPlayerStats;
        RecalculateCardStats();
    }

    public CardInstance AddCard(NewCardDataSO newCardData)
    {
        if (PlayerDataManager.CurrentRunData.ownedCards.Count >= maxOwnedSlots)
        {
            if (PlayerDataManager.CurrentRunData.equippedCards.Count > 0)
            {
                int randomIndex = Random.Range(0, PlayerDataManager.CurrentRunData.equippedCards.Count);
                CardInstance cardToRemove = PlayerDataManager.CurrentRunData.equippedCards[randomIndex];
                Debug.Log($"[CardManager] 카드 슬롯 가득 참. '{cardToRemove.CardData.basicInfo.cardName}' 제거.");
                Unequip(cardToRemove);
                PlayerDataManager.CurrentRunData.ownedCards.Remove(cardToRemove);
            }
            else
            {
                Debug.LogWarning("[CardManager] 카드 슬롯이 가득 찼지만, 장착된 카드가 없어 새 카드를 추가할 수 없습니다.");
                return null;
            }
        }
        CardInstance newInstance = new CardInstance(newCardData);
        PlayerDataManager.CurrentRunData.ownedCards.Add(newInstance);
        Debug.Log($"[CardManager] 새 카드 인스턴스 추가: {newInstance.CardData.name}");
        return newInstance;
    }

    public bool Equip(CardInstance cardInstance, int index = -1)
    {
        if (cardInstance == null || PlayerDataManager.CurrentRunData.equippedCards.Count >= maxEquipSlots || !PlayerDataManager.CurrentRunData.ownedCards.Contains(cardInstance) || PlayerDataManager.CurrentRunData.equippedCards.Contains(cardInstance))
        {
            return false;
        }

        if (index != -1 && index < PlayerDataManager.CurrentRunData.equippedCards.Count)
        {
            PlayerDataManager.CurrentRunData.equippedCards.Insert(index, cardInstance);
        }
        else
        {
            PlayerDataManager.CurrentRunData.equippedCards.Add(cardInstance);
        }

        if (playerStats != null) AddCardStats(cardInstance);
        return true;
    }

    public bool Unequip(CardInstance cardInstance)
    {
        if (cardInstance == null) return false;
        bool removed = PlayerDataManager.CurrentRunData.equippedCards.Remove(cardInstance);
        if (removed && playerStats != null)
        {
            playerStats.RemoveModifiersFromSource(cardInstance);
        }
        return removed;
    }

    public void SynthesizeCard(NewCardDataSO rewardCardData, CardInstance materialCard)
    {
        if (rewardCardData == null || materialCard == null) return;

        int equippedIndex = PlayerDataManager.CurrentRunData.equippedCards.IndexOf(materialCard);
        Unequip(materialCard);
        PlayerDataManager.CurrentRunData.ownedCards.Remove(materialCard);

        var baseSO = (Random.Range(0, 2) == 0) ? rewardCardData : materialCard.CardData;
        CardInstance newEnhancedCard = new CardInstance(baseSO) { EnhancementLevel = materialCard.EnhancementLevel + 1 };

        PlayerDataManager.CurrentRunData.ownedCards.Add(newEnhancedCard);
        Equip(newEnhancedCard, equippedIndex);
    }

    private void RecalculateCardStats()
    {
        if (playerStats == null) return;

        var allOwned = new List<CardInstance>(PlayerDataManager.CurrentRunData.ownedCards);
        foreach (var card in allOwned) playerStats.RemoveModifiersFromSource(card);

        var currentEquipped = new List<CardInstance>(PlayerDataManager.CurrentRunData.equippedCards);
        PlayerDataManager.CurrentRunData.equippedCards.Clear();
        foreach (var card in currentEquipped) Equip(card);
    }

    public void StartCardSelectionLoop()
    {
        if (_cardSelectionCts != null && !_cardSelectionCts.IsCancellationRequested) return;
        _cardSelectionCts = new CancellationTokenSource();
        SelectActiveCardLoop(_cardSelectionCts.Token).Forget();
    }

    public void StopCardSelectionLoop()
    {
        if (_cardSelectionCts != null)
        {
            _cardSelectionCts.Cancel();
            _cardSelectionCts.Dispose();
            _cardSelectionCts = null;
        }
    }

    private async UniTaskVoid SelectActiveCardLoop(CancellationToken token)
    {
        SelectActiveCard();
        try
        {
            while (!token.IsCancellationRequested)
            {
                float interval = (playerStats != null) ? playerStats.cardSelectionInterval : 10f;
                await UniTask.Delay(System.TimeSpan.FromSeconds(interval), cancellationToken: token);
                SelectActiveCard();
            }
        }
        catch (System.OperationCanceledException) { /* 루프 정상 종료 */ }
    }

    private void SelectActiveCard()
    {
        if (PlayerDataManager.CurrentRunData.equippedCards.Count == 0) return;

        var selectableCards = PlayerDataManager.CurrentRunData.equippedCards.Where(c => c.CardData.selectionWeight > 0).ToList();
        if (selectableCards.Count == 0)
        {
            activeCard = PlayerDataManager.CurrentRunData.equippedCards[Random.Range(0, PlayerDataManager.CurrentRunData.equippedCards.Count)];
        }
        else
        {
            float totalWeight = selectableCards.Sum(c => c.CardData.selectionWeight);
            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            foreach (var card in selectableCards)
            {
                currentWeight += card.CardData.selectionWeight;
                if (randomValue <= currentWeight)
                {
                    activeCard = card;
                    return;
                }
            }
            activeCard = selectableCards.LastOrDefault();
        }
    }

    public void ClearAndResetDeck()
    {
        if (PlayerDataManager?.CurrentRunData == null) return;
        PlayerDataManager.CurrentRunData.ownedCards.Clear();
        PlayerDataManager.CurrentRunData.equippedCards.Clear();
        activeCard = null;
    }

    public void SwapCards(CardInstance cardA, CardInstance cardB)
    {
        if (cardA == null && cardB == null) return;

        bool isA_Equipped = cardA != null && PlayerDataManager.CurrentRunData.equippedCards.Contains(cardA);
        int indexA = isA_Equipped ? PlayerDataManager.CurrentRunData.equippedCards.IndexOf(cardA) : -1;
        bool isB_Equipped = cardB != null && PlayerDataManager.CurrentRunData.equippedCards.Contains(cardB);
        int indexB = isB_Equipped ? PlayerDataManager.CurrentRunData.equippedCards.IndexOf(cardB) : -1;

        if (isA_Equipped && isB_Equipped)
        {
            playerStats.RemoveModifiersFromSource(cardA);
            playerStats.RemoveModifiersFromSource(cardB);
            (PlayerDataManager.CurrentRunData.equippedCards[indexA], PlayerDataManager.CurrentRunData.equippedCards[indexB]) = (PlayerDataManager.CurrentRunData.equippedCards[indexB], PlayerDataManager.CurrentRunData.equippedCards[indexA]);
            AddCardStats(cardA);
            AddCardStats(cardB);
        }
        else if (isA_Equipped && !isB_Equipped)
        {
            Unequip(cardA);
            Equip(cardB, indexA);
        }
        else if (!isA_Equipped && isB_Equipped)
        {
            Unequip(cardB);
            Equip(cardA, indexB);
        }
        else if (cardA != null && cardB == null) // A(소유) -> B(빈 장착칸)
        {
            Equip(cardA, indexB);
        }
        else if (cardA == null && cardB != null) // A(빈 장착칸) -> B(소유)
        {
            Equip(cardB, indexA);
        }

        if (playerStats != null) playerStats.CalculateFinalStats();
    }

    private void AddCardStats(CardInstance cardInstance)
    {
        if (playerStats == null || cardInstance == null) return;
        playerStats.AddModifier(StatType.Attack, new StatModifier(cardInstance.GetFinalDamageMultiplier(), cardInstance));
        playerStats.AddModifier(StatType.AttackSpeed, new StatModifier(cardInstance.GetFinalAttackSpeedMultiplier(), cardInstance));
        playerStats.AddModifier(StatType.MoveSpeed, new StatModifier(cardInstance.GetFinalMoveSpeedMultiplier(), cardInstance));
        playerStats.AddModifier(StatType.Health, new StatModifier(cardInstance.GetFinalHealthMultiplier(), cardInstance));
        playerStats.AddModifier(StatType.CritRate, new StatModifier(cardInstance.GetFinalCritRateMultiplier(), cardInstance));
        playerStats.AddModifier(StatType.CritMultiplier, new StatModifier(cardInstance.GetFinalCritDamageMultiplier(), cardInstance));
    }
}
