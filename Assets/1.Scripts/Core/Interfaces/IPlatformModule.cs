// ��õ ���: Assets/1.Scripts/Core/Interfaces/IPlatformModule.cs

/// <summary>
/// NewCardDataSO �÷����� �����Ǵ� ��� ��� ����� �����ؾ� �ϴ� �������̽��Դϴ�.
/// </summary>
public interface IPlatformModule
{
    /// <summary>
    /// ����� ���� ȿ�� ������ �����մϴ�.
    /// </summary>
    /// <param name="context">ȿ�� ���࿡ �ʿ��� ��� ������ ��� ������ ��ü</param>
    void Execute(EffectContext context);
}