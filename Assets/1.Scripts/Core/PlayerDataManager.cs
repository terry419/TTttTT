using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq; // [추가] Sum()과 같은 Linq 함수를 사용하기 위해 필요합니다.

// [개선안 #5 적용] 이벤트가 어떤 종류의 데이터 변경을 알릴지 enum으로 정의합니다.
public enum RunDataChangeType
{
    Cards,      // 카드 목록(소유/장착)이 변경되었을 때
    Artifacts,  // 유물 목록이 변경되었을 때
    Health,     // 체력이 변경되었을 때
    All         // 전체 데이터 리셋 등 모든 UI가 새로고침되어야 할 때
}

/// <summary>
/// 게임의 한 세션(런) 동안 유지되는 플레이어의 모든 데이터를 관리하는 중앙 허브입니다.
/// [3단계 수정] 현재 데이터 기반으로 최종 스탯을 계산하는 미리보기 기능이 추가되었습니다.
/// </summary>
public class PlayerDataManager : MonoBehaviour
{
#if UNITY_EDITOR
    [Serializable]
    public class PlayerDataDebugInfo
    {
        public string characterName;
        public float currentHealth;
        public int ownedCardCount;
        public int equippedCardCount;
        public bool isRunDataValid;
    }
    [SerializeField] private PlayerDataDebugInfo debugInfo = new PlayerDataDebugInfo();
#endif

    // [개선안 #5 적용] 이벤트가 이제 RunDataChangeType 정보를 함께 전달합니다.
    public static event Action<RunDataChangeType> OnRunDataChanged;
    public static event Action<float, float> OnHealthChanged;

    public PlayerRunData CurrentRunData { get; private set; }
    public bool IsRunInitialized { get; private set; } = false;

