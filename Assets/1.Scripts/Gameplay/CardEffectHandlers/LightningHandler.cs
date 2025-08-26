using UnityEngine;

public class LightningHandler : ICardEffectHandler
{
    public void Execute(CardDataSO cardData, EffectExecutor executor, CharacterStats casterStats, Transform spawnPoint)
    {
        Debug.Log($"[LightningHandler] Lightning effect executed for card: {cardData.cardName}");
        // 여기에 번개 효과 로직을 구현합니다.
        // 예: 특정 위치에 번개 이펙트 생성, 주변 적에게 데미지 적용 등
    }
}