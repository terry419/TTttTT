// 경로: ./TTttTT/Assets/1/Scripts/Gameplay/StatusEffectInstance.cs
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;




/// <summary>
/// 상태 효과의 중첩 방식을 정의하는 열거형.
/// </summary>
public enum StackingBehavior
{
    [Tooltip("기존 효과의 지속시간만 초기화합니다.")]
    RefreshDuration,
    [Tooltip("별개의 효과로 새로 추가하여 중첩시킵니다.")]
    StackEffect,
    [Tooltip("이미 효과가 있다면 무시합니다.")]
    NoStack
}
public enum DamageType { Flat, MaxHealthPercentage }
public enum HealType { Flat, MaxHealthPercentage }




public class StatusEffectInstance
{
    // --- 주요 속성 ---
    public GameObject Target { get; }
    public string EffectId { get; }
    public StackingBehavior StackingBehavior { get; }
    // --- 시간 관련 ---
    private readonly float initialDuration;
    private float duration;
    public bool IsExpired => duration > 0 && duration <= Time.deltaTime; // 1프레임 오차 방지를 위해 <= 사용
    public float RemainingDuration => duration;
    // --- 효과 내용 ---
    private readonly Dictionary<StatType, float> statBonuses;
    public float DamagePerSecond { get; }
    public float HealPerSecond { get; }
    // --- 스케일링 관련 ---
    private readonly bool scalesWithDmgBonus;
    private readonly CharacterStats casterStats;
    /// <summary>
    /// [호환성] StatusEffectDataSO로부터 인스턴스를 생성하는 생성자.
    /// </summary>

    public StatusEffectInstance(GameObject target, StatusEffectDataSO data, CharacterStats caster = null)
    {
        Target = target;
        casterStats = caster;
        EffectId = data.effectId;
        initialDuration = data.duration;
        duration = data.duration;
        DamagePerSecond = data.damageOverTime;
        HealPerSecond = data.healOverTime;
        StackingBehavior = StackingBehavior.RefreshDuration; // SO 기반 효과는 기본적으로 갱신만 지원
        statBonuses = new Dictionary<StatType, float>();
        // SO에 정의된 스탯 보너스들을 딕셔너리에 추가

        if (data.damageRatioBonus != 0) statBonuses[StatType.Attack] = data.damageRatioBonus;
        if (data.attackSpeedRatioBonus != 0) statBonuses[StatType.AttackSpeed] = data.attackSpeedRatioBonus;
        if (data.moveSpeedRatioBonus != 0) statBonuses[StatType.MoveSpeed] = data.moveSpeedRatioBonus;
        if (data.healthRatioBonus != 0) statBonuses[StatType.Health] = data.healthRatioBonus;
        if (data.critRateRatioBonus != 0) statBonuses[StatType.CritRate] = data.critRateRatioBonus;
        if (data.critDamageRatioBonus != 0) statBonuses[StatType.CritMultiplier] = data.critDamageRatioBonus;
    }
    /// <summary>
    /// [핵심] 신규 카드 모듈로부터 동적으로 인스턴스를 생성하는 생성자.
    /// </summary>

    public StatusEffectInstance(GameObject target, string id, float duration, Dictionary<StatType, float> bonuses, float dot, float hot, StackingBehavior stacking, bool scales, CharacterStats caster)
    {
        Target = target;
        EffectId = id;
        initialDuration = duration;
        this.duration = duration;
        statBonuses = bonuses ?? new Dictionary<StatType, float>();
        DamagePerSecond = dot;
        HealPerSecond = hot;
        StackingBehavior = stacking;
        scalesWithDmgBonus = scales;
        casterStats = caster;
    }
    /// <summary>
    /// 대상에게 스탯 변경 효과를 적용합니다.
    /// </summary>
    public void ApplyEffect()
    {
        if (Target.TryGetComponent<IStatHolder>(out var statHolder))
        {
            foreach (var bonus in statBonuses)
            {
                statHolder.AddModifier(bonus.Key, new StatModifier(bonus.Value, this));
            }
        }
    }
    /// <summary>
    /// 대상에게서 스탯 변경 효과를 제거합니다.
    /// </summary>

    public void RemoveEffect()
    {
        if (Target != null && Target.TryGetComponent<IStatHolder>(out var statHolder))
        {
            statHolder.RemoveModifiersFromSource(this);
        }
    }
    /// <summary>
    /// 매 프레임 호출되어 지속시간, DoT, HoT 등을 처리합니다.
    /// </summary>

    public void Tick(float deltaTime)
    {
        // 1. 지속시간 감소
        if (duration > 0) duration -= deltaTime;
        // 2. 지속 피해(DoT) 처리
        if (DamagePerSecond > 0 && Target.TryGetComponent<MonsterController>(out var monster))
        {
            float finalDot = DamagePerSecond;
            // 시전자의 공격력 보너스에 영향을 받는 옵션이 켜져있다면, 최종 피해량을 다시 계산
            if (scalesWithDmgBonus && casterStats != null) finalDot *= (1 + casterStats.FinalDamageBonus / 100f);
            monster.TakeDamage(finalDot * deltaTime);
        }
        // 3. 지속 회복(HoT) 처리
        if (HealPerSecond > 0 && Target.TryGetComponent<CharacterStats>(out var character))
        {
            character.Heal(HealPerSecond * deltaTime);
        }
    }
    /// <summary>
    /// 효과의 지속시간을 초기값으로 되돌립니다.
    /// </summary>
    public void RefreshDuration() => duration = initialDuration;

}