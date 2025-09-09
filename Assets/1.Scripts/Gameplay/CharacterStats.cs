// 경로: ./TTttTT/Assets/1/Scripts/Gameplay/CharacterStats.cs

using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(PlayerHealthBar))]
public class CharacterStats : MonoBehaviour, IStatHolder
{
    [Header("기본 능력치")]
    public BaseStats stats;
    [Header("현재 상태 (런타임)")]
    //public float currentHealth;
    public bool isInvulnerable = false;
    [Header("이벤트")]
    public UnityEvent OnFinalStatsCalculated = new UnityEvent();

    private PlayerHealthBar playerHealthBar;
    public float cardSelectionInterval = 10f;
    private readonly Dictionary<StatType, List<StatModifier>> statModifiers = new Dictionary<StatType, List<StatModifier>>();

    private PlayerDataManager playerDataManager;


    // [리팩토링] 최종 스탯을 실시간으로 계산하는 프로퍼티 (올바른 StatType 사용)
    public float FinalDamageBonus
    {
        get
        {
            float totalBonusRatio = statModifiers[StatType.Attack].Sum(mod => mod.Value);
            return stats.baseDamage + totalBonusRatio;
        }
    }
    public float FinalAttackSpeed => Mathf.Max(0.1f, CalculateFinalValue(StatType.AttackSpeed, stats.baseAttackSpeed));
    public float FinalMoveSpeed => Mathf.Max(0f, CalculateFinalValue(StatType.MoveSpeed, stats.baseMoveSpeed));
    public float FinalHealth => Mathf.Max(1f, CalculateFinalValue(StatType.Health, stats.baseHealth));
    public float FinalCritRate => Mathf.Clamp(CalculateFinalValue(StatType.CritRate, stats.baseCritRate), 0f, 100f);
    public float FinalCritDamage => Mathf.Max(0f, CalculateFinalValue(StatType.CritMultiplier, stats.baseCritDamage));

    // 이하 코드는 기존과 동일합니다...
    // Awake(), OnDestroy(), AddModifier(), RemoveModifiersFromSource() 등은 변경 없음

    void Awake()
    {
        playerHealthBar = GetComponent<PlayerHealthBar>();
        foreach (StatType type in System.Enum.GetValues(typeof(StatType)))
        {
            statModifiers[type] = new List<StatModifier>();
        }
        playerDataManager = ServiceLocator.Get<PlayerDataManager>();
    }

    void Start()
    {
        if (playerDataManager != null)
        {
            // UI가 현재 체력으로 초기화될 수 있도록 방송을 보냅니다.
            playerDataManager.UpdateHealth(playerDataManager.CurrentHealth);
        }
    }
    void OnDestroy()
    {
        // 체력은 TakeDamage/Heal에서 실시간으로 PlayerDataManager에 업데이트되므로,
        // 파괴 시점에 별도로 저장할 필요가 없습니다.

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
        if (playerDataManager == null) return;

        Debug.Log($"[DAMAGE-DEBUG 4/4] TakeDamage 호출됨. 데미지: {damage}, 현재 체력: {playerDataManager.CurrentHealth}");
        if (isInvulnerable)
        {
            Debug.Log("[DAMAGE-DEBUG] 무적 상태이므로 데미지를 받지 않습니다.");
            return;
        }

        float newHealth = playerDataManager.CurrentHealth - damage;
        playerDataManager.UpdateHealth(newHealth);

        if (playerDataManager.CurrentHealth <= 0)
        {
            playerDataManager.CurrentHealth = 0;
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
        if (playerDataManager == null) return;

        float newHealth = playerDataManager.CurrentHealth + amount;
        if (newHealth > FinalHealth)
        {
            newHealth = FinalHealth;
        }
        playerDataManager.UpdateHealth(newHealth);
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
        return stat == StatType.Health ?
            2f : 1f;
    }

    public float GetCurrentHealth()
    {
        return playerDataManager != null ? playerDataManager.CurrentHealth : 0f;
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