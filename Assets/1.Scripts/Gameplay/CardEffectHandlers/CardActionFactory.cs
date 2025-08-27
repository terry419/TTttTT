public static class CardActionFactory
{
    // �̸� �ν��Ͻ��� �����ξ� �Ź� new�� �����ϴ� ����� ���Դϴ�.
    private static readonly SingleShotAction _singleShotAction = new SingleShotAction();
    private static readonly SplitShotAction _splitShotAction = new SplitShotAction();

    public static ICardAction Create(CardEffectType effectType)
    {
        switch (effectType)
        {
            case CardEffectType.SplitShot:
                return _splitShotAction;
            // ���ο� ī�� �׼��� �߰��Ϸ��� ���⿡ case�� �߰��ϸ� �˴ϴ�.
            // case CardEffectType.Lightning:
            //     return _lightningAction;
            default:
                // �⺻�� �Ǵ� ���ǵ��� ���� Ÿ���� SingleShot���� ó��
                return _singleShotAction;
        }
    }
}