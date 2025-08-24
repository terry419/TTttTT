/// <summary>
/// 개별 스탯 보너스에 대한 정보를 담는 클래스입니다.
/// </summary>
public class StatModifier
{
    public readonly float Value; // 보너스 수치 (예: 0.1f는 10%)
    public readonly object Source; // 이 보너스를 제공한 객체 (CardDataSO, ArtifactDataSO 등)

    public StatModifier(float value, object source)
    {
        Value = value;
        Source = source;
    }
}