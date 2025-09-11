// 경로: ./TTttTT/Assets/1.Scripts/AI/Decisions/PlayerInRangeDecision.cs
using UnityEngine;

/// <summary>
/// [결정 부품] 플레이어가 지정된 범위 안에 있는지 판단하는 '센서'입니다.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Decisions/Player In Range")]
public class PlayerInRangeDecision : Decision
{
    [Tooltip("판단 기준이 되는 거리(반지름)입니다.")]
    public float range = 10f;

    [Tooltip("체크 시, 판단 결과를 반대로 뒤집습니다. (범위 '밖'에 있으면 true)")]
    public bool negate = false;

    public override bool Decide(MonsterController monster)
    {
        if (monster.targetTransform == null) return false;

        float sqrDistance = (monster.targetTransform.position - monster.transform.position).sqrMagnitude;
        bool isInRange = sqrDistance < range * range;
        bool result = negate ? !isInRange : isInRange;

        float distance = Mathf.Sqrt(sqrDistance);

        return result;
    }
}