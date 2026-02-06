using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;

namespace UnityDotNetSample.EditorConsoleLogProxy;

/// <summary>
/// ログ出力用のスレッドセーフなファイルライター。
/// <see cref="FileShare.Read"/> でファイルを開くため、
/// 外部ツール (AI エージェント) がログを同時読み取り可能。
/// 純粋な C# — Unity 依存なし。
/// </summary>
/// <remarks>
/// <see cref="WriteLine"/> はロックフリーのキューへのエンキューのみ行い、
/// タイマーまたはしきい値到達時にバッチフラッシュする。
/// </remarks>
public sealed class LogFileWriter : IDisposable
{
    private readonly StreamWriter _writer;
    private readonly ConcurrentQueue<string> _queue = new();
    private readonly Timer _flushTimer;
    private readonly object _flushLock = new();
    private volatile bool _disposed;
    private int _pendingCount;

    /// <summary>タイマーフラッシュ間隔 (ms)。</summary>
    private const int FlushIntervalMs = 500;

    /// <summary>この行数に達したら即座にフラッシュ。</summary>
    private const int FlushThreshold = 32;

    /// <summary>
    /// 新しいログファイルライターを生成する。ファイルは即座に作成（または上書き）される。
    /// 親ディレクトリが存在しない場合は自動的に作成される。
    /// </summary>
    public LogFileWriter(string filePath)
    {
        string? dir = Path.GetDirectoryName(filePath);
        if (dir is not null)
        {
            Directory.CreateDirectory(dir);
        }

        var stream = new FileStream(
            filePath, FileMode.Create, FileAccess.Write, FileShare.Read,
            bufferSize: 8192);
        _writer = new StreamWriter(stream, Encoding.UTF8, bufferSize: 8192);

        _flushTimer = new Timer(_ => FlushPending(), null, FlushIntervalMs, FlushIntervalMs);
    }

    /// <summary>
    /// ログファイルに1行書き込む。スレッドセーフ・ロックフリー。
    /// 実際のディスク書き込みはバッチフラッシュ時に行われる。
    /// </summary>
    public void WriteLine(string line)
    {
        if (_disposed) return;

        _queue.Enqueue(line);

        if (Interlocked.Increment(ref _pendingCount) >= FlushThreshold)
        {
            FlushPending();
        }
    }

    /// <summary>
    /// キューに蓄積されたログ行をすべてディスクに書き出す。
    /// </summary>
    public void Flush()
    {
        FlushPending();
    }

    private void FlushPending()
    {
        if (_disposed) return;

        if (!Monitor.TryEnter(_flushLock)) return;
        try
        {
            Interlocked.Exchange(ref _pendingCount, 0);
            while (_queue.TryDequeue(out string? line))
            {
                _writer.WriteLine(line);
            }
            _writer.Flush();
        }
        finally
        {
            Monitor.Exit(_flushLock);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _flushTimer.Dispose();

        // 最終フラッシュ — データ欠損を防ぐ
        lock (_flushLock)
        {
            while (_queue.TryDequeue(out string? line))
            {
                _writer.WriteLine(line);
            }
            _writer.Flush();
        }

        _writer.Dispose();
    }
}
