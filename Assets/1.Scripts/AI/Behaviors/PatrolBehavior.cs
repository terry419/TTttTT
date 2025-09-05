// 경로: ./TTttTT/Assets/1/Scripts/AI/Behaviors/PatrolBehavior.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Monster AI/Behaviors/Patrol")]
public class PatrolBehavior : MonsterBehavior
{
    [Tooltip("순찰할 반경입니다. 시작 위치로부터 이 거리 안에서 움직입니다.")]
    public float patrolRadius = 10f;
    [Tooltip("순찰 시 이동 속도 배율입니다.")]
    public float speedMultiplier = 0.5f;

    private Vector3 _patrolTargetPosition;

    public override void OnEnter(MonsterController monster)
    {
        UpdatePatrolTarget(monster);
        base.OnEnter(monster);
    }

    public override void OnExecute(MonsterController monster)
    {
        if (Vector3.SqrMagnitude(monster.transform.position - _patrolTargetPosition) < 1f)
        {
            UpdatePatrolTarget(monster);
        }

        Vector2 direction = (_patrolTargetPosition - monster.transform.position).normalized;
        monster.rb.velocity = direction * monster.monsterStats.FinalMoveSpeed * speedMultiplier;

        CheckTransitions(monster);
    }

    private void UpdatePatrolTarget(MonsterController monster)
    {
        Vector2 randomOffset = Random.insideUnitCircle * patrolRadius;
        _patrolTargetPosition = monster.startPosition + new Vector3(randomOffset.x, randomOffset.y, 0);
    }
}