// --- 파일명: CardManager.cs ---

using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [Header("실시간 카드 상태")]
    [Tooltip("주기적으로 랜덤하게 선택되어 현재 사용 중인 카드")]
    public CardDataSO activeCard;


    [Header("카드 목록")]
    public List<CardDataSO> ownedCards = new List<CardDataSO>();
    public List<CardDataSO> equippedCards = new List<CardDataSO>();

    [Header("슬롯 설정")]
    public int maxOwnedSlots = 20;
    public int maxEquipSlots = 5;

    /// <summary>
    /// 주기적으로 활성 카드를 선택하는 루프를 시작합니다.
    /// (수정됨: 고정된 10초 대신 플레이어의 능력치를 사용)
    /// </summary>
    public void StartCardSelectionLoop()
    {
        // 이전에 실행되던 루프가 있다면 중복 실행을 막기 위해 취소합니다.
        CancelInvoke(nameof(SelectActiveCard));

        // 플레이어의 스탯을 가져옵니다.
        FindPlayerStats(); // CharacterStats 참조를 확인하는 도우미 함수

        // 플레이어 스탯에 저장된 시간 간격을 사용하고, 만약 스탯을 찾지 못하면 기본값 10초를 사용합니다.
        float interval = (playerStats != null) ? playerStats.cardSelectionInterval : 10f;

        // 스탯에 저장된 시간 간격으로 루프를 실행합니다.
        InvokeRepeating(nameof(SelectActiveCard), interval, interval);

        // 게임 시작 시 첫 카드를 바로 선택하기 위해 즉시 1회 호출합니다.
        SelectActiveCard();
    }

    /// <summary>
    /// 장착된 카드 중에서 가중치를 기반으로 다음 주기에 사용할 카드를 랜덤으로 선택합니다.
    /// </summary>
    private void SelectActiveCard()
    {
        if (equippedCards.Count == 0)
        {
            activeCard = null;
            Debug.Log("[CardManager] 장착된 카드가 없어 활성 카드를 비웠습니다.");
            return;
        }

        // 1. 모든 장착 카드의 가중치 총합을 계산합니다.
        float totalWeight = 0f;
        foreach (var card in equippedCards)
        {
            // 가중치는 최소 0 이상이어야 합니다.
            totalWeight += Mathf.Max(0, card.selectionWeight);
        }

        // 만약 모든 카드의 가중치 총합이 0이면, 랜덤 선택이 불가능하므로 첫번째 카드를 선택하고 종료합니다.
        if (totalWeight <= 0)
        {
            activeCard = equippedCards[0];
            Debug.LogWarning("[CardManager] 모든 카드의 가중치 총합이 0 이하여서, 첫 번째 카드를 활성 카드로 선택합니다.");
            return;
        }

        // 2. 0부터 총합 사이의 랜덤한 숫자를 뽑습니다.
        float randomPoint = Random.Range(0, totalWeight);
        float currentWeightSum = 0f;

        // 3. 가중치를 순서대로 더해가며 랜덤 숫자가 어디에 속하는지 찾습니다.
        foreach (var card in equippedCards)
        {
            float weight = Mathf.Max(0, card.selectionWeight);
            if (randomPoint <= currentWeightSum + weight)
            {
                activeCard = card;
                float currentInterval = (playerStats != null) ? playerStats.cardSelectionInterval : 10f;
                Debug.Log($"[CardManager] 새로운 활성 카드가 선택되었습니다: {activeCard.cardName} (가중치: {weight}, 다음 {currentInterval}초 동안 사용)");
                return; // 카드를 선택했으니 함수를 종료합니다.
            }
            currentWeightSum += weight;
        }

        // 만약 루프가 끝날 때까지 선택되지 않았다면(부동소수점 오류 등 예외상황),
        // 안전하게 마지막 카드를 선택합니다.
        activeCard = equippedCards[equippedCards.Count - 1];
    }



    // [추가] 플레이어 스탯을 직접 제어하기 위한 참조
    private CharacterStats playerStats;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
    }


    // [추가] 게임 시작 시 플레이어 스탯을 찾아 저장하는 함수
    private void FindPlayerStats()
    {
        if (playerStats == null)
        {
            // PlayerController를 통해 안전하게 CharacterStats를 찾습니다.
            if (PlayerController.Instance != null)
            {
                playerStats = PlayerController.Instance.GetComponent<CharacterStats>();
            }
        }
    }

    public void AddCard(CardDataSO cardToAdd)
    {
        if (ownedCards.Count >= maxOwnedSlots)
        {
            int randomIndex = Random.Range(0, ownedCards.Count);
            CardDataSO removedCard = ownedCards[randomIndex];
            ownedCards.RemoveAt(randomIndex);
            if (equippedCards.Contains(removedCard))
            {
                Unequip(removedCard);
            }
        }
        ownedCards.Add(cardToAdd);
    }

    public bool Equip(CardDataSO card)
    {
        FindPlayerStats(); // 플레이어 스탯 참조가 없을 경우를 대비
        if (playerStats == null)
        {
            Debug.LogError("[CardManager] PlayerStats를 찾을 수 없어 카드를 장착할 수 없습니다.");
            return false;
        }

        if (equippedCards.Count >= maxEquipSlots) return false;
        if (!ownedCards.Contains(card)) return false;
        if (equippedCards.Contains(card)) return false;

        equippedCards.Add(card);
        ApplyCardStats(card); // [추가] 카드 장착 시 스탯 적용
        return true;
    }

    public bool Unequip(CardDataSO card)
    {
        if (!equippedCards.Contains(card)) return false;

        FindPlayerStats();
        if (playerStats == null)
        {
            Debug.LogError("[CardManager] PlayerStats를 찾을 수 없어 카드 스탯을 제거할 수 없습니다.");
            // 스탯 제거는 실패했지만, 목록에서는 제거
            return equippedCards.Remove(card);
        }

        RemoveCardStats(card); // [추가] 카드 해제 시 스탯 제거
        return equippedCards.Remove(card);
    }

    // [추가] 카드의 스탯 보너스를 플레이어에게 적용하는 함수
    private void ApplyCardStats(CardDataSO card)
    {
        playerStats.cardDamageRatio += card.damageMultiplier;
        playerStats.cardAttackSpeedRatio += card.attackSpeedMultiplier;
        playerStats.cardMoveSpeedRatio += card.moveSpeedMultiplier;
        playerStats.cardHealthRatio += card.healthMultiplier;
        playerStats.cardCritRateRatio += card.critRateMultiplier;
        playerStats.cardCritDamageRatio += card.critDamageMultiplier;

        // 스탯 변경 후 반드시 최종 스탯 재계산 호출
        playerStats.CalculateFinalStats();
    }

    // [추가] 적용됐던 카드의 스탯 보너스를 플레이어에게서 제거하는 함수
    private void RemoveCardStats(CardDataSO card)
    {
        playerStats.cardDamageRatio -= card.damageMultiplier;
        playerStats.cardAttackSpeedRatio -= card.attackSpeedMultiplier;
        playerStats.cardMoveSpeedRatio -= card.moveSpeedMultiplier;
        playerStats.cardHealthRatio -= card.healthMultiplier;
        playerStats.cardCritRateRatio -= card.critRateMultiplier;
        playerStats.cardCritDamageRatio -= card.critDamageMultiplier;

        // 스탯 변경 후 반드시 최종 스탯 재계산 호출
        playerStats.CalculateFinalStats();
    }

    public List<CardDataSO> GetEquippedCards()
    {
        return new List<CardDataSO>(equippedCards);
    }

    // ... 이하 나머지 코드는 동일 ...
    public void HandleTrigger(TriggerType type)
    {
        foreach (var card in equippedCards)
        {
            if (card.triggerType == type)
            {
                EffectExecutor.Instance.Execute(card);
            }
        }
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
        // ... (이하 생략)
    }
}