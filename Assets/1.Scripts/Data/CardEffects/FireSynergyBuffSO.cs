using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "Module_FireSynergyBuff_", menuName = "GameData/CardData/Modules/FireSynergyBuff")]
public class FireSynergyBuffSO : CardEffectSO
{
    [Header("시너지 설정")]
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

        // 1. 동적으로 StatusEffectDataSO 객체를 메모리에 생성합니다.
        var dynamicBuffData = ScriptableObject.CreateInstance<StatusEffectDataSO>();
        dynamicBuffData.effectId = this.buffId;
        dynamicBuffData.duration = this.buffDuration;
        dynamicBuffData.damageRatioBonus = totalBonus; // 공격력 보너스 설정

        // 2. 생성된 데이터를 기존 StatusEffectManager에 전달하여 일관된 패턴을 유지합니다.
        var statusManager = ServiceLocator.Get<StatusEffectManager>();
        statusManager?.ApplyStatusEffect(context.Caster.gameObject, dynamicBuffData);

        Debug.Log($"<{GetType().Name}> {targetCardType} 카드 {cardCount}개 감지. {context.Caster.name}에게 {buffDuration}초 동안 공격력 +{totalBonus}% 버프 적용.");
    }
}