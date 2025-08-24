using UnityEngine;

/// <summary>
/// 모든 카드 효과 처리기(Handler)가 구현해야 하는 인터페이스입니다.
/// 전략 패턴(Strategy Pattern)을 사용하여 각 카드 효과를 별도 클래스로 캡슐화합니다.
/// </summary>
public interface ICardEffectHandler
{
    /// <summary>
    /// 카드 효과를 실행합니다.
    /// </summary>
    /// <param name="cardData">실행할 효과가 담긴 카드 데이터입니다.</param>
    /// <param name="executor">다른 시스템에 접근할 때 사용하는 EffectExecutor의 인스턴스입니다.</param>
    /// <param name="casterStats">효과를 시전하는 주체의 CharacterStats입니다.</param>
    /// <param name="spawnPoint">효과가 생성될 위치입니다. (예: 총구 위치, 플레이어의 발사 지점)</param>
    void Execute(CardDataSO cardData, EffectExecutor executor, CharacterStats casterStats, Transform spawnPoint);
}