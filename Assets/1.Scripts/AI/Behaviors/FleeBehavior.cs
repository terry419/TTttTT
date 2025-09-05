// 경로: ./TTttTT/Assets/1.Scripts/AI/Behaviors/FleeBehavior.cs
using UnityEngine;

/// <summary>
/// [행동 부품] 대상으로부터 멀어지는 방향으로 이동하는 행동입니다.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Behaviors/Flee")]
public class FleeBehavior : MonsterBehavior
{
    [Tooltip("도망 시 기본 이동 속도에 곱해질 배율입니다.")]
    public float speedMultiplier = 1.2f; // 보통 도망은 추격보다 약간 빠르게 설정합니다.

    public override void OnExecute(MonsterController monster)
    {
        if (monster.playerTransform == null)
        {
            monster.rb.velocity = Vector2.zero;
            return;
        }

        // 1. 플레이어로부터 '멀어지는' 방향 벡터를 계산합니다. (자신의 위치 - 플레이어 위치)
        Vector2 direction = (monster.transform.position - monster.playerTransform.position).normalized;

        // 2. 계산된 방향으로 이동시킵니다.
        monster.rb.velocity = direction * monster.monsterStats.FinalMoveSpeed * speedMultiplier;

        // 3. 도망치다가 다른 행동으로 전환될 조건이 되었는지 확인합니다. (예: 플레이어가 충분히 멀어짐)
        CheckTransitions(monster);
    }
}