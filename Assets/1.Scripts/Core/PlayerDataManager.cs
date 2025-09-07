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

    // --- �ܺ� �Ŵ��� ���� ---
    private CardManager cardManager;
    private CharacterStats characterStats;

    private void Awake()
    {
        if (ServiceLocator.IsRegistered<PlayerDataManager>())
        {
            Destroy(gameObject);
            return;
        }
        ServiceLocator.Register<PlayerDataManager>(this);
        DontDestroyOnLoad(gameObject);

        runtimeData = new RuntimePlayerData();
        ownedCards = new List<CardInstance>();
        equippedCards = new List<CardInstance>();
        ownedArtifacts = new List<ArtifactDataSO>();

        Debug.Log("[PlayerDataManager] �ʱ�ȭ �Ϸ�.");
    }

    private void OnDestroy()
    {
        UnlinkManagers(); // ���� ���� �� �޸� ���� ������ ���� ���� ����
    }

    // PlayerInitializer�� ȣ���� �� �ʱ� ���� �� ���� �޼���
    public void LinkManagers(CardManager cm, CharacterStats cs)
    {
        UnlinkManagers(); // ���� ��, ���� ������ �ִٸ� �����ϰ� ����

        this.cardManager = cm;
        this.characterStats = cs;

        // �ٸ� �Ŵ������� '���'�� '����' ����
        if (cardManager != null) cardManager.OnInventoryChanged += HandleInventoryChanged;
        if (characterStats != null)
        {
            characterStats.OnFinalStatsCalculated.AddListener(HandleStatsChanged);
            characterStats.OnHealthChanged += HandleHealthChanged;
        }

        Debug.Log("[PlayerDataManager] CardManager �� CharacterStats �̺�Ʈ ���� �Ϸ�.");

        // ���� ����, ���� ���¸� �� �� ����ȭ
        if (cs != null)
        {
            HandleInventoryChanged();
            HandleStatsChanged();
            HandleHealthChanged(cs.currentHealth, cs.FinalHealth);
        }
    }

    // ��� �̺�Ʈ ������ �����ϰ� �����ϴ� �޼���
    private void UnlinkManagers()
    {
        if (cardManager != null) cardManager.OnInventoryChanged -= HandleInventoryChanged;
        if (characterStats != null)
        {
            characterStats.OnFinalStatsCalculated.RemoveListener(HandleStatsChanged);
            characterStats.OnHealthChanged -= HandleHealthChanged;
        }
    }

    // --- �̺�Ʈ �ڵ鷯 (��� ���� �� �߾� �̺�Ʈ ����) ---
    private void HandleInventoryChanged()
    {
        if (cardManager == null || characterStats == null) return;

        // CardManager�κ��� �ֽ� ����� ������ ���� �����͸� ����
        this.ownedCards = cardManager.ownedCards;
        this.equippedCards = cardManager.equippedCards;

        // �߾� �̺�Ʈ '����'
        OnInventoryChanged?.Invoke();

        // �κ��丮�� �ٲ�� ���ȵ� �ٲ�Ƿ�, ���� ������ '��û'
        characterStats.CalculateFinalStats();
    }

    private void HandleStatsChanged()
    {
        if (characterStats == null) return;

        // CharacterStats�κ��� �ֽ� ��� ����� ������ runtimeData�� 'ĳ��'
        runtimeData.FinalDamageBonus = characterStats.FinalDamageBonus;
        runtimeData.FinalAttackSpeed = characterStats.FinalAttackSpeed;
        runtimeData.FinalMoveSpeed = characterStats.FinalMoveSpeed;
        runtimeData.FinalHealth = characterStats.FinalHealth;
        runtimeData.FinalCritRate = characterStats.FinalCritRate;
        runtimeData.FinalCritDamage = characterStats.FinalCritDamage;

        // �߾� �̺�Ʈ '����'
        OnStatsChanged?.Invoke();
    }

    private void HandleHealthChanged(float current, float max)
    {
        // CharacterStats�κ��� �ֽ� ü�� ������ ������ runtimeData�� 'ĳ��'
        runtimeData.CurrentHealth = current;

        // �߾� �̺�Ʈ '����'
        OnHealthChanged?.Invoke(current, max);
    }

    #region Queries (������ ��ȸ API)

    public RuntimePlayerData GetRuntimeData()
    {
        return runtimeData;
    }

    public List<CardInstance> GetOwnedCards()
    {
        return new List<CardInstance>(ownedCards); // �ܺ� ������ ���� ���� ���纻 ��ȯ
    }

    public List<CardInstance> GetEquippedCards()
    {
        return new List<CardInstance>(equippedCards); // �ܺ� ������ ���� ���� ���纻 ��ȯ
    }

    #endregion
}