using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public abstract class EntityStats : MonoBehaviour, IStatHolder
{
    [Header("�⺻ �ɷ�ġ")]
    public BaseStats stats;
    [Header("���� ���� (��Ÿ��)")]
    public bool isInvulnerable = false;
    [Header("�̺�Ʈ")]
    public UnityEvent OnFinalStatsCalculated = new UnityEvent();
    public event Action<float, float> OnHealthChanged;

    public float CurrentHealth { get; protected set; }

    // �ڽ� Ŭ������ ������ �� �ֵ��� private���� protected�� ����
    protected readonly Dictionary<StatType, List<StatModifier>> statModifiers = new Dictionary<StatType, List<StatModifier>>();

    // ���� ���� ��� ������Ƽ (���� ����)
    public float FinalDamageBonus => stats.baseDamage + (statModifiers.ContainsKey(StatType.Attack) ? statModifiers[StatType.Attack].Sum(mod => mod.Value) : 0f);
    public float FinalAttackSpeed => Mathf.Max(0.1f, CalculateFinalValue(StatType.AttackSpeed, stats.baseAttackSpeed));
    public float FinalMoveSpeed => Mathf.Max(0f, CalculateFinalValue(StatType.MoveSpeed, stats.baseMoveSpeed));
    public float FinalHealth => Mathf.Max(1f, CalculateFinalValue(StatType.Health, stats.baseHealth));
    public float FinalCritRate => Mathf.Clamp(CalculateFinalValue(StatType.CritRate, stats.baseCritRate), 0f, 100f);
    public float FinalCritDamage => Mathf.Max(0f, CalculateFinalValue(StatType.CritMultiplier, stats.baseCritDamage));

    protected virtual void Awake()
    {
        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            statModifiers[type] = new List<StatModifier>();
        }
    }

    // [�߰�] �ٸ� ��ũ��Ʈ����� ȣȯ���� ���� GetCurrentHealth() �޼��� �߰�
    public float GetCurrentHealth()
    {
        return CurrentHealth;
    }

    public virtual void Initialize()
    {
        CalculateFinalStats();
        CurrentHealth = FinalHealth;
        OnHealthChanged?.Invoke(CurrentHealth, FinalHealth);
    }

    public void TakeDamage(float damage)
    {
        if (isInvulnerable) return;
        CurrentHealth -= damage;
        if (CurrentHealth < 0) CurrentHealth = 0;
        OnHealthChanged?.Invoke(CurrentHealth, FinalHealth);
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        CurrentHealth += amount;
        if (CurrentHealth > FinalHealth) CurrentHealth = FinalHealth;
        OnHealthChanged?.Invoke(CurrentHealth, FinalHealth);
    }

    protected void InvokeOnHealthChanged()
    {
        OnHealthChanged?.Invoke(CurrentHealth, FinalHealth);
    }

    protected abstract void Die();

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
        if (!statModifiers.ContainsKey(type)) return baseValue;
        float totalBonusRatio = statModifiers.ContainsKey(type) ? statModifiers[type].Sum(mod => mod.Value) : 0f;
        return baseValue * (1 + totalBonusRatio / 100f);
    }

    public void CalculateFinalStats()
    {
        OnFinalStatsCalculated?.Invoke();
    }
}