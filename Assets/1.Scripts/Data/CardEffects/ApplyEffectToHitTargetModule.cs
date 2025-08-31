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

        // 1. 유효성 검사: 이 모듈은 피격 대상(HitTarget)이 있어야만 의미가 있습니다.

        if (context.HitTarget == null)

        {

            Debug.LogWarning($"<color=yellow>[{GetType().Name}]</color> '{this.name}' 실행 중단: HitTarget이 없어 효과를 적용할 대상이 없습니다.");

            return;

        }

        Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' 실행 시도. 대상: {context.HitTarget.name}");
        // 2. 확률 체크: ApplicationChance에 따라 효과를 적용할지 결정합니다.

        if (Random.Range(0f, 100f) > ApplicationChance)

        {

            Debug.Log($"<color=yellow>[{GetType().Name}]</color> 확률({ApplicationChance}%) 체크에 실패하여 효과가 적용되지 않았습니다.");

            return;

        }

        // 3. 효과 데이터 구성: 인스펙터에 설정된 값들을 바탕으로 StatusEffectInstance에 전달할 데이터를 준비합니다.

        var statBonuses = new Dictionary<StatType, float>();

        if (MoveSpeedBonus != 0) statBonuses[StatType.MoveSpeed] = MoveSpeedBonus;

        // TODO: 2단계에서 DamageTakenBonus, ContactDamageBonus 스탯이 구현되면 여기에 추가해야 합니다.

        // 4. 효과 인스턴스 생성: 준비된 데이터를 사용하여 새로운 효과 인스턴스를 만듭니다.

        var effectInstance = new StatusEffectInstance(
      context.HitTarget.gameObject,
      StatusEffectID,
      Duration,
      statBonuses,
      DamageAmount,
      HealAmount,
      StackingBehavior,
      ScalesWithDmgBonus,
      context.Caster

    );

        // 5. 효과 적용 요청: StatusEffectManager에게 생성된 인스턴스를 전달하여 실제 효과 적용을 위임합니다.
        ServiceLocator.Get<StatusEffectManager>()?.ApplyStatusEffect(context.HitTarget.gameObject, effectInstance);
        Debug.Log($"<color=cyan>[{GetType().Name}]</color> '{context.HitTarget.name}'에게 '{StatusEffectID}' 효과 적용을 요청했습니다. (지속시간: {Duration}초, 초당피해: {DamageAmount})");
    }

}