public enum CardType
{
    Physical, 
    Magical
}

public enum CardRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

public enum TriggerType
{
    Interval,
    OnHit,
    OnCrit,
    OnSkillUse,
    OnLowHealth
}

public enum CardEffectType
{
    
    SplitShot,  // 분열샷
    Wave,       // 파동
    Lightning,  // 번개
    Spiral      // 나선형 발사
}

public enum StatType
{
    Attack,
    AttackSpeed,
    MoveSpeed,
    Health,
    CritMultiplier,
    CritRate
}

public enum TargetingType
{
    Forward,
    Nearest,
    HighestHealth,
    LowestHealth,
    Random
}
