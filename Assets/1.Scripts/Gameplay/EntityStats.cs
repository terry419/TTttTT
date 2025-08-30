using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

// StatSources와 CharacterPermanentStats는 아직 파일 위치를 찾지 못했으므로,
// 컴파일 오류를 막기 위해 임시로 정의합니다. 실제 정의를 찾으면 이 부분은 제거해야 합니다.
public enum StatSources { Permanent, Allocated }
public class CharacterPermanentStats
{
    public Dictionary<StatType, float> investedRatios = new Dictionary<StatType, float>();
    public List<StatType> GetUnlockedStats() { return new List<StatType>(); }
}

/// <summary>
/// 모든 캐릭터(플레이어, 몬스터 등)가 공유하는 핵심 능력치 시스템의 기반 클래스입니다.
/// 기본 능력치, 버프/디버프에 의한 능력치 변경 및 최종 능력치 계산을 담당합니다.
/// </summary>
public abstract class EntityStats : MonoBehaviour
{
    [Header("기본 능력치 (SO)")]
    [Tooltip("캐릭터의 기본 스탯 데이터")]
    public CharacterStatsSO baseStats;

    [Header("현재 상태 (런타임)")]
    public float currentHealth;
    public bool isInvulnerable = false;

    [Header("이벤트")]
    public UnityEvent OnFinalStatsCalculated = new UnityEvent();

    // 모든 스탯 변경자(모디파이어)를 저장하는 딕셔너리
    protected readonly Dictionary<StatType, List<StatModifier>> statModifiers = new Dictionary<StatType, List<StatModifier>>();

    #region 최종 능력치 프로퍼티
    // 각종 모디파이어가 적용된 최종 능력치를 계산하여 반환합니다.
    public float FinalDamageBonus => statModifiers[StatType.Attack].Sum(mod => mod.Value);
    public float FinalAttackSpeed => Mathf.Max(0.1f, CalculateFinalValue(StatType.AttackSpeed, baseStats.baseAttackSpeed));
    public float FinalMoveSpeed => Mathf.Max(0f, CalculateFinalValue(StatType.MoveSpeed, baseStats.baseMoveSpeed));
    public float FinalHealth => Mathf.Max(1f, CalculateFinalValue(StatType.Health, baseStats.baseHealth));
    public float FinalCritRate => Mathf.Clamp(CalculateFinalValue(StatType.CritRate, baseStats.baseCritRate), 0f, 100f);
    public float FinalCritDamage => Mathf.Max(0f, CalculateFinalValue(StatType.CritMultiplier, baseStats.baseCritDamage));
    #endregion

    /// <summary>
    /// 컴포넌트 초기화 시, 모든 StatType에 대한 리스트를 미리 생성합니다.
    /// </summary>
    protected virtual void Awake()
    {
        foreach (StatType type in System.Enum.GetValues(typeof(StatType)))
        {
            statModifiers[type] = new List<StatModifier>();
        }
    }

    /// <summary>
    /// 특정 스탯 타입에 스탯 변경자(모디파이어)를 추가합니다.
    /// </summary>
    public void AddModifier(StatType type, StatModifier modifier)
    {
        statModifiers[type].Add(modifier);
        CalculateFinalStats();
    }

    /// <summary>
    /// 특정 출처(Source)를 가진 모든 스탯 변경자를 제거합니다.
    /// </summary>
    public void RemoveModifiersFromSource(object source)
    {
        foreach (var key in statModifiers.Keys)
        {
            statModifiers[key].RemoveAll(mod => mod.Source == source);
        }
        CalculateFinalStats();
    }

    /// <summary>
    /// 기본 값과 모든 모디파이어를 바탕으로 특정 스탯의 최종 값을 계산합니다.
    /// </summary>
    protected float CalculateFinalValue(StatType type, float baseValue)
    {
        float totalBonusRatio = statModifiers[type].Sum(mod => mod.Value);
        return baseValue * (1 + totalBonusRatio / 100f);
    }

    /// <summary>
    /// 최종 스탯이 변경되었음을 알리는 이벤트를 호출합니다.
    /// </summary>
    public void CalculateFinalStats()
    {
        OnFinalStatsCalculated?.Invoke();
    }

    // 자식 클래스에서 반드시 구현해야 할 추상 메소드들
    public abstract void TakeDamage(float damage);
    public abstract void Die();

    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}

// StatType과 StatModifier는 StatusEffectDataSO에서 이곳으로 이동했습니다.
// 이제 능력치 시스템의 핵심 부분이므로, EntityStats와 함께 관리됩니다.
public enum StatType
{
    Health,
    Attack,
    AttackSpeed,
    MoveSpeed,
    CritRate,
    CritMultiplier
}

public class StatModifier
{
    public readonly float Value;
    public readonly object Source;

    public StatModifier(float value, object source)
    {
        Value = value;
        Source = source;
    }
}