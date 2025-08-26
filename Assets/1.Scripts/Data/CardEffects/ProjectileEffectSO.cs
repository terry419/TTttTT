using UnityEngine;

/// <summary>
/// 투사체에 관통, 튕김, 추적 등의 특수 능력을 부여하는 모듈입니다.
/// </summary>
[CreateAssetMenu(fileName = "Module_Projectile_", menuName = "GameData/v8.0/Modules/ProjectileEffect")]
public class ProjectileEffectSO : CardEffectSO
{
    [Header("투사체 특수 능력")]
    [Tooltip("관통 횟수")]
    public int pierceCount = 0;

    [Tooltip("튕김 횟수")]
    public int ricochetCount = 0;

    [Tooltip("가장 가까운 적을 추적하는 기능")]
    public bool isTracking = false;

    // TODO: Sequential Payloads (연쇄 효과) 기능 추가 필요

    /// <summary>
    /// ProjectileEffect의 로직을 실행합니다. (현재는 플레이스홀더)
    /// </summary>
    public override void Execute(EffectContext context)
    {
        Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' 실행.");
        // 여기에 실제 투사체 능력치에 관여하는 로직이 추가되어야 합니다.
        // 예: context.FiringSpec.pierceCount += this.pierceCount;
    }
}
