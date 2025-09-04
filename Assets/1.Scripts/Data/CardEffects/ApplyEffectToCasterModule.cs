// 경로: Assets/1.Scripts/Data/CardEffects/ApplyEffectToCasterModule.cs
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

/// <summary>
/// [신규] 카드 시전자 자신(Caster)에게만 모든 종류의 효과(버프, 스탯/지속 피해 및 회복, VFX)를 부여하는 통합 모듈입니다.
/// </summary>
[CreateAssetMenu(fileName = "Module_ApplyEffectToCaster_", menuName = "GameData/CardData/Modules/ApplyEffectToCaster")]
public class ApplyEffectToCasterModule : CardEffectSO
{
    [Header("[ 발동 조건 ]")]
    [Tooltip("효과 발동 시점을 설정합니다.")]
    public EffectTrigger Trigger = EffectTrigger.OnFire;

    [Tooltip("효과가 적용될 확률 (0.0 ~ 100.0)")]
    [Range(0f, 100f)] public float ApplicationChance = 100f;

    [Header("[ 효과 내용 - 기본 ]")]
    [Tooltip("이 효과를 식별할 고유 ID를 지정합니다. (예: 화상, 빙결, 가속)")]
    public string StatusEffectID;

    [Tooltip("상태 이상 및 지속 효과의 지속 시간(초). 0보다 큰 값을 반드시 입력해야 합니다.")]
    public float Duration = 5.0f;

    [Tooltip("상태 효과 중첩 방식을 설정합니다.")]
    public StackingBehavior StackingBehavior = StackingBehavior.RefreshDuration;

    [Header("[ 효과 내용 - 플레이어 스탯 보너스 (%) ]")]
    [Tooltip("플레이어의 최종 공격력 수식어를 설정합니다.")]
    public float FinalDamageBonus;
    [Tooltip("플레이어의 최종 공격 속도를 설정합니다.")]
    public float FinalAttackSpeedBonus;
    [Tooltip("플레이어의 최종 이동 속도를 설정합니다.")]
    public float FinalMoveSpeedBonus;
    [Tooltip("플레이어의 최종 최대 체력을 설정합니다.")]
    public float FinalHealthBonus;
    [Tooltip("플레이어의 최종 치명타 확률을 설정합니다.")]
    public float FinalCritRateBonus;
    [Tooltip("플레이어의 최종 치명타 데미지를 설정합니다.")]
    public float FinalCritDamageBonus;

    [Header("[ 효과 내용 - 지속 데미지 (DoT) ]")]
    [Tooltip("데미지 유형 (Flat: 고정 수치, MaxHealthPercentage: 최대 체력 비례)")]
    public DamageType DamageType = DamageType.Flat;
    [Tooltip("초당 지속 데미지. (틱 주기는 1초로 고정)")]
    public float DamageAmount;
    [Tooltip("체크 시, 자신의 FinalDamageBonus 스탯이 DoT 데미지에 영향을 줍니다.")]
    public bool ScalesWithDmgBonus = false;

    [Header("[ 효과 내용 - 회복 ]")]
    [Tooltip("회복이 일어나는 지속 시간 (초). 0은 즉시 회복을 의미합니다.")]
    public float HealDuration;
    [Tooltip("회복 유형 (Flat: 고정 수치, MaxHealthPercentage: 최대 체력 비례)")]
    public HealType HealType = HealType.Flat;
    [Tooltip("총 회복량.")]
    public float HealAmount;

    [Header("[ 효과 내용 - VFX ]")]
    [Tooltip("효과가 처음 적용되는 순간 1회 재생될 VFX의 프리팹 주소.")]
    public AssetReferenceGameObject OnApplyVFX;
    [Tooltip("효과가 지속되는 동안 플레이어에게 붙어서 계속 재생될 VFX의 프리팹 주소.")]
    public AssetReferenceGameObject LoopingVFX;
    [Tooltip("효과의 지속시간이 끝나 사라지는 순간 1회 재생될 VFX의 프리팹 주소.")]
    public AssetReferenceGameObject OnExpireVFX;

    public override void Execute(EffectContext context)
    {
        // 1. 유효성 검사: 시전자가 없으면 효과를 적용할 수 없습니다.
        if (context.Caster == null)
        {
            Debug.LogWarning($"<color=yellow>[{GetType().Name}]</color> '{this.name}' 실행 중단: Caster가 없어 효과를 적용할 대상이 없습니다.");
            return;
        }

        Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' 실행 시도. 대상: {context.Caster.name}");

        // 2. 확률 체크
        if (Random.Range(0f, 100f) > ApplicationChance)
        {
            Debug.Log($"<color=yellow>[{GetType().Name}]</color> 확률({ApplicationChance}%) 체크에 실패하여 효과가 적용되지 않았습니다.");
            return;
        }

        // 3. 스탯 수식어 딕셔너리 생성
        var statBonuses = new Dictionary<StatType, float>();
        if (FinalDamageBonus != 0) statBonuses.Add(StatType.Attack, FinalDamageBonus);
        if (FinalAttackSpeedBonus != 0) statBonuses.Add(StatType.AttackSpeed, FinalAttackSpeedBonus);
        if (FinalMoveSpeedBonus != 0) statBonuses.Add(StatType.MoveSpeed, FinalMoveSpeedBonus);
        if (FinalHealthBonus != 0) statBonuses.Add(StatType.Health, FinalHealthBonus);
        if (FinalCritRateBonus != 0) statBonuses.Add(StatType.CritRate, FinalCritRateBonus);
        if (FinalCritDamageBonus != 0) statBonuses.Add(StatType.CritMultiplier, FinalCritDamageBonus);

        // 4. 모든 데이터를 종합해 StatusEffectInstance를 생성합니다.
        var effectInstance = new StatusEffectInstance(
            target: context.Caster.gameObject,
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
        ServiceLocator.Get<StatusEffectManager>()?.ApplyStatusEffect(context.Caster.gameObject, effectInstance);
        Debug.Log($"<color=cyan>[{GetType().Name}]</color> '{context.Caster.name}'에게 '{StatusEffectID}' 효과 적용을 요청했습니다.");
    }
}