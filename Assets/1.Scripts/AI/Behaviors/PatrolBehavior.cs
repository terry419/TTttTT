// 경로: ./TTttTT/Assets/1.Scripts/AI/Behaviors/PatrolBehavior.cs
using UnityEngine;

/// <summary>
/// [행동 부품] 자신의 시작 위치 주변을 무작위로 돌아다니며 순찰하는 행동입니다.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Behaviors/Patrol")]
public class PatrolBehavior : MonsterBehavior
{
    [Tooltip("순찰할 반경입니다. 시작 위치로부터 이 거리 안에서 움직입니다.")]
    public float patrolRadius = 10f;
    [Tooltip("순찰 시 이동 속도 배율입니다.")]
    public float speedMultiplier = 0.5f;

    // 이 변수는 몬스터 개체마다 따로 저장되어야 하므로, 몬스터 컨트롤러에 저장해야 하지만,
    // 여기서는 간단한 구현을 위해 static을 사용합니다. (나중에 더 복잡한 AI를 만들 때 개선할 수 있습니다.)
    private Vector3 _patrolTargetPosition;

    public override void OnEnter(MonsterController monster)
    {
        // 순찰 행동을 시작하자마자 첫 번째 순찰 목표 지점을 정합니다.
        UpdatePatrolTarget(monster);
    }

    public override void OnExecute(MonsterController monster)
    {
        // 목표 지점에 거의 도착했다면, 다음 목표 지점을 새로 정합니다.
        if (Vector3.SqrMagnitude(monster.transform.position - _patrolTargetPosition) < 1f)
        {
            UpdatePatrolTarget(monster);
        }

        // 목표 지점을 향해 이동합니다.
        Vector2 direction = (_patrolTargetPosition - monster.transform.position).normalized;
        monster.rb.velocity = direction * monster.monsterStats.FinalMoveSpeed * speedMultiplier;

        // 순찰 중에 다른 행동으로 전환될 조건(플레이어 발견 등)을 확인합니다.
        CheckTransitions(monster);
    }

    // 순찰할 새로운 목표 지점을 계산하는 함수
    private void UpdatePatrolTarget(MonsterController monster)
    {
        Vector2 randomOffset = Random.insideUnitCircle * patrolRadius;
        _patrolTargetPosition = monster.startPosition + new Vector3(randomOffset.x, randomOffset.y, 0);
    }
}