// 경로: ./TTttTT/Assets/1/Scripts/Data/CardEffects/ApplyEffectToHitTargetModule.cs
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

/// <summary>
/// [v2.2 최종본] 오직 적중된 대상(HitTarget)에게만 모든 종류의 효과(스탯, 지속/즉발 피해 및 회복)를 적용하는 통합 모듈입니다.
/// </summary>
[CreateAssetMenu(fileName = "Module_ApplyEffectToTarget_", menuName = "GameData/CardData/Modules/ApplyEffectToHitTarget")]
public class ApplyEffectToHitTargetModule : CardEffectSO
{
    [Header("[ 발동 조건 ]")]
    [Tooltip("효과가 실제로 적용될 확률 (0.0 ~ 100.0)")]
    [Range(0f, 100f)] public float ApplicationChance = 100f;

    [Header("[ 효과 내용 - 공통 ]")]
    [Tooltip("이 효과를 식별할 고유 ID입니다. (예: Poison, Burn, Slow)")]
    public string StatusEffectID;

    [Tooltip("스탯 변경 및 지속 효과의 지속 시간 (초). 0으로 설정 시 즉발성 효과가 됩니다.")]
    public float Duration = 3.0f;

    [Tooltip("동일한 ID의 효과가 이미 대상에게 있을 때 어떻게 처리할지 결정합니다.")]
    public StackingBehavior StackingBehavior = StackingBehavior.RefreshDuration;

    [Header("[ 효과 내용 - 스탯 변경 (%) ]")]
    [Tooltip("대상의 이동속도를 % 단위로 변경합니다. (음수 가능)")]
    public float MoveSpeedBonus;

    [Tooltip("대상이 받는 모든 피해량을 % 단위로 변경합니다.")]
    public float DamageTakenBonus;

    [Tooltip("대상의 접촉 공격력을 % 단위로 변경합니다.")]
    public float ContactDamageBonus;

    [Header("[ 효과 내용 - 지속 피해 (DoT) ]")]
    [Tooltip("지속 피해량 계산 방식입니다.")]
    public DamageType DamageType = DamageType.Flat;

    [Tooltip("초당 가하는 피해량입니다.")]
    public float DamageAmount;

    [Tooltip("체크 시, 시전자(플레이어)의 최종 공격력 보너스 스탯이 DoT 피해량에 곱연산으로 영향을 줍니다.")]
    public bool ScalesWithDmgBonus = false;

    [Header("[ 효과 내용 - 회복 ]")]
    [Tooltip("회복이 이루어지는 시간(초). 0일 경우 즉시 회복됩니다.")]
    public float HealDuration;

    [Tooltip("회복량 계산 방식입니다.")]
    public HealType HealType = HealType.Flat;

    [Tooltip("총 회복량입니다.")]
    public float HealAmount;

    [Header("[ 효과 내용 - VFX ]")]
    [Tooltip("효과가 처음 적용되는 순간 1회 재생될 VFX의 어드레서블 주소.")]
    public AssetReferenceGameObject OnApplyVFX;

    [Tooltip("효과가 지속되는 동안 몬스터에게 붙어서 계속 재생될 VFX의 어드레서블 주소.")]
    public AssetReferenceGameObject LoopingVFX;

    [Tooltip("효과의 지속시간이 끝나 사라지는 순간 1회 재생될 VFX의 어드레서블 주소.")]
    public AssetReferenceGameObject OnExpireVFX;

    /// <summary>
    /// 모듈의 핵심 실행 로직입니다.
    /// </summary>
    public override void Execute(EffectContext context)
    {
        // 1. 유효성 검사: 피격 대상이 없으면 아무것도 하지 않습니다.
        if (context.HitTarget == null)
        {
            Debug.LogWarning($"<color=yellow>[{GetType().Name}]</color> '{this.name}' 실행 중단: HitTarget이 없어 효과를 적용할 대상이 없습니다.");
            return;
        }

        Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' 실행 시도. 대상: {context.HitTarget.name}");

        // 2. 확률 체크
        if (Random.Range(0f, 100f) > ApplicationChance)
        {
            Debug.Log($"<color=yellow>[{GetType().Name}]</color> 확률({ApplicationChance}%) 체크에 실패하여 효과가 적용되지 않았습니다.");
            return;
        }

        // 3. 스탯 보너스 데이터 구성
        var statBonuses = new Dictionary<StatType, float>();
        if (MoveSpeedBonus != 0) statBonuses.Add(StatType.MoveSpeed, MoveSpeedBonus);
        if (DamageTakenBonus != 0) statBonuses.Add(StatType.DamageTaken, DamageTakenBonus);
        if (ContactDamageBonus != 0) statBonuses.Add(StatType.ContactDamage, ContactDamageBonus);

        // 4. 모든 데이터를 담아 StatusEffectInstance를 생성합니다.
        var effectInstance = new StatusEffectInstance(
            target: context.HitTarget.gameObject,
            id: StatusEffectID,
            duration: Duration,
            bonuses: statBonuses,
            dotAmount: DamageAmount,
            dotType: DamageType,
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

        // 5. StatusEffectManager에게 효과 적용을 최종 요청합니다.
        ServiceLocator.Get<StatusEffectManager>()?.ApplyStatusEffect(context.HitTarget.gameObject, effectInstance);
        Debug.Log($"<color=cyan>[{GetType().Name}]</color> '{context.HitTarget.name}'에게 '{StatusEffectID}' 효과 적용을 요청했습니다.");
    }
}