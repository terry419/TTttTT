// 생성 경로: Assets/Scripts/AI/Decisions/Decision.cs
using UnityEngine;

/// <summary>
/// [신규 AI 시스템의 뼈대 2/4]
/// 모든 '판단' 부품('플레이어가 범위 안에 있는가?' 등)이 상속받아야 하는 추상 클래스입니다.
/// 이는 '센서' 부품의 규격을 정하는 것과 같습니다.
/// 모든 센서는 'Decide'라는 기능을 통해 "예" 또는 "아니오" (true/false) 중 하나로만 대답해야 합니다.
/// </summary>
public abstract class Decision : ScriptableObject
{
    /// <summary>
    /// 현재 상황을 판단하여 행동을 전환해야 하면 true, 아니면 false를 반환합니다.
    /// </summary>
    public abstract bool Decide(MonsterController monster);
}