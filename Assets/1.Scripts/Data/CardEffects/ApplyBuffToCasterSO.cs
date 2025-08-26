using UnityEngine;

/// <summary>
/// 시전자(Caster) 자신에게 지정된 상태 효과(주로 버프)를 부여하는 모듈입니다.
/// </summary>
[CreateAssetMenu(fileName = "Module_ApplyBuffToCaster_", menuName = "GameData/v8.0/Modules/ApplyBuffToCaster")]
public class ApplyBuffToCasterSO : CardEffectSO
{
    [Header("버프 설정")]
    [Tooltip("시전자에게 적용할 상태 이상 데이터(주로 버프)입니다.")]
    public StatusEffectDataSO buffToApply;

    public ApplyBuffToCasterSO()
    {
        // 버프는 일반적으로 발사 시점에 발동합니다.
        trigger = EffectTrigger.OnFire;
    }

    /// <summary>
    /// 시전자(context.Caster)에게 상태 이상을 적용합니다.
    /// </summary>
    public override void Execute(EffectContext context)
    {
        Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' 실행. 대상: {context.Caster?.name}");

        if (buffToApply == null)
        {
            Debug.LogWarning($"[ApplyBuffToCasterSO] '{this.name}' 모듈에 적용할 버프가 할당되지 않았습니다.");
            return;
        }

        if (context.Caster != null)
        {
            ServiceLocator.Get<StatusEffectManager>()?.ApplyStatusEffect(context.Caster.gameObject, buffToApply);
        }
    }
}