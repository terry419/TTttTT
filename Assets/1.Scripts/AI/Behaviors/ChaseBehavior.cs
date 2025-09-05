// 경로: ./TTttTT/Assets/1.Scripts/AI/Behaviors/ChaseBehavior.cs
using UnityEngine;

/// <summary>
/// [행동 부품] 대상을 향해 지정된 속도로 이동하는 행동입니다.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Behaviors/Chase")]
public class ChaseBehavior : MonsterBehavior
{
    [Tooltip("추격 시 기본 이동 속도에 곱해질 배율입니다. 1.0은 100% 속도를 의미합니다.")]
    public float speedMultiplier = 1.0f;

    public override void OnEnter(MonsterController monster)
    {
        // 특별한 초기화 작업은 없습니다.
    }

    public override void OnExecute(MonsterController monster)
    {
        // 플레이어가 없다면 움직이지 않습니다.
        if (monster.playerTransform == null)
        {
            monster.rb.velocity = Vector2.zero;
            return;
        }

        // 1. 플레이어 방향으로 향하는 벡터를 계산합니다.
        Vector2 direction = (monster.playerTransform.position - monster.transform.position).normalized;

        // 2. 몬스터의 최종 이동 속도에 배율을 곱하여 속도를 결정하고, 물리 엔진을 통해 이동시킵니다.
        monster.rb.velocity = direction * monster.monsterStats.FinalMoveSpeed * speedMultiplier;

        // 3. 추격을 하다가 다른 행동으로 전환될 조건이 되었는지 확인합니다. (예: 플레이어가 너무 멀어짐)
        CheckTransitions(monster);
    }

    public override void OnExit(MonsterController monster)
    {
        // 추격 행동을 멈출 때, 혹시 모를 관성을 없애기 위해 속도를 0으로 초기화합니다.
        monster.rb.velocity = Vector2.zero;
    }
}