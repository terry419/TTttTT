using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

// StatSources�� CharacterPermanentStats�� ���� ���� ��ġ�� ã�� �������Ƿ�,
// ������ ������ ���� ���� �ӽ÷� �����մϴ�. ���� ���Ǹ� ã���� �� �κ��� �����ؾ� �մϴ�.
public enum StatSources { Permanent, Allocated }
public class CharacterPermanentStats
{
    public Dictionary<StatType, float> investedRatios = new Dictionary<StatType, float>();
    public List<StatType> GetUnlockedStats() { return new List<StatType>(); }
}

/// <summary>
/// ��� ĳ����(�÷��̾�, ���� ��)�� �����ϴ� �ٽ� �ɷ�ġ �ý����� ��� Ŭ�����Դϴ�.
/// �⺻ �ɷ�ġ, ����/������� ���� �ɷ�ġ ���� �� ���� �ɷ�ġ ����� ����մϴ�.
/// </summary>
public abstract class EntityStats : MonoBehaviour
{
    [Header("�⺻ �ɷ�ġ (SO)")]
    [Tooltip("ĳ������ �⺻ ���� ������")]
    public CharacterStatsSO baseStats;

    [Header("���� ���� (��Ÿ��)")]
    public float currentHealth;
    public bool isInvulnerable = false;

    [Header("�̺�Ʈ")]
    public UnityEvent OnFinalStatsCalculated = new UnityEvent();

    // ��� ���� ������(������̾�)�� �����ϴ� ��ųʸ�
    protected readonly Dictionary<StatType, List<StatModifier>> statModifiers = new Dictionary<StatType, List<StatModifier>>();

    #region ���� �ɷ�ġ ������Ƽ
    // ���� ������̾ ����� ���� �ɷ�ġ�� ����Ͽ� ��ȯ�մϴ�.
    public float FinalDamageBonus => statModifiers[StatType.Attack].Sum(mod => mod.Value);
    public float FinalAttackSpeed => Mathf.Max(0.1f, CalculateFinalValue(StatType.AttackSpeed, baseStats.baseAttackSpeed));
    public float FinalMoveSpeed => Mathf.Max(0f, CalculateFinalValue(StatType.MoveSpeed, baseStats.baseMoveSpeed));
    public float FinalHealth => Mathf.Max(1f, CalculateFinalValue(StatType.Health, baseStats.baseHealth));
    public float FinalCritRate => Mathf.Clamp(CalculateFinalValue(StatType.CritRate, baseStats.baseCritRate), 0f, 100f);
    public float FinalCritDamage => Mathf.Max(0f, CalculateFinalValue(StatType.CritMultiplier, baseStats.baseCritDamage));
    #endregion

    /// <summary>
    /// ������Ʈ �ʱ�ȭ ��, ��� StatType�� ���� ����Ʈ�� �̸� �����մϴ�.
    /// </summary>
    protected virtual void Awake()
    {
        foreach (StatType type in System.Enum.GetValues(typeof(StatType)))
        {
            statModifiers[type] = new List<StatModifier>();
        }
    }

    /// <summary>
    /// Ư�� ���� Ÿ�Կ� ���� ������(������̾�)�� �߰��մϴ�.
    /// </summary>
    public void AddModifier(StatType type, StatModifier modifier)
    {
        statModifiers[type].Add(modifier);
        CalculateFinalStats();
    }

    /// <summary>
    /// Ư�� ��ó(Source)�� ���� ��� ���� �����ڸ� �����մϴ�.
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
    /// �⺻ ���� ��� ������̾ �������� Ư�� ������ ���� ���� ����մϴ�.
    /// </summary>
    protected float CalculateFinalValue(StatType type, float baseValue)
    {
        float totalBonusRatio = statModifiers[type].Sum(mod => mod.Value);
        return baseValue * (1 + totalBonusRatio / 100f);
    }

    /// <summary>
    /// ���� ������ ����Ǿ����� �˸��� �̺�Ʈ�� ȣ���մϴ�.
    /// </summary>
    public void CalculateFinalStats()
    {
        OnFinalStatsCalculated?.Invoke();
    }

    // �ڽ� Ŭ�������� �ݵ�� �����ؾ� �� �߻� �޼ҵ��
    public abstract void TakeDamage(float damage);
    public abstract void Die();

    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}

// StatType�� StatModifier�� StatusEffectDataSO���� �̰����� �̵��߽��ϴ�.
// ���� �ɷ�ġ �ý����� �ٽ� �κ��̹Ƿ�, EntityStats�� �Բ� �����˴ϴ�.
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