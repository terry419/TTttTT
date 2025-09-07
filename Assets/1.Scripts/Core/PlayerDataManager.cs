// ���� ���: Assets/1.Scripts/Core/PlayerDataManager.cs

using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    // --- ���� ������ (Private Fields) ---
    private RuntimePlayerData runtimeData;
    private List<CardInstance> ownedCards;
    private List<CardInstance> equippedCards;
    private List<ArtifactDataSO> ownedArtifacts;

    // --- �̺�Ʈ (Events) ---
    public event Action OnInventoryChanged;
    public event Action OnStatsChanged;
    public event Action<float, float> OnHealthChanged;

    private void Awake()
    {
        // ServiceLocator�� ���� �̱��� �� DontDestroyOnLoad ����
        if (ServiceLocator.IsRegistered<PlayerDataManager>())
        {
            Destroy(gameObject);
            return;
        }
        ServiceLocator.Register<PlayerDataManager>(this);
        DontDestroyOnLoad(gameObject);

        // ������ �����̳� �ʱ�ȭ
        runtimeData = new RuntimePlayerData();
        ownedCards = new List<CardInstance>();
        equippedCards = new List<CardInstance>();
        ownedArtifacts = new List<ArtifactDataSO>();

        Debug.Log("[PlayerDataManager] �ʱ�ȭ �Ϸ�.");
    }

    // --- �ֿ� API (Public Methods) ---

    #region Commands (������ ����)
    public void AcquireCard(NewCardDataSO cardData)
    {
        Debug.Log($"[PlayerDataManager] AcquireCard: {cardData.name}");
        // TODO: ī�� �߰� ���� ����
    }

    public void EquipCard(CardInstance card)
    {
        Debug.Log($"[PlayerDataManager] EquipCard: {card.CardData.name}");
        // TODO: ī�� ���� ���� ����
    }

    // ... Unequip, Swap, AddArtifact, ApplyDamage, Heal �� �ٸ� Command �޼���鵵 �ʿ信 ���� �߰� ...

    #endregion

    #region Queries (������ ��ȸ)

    public RuntimePlayerData GetRuntimeData()
    {
        return runtimeData;
    }

    public List<CardInstance> GetOwnedCards()
    {
        return new List<CardInstance>(ownedCards); // ���纻 ��ȯ
    }

    public List<CardInstance> GetEquippedCards()
    {
        return new List<CardInstance>(equippedCards); // ���纻 ��ȯ
    }

    #endregion
}