// 경로: ./TTttTT/Assets/1.Scripts/Core/Log.cs
using System.Diagnostics;
using System.Text;
using UnityEngine;

/// <summary>
/// [2단계 업그레이드]
/// 로그 레벨과 카테고리를 부여하여 디버깅 효율을 극대화하는 커스텀 로그 시스템입니다.
/// UnityEngine.Debug.Log 대신 Log.Info(), Log.Warn(), Log.Error() 등을 사용하세요.
/// </summary>
public static class Log
{
    // enum은 '선택지 목록'이라고 생각하시면 됩니다. 로그의 종류를 미리 정해두는 것입니다.
    public enum LogCategory
    {
        AI_Init,        // AI 초기화 관련
        AI_Transition,  // AI 상태 전환 관련
        AI_Behavior,    // AI 행동 실행 관련
        AI_Decision,    // AI 결정 판단 관련
        GameManager,    // 게임 매니저 관련
        UI,             // UI 관련
        Data,           // 데이터 로딩/저장 관련
        PoolManager     // 풀링 시스템 관련
    }

    public enum LogLevel
    {
        Info,    // 파란색: 일반 정보 (흐름 파악용)
        Warning, // 노란색: 경고 (오류는 아니지만 의도치 않은 동작일 수 있음)
        Error    // 빨간색: 오류 (반드시 수정해야 하는 문제)
    }

    private static StringBuilder _logBuilder = new StringBuilder();

    // [Conditional(...)]는 C#의 특별 기능으로,
    // 최종 완성본 게임(Release Build)에서는 이 로그 함수들이 자동으로 코드에서 삭제되어 성능에 영향을 주지 않도록 합니다.
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

    // Error는 언제나 표시되어야 하므로 Conditional을 붙이지 않습니다.
    public static void Error(LogCategory category, string message)
    {
        Print(LogLevel.Error, category, message);
    }

    private static void Print(LogLevel level, LogCategory category, string message)
    {
        _logBuilder.Clear();
        _logBuilder.Append($"[{Time.time:F2}s]"); // 시간
        _logBuilder.Append($"[{category}]");      // 카테고리
        _logBuilder.Append($" {message}");        // 메시지

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