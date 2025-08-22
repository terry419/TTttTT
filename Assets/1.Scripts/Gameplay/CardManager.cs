// --- 파일명: CardManager.cs ---

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
        Debug.Log($"[생명주기] {GetType().Name} (ID: {gameObject.GetInstanceID()}) - Awake() 시작. (프레임: {Time.frameCount})");
        ServiceLocator.Register<CardManager>(this);
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
    }

    void OnEnable()
    {
        RoundManager.OnRoundEnded += HandleRoundEnd;
    }

    void OnDisable()
    {
        RoundManager.OnRoundEnded -= HandleRoundEnd;
    }

    private void OnDestroy()
    {
        Debug.Log($"[생명주기] {GetType().Name} (ID: {gameObject.GetInstanceID()}) - OnDestroy() 시작. (프레임: {Time.frameCount})");
    }

    private void HandleRoundEnd(bool success)
    {
        // "SelectActiveCard"이라는 이름으로 반복 실행되던 모든 동작을 취소합니다.
        CancelInvoke(nameof(SelectActiveCard));
        Debug.Log("[CardManager] 라운드 종료. 활성 카드 선택을 중지합니다.");
    }

    // FindPlayerStats() 함수는 이제 필요 없으므로 삭제합니다.

    public void LinkToNewPlayer(CharacterStats newPlayerStats)
    {
        Debug.Log($"[CardManager] 새로운 플레이어({newPlayerStats.name})와 연결하고 스탯 재계산을 시작합니다.");
        playerStats = newPlayerStats;
        RecalculateCardStats();
    }

    /// <summary>
    /// [신규] 카드 보상 획득 시 호출되는 새로운 통합 메서드입니다.
    /// 소유/장착 슬롯 상태에 따라 파괴/장착해제/장착을 모두 처리합니다.
    /// </summary>
    public void AcquireNewCard(CardDataSO newCard)
    {
        // 1단계: 전체 소유 슬롯이 가득 찼는지 확인하고, 가득 찼다면 하나를 파괴합니다.
        if (ownedCards.Count >= maxOwnedSlots)
        {
            Debug.Log($"소유 슬롯({maxOwnedSlots})이 가득 차, 소유 카드 중 1장을 랜덤으로 파괴합니다.");
            int randomIndex = Random.Range(0, ownedCards.Count);
            CardDataSO cardToRemove = ownedCards[randomIndex];
            
            // equippedCards 리스트에서도 제거해야 합니다.
            if (equippedCards.Contains(cardToRemove))
            {
                equippedCards.Remove(cardToRemove);
            }
            ownedCards.Remove(cardToRemove);
            Debug.Log($"[CardManager] 파괴된 카드: {cardToRemove.name}");
        }

        // 2단계: 새로운 카드를 소유 목록에 추가합니다.
        ownedCards.Add(newCard);
        Debug.Log($"[CardManager] 획득한 카드: {newCard.name}");

        // 3단계: 장착 슬롯이 가득 찼는지 확인하고, 가득 찼다면 하나를 장착 해제합니다.
        if (equippedCards.Count >= maxEquipSlots)
        {
            Debug.Log($"장착 슬롯({maxEquipSlots})이 가득 차, 장착 카드 중 1장을 랜덤으로 장착 해제합니다.");
            int randomIndex = Random.Range(0, equippedCards.Count);
            CardDataSO cardToUnequip = equippedCards[randomIndex];
            equippedCards.Remove(cardToUnequip); // 장착 목록에서만 제거
            Debug.Log($"[CardManager] 장착 해제된 카드: {cardToUnequip.name}");
        }

        // 4단계: 새로운 카드를 장착 목록에 추가합니다.
        equippedCards.Add(newCard);
        Debug.Log($"[CardManager] 장착한 카드: {newCard.name}");

        // 5단계: 모든 카드 목록 변경이 끝난 후, 스탯을 단 한 번만 재계산합니다.
        // RecalculateCardStats() 호출 제거
    }

    // [복원] PlayerInitializer에서 시작 카드를 추가할 때 사용됩니다.
    public void AddCard(CardDataSO cardToAdd)
    {
        // 이 함수는 주로 시작 카드나 특정 이벤트로 카드를 '소유'만 할 때 사용됩니다.
        // 슬롯이 가득 찼을 때의 처리는 AcquireNewCard에서 통합 관리되므로 여기서는 간단히 처리합니다.
        if (ownedCards.Count >= maxOwnedSlots)
        {
            Debug.LogWarning($"[CardManager] AddCard: 소유 슬롯({maxOwnedSlots})이 가득 차, {cardToAdd.name} 카드를 추가할 수 없습니다.");
            return;
        }
        ownedCards.Add(cardToAdd);
        Debug.Log($"[CardManager] AddCard: {cardToAdd.name} 카드를 소유 목록에 추가했습니다.");
    }

    public bool Equip(CardDataSO card)
    {
        if (equippedCards.Count >= maxEquipSlots) return false;
        if (!ownedCards.Contains(card)) return false;

        equippedCards.Add(card);
        // RecalculateCardStats() 호출 제거
        return true;
    }

    public bool Unequip(CardDataSO card)
    {
        if (!equippedCards.Contains(card)) return false;

        bool removed = equippedCards.Remove(card);
        if (removed)
        {
            // RecalculateCardStats() 호출 제거
        }
        return removed;
    }

    private void RecalculateCardStats()
    {
        if (playerStats == null)
        {
            Debug.LogError("[CardManager] PlayerStats를 찾을 수 없어 스탯을 재계산할 수 없습니다. LinkToNewPlayer가 호출되었는지 확인하세요.");
            return;
        }

        playerStats.cardDamageRatio = 0f;
        playerStats.cardAttackSpeedRatio = 0f;
        playerStats.cardMoveSpeedRatio = 0f;
        playerStats.cardHealthRatio = 0f;
        playerStats.cardCritRateRatio = 0f;
        playerStats.cardCritDamageRatio = 0f;

        foreach (var card in equippedCards)
        {
            playerStats.cardDamageRatio += card.damageMultiplier;
            playerStats.cardAttackSpeedRatio += card.attackSpeedMultiplier;
            playerStats.cardMoveSpeedRatio += card.moveSpeedMultiplier;
            playerStats.cardHealthRatio += card.healthMultiplier;
            playerStats.cardCritRateRatio += card.critRateMultiplier;
            playerStats.cardCritDamageRatio += card.critDamageMultiplier;
        }
        
        if (equippedCards.Count > 0)
        {
            string cardNames = string.Join(", ", equippedCards.Select(c => c.cardName));
            Debug.Log($"[CardManager] 카드 스탯 재계산 완료. 장착된 카드 ({equippedCards.Count}개): {cardNames}");
        }
        else
        {
            Debug.Log("[CardManager] 카드 스탯 재계산 완료. 장착된 카드가 없습니다.");
        }

        playerStats.CalculateFinalStats();
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

    private void SelectActiveCard()
    {
        if (equippedCards.Count == 0)
        {
            activeCard = null;
            return;
        }

        float totalWeight = 0f;
        foreach (var card in equippedCards)
        {
            totalWeight += Mathf.Max(0, card.selectionWeight);
        }

        if (totalWeight <= 0)
        {
            activeCard = equippedCards[0];
            return;
        }

        float randomPoint = Random.Range(0, totalWeight);
        float currentWeightSum = 0f;

        foreach (var card in equippedCards)
        {
            float weight = Mathf.Max(0, card.selectionWeight);
            if (randomPoint <= currentWeightSum + weight)
            {
                activeCard = card;
                Debug.Log($"[CardManager] 활성 카드 선택: {activeCard.cardName}");
                return;
            }
            currentWeightSum += weight;
        }

        activeCard = equippedCards[equippedCards.Count - 1];
    }

    public bool HasSynthesizablePair(CardDataSO card)
    {
        if (card == null) return false;
        foreach (var ownedCard in ownedCards)
        {
            if (ownedCard == card) continue;
            if (ownedCard.type == card.type && ownedCard.rarity == card.rarity)
            {
                return true;
            }
        }
        return false;
    }

    public List<CardDataSO> GetSynthesizablePairs(CardDataSO card)
    {
        List<CardDataSO> pairList = new List<CardDataSO>();
        if (card == null) return pairList;
        foreach (var ownedCard in ownedCards)
        {
            if (ownedCard == card) continue;
            if (ownedCard.type == card.type && ownedCard.rarity == card.rarity)
            {
                pairList.Add(ownedCard);
            }
        }
        return pairList;
    }

    public void SynthesizeCards(CardDataSO card1, CardDataSO card2)
    {
        // 합성 시에는 장착된 카드 스탯을 먼저 제거해야 함
        if (equippedCards.Contains(card1)) Unequip(card1);
        if (equippedCards.Contains(card2)) Unequip(card2);

        ownedCards.Remove(card1);
        ownedCards.Remove(card2);

        CardDataSO baseCard = Random.Range(0, 2) == 0 ? card1 : card2;
        CardDataSO enhancedCard = ScriptableObject.CreateInstance<CardDataSO>();

        enhancedCard.cardID = baseCard.cardID + "_enhanced";
        enhancedCard.cardName = baseCard.cardName + "+";
    }
}
