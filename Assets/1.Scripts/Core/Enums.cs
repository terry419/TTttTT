/// <summary>
/// 카드의 타입을 정의합니다.
/// </summary>
public enum CardType
{
    Physical, // 물리
    Magical   // 마법
}

/// <summary>
/// 카드의 희귀도를 정의합니다.
/// </summary>
public enum CardRarity
{
    Common,    // 일반
    Rare,      // 희귀
    Epic,      // 영웅
    Legendary  // 전설
}

/// <summary>
/// 카드가 발동되는 조건을 정의합니다.
/// project_plan.md에 명시된 TriggerType 열거형입니다.
/// </summary>
public enum TriggerType
{
    Interval,    // 주기적 발동 (예: 10초마다)
    OnHit,       // 적중 시 발동
    OnCrit,      // 치명타 시 발동
    OnSkillUse,  // 스킬 사용 시 발동
    OnLowHealth  // 체력이 낮을 때 발동 (project_plan.md 및 CardDataSO.cs 주석 참조)
}

/// <summary>
/// 카드의 특수 효과 타입을 정의합니다.
/// </summary>
public enum CardEffectType
{
    None,       // 특수 효과 없음
    SplitShot,  // 분열샷 (다중 공격)
    Wave,       // 파동
    Lightning,  // 번개
    Spiral,     // 나선형 발사
    // TODO: project_plan.md에 언급된 다른 특수 효과들 추가 (예: 독성, 과유불급 등)
}
