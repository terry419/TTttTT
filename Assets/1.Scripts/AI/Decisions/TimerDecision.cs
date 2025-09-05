// 경로: ./TTttTT/Assets/1.Scripts/AI/Decisions/TimerDecision.cs
using UnityEngine;

/// <summary>
/// [결정 부품] 현재 행동을 시작한 후 지정된 시간이 경과했는지 판단하는 '타이머 센서'입니다.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Decisions/Timer")]
public class TimerDecision : Decision
{
    [Tooltip("판단 기준이 되는 시간(초)입니다.")]
    public float duration = 5f;

    public override bool Decide(MonsterController monster)
    {
        // MonsterController에 만들어 둔 stateTimer(현재 행동 경과 시간)를 사용합니다.
        // 경과 시간이 설정된 duration보다 크거나 같으면 true를 반환합니다.
        return monster.stateTimer >= duration;
    }
}