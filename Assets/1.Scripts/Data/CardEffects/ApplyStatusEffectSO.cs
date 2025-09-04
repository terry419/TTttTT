using UnityEngine;

/// <summary>
/// 피격된 대상에게 지정된 상태 이상을 부여하는 모듈입니다.
/// </summary>
[CreateAssetMenu(fileName = "Module_ApplyStatus_", menuName = "GameData/CardData/Modules/ApplyStatusEffect")]
public class ApplyStatusEffectSO : CardEffectSO
{
    [Header("상태 이상 정보")]
    [Tooltip("적용할 상태 이상 데이터(독, 화상 등)입니다.")]
    public StatusEffectDataSO statusToApply;

    /// <summary>
    /// 이 효과는 발사체가 몬스터에 명중했을 때(OnHit) 발동되는 것이 일반적입니다.
    /// </summary>
    public ApplyStatusEffectSO()
    {
        trigger = EffectTrigger.OnHit;
    }

    /// <summary>
    /// 피격된 적(context.HitTarget)에게 상태 이상을 적용합니다.
    /// </summary>
    public override void Execute(EffectContext context)
    {
        Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' 실행. 대상: {context.HitTarget?.name}");
        if (statusToApply != null && context.HitTarget != null)
        {
            ServiceLocator.Get<StatusEffectManager>()?.ApplyStatusEffect(context.HitTarget.gameObject, statusToApply);
        }
    }
}