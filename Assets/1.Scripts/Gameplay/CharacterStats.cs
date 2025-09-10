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

    void OnEnable()
    {
        // PlayerDataManager의 데이터 변경 방송을 구독합니다.
        PlayerDataManager.OnRunDataChanged += HandleRunDataChanged;
    }

    void OnDisable()
    {
        // 오브젝트가 비활성화되면 구독을 해제하여 메모리 누수를 방지합니다.
        PlayerDataManager.OnRunDataChanged -= HandleRunDataChanged;
    }

    // 데이터 변경 방송을 수신했을 때 호출될 함수입니다.
    private void HandleRunDataChanged(RunDataChangeType changeType)
    {
        // 카드나 유물 데이터가 변경되었을 때만 스탯을 다시 계산합니다.
        if (changeType == RunDataChangeType.Cards || changeType == RunDataChangeType.Artifacts || changeType == RunDataChangeType.All)
        {
            Debug.Log($"[CharacterStats] '{changeType}' 타입 데이터 변경 감지. 스탯을 새로고침합니다.");
            RecalculateAllModifiers();
        }
    }

    // 모든 카드와 유물 보너스를 처음부터 다시 적용하는 함수입니다.
    private void RecalculateAllModifiers()
    {
        // 1. 기존에 적용된 모든 보너스를 초기화합니다.
        foreach (var key in statModifiers.Keys)
        {
            statModifiers[key].Clear();
        }

        // 2. PlayerDataManager로부터 최신 데이터를 가져옵니다.
        var runData = ServiceLocator.Get<PlayerDataManager>().CurrentRunData;
        if (runData == null) return;

        // 3. 현재 장착된 모든 카드의 보너스를 다시 적용합니다.
        foreach (var card in runData.equippedCards)
        {
            AddModifier(StatType.Attack, new StatModifier(card.GetFinalDamageMultiplier(), card));
            AddModifier(StatType.AttackSpeed, new StatModifier(card.GetFinalAttackSpeedMultiplier(), card));
            AddModifier(StatType.MoveSpeed, new StatModifier(card.GetFinalMoveSpeedMultiplier(), card));
            AddModifier(StatType.Health, new StatModifier(card.GetFinalHealthMultiplier(), card));
            AddModifier(StatType.CritRate, new StatModifier(card.GetFinalCritRateMultiplier(), card));
            AddModifier(StatType.CritMultiplier, new StatModifier(card.GetFinalCritDamageMultiplier(), card));
        }

        // 4. (향후 확장) 모든 유물의 보너스를 다시 적용합니다.
        var artifactManager = ServiceLocator.Get<ArtifactManager>();
        if (artifactManager != null)
        {
            // artifactManager.RecalculateArtifactStats(); // 필요 시 이와 유사한 함수 호출
        }

        // 5. 최종적으로 스탯을 다시 계산하고 이벤트를 발생시켜 UI 등에 알립니다.
        CalculateFinalStats();
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