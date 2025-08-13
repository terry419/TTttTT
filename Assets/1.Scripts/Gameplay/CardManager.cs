using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static CardManager Instance { get; private set; }

    [Header("카드 목록")]
    public List<CardDataSO> ownedCards;         // 현재 소유한 카드
    public List<CardDataSO> equippedCards;      // 현재 장착된 카드

    [Header("슬롯 설정")]
    public int maxOwnedSlots = 20;              // 최대 소유 가능 카드 수
    public int maxEquipSlots = 5;               // 최대 장착 가능 카드 수

    private void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 게임 시작 시 빈 카드 목록으로 초기화
        ownedCards = new List<CardDataSO>();
        equippedCards = new List<CardDataSO>();
    }

    /// <summary>
    /// 소유 목록에 새 카드를 추가합니다. 슬롯이 가득 찼다면 랜덤한 카드를 제거하고 추가합니다.
    /// </summary>
    /// <param name="cardToAdd">추가할 카드</param>
    public void AddCard(CardDataSO cardToAdd)
    {
        if (ownedCards.Count >= maxOwnedSlots)
        {
            // 기획서: 소유 슬롯이 만석이면 랜덤한 카드 1장 소멸
            int randomIndex = Random.Range(0, ownedCards.Count);
            CardDataSO removedCard = ownedCards[randomIndex];
            ownedCards.RemoveAt(randomIndex);
            Debug.LogWarning($"소유 카드 슬롯이 가득 차서 랜덤 카드 '{removedCard.cardName}'을(를) 제거했습니다.");

            // 제거된 카드가 장착 중이었다면 장착 해제
            if (equippedCards.Contains(removedCard))
            {
                Unequip(removedCard);
            }
        }

        ownedCards.Add(cardToAdd);
        Debug.Log($"카드 획득: {cardToAdd.cardName}");
        // TODO: 카드 획득에 대한 UI 피드백 (이벤트 호출 등)
    }


    /// <summary>카드를 장착 목록에 추가</summary>
    public bool Equip(CardDataSO card)
    {
        if (equippedCards.Count >= maxEquipSlots)
        {
            Debug.LogWarning("장착 슬롯이 가득 찼습니다.");
            return false;
        }
        if (!ownedCards.Contains(card))
        {
            Debug.LogWarning($"소유하지 않은 카드: {card.cardID}");
            return false;
        }
        if (equippedCards.Contains(card))
        {
            Debug.LogWarning($"이미 장착한 카드: {card.cardID}");
            return false;
        }
        equippedCards.Add(card);
        return true;
    }

    /// <summary>장착 목록에서 카드 제거</summary>
    public bool Unequip(CardDataSO card)
    {
        return equippedCards.Remove(card);
    }

    /// <summary>현재 장착된 카드 목록 반환</summary>
    public List<CardDataSO> GetEquippedCards()
    {
        return new List<CardDataSO>(equippedCards);
    }

    /// <summary>지정된 트리거 타입에 해당하는 장착 카드 효과 발동</summary>
    public void HandleTrigger(TriggerType type)
    {
        foreach (var card in equippedCards)
        {
            if (card.triggerType == type)
            {
                // EffectExecutor의 시그니처 변경에 따라 actualDamageDealt 매개변수 추가
                EffectExecutor.Instance.Execute(card, 0f);
            }
        }
    }

    /// <summary>
    /// 주어진 카드와 합성 가능한 다른 카드를 소유하고 있는지 확인합니다.
    /// </summary>
    /// <param name="card">합성 기준이 될 카드</param>
    /// <returns>합성 가능한 카드가 있으면 true, 없으면 false</returns>
    public bool HasSynthesizablePair(CardDataSO card)
    {
        if (card == null) return false;

        // 소유한 카드 목록에서 주어진 카드와 동일한 속성(type)과 등급(rarity)을 가진 다른 카드가 있는지 찾습니다.
        foreach (var ownedCard in ownedCards)
        {
            // 자기 자신과는 합성할 수 없으므로 건너뜁니다.
            if (ownedCard == card) continue;

            if (ownedCard.type == card.type && ownedCard.rarity == card.rarity)
            {
                // 합성 가능한 짝을 찾았으므로 즉시 true를 반환합니다.
                return true;
            }
        }

        // 루프가 끝날 때까지 찾지 못하면 합성 가능한 카드가 없는 것입니다.
        return false;
    }

    /// <summary>
    /// 주어진 카드와 합성 가능한 카드 목록 전체를 반환합니다.
    /// </summary>
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

    /// <summary>
    /// 두 카드를 소모하여 10% 강화된 새 카드를 생성하고 소유 목록에 추가합니다.
    /// </summary>
    public void SynthesizeCards(CardDataSO card1, CardDataSO card2)
    {
        // 1. 재료가 될 두 카드를 소유/장착 목록에서 제거합니다.
        ownedCards.Remove(card1);
        ownedCards.Remove(card2);
        equippedCards.Remove(card1);
        equippedCards.Remove(card2);

        // 2. 두 카드 중 하나를 기반으로 랜덤하게 선택합니다.
        CardDataSO baseCard = Random.Range(0, 2) == 0 ? card1 : card2;

        // 3. 원본 에셋을 건드리지 않기 위해, 런타임에 새 인스턴스를 생성합니다.
        CardDataSO enhancedCard = ScriptableObject.CreateInstance<CardDataSO>();

        // 4. 기본 정보를 복사합니다.
        enhancedCard.cardID = baseCard.cardID + "_enhanced"; // ID는 고유해야 함
        enhancedCard.cardName = baseCard.cardName + "+";
        enhancedCard.type = baseCard.type;
        enhancedCard.rarity = baseCard.rarity;
        enhancedCard.effectDescription = baseCard.effectDescription + " (강화됨)";
        enhancedCard.triggerType = baseCard.triggerType;
        enhancedCard.effectType = baseCard.effectType;
        enhancedCard.rewardAppearanceWeight = baseCard.rewardAppearanceWeight; // 강화된 카드는 보상으로 등장하지 않게 0으로 설정할 수도 있음

        // 5. 수치적 능력치를 10% 강화합니다. (0 이상일 경우에만)
        enhancedCard.damageMultiplier = baseCard.damageMultiplier > 0 ? baseCard.damageMultiplier * 1.1f : 0;
        enhancedCard.attackSpeedMultiplier = baseCard.attackSpeedMultiplier > 0 ? baseCard.attackSpeedMultiplier * 1.1f : 0;
        enhancedCard.moveSpeedMultiplier = baseCard.moveSpeedMultiplier > 0 ? baseCard.moveSpeedMultiplier * 1.1f : 0;
        enhancedCard.healthMultiplier = baseCard.healthMultiplier > 0 ? baseCard.healthMultiplier * 1.1f : 0;
        enhancedCard.critRateMultiplier = baseCard.critRateMultiplier > 0 ? baseCard.critRateMultiplier * 1.1f : 0;
        enhancedCard.critDamageMultiplier = baseCard.critDamageMultiplier > 0 ? baseCard.critDamageMultiplier * 1.1f : 0;
        enhancedCard.lifestealPercentage = baseCard.lifestealPercentage > 0 ? baseCard.lifestealPercentage * 1.1f : 0;

        // 6. 강화된 새 카드를 소유 목록에 추가합니다.
        AddCard(enhancedCard);

        Debug.Log($"{card1.name}, {card2.name} 합성 -> {enhancedCard.name} 생성!");
    }
}