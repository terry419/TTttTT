using UnityEngine;
using System.Collections.Generic;



/// <summary>

/// [v2.2] 적중된 대상(HitTarget)에게 모든 종류의 효과(스탯, 지속/즉발 피해 및 회복)를 적용하는 통합 모듈입니다.

/// </summary>

[CreateAssetMenu(fileName = "Module_ApplyEffectToTarget_", menuName = "GameData/v8.0/Modules/ApplyEffectToHitTarget")]

public class ApplyEffectToHitTargetModule : CardEffectSO

{
    [Header("[ 발동 조건 ]")]
    [Tooltip("효과가 실제로 적용될 확률 (0.0 ~ 100.0)")]
    [Range(0f, 100f)] public float ApplicationChance = 100f;

    [Header("[ 효과 내용 - 공통 ]")]
    [Tooltip("이 효과를 식별할 고유 ID입니다. Detonate, Curse 등 다른 모듈에서 이 ID를 사용하여 효과를 찾습니다. (예: Poison, Burn, Slow)")]

    public string StatusEffectID;
    [Tooltip("스탯 변경 및 지속 효과의 지속 시간 (초). 0으로 설정 시 즉발성 효과가 됩니다.")]
    public float Duration = 3.0f;
    [Tooltip("동일한 ID의 효과가 이미 대상에게 있을 때 어떻게 처리할지 결정합니다.")]
    public StackingBehavior StackingBehavior = StackingBehavior.RefreshDuration;


    [Header("[ 효과 내용 - 스탯 변경 (%)]")]

    [Tooltip("대상의 이동속도를 % 단위로 변경합니다. (음수 가능)")]

    public float MoveSpeedBonus;

    [Tooltip("대상이 받는 모든 피해량을 % 단위로 변경합니다. (2단계 구현 예정)")]

    public float DamageTakenBonus;

    [Tooltip("대상의 접촉 공격력을 % 단위로 변경합니다. (2단계 구현 예정)")]

    public float ContactDamageBonus;



    [Header("[ 효과 내용 - 지속 피해 (DoT) ]")]

    [Tooltip("초당 가하는 피해량입니다.")]

    public float DamageAmount;

    [Tooltip("체크 시, 시전자(플레이어)의 최종 공격력 보너스 스탯이 DoT 피해량에 곱연산으로 영향을 줍니다.")]

    public bool ScalesWithDmgBonus = false;



    [Header("[ 효과 내용 - 회복 (HoT) ]")]

    [Tooltip("초당 회복량입니다.")]

    public float HealAmount;



    public override void Execute(EffectContext context)
    {
        if (context.HitTarget == null) return;
        if (Random.Range(0f, 100f) > ApplicationChance) return;

        var statBonuses = new Dictionary<StatType, float>();
        if (MoveSpeedBonus != 0) statBonuses.Add(StatType.MoveSpeed, MoveSpeedBonus);
        if (DamageTakenBonus != 0) statBonuses.Add(StatType.DamageTaken, DamageTakenBonus);
        if (ContactDamageBonus != 0) statBonuses.Add(StatType.ContactDamage, ContactDamageBonus);

        // [ 핵심 변경] 2단계 최종 버전에 맞는 StatusEffectInstance 생성자를 호출합니다.
        var effectInstance = new StatusEffectInstance(
            target: context.HitTarget.gameObject,
            id: StatusEffectID,
            duration: Duration,
            bonuses: statBonuses,
            dotAmount: DamageAmount,
            dotType: DamageType, // DamageType, HealType 등 모든 파라미터 전달
            scales: ScalesWithDmgBonus,
            healAmount: HealAmount,
            healDuration: HealDuration,
            healType: HealType,
            stacking: StackingBehavior,
            caster: context.Caster,
            onApplyVFX: OnApplyVFX,
            loopingVFX: LoopingVFX,
            onExpireVFX: OnExpireVFX
        );

        ServiceLocator.Get<StatusEffectManager>()?.ApplyStatusEffect(context.HitTarget.gameObject, effectInstance);
    }

}