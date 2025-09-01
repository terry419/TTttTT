// 경로: ./TTttTT/Assets/1/Scripts/Gameplay/MonsterStats.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(MonsterController))]
public class MonsterStats : MonoBehaviour, IStatHolder
{
    // --- 참조 ---
    private MonsterController controller;

    // --- 능력치 ---
    private float baseMaxHealth;
    private float baseMoveSpeed;
    private float baseContactDamage;
    public float CurrentHealth { get; private set; }
    private readonly Dictionary<StatType, List<StatModifier>> statModifiers = new Dictionary<StatType, List<StatModifier>>();

    // --- 최종 능력치 프로퍼티 ---
    public float FinalMaxHealth => CalculateFinalValue(StatType.Health, baseMaxHealth);
    public float FinalMoveSpeed => Mathf.Max(0f, CalculateFinalValue(StatType.MoveSpeed, baseMoveSpeed));
    public float FinalContactDamage => Mathf.Max(0f, CalculateFinalValue(StatType.ContactDamage, baseContactDamage));
    public float FinalDamageTakenBonus => statModifiers.ContainsKey(StatType.DamageTaken) ? statModifiers[StatType.DamageTaken].Sum(mod => mod.Value) : 0f;

    void Awake()
    {
        controller = GetComponent<MonsterController>();
        foreach (StatType type in System.Enum.GetValues(typeof(StatType)))
        {
            statModifiers[type] = new List<StatModifier>();
        }
    }

    public void Initialize(MonsterDataSO data)
    {
        baseMaxHealth = data.maxHealth;
        baseMoveSpeed = data.moveSpeed;
        baseContactDamage = data.contactDamage;
        CurrentHealth = FinalMaxHealth;
    }

    public void TakeDamage(float damage)
    {
        float damageMultiplier = 1 + (FinalDamageTakenBonus / 100f);
        float finalDamage = damage * damageMultiplier;
        CurrentHealth -= finalDamage;
        
        // [ 핵심 변경] MonsterController에게 이벤트 호출을 위임합니다.
        controller.NotifyDamageTaken(finalDamage);
    }

    public void Heal(float amount)
    {
        if (amount <= 0) return;
        CurrentHealth = Mathf.Min(CurrentHealth + amount, FinalMaxHealth);
    }

    public bool IsDead() => CurrentHealth <= 0;

    private float CalculateFinalValue(StatType type, float baseValue)
    {
        float totalBonusRatio = statModifiers.ContainsKey(type) ? statModifiers[type].Sum(mod => mod.Value) : 0f;
        return baseValue * (1 + totalBonusRatio / 100f);
    }

    #region IStatHolder 인터페이스 구현
    public void AddModifier(StatType type, StatModifier modifier)
    {
        if (!statModifiers.ContainsKey(type)) statModifiers[type] = new List<StatModifier>();
        statModifiers[type].Add(modifier);
    }

    public void RemoveModifiersFromSource(object source)
    {
        foreach (var list in statModifiers.Values)
        {
            list.RemoveAll(mod => mod.Source == source);
        }
    }
    #endregion
}