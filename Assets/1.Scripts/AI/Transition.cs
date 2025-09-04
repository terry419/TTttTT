// 생성 경로: Assets/Scripts/AI/Transition.cs
using System;
using UnityEngine;

/// <summary>
/// [신규 AI 시스템의 뼈대 3/4]
/// "어떤 '판단'이 참일 때, 다음 '행동'은 무엇인가?"라는 하나의 규칙을 정의하는 데이터 구조입니다.
/// </summary>
[Serializable]
public class Transition
{
    [Tooltip("'만약'에 해당하는 '판단' 부품(Decision)을 여기에 연결합니다.")]
    public Decision decision;

    [Tooltip("위의 '판단'이 참일 경우, 전환될 다음 '행동' 부품(Behavior)을 여기에 연결합니다.")]
    public MonsterBehavior nextBehavior;
}