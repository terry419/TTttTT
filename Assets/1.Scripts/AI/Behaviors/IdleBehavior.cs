// 경로: ./TTttTT/Assets/1.Scripts/AI/Behaviors/IdleBehavior.cs
using UnityEngine;

/// <summary>
/// [행동 부품] 아무것도 하지 않고 제자리에 서서, 다른 행동으로 전환될 조건만 확인하는 가장 기본적인 행동입니다.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Behaviors/Idle")]
public class IdleBehavior : MonsterBehavior
{
    public override void OnEnter(MonsterController monster)
    {
        // 이 행동으로 들어왔을 때, 몬스터의 움직임을 완전히 멈춥니다.
        monster.rb.velocity = Vector2.zero;
    }

    public override void OnExecute(MonsterController monster)
    {
        // 매 프레임 특별히 할 일은 없으므로, 다른 행동으로 전환할 조건이 되었는지 검사만 합니다.
        CheckTransitions(monster);
    }

    public override void OnExit(MonsterController monster)
    {
        // 다른 행동으로 넘어갈 때 특별히 정리할 것은 없습니다.
    }
}