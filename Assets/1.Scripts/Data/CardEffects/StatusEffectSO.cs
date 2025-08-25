using UnityEngine;

[CreateAssetMenu(fileName = "Status_", menuName = "GameData/Card Effects/Apply Status Effect")]
public class StatusEffectSO : CardEffectSO
{
    [Header("상태 이상 설정")]
    [Tooltip("적용할 상태 이상 데이터 에셋")]
    public StatusEffectDataSO statusToApply;

    public override void Execute(EffectContext context)
    {
        // 이 로직은 6단계(EffectExecutor)에서 최종 구현됩니다.
        Debug.Log($"<color=lime>[StatusEffect]</color> '{this.name}' 실행. (로직 구현 대기중)");
    }
}