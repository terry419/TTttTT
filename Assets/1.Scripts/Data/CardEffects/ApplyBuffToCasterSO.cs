using UnityEngine;

/// <summary>
/// 시전자(Caster) 자신에게 상태 이상 효과(버프 등)를 부여하는 모듈입니다.
/// </summary>
[CreateAssetMenu(fileName = "Module_ApplyBuffToCaster_", menuName = "GameData/CardData/Modules/ApplyBuffToCaster")]
public class ApplyBuffToCasterSO : CardEffectSO
{
    [Header("버프 정보")]
    [Tooltip("시전자에게 적용할 상태 이상 데이터(버프 등)입니다.")]
    public StatusEffectDataSO buffToApply;

    public ApplyBuffToCasterSO()
    {
        // 기본적으로는 발사 시에 발동됩니다.
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
            // TODO: VFXManager를 통해 onBuffAppliedVFXRef 재생 로직 추가
        }
    }
}