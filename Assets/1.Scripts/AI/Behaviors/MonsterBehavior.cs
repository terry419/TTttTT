// 경로: ./TTttTT/Assets/1.Scripts/AI/Behaviors/MonsterBehavior.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// [모듈형 AI 시스템의 핵심 설계도 1/3]
/// '추격', '돌진' 등 모든 구체적인 '행동' 부품들이 반드시 따라야 하는 기본 구조(설계도)입니다.
/// ScriptableObject를 상속하여, 각 행동을 파일(.asset)로 만들어 재사용하고 조립할 수 있습니다.
/// </summary>
public abstract class MonsterBehavior : ScriptableObject
{
    [Header("행동 전환 규칙 목록")]
    [Tooltip("이 행동이 다른 행동으로 전환될 수 있는 '규칙'들의 목록입니다.")]
    public List<Transition> transitions = new List<Transition>();

    /// <summary>
    /// 이 행동이 처음 시작될 때 단 한 번 호출되는 초기화 함수입니다.
    /// (예: 돌진 공격의 방향을 처음 한 번만 계산)
    /// </summary>
    /// <param name="monster">이 행동을 실행하는 몬스터의 '엔진'</param>
    public virtual void OnEnter(MonsterController monster) { }

    /// <summary>
    /// 이 행동이 활성화된 동안 매 프레임(또는 주기적으로) 실행되는 핵심 로직입니다.
    /// </summary>
    /// <param name="monster">이 행동을 실행하는 몬스터의 '엔진'</param>
    public abstract void OnExecute(MonsterController monster);

    /// <summary>
    /// 이 행동이 다른 행동으로 전환되기 직전, 단 한 번 호출되는 마무리 함수입니다.
    /// (예: 사용했던 타이머나 변수들을 초기화)
    /// </summary>
    /// <param name="monster">이 행동을 실행하는 몬스터의 '엔진'</param>
    public virtual void OnExit(MonsterController monster) { }

    /// <summary>
    /// 모든 행동 부품들이 공통으로 사용하는 '상태 전환 검사' 기능입니다.
    /// transitions 목록에 있는 모든 '규칙'을 하나씩 확인하여, 조건이 맞으면 다음 행동으로 전환시킵니다.
    /// </summary>
    /// <param name="monster">이 행동을 실행하는 몬스터의 '엔진'</param>
    protected void CheckTransitions(MonsterController monster)
    {
        if (transitions == null || transitions.Count == 0) return;

        foreach (var transition in transitions)
        {
            // 1. 이 전환 규칙의 '결정' 부품에게 지금 상태를 바꿀지 물어봅니다.
            if (transition.decision != null && transition.decision.Decide(monster))
            {
                // 2. "바꿔라!" (true) 라는 답이 오면, 지정된 다음 행동으로 전환합니다.
                if (transition.nextBehavior != null)
                {
                    monster.ChangeBehavior(transition.nextBehavior);
                    return; // 한 번에 하나의 전환만 처리하고 즉시 종료합니다.
                }
            }
        }
    }
}