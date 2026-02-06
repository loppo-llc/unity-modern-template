using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityDotNetSample.EditorConsoleLogProxy;

/// <summary>
/// Unity コンソールのログ出力をプロジェクトローカルファイルへ中継する Editor 拡張。
/// AI エージェントや外部ツールが <c>{ProjectRoot}/Logs/console.log</c> を
/// JSON Lines 形式で読み取れるようにする。
/// </summary>
/// <remarks>
/// Unity Logging パッケージ (com.unity.logging) の Sink 設計に着想を得ている。
/// <see cref="InitializeOnLoad"/> により Editor 起動時・ドメインリロード時に自動初期化。
/// </remarks>
[InitializeOnLoad]
public static class EditorConsoleLogProxy
{
    private static LogFileWriter? _writer;

    static EditorConsoleLogProxy()
    {
        Initialize();
    }

    private static void Initialize()
    {
        // Application.dataPath = "{project}/Assets" → 1階層上がってプロジェクトルートへ
        string logDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs"));
        string logPath = Path.Combine(logDir, "console.log");
        string prevLogPath = Path.Combine(logDir, "console.prev.log");

        _writer?.Dispose();

        // 前回セッションのログを console.prev.log として保持
        if (File.Exists(logPath))
        {
            File.Copy(logPath, prevLogPath, overwrite: true);
        }

        _writer = new LogFileWriter(logPath);

        // ドメインリロード時の二重登録を防ぐため先に解除
        Application.logMessageReceivedThreaded -= OnLogReceived;
        Application.logMessageReceivedThreaded += OnLogReceived;

        Debug.Log($"[EditorConsoleLogProxy] Logging to: {logPath}");
    }

    private static void OnLogReceived(string message, string stackTrace, LogType type)
    {
        string level = type switch
        {
            LogType.Error => "Error",
            LogType.Assert => "Assert",
            LogType.Warning => "Warning",
            LogType.Log => "Log",
            LogType.Exception => "Exception",
            _ => "Unknown"
        };

        // Error, Exception, Assert の場合のみスタックトレースを含める
        string? trace = type is LogType.Error or LogType.Exception or LogType.Assert
            ? stackTrace
            : null;

        var entry = new LogEntry(
            DateTime.UtcNow.ToString("o"),
            level,
            message,
            trace
        );

        _writer?.WriteLine(LogFormatter.Format(entry));
    }
}
