// 파일명: Log.cs
using System.Diagnostics;

/// <summary>
/// 릴리즈 빌드에서는 호출 자체가 제외되는 조건부 로그 시스템입니다.
/// UnityEngine.Debug.Log 대신 Log.Print()를 사용하세요.
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
    /// 단위 테스트 등에서 로그 카운트를 초기화하기 위해 사용됩니다.
    /// </summary>
    public static void ResetCount()
    {
        LogCount = 0;
    }
}