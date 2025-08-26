// 추천 경로: Assets/1.Scripts/Core/Interfaces/IPlatformModule.cs

/// <summary>
/// NewCardDataSO 플랫폼에 장착되는 모든 기능 모듈이 구현해야 하는 인터페이스입니다.
/// </summary>
public interface IPlatformModule
{
    /// <summary>
    /// 모듈의 실제 효과 로직을 실행합니다.
    /// </summary>
    /// <param name="context">효과 실행에 필요한 모든 정보가 담긴 데이터 객체</param>
    void Execute(EffectContext context);
}