// 경로: ./TTttTT/Assets/1/Scripts/Gameplay/CharacterStats.cs

using System;
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
    public float currentHealth;
    public bool isInvulnerable = false;
    [Header("이벤트")]
    public UnityEvent OnFinalStatsCalculated = new UnityEvent();

    private PlayerHealthBar playerHealthBar;
    public float cardSelectionInterval = 10f;
    private readonly Dictionary<StatType, List<StatModifier>> statModifiers = new Dictionary<StatType, List<StatModifier>>();
    public event Action<float, float> OnHealthChanged;

    // [리팩토링] 최종 스탯을 실시간으로 계산하는 프로퍼티 (올바른 StatType 사용)
    // [수정] FinalDamage -> FinalDamageBonus로 변경하여 '추가 피해량 %'임을 명확히 함
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
    }

    void Start()
    {
        var gameManager = ServiceLocator.Get<GameManager>();
        if (gameManager != null)
        {
            if (gameManager.isFirstRound)
            {
                // 새 게임의 첫 라운드이므로, 체력을 최대로 설정합니다.
                currentHealth = FinalHealth;
            }
            else
            {
                // 이전 라운드에서 이어지는 경우, 저장된 체력을 가져옵니다.
                // (혹시 모를 오류에 대비해, 저장된 값이 없으면 최대로 설정)
                currentHealth = gameManager.GetCurrentHealth() ?? FinalHealth;
            }
        }
        else
        {
            // GameManager를 찾을 수 없는 예외적인 경우, 체력을 최대로 설정합니다.
            currentHealth = FinalHealth;
        }

        // 체력 초기화 후, HUD UI를 즉시 업데이트합니다.
        playerHealthBar.UpdateHealth(currentHealth, FinalHealth);
    }

    void OnDestroy()
    {
        var gameManager = ServiceLocator.Get<GameManager>();
        if (gameManager != null)
        {
            Debug.Log($"[DEBUG-HEALTH] CharacterStats.OnDestroy: GameManager에 체력 저장을 요청합니다. 저장할 체력: {currentHealth}");
            gameManager.SetCurrentHealth(currentHealth);
        }

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
        Debug.Log($"[DAMAGE-DEBUG 4/4] TakeDamage 호출됨. 데미지: {damage}, 현재 체력: {currentHealth}");
        if (isInvulnerable)
        {
            Debug.Log("[DAMAGE-DEBUG] 무적 상태이므로 데미지를 받지 않습니다.");
            return;
        }
        currentHealth -= damage;
        playerHealthBar.UpdateHealth(currentHealth, FinalHealth);
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnHealthChanged?.Invoke(currentHealth, FinalHealth);
            Die();
        }
        else
        {
            OnHealthChanged?.Invoke(currentHealth, FinalHealth);
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
        OnHealthChanged?.Invoke(currentHealth, FinalHealth); 
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
            StatType targetStat = availableStats[UnityEngine.Random.Range(0, availableStats.Count)];
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