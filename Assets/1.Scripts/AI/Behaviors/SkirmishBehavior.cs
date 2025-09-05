// 경로: ./TTttTT/Assets/1/Scripts/AI/Behaviors/SkirmishBehavior.cs
using UnityEngine;

/// <summary>
/// [신규 고급 행동 부품] 대상과 최적의 거리를 유지하기 위해 접근 및 후퇴를 반복하는 '거리 유지' 행동입니다.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Behaviors/Skirmish")]
public class SkirmishBehavior : MonsterBehavior
{
    [Header("거리 설정")]
    [Tooltip("이 거리보다 가까워지면 후퇴합니다.")]
    public float tooCloseDistance = 7f;
    [Tooltip("이 거리보다 멀어지면 접근합니다.")]
    public float tooFarDistance = 15f;
    [Tooltip("거리 조절 시 이동 속도 배율입니다.")]
    public float speedMultiplier = 1.0f;

    public override void OnExecute(MonsterController monster)
    {
        if (monster.playerTransform == null)
        {
            monster.rb.velocity = Vector2.zero;
            return;
        }

        Vector3 monsterPos = monster.transform.position;
        Vector3 playerPos = monster.playerTransform.position;
        float distanceSq = Vector3.SqrMagnitude(playerPos - monsterPos);

        // 1. 플레이어와 너무 가까운 경우: 후퇴
        if (distanceSq < tooCloseDistance * tooCloseDistance)
        {
            Vector2 direction = (monsterPos - playerPos).normalized;
            monster.rb.velocity = direction * monster.monsterStats.FinalMoveSpeed * speedMultiplier;
        }
        // 2. 플레이어와 너무 먼 경우: 접근
        else if (distanceSq > tooFarDistance * tooFarDistance)
        {
            Vector2 direction = (playerPos - monsterPos).normalized;
            monster.rb.velocity = direction * monster.monsterStats.FinalMoveSpeed * speedMultiplier;
        }
        // 3. 최적의 거리인 경우: 움직임을 멈춤
        else
        {
            monster.rb.velocity = Vector2.zero;
        }

        // 거리 유지 행동을 하다가, 다른 행동(예: 소환)으로 전환할 조건이 되었는지 확인합니다.
        CheckTransitions(monster);
    }
}