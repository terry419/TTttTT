using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [Header("카드 목록")]
    public List<CardDataSO> ownedCards = new List<CardDataSO>();
    public List<CardDataSO> equippedCards = new List<CardDataSO>();

    [Header("슬롯 설정")]
    public int maxOwnedSlots = 7;
    public int maxEquipSlots = 5;

    [Header("실시간 카드 상태")]
    public CardDataSO activeCard;

    private CharacterStats playerStats;

    private void Awake()
    {
        // ServiceLocator에 CardManager가 아직 등록되지 않았다면,
        if (!ServiceLocator.IsRegistered<CardManager>())
        {
            // 자기 자신을 최초의 인스턴스로 등록하고 파괴되지 않도록 설정합니다.
            ServiceLocator.Register<CardManager>(this);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 만약 이미 등록된 CardManager가 있다면, 지금 생성된 나는 중복이므로 스스로를 파괴합니다.
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        RoundManager.OnRoundEnded += HandleRoundEnd;
    }

    void OnDisable()
    {
        RoundManager.OnRoundEnded -= HandleRoundEnd;
    }

    private void HandleRoundEnd(bool success)
    {
        CancelInvoke(nameof(SelectActiveCard));
    }

    public void LinkToNewPlayer(CharacterStats newPlayerStats)
    {
        playerStats = newPlayerStats;
        RecalculateCardStats();
    }

    public void AcquireNewCard(CardDataSO newCard)
    {
        if (ownedCards.Count >= maxOwnedSlots)
        {
            CardDataSO cardToRemoveFromOwned = ownedCards[0];
            if (equippedCards.Contains(cardToRemoveFromOwned))
            {
                Unequip(cardToRemoveFromOwned);
            }
            ownedCards.RemoveAt(0);
        }
        ownedCards.Add(newCard);
        Debug.Log($"[CardManager] 카드 획득: <color=green>{newCard.cardName}</color>. 현재 보유 카드 수: {ownedCards.Count}");

        if (equippedCards.Count < maxEquipSlots)
        {
            Equip(newCard);
        }
        else
        {
            int randomIndex = Random.Range(0, equippedCards.Count);
            CardDataSO cardToUnequip = equippedCards[randomIndex];
            Debug.LogWarning($"[CardManager] 장착 슬롯이 가득 차({equippedCards.Count}/{maxEquipSlots}), 랜덤 카드 '<color=orange>{cardToUnequip.cardName}</color>'을(를) 제거하고 '<color=yellow>{newCard.cardName}</color>'을(를) 장착합니다.");
            Unequip(cardToUnequip);
            Equip(newCard);
        }

        PrintEquippedCards("AcquireNewCard 직후");
    }

    public void AddCard(CardDataSO cardToAdd)
    {
        if (ownedCards.Count >= maxOwnedSlots) return;
        ownedCards.Add(cardToAdd);
    }

    public bool Equip(CardDataSO card)
    {
        // 1. playerStats가 null이라는 이유만으로 함수가 종료되지 않도록 조건을 수정합니다.
        if (equippedCards.Count >= maxEquipSlots || !ownedCards.Contains(card) || equippedCards.Contains(card))
        {
            // 실패 로그는 그대로 두거나 필요에 맞게 수정합니다.
            // Debug.LogError($"[Equip 실패] 슬롯 부족, 미소유, 또는 중복 장착 시도: {card.cardName}");
            return false;
        }

        equippedCards.Add(card);

        // 2. 스탯 적용 로직은 playerStats 참조가 유효할 때만 실행되도록 합니다.
        if (playerStats != null)
        {
            playerStats.AddModifier(StatType.Attack, new StatModifier(card.damageMultiplier, card));
            playerStats.AddModifier(StatType.AttackSpeed, new StatModifier(card.attackSpeedMultiplier, card));
            playerStats.AddModifier(StatType.MoveSpeed, new StatModifier(card.moveSpeedMultiplier, card));
            playerStats.AddModifier(StatType.Health, new StatModifier(card.healthMultiplier, card));
            playerStats.AddModifier(StatType.CritRate, new StatModifier(card.critRateMultiplier, card));
            playerStats.AddModifier(StatType.CritMultiplier, new StatModifier(card.critDamageMultiplier, card));
        }

        return true;
    }
    public bool Unequip(CardDataSO card)
    {
        // playerStats와 무관하게 리스트에서는 항상 제거되도록 합니다.
        bool removed = equippedCards.Remove(card);

        if (removed && playerStats != null) // 제거에 성공했고, playerStats가 유효할 때만 스탯을 되돌립니다.
        {
            playerStats.RemoveModifiersFromSource(card);
        }

        return removed;
    }
    private void RecalculateCardStats()
    {
        if (playerStats == null) return;

        // ▼▼▼ 1. 함수 진입 직후 상태 ▼▼▼
        PrintEquippedCards("RecalculateCardStats 진입");

        var allOwnedCards = new List<CardDataSO>(ownedCards);
        foreach (var card in allOwnedCards)
        {
            playerStats.RemoveModifiersFromSource(card);
        }

        var currentEquippedCards = new List<CardDataSO>(equippedCards);
        
        // ▼▼▼ 2. 임시 리스트 복사 후 상태 ▼▼▼
        var tempCardNames = currentEquippedCards.Select(c => c.cardName).ToArray();
        Debug.Log($"[디버그 추적] 임시 리스트 복사 완료 | 개수: {currentEquippedCards.Count} | 목록: [{string.Join(", ", tempCardNames)}]");

        equippedCards.Clear();
        
        // ▼▼▼ 3. Clear() 직후 상태 ▼▼▼
        PrintEquippedCards("equippedCards.Clear() 직후");

        foreach (var card in currentEquippedCards)
        {
            // ▼▼▼ 4. 재장착 시도하는 카드 정보 ▼▼▼
            Debug.Log($"[디버그 추적] '{card.cardName}' 재장착 시도...");
            Equip(card);
        }
        
        // ▼▼▼ 5. 함수 종료 직전 최종 상태 ▼▼▼
        PrintEquippedCards("RecalculateCardStats 종료");
    }

    public List<CardDataSO> GetEquippedCards()
    {
        return new List<CardDataSO>(equippedCards);
    }

    public void StartCardSelectionLoop()
    {
        CancelInvoke(nameof(SelectActiveCard));
        float interval = (playerStats != null) ? playerStats.cardSelectionInterval : 10f;
        InvokeRepeating(nameof(SelectActiveCard), 0f, interval);
        SelectActiveCard();
    }

    // ▼▼▼ [핵심 수정] SelectActiveCard 함수를 아래 내용으로 완전히 교체합니다. ▼▼▼
    private void SelectActiveCard()
    {
        if (equippedCards.Count == 0)
        {
            activeCard = null;
            return;
        }

        // 가중치가 0보다 큰 카드만 선택 후보로 간주합니다.
        var selectableCards = equippedCards.Where(c => c.selectionWeight > 0).ToList();
        if (selectableCards.Count == 0)
        {
            // 모든 카드의 가중치가 0이라면, 그냥 첫 번째 카드를 선택합니다.
            activeCard = equippedCards[0];
        }
        else
        {
            float totalWeight = selectableCards.Sum(card => card.selectionWeight);
            float randomPoint = Random.Range(0, totalWeight);
            float currentWeightSum = 0f;

            foreach (var card in selectableCards)
            {
                currentWeightSum += card.selectionWeight;
                if (randomPoint <= currentWeightSum)
                {
                    activeCard = card;
                    break; // 카드를 선택했으므로 루프를 즉시 종료합니다.
                }
            }
        }

        if (activeCard != null)
        {
            Debug.Log($"[CardManager] <color=cyan>활성 카드 선택됨: {activeCard.cardName}</color>");
        }
    }

    public bool HasSynthesizablePair(CardDataSO card)
    {
        if (card == null) return false;
        return ownedCards.Any(ownedCard => ownedCard.type == card.type && ownedCard.rarity == card.rarity);
    }

    public List<CardDataSO> GetSynthesizablePairs(CardDataSO card)
    {
        return ownedCards.Where(ownedCard => ownedCard.type == card.type && ownedCard.rarity == card.rarity).ToList();
    }

    public void SynthesizeCards(CardDataSO rewardCard, CardDataSO materialCard)
    {
        bool wasEquipped = equippedCards.Contains(materialCard);
        if (wasEquipped) Unequip(materialCard);
        ownedCards.Remove(materialCard);

        CardDataSO baseCard = Random.Range(0, 2) == 0 ? rewardCard : materialCard;
        CardDataSO enhancedCard = Instantiate(baseCard);
        enhancedCard.name = baseCard.name + "_Synth";
        enhancedCard.cardName = baseCard.cardName + "+";
        if (enhancedCard.baseDamage > 0) enhancedCard.baseDamage *= 1.1f;

        ownedCards.Add(enhancedCard);
        if (wasEquipped) Equip(enhancedCard);
    }

    public void ClearAndResetDeck()
    {
        ownedCards.Clear();
        equippedCards.Clear();
        Debug.LogWarning("[CardManager] 새 게임 시작. 모든 보유/장착 카드 목록을 초기화합니다.");
    }

    private void PrintEquippedCards(string context)
    {
        var cardNames = equippedCards.Select(c => c.cardName).ToArray();
        string cardList = string.Join(", ", cardNames);
        Debug.Log($"[디버그 추적] {context} | 장착 카드 수: {equippedCards.Count} | 목록: [{cardList}]");
    }
}