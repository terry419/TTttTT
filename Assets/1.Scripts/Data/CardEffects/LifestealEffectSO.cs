using UnityEngine;

/// <summary>
/// OnHit 시점에 피해량의 일정 비율만큼 체력을 회복하는 모듈입니다.
/// </summary>
[CreateAssetMenu(fileName = "Module_Lifesteal_", menuName = "GameData/v8.0/Modules/LifestealEffect")]
public class LifestealEffectSO : CardEffectSO
{
    [Header("흡혈 설정")]
    [Tooltip("피해량 대비 회복할 체력의 비율 (%)")]
    [Range(0f, 100f)]
    public float lifestealPercentage = 10f;

    public LifestealEffectSO()
    {
        // 흡혈은 일반적으로 피격 시점에 발동합니다.
        trigger = EffectTrigger.OnHit;
    }

    /// <summary>
    /// 실제 흡혈 로직은 EffectExecutor 또는 BulletController에서
    /// 이 모듈의 존재 여부와 lifestealPercentage 값을 확인하여 처리하게 됩니다.
    /// 따라서 이 모듈의 Execute는 비워둡니다.
    /// </summary>
    public override void Execute(EffectContext context)
    {
        Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' 실행. 흡혈률: {lifestealPercentage}%.");
        // 이 모듈은 데이터를 제공하는 역할만 합니다.
    }
}