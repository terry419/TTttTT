using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "Module_FireSynergyBuff_", menuName = "GameData/CardData/Modules/FireSynergyBuff")]
public class FireSynergyBuffSO : CardEffectSO
{
    [Header("�ó��� ����")]
    public CardType targetCardType = CardType.Fire;
    public float damagePercentPerCard = 5f;
    public float buffDuration = 120f;
    public string buffId = "FireSynergyBuff";

    public FireSynergyBuffSO() { trigger = EffectTrigger.OnFire; }

    public override void Execute(EffectContext context)
    {
        var playerDataManager = ServiceLocator.Get<PlayerDataManager>();
        if (playerDataManager?.CurrentRunData == null) return;

        int cardCount = playerDataManager.CurrentRunData.equippedCards.Count(c => c.CardData.basicInfo.type == targetCardType);
        float totalBonus = cardCount * damagePercentPerCard;

        // 1. �������� StatusEffectDataSO ��ü�� �޸𸮿� �����մϴ�.
        var dynamicBuffData = ScriptableObject.CreateInstance<StatusEffectDataSO>();
        dynamicBuffData.effectId = this.buffId;
        dynamicBuffData.duration = this.buffDuration;
        dynamicBuffData.damageRatioBonus = totalBonus; // ���ݷ� ���ʽ� ����

        // 2. ������ �����͸� ���� StatusEffectManager�� �����Ͽ� �ϰ��� ������ �����մϴ�.
        var statusManager = ServiceLocator.Get<StatusEffectManager>();
        statusManager?.ApplyStatusEffect(context.Caster.gameObject, dynamicBuffData);

        Debug.Log($"<{GetType().Name}> {targetCardType} ī�� {cardCount}�� ����. {context.Caster.name}���� {buffDuration}�� ���� ���ݷ� +{totalBonus}% ���� ����.");
    }
}