namespace UnityDotNetSample.EditorConsoleLogProxy;

/// <summary>
/// コンソールログ1件を表す不変データ構造体。
/// 純粋な C# — Unity 依存なし。
/// </summary>
public readonly struct LogEntry
{
    /// <summary>ISO 8601 形式のタイムスタンプ (UTC)。</summary>
    public string Timestamp { get; }

    /// <summary>ログの重要度: "Log", "Warning", "Error", "Exception", "Assert" のいずれか。</summary>
    public string Level { get; }

    /// <summary>ログメッセージの内容。</summary>
    public string Message { get; }

    /// <summary>スタックトレース (Error, Exception, Assert の場合のみ。それ以外は null)。</summary>
    public string? StackTrace { get; }

    public LogEntry(string timestamp, string level, string message, string? stackTrace)
    {
        Timestamp = timestamp;
        Level = level;
        Message = message;
        StackTrace = stackTrace;
    }
}
