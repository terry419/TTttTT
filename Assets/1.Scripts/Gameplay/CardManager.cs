using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    // �̱��� �ν��Ͻ�
    public static CardManager Instance { get; private set; }

    [Header("���� ī�� ���")]
    public List<CardDataSO> ownedCards;         // DataManager�κ��� �ε�
    [Header("���� ī�� ���")]
    public List<CardDataSO> equippedCards;      // ��Ÿ�ӿ� ������ ī��

    [Header("���� ���� ����")]
    public int maxEquipSlots = 5;               // �ν����Ϳ��� ���� ����

    private void Awake()
    {
        // �̱��� �ʱ�ȭ
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

        // DataManager���� ��� ī�� ������ �ε�
        ownedCards = DataManager.Instance.GetAllCards();
        equippedCards = new List<CardDataSO>();
    }

    /// <summary>ī�带 ���� ��Ͽ� �߰�</summary>
    public bool Equip(CardDataSO card)
    {
        if (equippedCards.Count >= maxEquipSlots)
        {
            Debug.LogWarning("���� ������ ���� á���ϴ�.");
            return false;
        }
        if (!ownedCards.Contains(card))
        {
            Debug.LogWarning($"�������� ���� ī��: {card.cardID}");
            return false;
        }
        equippedCards.Add(card);
        return true;
    }

    /// <summary>���� ��Ͽ��� ī�� ����</summary>
    public bool Unequip(CardDataSO card)
    {
        return equippedCards.Remove(card);
    }

    /// <summary>���� ������ ī�� ��� ��ȯ</summary>
    public List<CardDataSO> GetEquippedCards()
    {
        return new List<CardDataSO>(equippedCards);
    }

    /// <summary>������ Ʈ���� Ÿ�Կ� �����ϴ� ���� ī�� �ߵ�</summary>
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
