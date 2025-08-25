using UnityEngine;

[CreateAssetMenu(fileName = "Buff_", menuName = "GameData/Card Effects/Apply Buff to Caster")]
public class ApplyBuffToCasterEffectSO : CardEffectSO
{
    [Header("버프 설정")]
    [Tooltip("시전자에게 적용할 버프 데이터 에셋")]
    public StatusEffectDataSO buffToApply;

    [Header("시각 효과 (VFX)")]
    [Tooltip("버프가 적용되는 순간 시전자 위치에 재생할 VFX의 ID")]
    public string onBuffAppliedVFXKey;

    public override void Execute(EffectContext context)
    {
        // 이 로직은 6단계(EffectExecutor)에서 최종 구현됩니다.
        Debug.Log($"<color=lime>[ApplyBuffToCaster]</color> '{this.name}' 실행. (로직 구현 대기중)");
    }
}