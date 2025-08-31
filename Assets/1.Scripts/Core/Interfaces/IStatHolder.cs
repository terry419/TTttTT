// ���: ./TTttTT/Assets/1/Scripts/Core/Interfaces/IStatHolder.cs

/// <summary>
/// ���� ���� ȿ��(Modifier)�� ������� �� �ִ� ��� ��ü�� �����ؾ� �ϴ� �������̽��Դϴ�.
/// </summary>
public interface IStatHolder
{
    /// <summary> ����� Ư�� ���ȿ� ���ο� Modifier�� �߰��մϴ�. </summary>
    void AddModifier(StatType type, StatModifier modifier);

    /// <summary> Ư�� ��ó(Source)�� ���� ��� Modifier�� �����մϴ�. </summary>
    void RemoveModifiersFromSource(object source);
}