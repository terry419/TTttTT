using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public int MaxEquipSlots { get; private set; }
    public int MaxInventorySlots { get; private set; }
    public int TotalSlots => MaxEquipSlots + MaxInventorySlots;

    [Header("실시간 카드 상태")]
    public CardInstance activeCard;

    private CharacterStats playerStats;
    private CancellationTokenSource _cardSelectionCts;
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

    void OnEnable()
    {
        RoundManager.OnRoundEnded += HandleRoundEnd;
        PlayerDataManager.OnRunDataChanged += HandleRunDataChanged; // 이벤트 구독
    }

    void OnDisable()
    {
        RoundManager.OnRoundEnded -= HandleRoundEnd;
        StopCardSelectionLoop();
        PlayerDataManager.OnRunDataChanged -= HandleRunDataChanged; // 이벤트 구독 해제
    }

    private void HandleRunDataChanged(RunDataChangeType changeType)
    {
        if (changeType == RunDataChangeType.Artifacts || changeType == RunDataChangeType.All)
        {
            InitializeSlotCounts(); // 슬롯 수를 다시 계산합니다.
        }
    }

    public void InitializeSlotCounts()
    {
        if (PlayerDataManager?.CurrentRunData?.characterData == null) return;

        CharacterDataSO characterData = PlayerDataManager.CurrentRunData.characterData;

        // 1. 캐릭터의 기본 슬롯 수를 가져옵니다.
        int baseEquipSlots = characterData.maxEquipSlots;
        int baseInventorySlots = characterData.maxInventorySlots;

        // 2. 유물로 인한 보너스 슬롯 수를 계산합니다.
        int bonusInventorySlots = 0;
        if (PlayerDataManager.CurrentRunData.ownedArtifacts != null)
        {
            bonusInventorySlots = PlayerDataManager.CurrentRunData.ownedArtifacts.Sum(artifact => artifact.ownedCardSlotBonus);
        }

        // 3. 최종 슬롯 수를 확정합니다.
        MaxEquipSlots = baseEquipSlots; // 장착 슬롯은 현재 기획상 보너스가 없음
        MaxInventorySlots = baseInventorySlots + bonusInventorySlots;

        Debug.Log($"[CardManager] 슬롯 설정 업데이트 완료. 장착: {MaxEquipSlots}, 보관(기본 {baseInventorySlots} + 보너스 {bonusInventorySlots}): {MaxInventorySlots}");
    }


    private void OnDestroy()
    {
        if (ServiceLocator.IsRegistered<CardManager>() && ServiceLocator.Get<CardManager>() == this)
        {
            ServiceLocator.Unregister<CardManager>(this);
        }
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
        if (PlayerDataManager.CurrentRunData.ownedCards.Count >= TotalSlots)
        {
            Debug.LogWarning($"[CardManager] 소유 가능한 카드 개수({TotalSlots})를 초과하여 장착된 카드 중 하나를 무작위로 제거합니다.");
            if (PlayerDataManager.CurrentRunData.equippedCards.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, PlayerDataManager.CurrentRunData.equippedCards.Count);
                CardInstance cardToRemove = PlayerDataManager.CurrentRunData.equippedCards[randomIndex];
                Unequip(cardToRemove);
                PlayerDataManager.CurrentRunData.ownedCards.Remove(cardToRemove);
            }
        }
        CardInstance newInstance = new CardInstance(newCardData);
        PlayerDataManager.CurrentRunData.ownedCards.Add(newInstance);
        return newInstance;
    }

    public bool Equip(CardInstance cardInstance, int index = -1)
    {
        if (cardInstance == null || PlayerDataManager.CurrentRunData.equippedCards.Contains(cardInstance))
        {
            Debug.LogWarning($"[CardManager] 카드 장착 실패: Null 또는 이미 장착된 카드 ({cardInstance?.CardData.basicInfo.cardName})");
            return false;
        }
        if (PlayerDataManager.CurrentRunData.equippedCards.Count >= MaxEquipSlots)
        {
            Debug.LogWarning($"[CardManager] 카드 장착 실패: 장착 슬롯 가득 참 ({PlayerDataManager.CurrentRunData.equippedCards.Count}/{MaxEquipSlots})");
            return false;
        }

        Debug.Log($"[CardManager] 카드 장착 시도: {cardInstance.CardData.basicInfo.cardName} (to index: {index})");
        Debug.Log($"[CardManager] 장착 전 equippedCards: {PlayerDataManager.CurrentRunData.equippedCards.Count}개");

        if (index != -1 && index < PlayerDataManager.CurrentRunData.equippedCards.Count)
        {
            PlayerDataManager.CurrentRunData.equippedCards.Insert(index, cardInstance);
        }
        else
        {
            PlayerDataManager.CurrentRunData.equippedCards.Add(cardInstance);
        }

        Debug.Log($"[CardManager] 장착 후 equippedCards: {PlayerDataManager.CurrentRunData.equippedCards.Count}개");
        if (playerStats != null) AddCardStats(cardInstance);
        return true;
    }

    // ▼▼▼ [수정] Unequip 메서드에 로그 추가 ▼▼▼
    public bool Unequip(CardInstance cardInstance)
    {
        if (cardInstance == null) return false;

        Debug.Log($"[CardManager] 카드 장착 해제 시도: {cardInstance.CardData.basicInfo.cardName}");
        Debug.Log($"[CardManager] 해제 전 equippedCards: {PlayerDataManager.CurrentRunData.equippedCards.Count}개");

        bool removed = PlayerDataManager.CurrentRunData.equippedCards.Remove(cardInstance);

        Debug.Log($"[CardManager] 해제 성공 여부: {removed}. 해제 후 equippedCards: {PlayerDataManager.CurrentRunData.equippedCards.Count}개");

        if (removed && playerStats != null)
        {
            playerStats.RemoveModifiersFromSource(cardInstance);
        }
        return removed;
    }

    public void SynthesizeCard(NewCardDataSO rewardCardData, CardInstance materialCard)
    {
        if (rewardCardData == null || materialCard == null)
        {
            Debug.LogError("[CardManager] 합성 오류: 유효하지 않은 카드 데이터입니다.");
            return;
        }

        Debug.Log($"[CardManager] 합성 시작: 보상 카드({rewardCardData.basicInfo.cardName}), 재료 카드({materialCard.CardData.basicInfo.cardName}) / 재료 레벨: {materialCard.EnhancementLevel}");
        int equippedIndex = PlayerDataManager.CurrentRunData.equippedCards.IndexOf(materialCard);
        Unequip(materialCard);
        PlayerDataManager.CurrentRunData.ownedCards.Remove(materialCard);

        var baseSO = (Random.Range(0, 2) == 0) ? rewardCardData : materialCard.CardData;
        CardInstance newEnhancedCard = new CardInstance(baseSO)
        {
            EnhancementLevel = materialCard.EnhancementLevel + 1
        };
        Debug.Log($"[CardManager] 새로운 강화 카드 생성: {newEnhancedCard.CardData.basicInfo.cardName}, 강화 레벨: {newEnhancedCard.EnhancementLevel}");

        PlayerDataManager.CurrentRunData.ownedCards.Add(newEnhancedCard);
        Equip(newEnhancedCard, equippedIndex);
    }

    public void SwapCards(CardInstance cardA, CardInstance cardB)
    {
        var runData = PlayerDataManager.CurrentRunData;
        if (runData == null || cardA == null || cardB == null || cardA == cardB) return;
        bool isA_Equipped = runData.equippedCards.Contains(cardA);
        int indexA = isA_Equipped ? runData.equippedCards.IndexOf(cardA) : -1;
        bool isB_Equipped = runData.equippedCards.Contains(cardB);
        int indexB = isB_Equipped ? runData.equippedCards.IndexOf(cardB) : -1;

        if (isA_Equipped && isB_Equipped)
        {
            playerStats.RemoveModifiersFromSource(cardA);
            playerStats.RemoveModifiersFromSource(cardB);
            (runData.equippedCards[indexA], runData.equippedCards[indexB]) = (runData.equippedCards[indexB], runData.equippedCards[indexA]);
            AddCardStats(cardA);
            AddCardStats(cardB);
        }
        else
        {
            Unequip(isA_Equipped ? cardA : cardB);
            Equip(isA_Equipped ? cardB : cardA, isA_Equipped ? indexA : indexB);
        }

        if (playerStats != null) playerStats.CalculateFinalStats();
        PlayerDataManager.NotifyRunDataChanged(RunDataChangeType.Cards);
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
                float interval = (playerStats != null) ?
                playerStats.cardSelectionInterval : 10f;
                await UniTask.Delay(System.TimeSpan.FromSeconds(interval), cancellationToken: token);
                SelectActiveCard();
            }
        }
        catch (System.OperationCanceledException) { }
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
                    Debug.Log($"[CardManager] Selected Card: {activeCard.CardData.basicInfo.cardName}");
                    return;
                }
            }
            activeCard = selectableCards.LastOrDefault();
        }
        Debug.Log($"[CardManager] Selected Card: {activeCard.CardData.basicInfo.cardName}");
    }

    public void ClearAndResetDeck()
    {
        if (PlayerDataManager?.CurrentRunData == null) return;
        PlayerDataManager.CurrentRunData.ownedCards.Clear();
        PlayerDataManager.CurrentRunData.equippedCards.Clear();
        activeCard = null;
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