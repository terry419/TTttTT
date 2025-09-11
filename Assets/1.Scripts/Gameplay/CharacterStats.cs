using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerHealthBar))]
public class CharacterStats : EntityStats
{
    private PlayerDataManager playerDataManager;
    public float cardSelectionInterval = 10f;

    protected override void Awake()
    {
        base.Awake();
        playerDataManager = ServiceLocator.Get<PlayerDataManager>();
        if (playerDataManager == null)
        {
            Debug.LogError($"[{GetType().Name}] CRITICAL: PlayerDataManager를 찾을 수 없습니다!");
        }
    }

    private void OnEnable()
    {
        PlayerDataManager.OnRunDataChanged += HandleRunDataChanged;
        // [수정] PlayerDataManager와 체력을 동기화하는 이벤트 구독
        OnHealthChanged += SyncHealthToDataManager;
    }

    private void OnDisable()
    {
        PlayerDataManager.OnRunDataChanged -= HandleRunDataChanged;
        OnHealthChanged -= SyncHealthToDataManager;
    }

    public override void Initialize()
    {
        // PlayerDataManager에서 이전 씬의 체력 정보를 가져옵니다.
        float previousHealth = playerDataManager.CurrentRunData.currentHealth;
        float previousMaxHealth = playerDataManager.CurrentRunData.maxHealth;

        // 이전 체력 비율을 계산합니다.
        // 만약 previousMaxHealth가 0이라면 (예: 데이터 오류), 비율을 1로 설정하여 최대 체력으로 시작합니다.
        float healthRatio = (previousMaxHealth > 0) ? (previousHealth / previousMaxHealth) : 1f;

        // 현재 씬의 아이템과 스탯을 기반으로 최종 스탯을 다시 계산합니다.
        // 이 호출 후 FinalHealth 프로퍼티는 새로운 최대 체력 값을 반환합니다.
        CalculateFinalStats();

        // 새로운 최대 체력에 이전 비율을 적용하여 현재 체력을 설정합니다.
        float newMaxHealth = FinalHealth;
        CurrentHealth = newMaxHealth * healthRatio;

        // 체력 변경 이벤트를 호출하여 UI를 업데이트하고, 변경된 체력 정보를 PlayerDataManager에 동기화합니다.
        InvokeOnHealthChanged();
    }

    protected override void Die()
    {
        Debug.Log("[CharacterStats] 플레이어가 사망했습니다. 게임오버 상태로 전환합니다.");
        gameObject.SetActive(false);
        ServiceLocator.Get<GameManager>().ChangeState(GameManager.GameState.GameOver);
    }

    public void LoadHealthFromDataManager()
    {
        if (playerDataManager != null && playerDataManager.CurrentRunData != null)
        {
            // 부모의 Heal/TakeDamage를 이용해 체력을 설정하고 이벤트를 발생시킵니다.
            float healthToSet = Mathf.Min(playerDataManager.CurrentRunData.currentHealth, FinalHealth);
            if (healthToSet > CurrentHealth)
            {
                Heal(healthToSet - CurrentHealth);
            }
            else
            {
                TakeDamage(CurrentHealth - healthToSet);
            }
        }
    }

    private void SyncHealthToDataManager(float current, float max)
    {
        if (playerDataManager != null)
        {
            playerDataManager.UpdateHealth(current, max);
        }
    }

    private void HandleRunDataChanged(RunDataChangeType changeType)
    {
        if (changeType == RunDataChangeType.Cards || changeType == RunDataChangeType.Artifacts || changeType == RunDataChangeType.All)
        {
            // 체력 비율 보존 로직 추가
            float oldMaxHealth = FinalHealth;
            float healthRatio = (oldMaxHealth > 0) ? (CurrentHealth / oldMaxHealth) : 1f;

            RecalculateAllModifiers(); // 스탯 재계산 (FinalHealth 변경)

            float newMaxHealth = FinalHealth;
            // 새 최대 체력에 맞춰 현재 체력 조정
            CurrentHealth = newMaxHealth * healthRatio;
            
            // 체력 변경 이벤트 호출
            InvokeOnHealthChanged();
        }
    }

    // [추가] 누락되었던 메서드들
    public void RecalculateAllModifiers()
    {
        foreach (var key in statModifiers.Keys)
        {
            // 영구 스탯이나 분배 포인트로 인한 모디파이어는 제거하지 않도록, 카드 인스턴스로부터 온 모디파이어만 제거합니다.
            statModifiers[key].RemoveAll(mod => mod.Source is CardInstance);
        }

        var runData = ServiceLocator.Get<PlayerDataManager>().CurrentRunData;
        if (runData == null) return;

        foreach (var card in runData.equippedCards)
        {
            AddModifier(StatType.Attack, new StatModifier(card.GetFinalDamageMultiplier(), card));
            AddModifier(StatType.AttackSpeed, new StatModifier(card.GetFinalAttackSpeedMultiplier(), card));
            AddModifier(StatType.MoveSpeed, new StatModifier(card.GetFinalMoveSpeedMultiplier(), card));
            AddModifier(StatType.Health, new StatModifier(card.GetFinalHealthMultiplier(), card));
            AddModifier(StatType.CritRate, new StatModifier(card.GetFinalCritRateMultiplier(), card));
            AddModifier(StatType.CritMultiplier, new StatModifier(card.GetFinalCritDamageMultiplier(), card));
        }

        var artifactManager = ServiceLocator.Get<ArtifactManager>();
        if (artifactManager != null)
        {
            // artifactManager.RecalculateArtifactStats();
        }

        CalculateFinalStats();
    }

    public void ApplyPermanentStats(CharacterPermanentStats permanentStats)
    {
        if (permanentStats == null) return;
        RemoveModifiersFromSource(StatSources.Permanent);
        foreach (var stat in permanentStats.investedRatios)
        {
            AddModifier(stat.Key, new StatModifier(stat.Value, StatSources.Permanent));
        }
    }

    public void ApplyAllocatedPoints(int points, CharacterPermanentStats permStats)
    {
        if (points <= 0 || permStats == null) return;
        RemoveModifiersFromSource(StatSources.Allocated);

        List<StatType> availableStats = permStats.GetUnlockedStats();
        if (availableStats.Count == 0) return;
        for (int i = 0; i < points; i++)
        {
            StatType targetStat = availableStats[Random.Range(0, availableStats.Count)];
            float weight = GetWeightForStat(targetStat);
            AddModifier(targetStat, new StatModifier(weight, StatSources.Allocated));
        }
    }

    private float GetWeightForStat(StatType stat)
    {
        return stat == StatType.Health ? 2f : 1f;
    }
}