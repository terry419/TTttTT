using UnityEngine;

[CreateAssetMenu(fileName = "Lifesteal_", menuName = "GameData/Card Effects/Lifesteal")]
public class LifestealEffectSO : CardEffectSO
{
    [Header("흡혈 설정")]
    [Tooltip("입힌 피해량 대비 회복할 체력의 비율 (%)")]
    [Range(0, 100)]
    public float lifestealPercentage;

    public override void Execute(EffectContext context)
    {
        // 이 로직은 6단계(EffectExecutor)에서 최종 구현됩니다.
        Debug.Log($"<color=lime>[LifestealEffect]</color> '{this.name}' 실행. (로직 구현 대기중)");
    }
}