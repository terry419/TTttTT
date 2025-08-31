// 경로: ./TTttTT/Assets/1/Scripts/Core/Interfaces/IStatHolder.cs

/// <summary>
/// 스탯 변경 효과(Modifier)를 적용받을 수 있는 모든 객체가 구현해야 하는 인터페이스입니다.
/// </summary>
public interface IStatHolder
{
    /// <summary> 대상의 특정 스탯에 새로운 Modifier를 추가합니다. </summary>
    void AddModifier(StatType type, StatModifier modifier);

    /// <summary> 특정 출처(Source)를 가진 모든 Modifier를 제거합니다. </summary>
    void RemoveModifiersFromSource(object source);
}