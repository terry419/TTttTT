using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(PlayerHealthBar))]
public class CharacterStats : MonoBehaviour
{
    [Header("기본 능력치")]
    public BaseStats stats;

    [Header("현재 상태 (런타임)")]
    public float currentHealth;
    public bool isInvulnerable = false;

    [Header("이벤트")]
    public UnityEvent OnFinalStatsCalculated = new UnityEvent();

    private PlayerHealthBar playerHealthBar;
    public float cardSelectionInterval = 10f;

    private readonly Dictionary<StatType, List<StatModifier>> statModifiers = new Dictionary<StatType, List<StatModifier>>();

    // [리팩토링] 최종 스탯을 실시간으로 계산하는 프로퍼티 (올바른 StatType 사용)
    public float FinalDamage => CalculateFinalValue(StatType.Attack, stats.baseDamage);
    public float FinalAttackSpeed => Mathf.Max(0.1f, CalculateFinalValue(StatType.AttackSpeed, stats.baseAttackSpeed));
    public float FinalMoveSpeed => Mathf.Max(0f, CalculateFinalValue(StatType.MoveSpeed, stats.baseMoveSpeed));
    public float FinalHealth => Mathf.Max(1f, CalculateFinalValue(StatType.Health, stats.baseHealth));
    public float FinalCritRate => Mathf.Clamp(CalculateFinalValue(StatType.CritRate, stats.baseCritRate), 0f, 100f);
    public float FinalCritDamage => Mathf.Max(0f, CalculateFinalValue(StatType.CritMultiplier, stats.baseCritDamage));

    void Awake()
    {
        playerHealthBar = GetComponent<PlayerHealthBar>();
        foreach (StatType type in System.Enum.GetValues(typeof(StatType)))
        {
            statModifiers[type] = new List<StatModifier>();
        }
    }

    void OnDestroy()
    {
        var debugManager = ServiceLocator.Get<DebugManager>();
        if (debugManager != null)
        {
            debugManager.UnregisterPlayer();
        }
    }

    public void AddModifier(StatType type, StatModifier modifier)
    {
        statModifiers[type].Add(modifier);
        CalculateFinalStats();
    }

    public void RemoveModifiersFromSource(object source)
    {
        foreach (var key in statModifiers.Keys)
        {
            statModifiers[key].RemoveAll(mod => mod.Source == source);
        }
        CalculateFinalStats();
    }

    private float CalculateFinalValue(StatType type, float baseValue)
    {
        float totalBonusRatio = statModifiers[type].Sum(mod => mod.Value);
        return baseValue * (1 + totalBonusRatio / 100f);
    }

    public void CalculateFinalStats()
    {
        OnFinalStatsCalculated?.Invoke();
    }

    public void TakeDamage(float damage)
    {
        if (isInvulnerable) return;
        currentHealth -= damage;
        playerHealthBar.UpdateHealth(currentHealth, FinalHealth);
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("[CharacterStats] 플레이어가 사망했습니다. 게임오버 상태로 전환합니다.");
        gameObject.SetActive(false);
        ServiceLocator.Get<GameManager>().ChangeState(GameManager.GameState.GameOver);
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > FinalHealth) currentHealth = FinalHealth;
        playerHealthBar.UpdateHealth(currentHealth, FinalHealth);
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

    public float GetCurrentHealth()
    {
        return currentHealth;
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
}