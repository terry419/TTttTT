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

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
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
        if (equippedCards.Count >= maxEquipSlots) return false;
        if (!ownedCards.Contains(card)) return false;
        if (equippedCards.Contains(card)) return false;
        equippedCards.Add(card);
        return true;
    }

    public bool Unequip(CardDataSO card)
    {
        return equippedCards.Remove(card);
    }

    public List<CardDataSO> GetEquippedCards()
    {
        return new List<CardDataSO>(equippedCards);
    }

    /// <summary>
    /// 지정된 트리거 타입에 해당하는 장착 카드 효과를 발동시킵니다.
    /// </summary>
    public void HandleTrigger(TriggerType type)
    {
        foreach (var card in equippedCards)
        {
            if (card.triggerType == type)
            {
                // [수정] Execute 함수는 이제 인수 1개만 받습니다.
                // 만약 OnHit 같은 경우라면, 데미지 값을 받아와 인수 2개짜리를 호출해야 합니다.
                // 지금은 Interval만 가정하므로, 인수 1개짜리를 호출합니다.
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
        ownedCards.Remove(card1);
        ownedCards.Remove(card2);
        equippedCards.Remove(card1);
        equippedCards.Remove(card2);

        CardDataSO baseCard = Random.Range(0, 2) == 0 ? card1 : card2;
        CardDataSO enhancedCard = ScriptableObject.CreateInstance<CardDataSO>();

        enhancedCard.cardID = baseCard.cardID + "_enhanced";
        enhancedCard.cardName = baseCard.cardName + "+";
        enhancedCard.type = baseCard.type;
        enhancedCard.rarity = baseCard.rarity;
        enhancedCard.effectDescription = baseCard.effectDescription + " (강화됨)";
        enhancedCard.triggerType = baseCard.triggerType;
        enhancedCard.effectType = baseCard.effectType;
        enhancedCard.rewardAppearanceWeight = baseCard.rewardAppearanceWeight;

        enhancedCard.damageMultiplier = baseCard.damageMultiplier > 0 ? baseCard.damageMultiplier * 1.1f : 0;
        enhancedCard.attackSpeedMultiplier = baseCard.attackSpeedMultiplier > 0 ? baseCard.attackSpeedMultiplier * 1.1f : 0;
        enhancedCard.moveSpeedMultiplier = baseCard.moveSpeedMultiplier > 0 ? baseCard.moveSpeedMultiplier * 1.1f : 0;
        enhancedCard.healthMultiplier = baseCard.healthMultiplier > 0 ? baseCard.healthMultiplier * 1.1f : 0;
        enhancedCard.critRateMultiplier = baseCard.critRateMultiplier > 0 ? baseCard.critRateMultiplier * 1.1f : 0;
        enhancedCard.critDamageMultiplier = baseCard.critDamageMultiplier > 0 ? baseCard.critDamageMultiplier * 1.1f : 0;
        enhancedCard.lifestealPercentage = baseCard.lifestealPercentage > 0 ? baseCard.lifestealPercentage * 1.1f : 0;

        AddCard(enhancedCard);
    }
}