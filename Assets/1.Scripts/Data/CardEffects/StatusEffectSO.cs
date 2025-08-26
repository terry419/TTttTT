using UnityEngine;

/// <summary>
/// 피격된 대상에게 지정된 상태 이상을 부여하는 모듈입니다.
/// </summary>
[CreateAssetMenu(fileName = "Module_ApplyStatus_", menuName = "GameData/v8.0/Modules/ApplyStatusEffect")]
public class ApplyStatusEffectSO : CardEffectSO
{
    [Header("상태 이상 설정")]
    [Tooltip("적에게 적용할 상태 이상 데이터(독, 화상 등)입니다.")]
    public StatusEffectDataSO statusToApply;

    /// <summary>
    /// 이 모듈은 투사체가 적에게 명중했을 때(OnHit) 실행되는 것이 일반적입니다.
    /// </summary>
    public ApplyStatusEffectSO()
    {
        trigger = EffectTrigger.OnHit;
    }

    /// <summary>
    /// 피격된 대상(context.HitTarget)에게 상태 이상을 적용합니다.
    /// </summary>
    public override void Execute(EffectContext context)
    {
        Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' 실행. 대상: {context.HitTarget?.name}");

        if (statusToApply == null)
        {
            Debug.LogWarning($"[ApplyStatusEffectSO] '{this.name}' 모듈에 적용할 StatusEffectDataSO가 할당되지 않았습니다.");
            return;
        }

        if (context.HitTarget != null)
        {
            // StatusEffectManager를 통해 대상에게 상태 이상 효과를 적용하도록 요청합니다.
            ServiceLocator.Get<StatusEffectManager>()?.ApplyStatusEffect(context.HitTarget.gameObject, statusToApply);
        }
    }
}