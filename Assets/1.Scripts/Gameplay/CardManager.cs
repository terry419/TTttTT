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
}