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
        if (monster.playerTransform == null) return false;

        // Vector3.Distance는 제곱근 연산 때문에 비교적 느립니다.
        // 거리 자체보다 '안/밖' 여부만 판단할 때는, 제곱 거리를 비교하는 것이 훨씬 효율적입니다.
        float sqrDistance = (monster.playerTransform.position - monster.transform.position).sqrMagnitude;

        bool isInRange = sqrDistance < range * range;

        // negate가 체크되어 있으면 결과를 뒤집어서 반환합니다.
        return negate ? !isInRange : isInRange;
    }
}