    void Awake()
    {
        if (!ServiceLocator.IsRegistered<PlayerDataManager>())
        {
            ServiceLocator.Register<PlayerDataManager>(this);
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (CurrentRunData != null && CurrentRunData.IsValid())
        {
            debugInfo.characterName = CurrentRunData.characterData?.characterName ?? "N/A";
            debugInfo.currentHealth = CurrentRunData.currentHealth;
            debugInfo.ownedCardCount = CurrentRunData.ownedCards.Count;
            debugInfo.equippedCardCount = CurrentRunData.equippedCards.Count;
            debugInfo.isRunDataValid = CurrentRunData.IsValid();
        }
    }
#endif

    public void ResetRunData(CharacterDataSO characterData)
    {
        CurrentRunData = new PlayerRunData
        {
            characterData = characterData,
            baseStats = characterData.baseStats,
            currentHealth = characterData.baseStats.baseHealth
        };

        CurrentRunData.ownedCards.Clear();
        CurrentRunData.equippedCards.Clear();
        CurrentRunData.ownedArtifacts.Clear();
        IsRunInitialized = true;
    }

    public void CompleteRunInitialization()
    {
        IsRunInitialized = false;
    }

    public void UpdateHealth(float newHealth, float maxHealth)
    {
        if (CurrentRunData == null || !CurrentRunData.IsValid()) return;

        if (newHealth < 0) newHealth = 0;
        if (newHealth > maxHealth) newHealth = maxHealth;

        float oldHealth = CurrentRunData.currentHealth;
        CurrentRunData.currentHealth = newHealth;
        CurrentRunData.maxHealth = maxHealth; // [추가] 최대 체력도 함께 저장합니다.

        OnHealthChanged?.Invoke(CurrentRunData.currentHealth, maxHealth);
        if (Mathf.Approximately(oldHealth, CurrentRunData.currentHealth) == false)
            Debug.Log($"[PlayerDataManager] 플레이어 체력 변경: {oldHealth:F1} -> {CurrentRunData.currentHealth:F1} (최대: {maxHealth:F1})");
    }

    /// <summary>
    /// [신규] 현재 장착된 아이템들을 기반으로 최종 능력치를 계산하여 반환하는 '미리보기' 함수입니다.
    /// CharacterStats가 없는 씬에서도 스탯을 확인할 수 있게 해줍니다.
    /// </summary>
    /// <returns>계산된 최종 능력치가 담긴 BaseStats 객체. 계산할 수 없는 경우 null을 반환합니다.</returns>
    public BaseStats CalculatePreviewStats()
    {
        if (CurrentRunData == null || !CurrentRunData.IsValid())
        {
            Debug.LogError("[PlayerDataManager] 런 데이터가 유효하지 않아 미리보기 스탯을 계산할 수 없습니다.");
            return null;
        }

        // 1. 모든 스탯 타입에 대한 보너스 합계를 저장할 딕셔너리를 생성합니다.
        var totalBonuses = new Dictionary<StatType, float>();
        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            totalBonuses[type] = 0f;
        }

        // 2. 장착된 모든 카드를 순회하며 스탯 보너스를 누적합니다.
        foreach (var card in CurrentRunData.equippedCards)
        {
            totalBonuses[StatType.Attack] += card.GetFinalDamageMultiplier();
            totalBonuses[StatType.AttackSpeed] += card.GetFinalAttackSpeedMultiplier();
            totalBonuses[StatType.MoveSpeed] += card.GetFinalMoveSpeedMultiplier();
            totalBonuses[StatType.Health] += card.GetFinalHealthMultiplier();
            totalBonuses[StatType.CritRate] += card.GetFinalCritRateMultiplier();
            totalBonuses[StatType.CritMultiplier] += card.GetFinalCritDamageMultiplier();
        }

        // 3. 소유한 모든 유물을 순회하며 스탯 보너스를 누적합니다.
        foreach (var artifact in CurrentRunData.ownedArtifacts)
        {
            totalBonuses[StatType.Attack] += artifact.attackBoostRatio;
            totalBonuses[StatType.Health] += artifact.healthBoostRatio;
            totalBonuses[StatType.MoveSpeed] += artifact.moveSpeedBoostRatio;
            totalBonuses[StatType.CritRate] += artifact.critChanceBoostRatio;
            totalBonuses[StatType.CritMultiplier] += artifact.critDamageBoostRatio;
        }

        // 4. 기본 스탯에 누적된 보너스를 적용하여 최종 스탯을 계산합니다.
        BaseStats finalStats = new BaseStats();
        BaseStats baseStats = CurrentRunData.baseStats;

        // CharacterStats의 계산 로직과 동일한 공식을 사용합니다.
        finalStats.baseDamage = baseStats.baseDamage + totalBonuses[StatType.Attack]; // 공격력은 합연산 보너스
        finalStats.baseAttackSpeed = baseStats.baseAttackSpeed * (1 + totalBonuses[StatType.AttackSpeed] / 100f);
        finalStats.baseMoveSpeed = baseStats.baseMoveSpeed * (1 + totalBonuses[StatType.MoveSpeed] / 100f);
        finalStats.baseHealth = baseStats.baseHealth * (1 + totalBonuses[StatType.Health] / 100f);
        finalStats.baseCritRate = baseStats.baseCritRate + totalBonuses[StatType.CritRate]; // 치명타 확률은 합연산
        finalStats.baseCritDamage = baseStats.baseCritDamage + totalBonuses[StatType.CritMultiplier]; // 치명타 피해도 합연산

        return finalStats;
    }

    // [개선안 #1, #5 적용] 외부에서 이벤트를 호출할 때, 어떤 데이터가 변경되었는지 명시하도록 변경합니다.
    public void NotifyRunDataChanged(RunDataChangeType changeType)
    {
        Debug.Log($"[PlayerDataManager] OnRunDataChanged 이벤트를 '{changeType}' 타입으로 방송합니다.");
        OnRunDataChanged?.Invoke(changeType);
    }
    private void OnDestroy()
    {
        // ServiceLocator에 내가 등록되어 있을 경우에만 등록 해제를 시도합니다.
        if (ServiceLocator.IsRegistered<PlayerDataManager>() && ServiceLocator.Get<PlayerDataManager>() == this)
        {
            ServiceLocator.Unregister<PlayerDataManager>(this);
        }
    }

}