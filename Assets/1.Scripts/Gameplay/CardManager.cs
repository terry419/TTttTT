using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static CardManager Instance { get; private set; }

    [Header("소유 카드 목록")]
    public List<CardDataSO> ownedCards;         // DataManager로부터 로드
    [Header("장착 카드 목록")]
    public List<CardDataSO> equippedCards;      // 런타임에 장착된 카드

    [Header("장착 슬롯 제한")]
    public int maxEquipSlots = 5;               // 인스펙터에서 조정 가능

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

        // DataManager에서 모든 카드 데이터 로드
        ownedCards = DataManager.Instance.GetAllCards();
        equippedCards = new List<CardDataSO>();
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

    /// <summary>지정된 트리거 타입에 대응하는 장착 카드 발동</summary>
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
}
