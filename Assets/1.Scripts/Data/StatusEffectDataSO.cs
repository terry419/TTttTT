using UnityEngine;

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
    public float damageOverTime;
    public float healOverTime;

    /// <summary>
    /// [리팩토링] 대상 캐릭터에게 이 상태 효과의 능력치 보너스를 적용합니다.
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
    /// [리팩토링] 대상 캐릭터에게서 이 상태 효과의 능력치 보너스를 제거합니다.
    /// </summary>
    public void RemoveEffect(CharacterStats targetStats)
    {
        if (targetStats == null) return;

        targetStats.RemoveModifiersFromSource(this);
    }
}