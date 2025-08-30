using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 플레이어 캐릭터의 능력치를 관리합니다. 범용 EntityStats 클래스를 상속받아 확장합니다.
/// </summary>
[RequireComponent(typeof(PlayerHealthBar))]
public class CharacterStats : EntityStats
{
    // 플레이어 전용 UI 및 변수
    private PlayerHealthBar playerHealthBar;
    public float cardSelectionInterval = 10f;

    /// <summary>
    /// 컴포넌트 초기화 시, 부모 클래스의 초기화 로직을 호출하고 플레이어 전용 컴포넌트를 설정합니다.
    /// </summary>
    protected override void Awake()
    {
        base.Awake(); // 부모 클래스(EntityStats)의 Awake()를 호출하여 statModifiers를 초기화
        playerHealthBar = GetComponent<PlayerHealthBar>();
    }

    private void OnDestroy()
    {
        var debugManager = ServiceLocator.Get<DebugManager>();
        if (debugManager != null)
        {
            debugManager.UnregisterPlayer();
        }
    }

    /// <summary>
    /// 플레이어가 피해를 입었을 때 호출됩니다. EntityStats의 기본 로직을 오버라이드합니다.
    /// </summary>
    public override void TakeDamage(float damage)
    {
        if (isInvulnerable) return;

        currentHealth -= damage;
        Debug.Log($"[PlayerStats] 플레이어 피해 입음! 데미지: {damage}, 현재 체력: {currentHealth}");

        // 플레이어 전용 체력 UI 업데이트
        playerHealthBar.UpdateHealth(currentHealth, FinalHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    /// <summary>
    /// 플레이어가 사망했을 때 호출됩니다. EntityStats의 기본 로직을 오버라이드합니다.
    /// </summary>
    public override void Die()
    {
        Debug.Log("[PlayerStats] 플레이어 사망. 게임오버 상태로 전환합니다.");
        gameObject.SetActive(false);
        ServiceLocator.Get<GameManager>().ChangeState(GameManager.GameState.GameOver);
    }

    /// <summary>
    /// 플레이어의 체력을 회복시킵니다.
    /// </summary>
    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > FinalHealth) currentHealth = FinalHealth;
        playerHealthBar.UpdateHealth(currentHealth, FinalHealth);
    }

    #region 플레이어 전용 영구/포인트 스탯 적용 로직
    // 아래 메소드들은 플레이어의 영구 스탯 및 포인트 분배와 관련된 고유 로직이므로 이 클래스에 유지됩니다.
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

    public static BaseStats CalculatePreviewStats(BaseStats baseStats, int allocatedPoints)
    {
        BaseStats previewStats = new BaseStats();
        float healthGeneBoosterRatio = allocatedPoints * 2f;
        float otherStatsGeneBoosterRatio = allocatedPoints * 1f;
        previewStats.baseHealth = baseStats.baseHealth * (1 + healthGeneBoosterRatio / 100f);
        previewStats.baseDamage = baseStats.baseDamage * (1 + otherStatsGeneBoosterRatio / 100f);
        previewStats.baseAttackSpeed = baseStats.baseAttackSpeed * (1 + otherStatsGeneBoosterRatio / 100f);
        previewStats.baseMoveSpeed = baseStats.baseMoveSpeed * (1 + otherStatsGeneBoosterRatio / 100f);
        previewStats.baseCritDamage = baseStats.baseCritDamage * (1 + otherStatsGeneBoosterRatio / 100f);
        previewStats.baseCritRate = baseStats.baseCritRate;
        return previewStats;
    }
    #endregion
}