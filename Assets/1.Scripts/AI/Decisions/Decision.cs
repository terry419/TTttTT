// 경로: ./TTttTT/Assets/1.Scripts/AI/Decisions/Decision.cs
using UnityEngine;

/// <summary>
/// [모듈형 AI 시스템의 핵심 설계도 2/3]
/// '플레이어가 범위 안에 있는가?'와 같은 모든 '결정' 부품들이 따라야 하는 기본 구조(설계도)입니다.
/// 모든 결정 부품은 반드시 "예(true)" 또는 "아니오(false)" 중 하나만 대답해야 합니다.
/// </summary>
public abstract class Decision : ScriptableObject
{
    /// <summary>
    /// 현재 몬스터의 상황을 판단하여 행동을 전환해야 하면 true, 아니면 false를 반환합니다.
    /// </summary>
    /// <param name="monster">판단의 주체가 되는 몬스터의 '엔진'</param>
    /// <returns>행동을 전환할지 여부 (true/false)</returns>
    public abstract bool Decide(MonsterController monster);
}