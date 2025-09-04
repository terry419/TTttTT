// 생성 경로: Assets/1.Scripts/AI/Decisions/PlayerInRangeDecision.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Monster AI/Decisions/Player In Range")]
public class PlayerInRangeDecision : Decision
{
    public float range = 10f;
    public bool negate = false;

    public override bool Decide(MonsterController monster)
    {
        if (monster.playerTransform == null) return false;
        float sqrDistance = (monster.playerTransform.position - monster.transform.position).sqrMagnitude;
        bool isInRange = sqrDistance < range * range;
        return negate ? !isInRange : isInRange;
    }
}