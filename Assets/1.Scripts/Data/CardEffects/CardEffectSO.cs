using UnityEngine;

/// <summary>
/// 모든 카드 효과 '옵션'의 기반이 되는 추상 ScriptableObject입니다.
/// 각 효과는 이 클래스를 상속받아 자신만의 Execute 로직을 구현해야 합니다.
/// 자식 클래스의 Execute 메소드 최상단에는 자신의 실행을 알리는 디버그 로그를 반드시 추가해야 합니다.
/// 예: Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' 실행.");
/// </summary>
public abstract class CardEffectSO : ScriptableObject
{
    /// <summary>
    /// 이 효과가 언제 발동될지를 정의합니다.
    /// </summary>
    public enum EffectTrigger
    {
        OnFire,           // 카드가 발사되는 즉시
        OnHit,            // 투사체가 무언가에 명중했을 때
        OnCrit,           // 치명타로 명중했을 때
        OnKill,           // 적을 처치했을 때
        OnLastRicochetHit // 마지막 리코셰(튕김)가 명중했을 때
    }

    [Header("효과 발동 조건")]
    [Tooltip("이 효과가 어떤 시점에 발동될지를 선택합니다.")]
    public EffectTrigger trigger;

    /// <summary>
    /// 이 효과의 실제 로직을 실행하는 메소드입니다.
    /// 자식 클래스에서 이 메소드를 반드시 재정의(override)해야 합니다.
    /// </summary>
    /// <param name="context">효과 실행에 필요한 모든 정보가 담긴 컨텍스트 객체</param>
    public abstract void Execute(EffectContext context);
}