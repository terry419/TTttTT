// 경로: ./TTttTT/Assets/1.Scripts/AI/Transition.cs
using System;
using UnityEngine;

/// <summary>
/// [모듈형 AI 시스템의 핵심 설계도 3/3]
/// "어떤 '결정'을 통해, 어떤 '행동'으로 전환될 것인가?" 라는 단일 규칙을 정의하는 데이터 상자입니다.
/// [System.Serializable]은 이 클래스를 Unity Inspector 창에 표시되게 해주는 C#의 특별 기능입니다.
/// </summary>
[Serializable]
public class Transition
{
    [Tooltip("행동 전환 여부를 판단할 '결정' 부품(.asset)을 여기에 연결합니다.")]
    public Decision decision;

    [Tooltip("위 '결정'의 결과가 '참'일 경우, 다음으로 전환될 '행동' 부품(.asset)을 여기에 연결합니다.")]
    public MonsterBehavior nextBehavior;
}