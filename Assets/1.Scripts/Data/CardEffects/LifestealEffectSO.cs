using UnityEngine;

/// <summary>
/// OnHit 이벤트 발생 시, 가한 데미지의 일정 비율만큼 체력을 회복하는 모듈입니다.
/// </summary>
[CreateAssetMenu(fileName = "Module_Lifesteal_", menuName = "GameData/CardData/Modules/LifestealEffect")]
public class LifestealEffectSO : CardEffectSO
{
    [Header("흡혈 설정")]
    [Tooltip("가한 데미지 대비 회복할 체력의 비율 (%)")]
    [Range(0f, 100f)]
    public float lifestealPercentage = 10f;

    public LifestealEffectSO()
    {
        // 기본적으로는 피격 시에 발동됩니다.
        trigger = EffectTrigger.OnHit;
    }

    /// <summary>
    /// 피해를 입힌 주체(Caster)가 가한 데미지(context.DamageDealt)에 비례하여 체력을 회복합니다.
    /// </summary>
    public override void Execute(EffectContext context)
    {
        if (context.Caster != null && context.DamageDealt > 0)
        {
            float healAmount = context.DamageDealt * (lifestealPercentage / 100f);
            context.Caster.Heal(healAmount);
        }
    }
}