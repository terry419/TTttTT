public static class CardActionFactory
{
    // 미리 인스턴스를 만들어두어 매번 new로 생성하는 비용을 줄입니다.
    private static readonly SingleShotAction _singleShotAction = new SingleShotAction();
    private static readonly SplitShotAction _splitShotAction = new SplitShotAction();

    public static ICardAction Create(CardEffectType effectType)
    {
        switch (effectType)
        {
            case CardEffectType.SplitShot:
                return _splitShotAction;
            // 새로운 카드 액션을 추가하려면 여기에 case만 추가하면 됩니다.
            // case CardEffectType.Lightning:
            //     return _lightningAction;
            default:
                // 기본값 또는 정의되지 않은 타입은 SingleShot으로 처리
                return _singleShotAction;
        }
    }
}