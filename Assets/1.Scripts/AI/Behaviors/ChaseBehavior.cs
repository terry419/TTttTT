// 생성 경로: Assets/Scripts/AI/Behaviors/ChaseBehavior.cs
using UnityEngine;

/// <summary>
/// [신규 AI 시스템의 첫 번째 부품 1/1]
/// '플레이어를 추격한다'는 단일 기능을 수행하는 행동(Behavior) 부품입니다.
/// PoC 단계에서는 이 행동 하나만으로 새 시스템의 동작을 검증합니다.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Behaviors/Chase")]
public class ChaseBehavior : MonsterBehavior
{
    [Tooltip("추격 시 몬스터의 기본 이동 속도에 곱해질 배율입니다. 1.0은 100% 속도를 의미합니다.")]
    public float speedMultiplier = 1.0f;

    public override void OnEnter(MonsterController monster)
    {
        // 3번 개선안(보기 편한 로그)을 적용한 예시 로그입니다.
        // 나중에는 이 로그를 전담하는 시스템을 만들겠지만, PoC 단계에서는 이렇게 직접 기록합니다.
        Debug.Log($"[AI Log | Time: {Time.time:F2}] Monster: '{monster.gameObject.name}', Event: State Entered, State: 'ChaseBehavior'");
    }

    public override void OnExecute(MonsterController monster)
    {
        if (monster.playerTransform == null)
        {
            monster.rb.velocity = Vector2.zero;
            return;
        }

        Vector2 direction = ((Vector3)monster.playerTransform.position - monster.transform.position).normalized;
        monster.rb.velocity = direction * monster.monsterStats.FinalMoveSpeed * speedMultiplier;
    }
}