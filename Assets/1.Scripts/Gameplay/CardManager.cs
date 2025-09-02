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
        // 슬롯에 여유가 있을 경우, 기존 로직대로 카드를 추가합니다.
        if (ownedCards.Count < maxOwnedSlots)
        {
            CardInstance newInstance = new CardInstance(newCardData);
            ownedCards.Add(newInstance);
            Debug.Log($"[CardManager] 새 카드 인스턴스 추가: {newInstance.CardData.name} ({newInstance.InstanceId})");
            return newInstance;
        }
        // 슬롯이 가득 찼을 경우, 새로운 교체 로직을 실행합니다.
        else
        {
            Debug.Log("[CardManager] 소유 카드 슬롯이 가득 찼습니다. 장착된 카드 중 하나를 랜덤으로 교체합니다.");

            // 엣지 케이스: 장착된 카드가 하나도 없으면 교체할 수 없으므로, 기존처럼 null을 반환합니다.
            if (equippedCards.Count == 0)
            {
                Debug.LogWarning("[CardManager] 소유 슬롯이 가득 찼지만, 장착된 카드가 없어 교체할 수 없습니다. 카드를 획득하지 못했습니다.");
                return null;
            }

            // 1. 장착된 카드 목록에서 무작위로 하나를 선택합니다.
            int randomIndex = Random.Range(0, equippedCards.Count);
            CardInstance cardToRemove = equippedCards[randomIndex];
            Debug.Log($"[CardManager] 제거할 카드로 '{cardToRemove.CardData.basicInfo.cardName}'(이)가 선택되었습니다 (인덱스: {randomIndex}).");

            // 2. 선택된 카드를 장착 해제하고, 소유 목록에서도 제거합니다.
            Unequip(cardToRemove);
            ownedCards.Remove(cardToRemove);

            // 3. 새로 획득한 카드를 생성하고 소유 목록에 추가합니다.
            CardInstance newInstance = new CardInstance(newCardData);
            ownedCards.Add(newInstance);

            // 4. 새로 추가된 카드를 이전에 카드가 제거된 바로 그 위치에 장착합니다.
            Equip(newInstance, randomIndex);
            Debug.Log($"[CardManager] 새로운 카드 '{newInstance.CardData.basicInfo.cardName}'(을)를 추가하고 {randomIndex} 인덱스에 장착했습니다.");

            return newInstance;
        }
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

        // ▼▼▼ [수정] 기존 if문을 아래 블록으로 교체합니다 ▼▼▼
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
}