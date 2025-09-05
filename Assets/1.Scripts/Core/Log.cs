// ���: ./TTttTT/Assets/1.Scripts/Core/Log.cs
using System.Diagnostics;
using System.Text;
using UnityEngine;

/// <summary>
/// [2�ܰ� ���׷��̵�]
/// �α� ������ ī�װ��� �ο��Ͽ� ����� ȿ���� �ش�ȭ�ϴ� Ŀ���� �α� �ý����Դϴ�.
/// UnityEngine.Debug.Log ��� Log.Info(), Log.Warn(), Log.Error() ���� ����ϼ���.
/// </summary>
public static class Log
{
    // enum�� '������ ���'�̶�� �����Ͻø� �˴ϴ�. �α��� ������ �̸� ���صδ� ���Դϴ�.
    public enum LogCategory
    {
        AI_Init,        // AI �ʱ�ȭ ����
        AI_Transition,  // AI ���� ��ȯ ����
        AI_Behavior,    // AI �ൿ ���� ����
        AI_Decision,    // AI ���� �Ǵ� ����
        GameManager,    // ���� �Ŵ��� ����
        UI,             // UI ����
        Data,           // ������ �ε�/���� ����
        PoolManager     // Ǯ�� �ý��� ����
    }

    public enum LogLevel
    {
        Info,    // �Ķ���: �Ϲ� ���� (�帧 �ľǿ�)
        Warning, // �����: ��� (������ �ƴ����� �ǵ�ġ ���� ������ �� ����)
        Error    // ������: ���� (�ݵ�� �����ؾ� �ϴ� ����)
    }

    private static StringBuilder _logBuilder = new StringBuilder();

    // [Conditional(...)]�� C#�� Ư�� �������,
    // ���� �ϼ��� ����(Release Build)������ �� �α� �Լ����� �ڵ����� �ڵ忡�� �����Ǿ� ���ɿ� ������ ���� �ʵ��� �մϴ�.
    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Info(LogCategory category, string message)
    {
        Print(LogLevel.Info, category, message);
    }

    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Warn(LogCategory category, string message)
    {
        Print(LogLevel.Warning, category, message);
    }

    // Error�� ������ ǥ�õǾ�� �ϹǷ� Conditional�� ������ �ʽ��ϴ�.
    public static void Error(LogCategory category, string message)
    {
        Print(LogLevel.Error, category, message);
    }

    private static void Print(LogLevel level, LogCategory category, string message)
    {
        _logBuilder.Clear();
        _logBuilder.Append($"[{Time.time:F2}s]"); // �ð�
        _logBuilder.Append($"[{category}]");      // ī�װ�
        _logBuilder.Append($" {message}");        // �޽���

        string formattedMessage = _logBuilder.ToString();

        switch (level)
        {
            case LogLevel.Info:
                UnityEngine.Debug.Log($"<color=cyan>{formattedMessage}</color>");
                break;
            case LogLevel.Warning:
                UnityEngine.Debug.LogWarning(formattedMessage);
                break;
            case LogLevel.Error:
                UnityEngine.Debug.LogError(formattedMessage);
                break;
        }
    }
}