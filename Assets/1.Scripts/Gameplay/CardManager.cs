// --- 파일명: CardManager.cs ---

using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [Header("카드 목록")]
    public List<CardDataSO> ownedCards = new List<CardDataSO>();
    public List<CardDataSO> equippedCards = new List<CardDataSO>();

    [Header("슬롯 설정")]
    public int maxOwnedSlots = 20;
    public int maxEquipSlots = 5;

    // [추가] 플레이어 스탯을 직접 제어하기 위한 참조
    private CharacterStats playerStats;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
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