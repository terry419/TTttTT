// ���ϸ�: Log.cs
using System.Diagnostics;

/// <summary>
/// ������ ���忡���� ȣ�� ��ü�� ���ܵǴ� ���Ǻ� �α� �ý����Դϴ�.
/// UnityEngine.Debug.Log ��� Log.Print()�� ����ϼ���.
/// </summary>
public static class Log
{
    public static int LogCount { get; private set; }

    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Print(string message)
    {
        UnityEngine.Debug.Log(message);
        LogCount++;
    }

    /// <summary>
    /// ���� �׽�Ʈ ��� �α� ī��Ʈ�� �ʱ�ȭ�ϱ� ���� ���˴ϴ�.
    /// </summary>
    public static void ResetCount()
    {
        LogCount = 0;
    }
}