// 경로: ./TTttTT/Assets/1/Scripts/Gameplay/CharacterStats.cs

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

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
        if (playerDataManager == null)
        {
            Debug.LogError($"[{GetType().Name}] CRITICAL: PlayerDataManager를 찾을 수 없습니다! 게임이 정상 동작하지 않을 수 있습니다.");
        }
    }

    void Start()
    {
        // 제안해주신 '준비될 때까지 기다리는' 안전한 초기화 로직입니다.
        StartCoroutine(InitializeWhenReady());
    }

    private IEnumerator InitializeWhenReady()
    {
        // PlayerDataManager의 런 데이터가 준비될 때까지 안전하게 대기합니다.
        while (playerDataManager == null || playerDataManager.CurrentRunData == null)
        {
            // PlayerDataManager가 아직 Awake되지 않았을 수 있으므로, 매 프레임 다시 찾아봅니다.
            if (playerDataManager == null) playerDataManager = ServiceLocator.Get<PlayerDataManager>();

            Debug.LogWarning("[CharacterStats] PlayerDataManager 또는 RunData가 아직 준비되지 않아 대기합니다...");
            yield return null;
        }

        Debug.Log("[CharacterStats] PlayerDataManager 준비 완료. 체력 초기화를 진행합니다.");

        // PlayerInitializer에서 모든 스탯 보너스(카드, 유물 등)가 적용된 후,
        // 최종 최대 체력으로 현재 체력을 설정하고 UI에 알립니다.
        CalculateFinalStats();
        playerDataManager.UpdateHealth(FinalHealth, FinalHealth);
    }

    void OnDestroy()
    {
        // 이제 OnDestroy에서 체력을 저장할 필요가 없습니다.
        // TakeDamage/Heal 함수에서 실시간으로 PlayerDataManager에 업데이트하기 때문입니다.
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
        if (isInvulnerable) return;

        float newHealth = playerDataManager.CurrentRunData.currentHealth - damage;
        // 체력 변경을 PlayerDataManager에게 위임하고, 변경된 값을 UI에 방송하도록 요청합니다.
        playerDataManager.UpdateHealth(newHealth, FinalHealth);

        if (playerDataManager.CurrentRunData.currentHealth <= 0)
        {
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
        float newHealth = playerDataManager.CurrentRunData.currentHealth + amount;
        // 체력 변경을 PlayerDataManager에게 위임합니다.
        playerDataManager.UpdateHealth(newHealth, FinalHealth);
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
        return playerDataManager != null ? playerDataManager.CurrentRunData.currentHealth : 0f;
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