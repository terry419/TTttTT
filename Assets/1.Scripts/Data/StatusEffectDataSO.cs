using UnityEngine;
using System; // [Serializable] 속성을 사용하기 위해 추가

/// <summary>
/// 지속 피해(DoT)의 유형과 값을 정의하는 클래스입니다.
/// </summary>
[Serializable]
public class DamageOverTimeInfo
{
    /// <summary>
    /// 지속 피해의 유형을 정의합니다.
    /// </summary>
    public enum DamageType
    {
        Fixed,              // 고정된 수치로 피해를 줍니다.
        PercentOfMaxHealth  // 대상의 최대 체력에 비례한 피해를 줍니다.
    }

    public DamageType damageType = DamageType.Fixed;

    [Tooltip("damageType이 Fixed일 때의 초당 피해량입니다.")]
    public float damageAmount;

    [Tooltip("damageType이 PercentOfMaxHealth일 때의 초당 최대 체력 비례(%) 피해량입니다.")]
    public float percentOfMaxHealth;
}

/// <summary>
/// 상태 효과의 모든 데이터를 정의하는 ScriptableObject입니다.
/// </summary>
[CreateAssetMenu(fileName = "StatusEffectData_", menuName = "GameData/StatusEffectData")]
public class StatusEffectDataSO : ScriptableObject
{
    [Header("기본 정보")]
    public string effectId;
    public string effectName;
    public Sprite icon;

    [Header("효과 속성")]
    public float duration;
    public bool isBuff;

    [Header("능력치 변경 효과 (백분율, %)")]
    public float damageRatioBonus;
    public float attackSpeedRatioBonus;
    public float moveSpeedRatioBonus;
    public float healthRatioBonus;
    public float critRateRatioBonus;
    public float critDamageRatioBonus;

    [Header("지속 피해/회복 효과")]
    public DamageOverTimeInfo damageOverTime;
    public float healOverTime;

    [Header("시각 효과 (VFX)")]
    [Tooltip("상태 이상이 지속되는 동안 대상에게 표시될 시각 효과 프리팹입니다.")]
    public GameObject statusVFX;

    /// <summary>
    /// 대상 캐릭터에게 이 상태 효과의 능력치 보너스를 적용합니다.
    /// </summary>
    public void ApplyEffect(CharacterStats targetStats)
    {
        if (targetStats == null) return;

        // 각 보너스 값이 0이 아닐 때만 Modifier를 추가합니다.
        if (damageRatioBonus != 0) targetStats.AddModifier(StatType.Attack, new StatModifier(damageRatioBonus, this));
        if (attackSpeedRatioBonus != 0) targetStats.AddModifier(StatType.AttackSpeed, new StatModifier(attackSpeedRatioBonus, this));
        if (moveSpeedRatioBonus != 0) targetStats.AddModifier(StatType.MoveSpeed, new StatModifier(moveSpeedRatioBonus, this));
        if (healthRatioBonus != 0) targetStats.AddModifier(StatType.Health, new StatModifier(healthRatioBonus, this));
        if (critRateRatioBonus != 0) targetStats.AddModifier(StatType.CritRate, new StatModifier(critRateRatioBonus, this));
        if (critDamageRatioBonus != 0) targetStats.AddModifier(StatType.CritMultiplier, new StatModifier(critDamageRatioBonus, this));
    }

    /// <summary>
    /// 대상 캐릭터에게서 이 상태 효과의 능력치 보너스를 제거합니다.
    /// </summary>
    public void RemoveEffect(CharacterStats targetStats)
    {
        if (targetStats == null) return;

        targetStats.RemoveModifiersFromSource(this);
    }
}