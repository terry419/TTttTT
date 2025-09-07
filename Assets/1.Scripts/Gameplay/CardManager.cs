using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [Header("카드 인스턴스 목록")]
    public List<CardInstance> ownedCards = new List<CardInstance>();
    public List<CardInstance> equippedCards = new List<CardInstance>();

    [Header("슬롯 설정")]
    public int maxOwnedSlots = 7;
    public int maxEquipSlots = 5;

    [Header("실시간 카드 상태")]
    public CardInstance activeCard;

    public CharacterStats PlayerStats => playerStats;

    private CharacterStats playerStats;
    private CancellationTokenSource _cardSelectionCts;

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
    void OnDisable()
    {
        RoundManager.OnRoundEnded -= HandleRoundEnd;
        _cardSelectionCts?.Cancel();
        _cardSelectionCts?.Dispose();
        _cardSelectionCts = null;
    }
    private void HandleRoundEnd(bool success)
    {
        _cardSelectionCts?.Cancel();
        _cardSelectionCts?.Dispose();
        _cardSelectionCts = null;
    }
    public void LinkToNewPlayer(CharacterStats newPlayerStats)
    {
        playerStats = newPlayerStats;
        RecalculateCardStats();
    }

    public CardInstance AddCard(NewCardDataSO newCardData)
    {
        if (ownedCards.Count >= maxOwnedSlots)
        {
            if (equippedCards.Count > 0)
            {
                // 1. 무작위로 장착된 카드 선택
                int randomIndex = Random.Range(0, equippedCards.Count);
                CardInstance cardToRemove = equippedCards[randomIndex];

                Debug.Log($"[CardManager] 카드 슬롯이 가득 찼습니다. 장착된 카드 '{cardToRemove.CardData.basicInfo.cardName}'(을)를 제거합니다.");

                // 2. 해당 카드 장착 해제 및 소유 목록에서 제거
                Unequip(cardToRemove);
                ownedCards.Remove(cardToRemove);
            }
            else
            {
                // 소유 슬롯은 가득 찼지만 장착된 카드가 없어 제거할 수 없는 경우
                Debug.LogWarning("[CardManager] 카드 슬롯이 가득 찼지만, 장착된 카드가 없어 새 카드를 추가할 수 없습니다.");
                return null;
            }
        }

        // 새 카드 인스턴스 생성 및 추가
        CardInstance newInstance = new CardInstance(newCardData);
        ownedCards.Add(newInstance);
        Debug.Log($"[CardManager] 새 카드 인스턴스 추가: {newInstance.CardData.name} ({newInstance.InstanceId})");
        return newInstance;
    }

    // [수정] 특정 위치에 카드를 장착할 수 있도록 index 파라미터 추가
    public bool Equip(CardInstance cardInstance, int index = -1)
    {
        if (cardInstance == null || equippedCards.Count >= maxEquipSlots || !ownedCards.Contains(cardInstance) || equippedCards.Contains(cardInstance))
        {
            return false;
        }

        if (index != -1 && index < equippedCards.Count)
        {
            equippedCards.Insert(index, cardInstance);
        }
        else
        {
            equippedCards.Add(cardInstance);
        }

        if (playerStats != null)
        {
            // [수정] CardData의 기본 수치 대신, 강화 레벨이 적용된 CardInstance의 최종 수치를 사용
            playerStats.AddModifier(StatType.Attack, new StatModifier(cardInstance.GetFinalDamageMultiplier(), cardInstance));
            playerStats.AddModifier(StatType.AttackSpeed, new StatModifier(cardInstance.GetFinalAttackSpeedMultiplier(), cardInstance));
            playerStats.AddModifier(StatType.MoveSpeed, new StatModifier(cardInstance.GetFinalMoveSpeedMultiplier(), cardInstance));
            playerStats.AddModifier(StatType.Health, new StatModifier(cardInstance.GetFinalHealthMultiplier(), cardInstance));
            playerStats.AddModifier(StatType.CritRate, new StatModifier(cardInstance.GetFinalCritRateMultiplier(), cardInstance));
            playerStats.AddModifier(StatType.CritMultiplier, new StatModifier(cardInstance.GetFinalCritDamageMultiplier(), cardInstance));
        }
        return true;
    }

    public bool Unequip(CardInstance cardInstance)
    {
        if (cardInstance == null) return false;
        bool removed = equippedCards.Remove(cardInstance);
        if (removed && playerStats != null)
        {
            playerStats.RemoveModifiersFromSource(cardInstance);
        }
        return removed;
    }

    // [추가] 카드 합성(강화) 로직
    public void SynthesizeCard(NewCardDataSO rewardCardData, CardInstance materialCard)
    {
        if (rewardCardData == null || materialCard == null)
        {
            Debug.LogError("[CardManager] 합성 오류: 유효하지 않은 카드 데이터입니다.");
            return;
        }

        Debug.Log($"[CardManager] 합성 시작: 보상 카드({rewardCardData.basicInfo.cardName}), 재료 카드({materialCard.CardData.basicInfo.cardName}) / 재료 레벨: {materialCard.EnhancementLevel}");

        // 1. 재료 카드의 장착 위치 확인 및 제거
        int equippedIndex = equippedCards.IndexOf(materialCard);
        Unequip(materialCard);
        ownedCards.Remove(materialCard);

        // 2. 보상 카드와 재료 카드 중 하나를 무작위로 선택
        var baseSO = (Random.Range(0, 2) == 0) ? rewardCardData : materialCard.CardData;
        Debug.Log($"[CardManager] 합성 베이스 카드로 '{baseSO.basicInfo.cardName}'가 선택되었습니다.");

        // 3. 강화된 새 카드 생성
        CardInstance newEnhancedCard = new CardInstance(baseSO);
        newEnhancedCard.EnhancementLevel = materialCard.EnhancementLevel + 1;
        Debug.Log($"[CardManager] 새로운 강화 카드 생성: {newEnhancedCard.CardData.basicInfo.cardName}, 강화 레벨: {newEnhancedCard.EnhancementLevel}");

        // 4. 새 카드를 소유 목록에 추가하고, 재료가 있던 위치에 장착
        ownedCards.Add(newEnhancedCard);
        Equip(newEnhancedCard, equippedIndex);
    }

    private void RecalculateCardStats()
    {
        if (playerStats == null) return;

        var allOwned = new List<CardInstance>(ownedCards);
        foreach (var card in allOwned) playerStats.RemoveModifiersFromSource(card);

        var currentEquipped = new List<CardInstance>(equippedCards);
        equippedCards.Clear();
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
            Debug.Log("[CardManager] Card selection loop stopped.");
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
        catch (System.OperationCanceledException)
        {
            Debug.Log("[CardManager] SelectActiveCardLoop safely cancelled.");
        }
    }

    private void SelectActiveCard()
    {
        if (equippedCards.Count == 0)
        {
            Debug.LogWarning("[CARD SELECT] 활성 카드를 선택하지 못했습니다. (장착된 카드가 없습니다)");
            return;
        }

        var selectableCards = equippedCards.Where(c => c.CardData.selectionWeight > 0).ToList();
        if (selectableCards.Count == 0)
        {
            // 가중치가 모두 0이면, 그냥 장착된 카드 중 하나를 무작위로 선택합니다.
            activeCard = equippedCards[Random.Range(0, equippedCards.Count)];
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
                    break;
                }
            }
        }

        if (activeCard == null && selectableCards.Count > 0)
        {
            activeCard = selectableCards.Last();
        }

        if (activeCard != null)
        {
            Debug.Log($"<color=cyan>[CARD SELECT]</color> 활성 카드 선택됨: {activeCard.CardData.basicInfo.cardName} (Lv.{activeCard.EnhancementLevel + 1})");
        }
        else
        {
            // 이 경우는 selectableCards는 있었으나 어떤 이유로 선택되지 않은 엣지 케이스입니다.
            Debug.LogWarning("[CARD SELECT] 활성 카드를 선택하지 못했습니다. (선택 가능한 카드가 있었으나, 선택 로직에 실패했습니다)");
        }
    }

    public void ClearAndResetDeck()
    {
        ownedCards.Clear();
        equippedCards.Clear();
        activeCard = null;
    }

    /// <summary>
    /// 두 카드의 위치를 서로 교환합니다. 장착-장착, 장착-소유 교환을 모두 처리합니다.
    /// </summary>
    /// <param name="cardA">교환할 첫 번째 카드</param>
    /// <param name="cardB">교환할 두 번째 카드</param>
    public void SwapCards(CardInstance cardA, CardInstance cardB)
    {
        if (cardA == null || cardB == null || cardA == cardB) return;

        // 각 카드의 장착 여부와 인덱스를 미리 파악합니다.
        bool isA_Equipped = equippedCards.Contains(cardA);
        int indexA = isA_Equipped ? equippedCards.IndexOf(cardA) : -1;

        bool isB_Equipped = equippedCards.Contains(cardB);
        int indexB = isB_Equipped ? equippedCards.IndexOf(cardB) : -1;

        // --- 경우의 수에 따른 처리 ---

        // 1. 장착 ↔ 장착 교환
        if (isA_Equipped && isB_Equipped)
        {
            Debug.Log($"[CardManager] 장착카드 교환: '{cardA.CardData.basicInfo.cardName}' ↔ '{cardB.CardData.basicInfo.cardName}'");
            // 두 카드의 스탯 보너스를 먼저 제거합니다.
            playerStats.RemoveModifiersFromSource(cardA);
            playerStats.RemoveModifiersFromSource(cardB);

            // 리스트 내에서 위치를 교환합니다.
            (equippedCards[indexA], equippedCards[indexB]) = (equippedCards[indexB], equippedCards[indexA]);

            // 교체된 위치 기준으로 스탯 보너스를 다시 적용합니다.
            // (Equip 함수는 스탯 추가 로직을 포함하므로 재사용하지 않습니다.)
            AddCardStats(cardA);
            AddCardStats(cardB);
        }
        // 2. 장착 ↔ 소유 교환
        else if (isA_Equipped && !isB_Equipped)
        {
            Debug.Log($"[CardManager] 장착↔소유 교환: '{cardA.CardData.basicInfo.cardName}' ↔ '{cardB.CardData.basicInfo.cardName}'");
            Unequip(cardA); // cardA를 장착 해제 (이제 cardA는 소유 상태)
            Equip(cardB, indexA); // cardA가 있던 자리에 cardB를 장착
        }
        else if (!isA_Equipped && isB_Equipped)
        {
            Debug.Log($"[CardManager] 소유↔장착 교환: '{cardA.CardData.basicInfo.cardName}' ↔ '{cardB.CardData.basicInfo.cardName}'");
            Unequip(cardB); // cardB를 장착 해제
            Equip(cardA, indexB); // cardB가 있던 자리에 cardA를 장착
        }
        // 3. 소유 ↔ 소유 교환
        // 데이터상 ownedCards 리스트의 순서를 바꾸는 것은 큰 의미가 없으므로,
        // InventoryController에서 UI를 새로고침하는 것만으로 충분합니다. 여기서는 별도 로직이 필요 없습니다.

        // 최종적으로 플레이어 스탯을 다시 계산하여 UI에 반영되도록 합니다.
        playerStats.CalculateFinalStats();
    }

    /// <summary>
    /// 특정 카드를 지정된 빈 장착 슬롯으로 이동시킵니다.
    /// </summary>
    /// <param name="cardToMove">이동시킬 카드 (주로 소유 카드)</param>
    /// <param name="targetEquipIndex">목표로 하는 빈 장착 슬롯 인덱스</param>
    public void MoveCardToEmptyEquipSlot(CardInstance cardToMove, int targetEquipIndex)
    {
        if (cardToMove == null || equippedCards.Count >= maxEquipSlots) return;
        if (targetEquipIndex >= maxEquipSlots) return;

        // 이미 장착된 카드였다면 먼저 장착 해제
        if (equippedCards.Contains(cardToMove))
        {
            Unequip(cardToMove);
        }

        Debug.Log($"[CardManager] '{cardToMove.CardData.basicInfo.cardName}' 카드를 {targetEquipIndex}번 장착 슬롯으로 이동.");
        Equip(cardToMove, targetEquipIndex);
        playerStats.CalculateFinalStats();
    }

    private void AddCardStats(CardInstance cardInstance)
    {
        if (playerStats == null || cardInstance == null) return;

        // CardInstance의 GetFinal- 메서드들을 사용하여 강화 레벨이 적용된 최종 수치를 가져옵니다.
        playerStats.AddModifier(StatType.Attack, new StatModifier(cardInstance.GetFinalDamageMultiplier(), cardInstance));
        playerStats.AddModifier(StatType.AttackSpeed, new StatModifier(cardInstance.GetFinalAttackSpeedMultiplier(), cardInstance));
        playerStats.AddModifier(StatType.MoveSpeed, new StatModifier(cardInstance.GetFinalMoveSpeedMultiplier(), cardInstance));
        playerStats.AddModifier(StatType.Health, new StatModifier(cardInstance.GetFinalHealthMultiplier(), cardInstance));
        playerStats.AddModifier(StatType.CritRate, new StatModifier(cardInstance.GetFinalCritRateMultiplier(), cardInstance));
        playerStats.AddModifier(StatType.CritMultiplier, new StatModifier(cardInstance.GetFinalCritDamageMultiplier(), cardInstance));
    }
}