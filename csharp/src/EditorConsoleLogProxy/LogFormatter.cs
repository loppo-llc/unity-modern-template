using System;
using System.Text;
using System.Threading;

namespace UnityDotNetSample.EditorConsoleLogProxy;

/// <summary>
/// <see cref="LogEntry"/> を JSON Lines 形式（1行1JSONオブジェクト）にフォーマットする。
/// 純粋な C# — Unity 依存なし、外部 JSON ライブラリ不要。
/// </summary>
public static class LogFormatter
{
    /// <summary>JSON エスケープが必要な文字のセット。</summary>
    private static ReadOnlySpan<char> JsonEscapeChars => ['"', '\\', '\n', '\r', '\t'];

    /// <summary>JSON スケルトンのオーバーヘッド: {"t":"","l":"","m":"","s":""}</summary>
    private const int JsonSkeletonSize = 32;

    /// <summary>キャッシュ SB の初期容量。</summary>
    private const int DefaultCapacity = 512;

    /// <summary>これを超えたら SB を縮小してメモリ肥大化を防ぐ。</summary>
    private const int MaxRetainedCapacity = 8192;

    [ThreadStatic]
    private static StringBuilder? t_cachedSb;

    /// <summary>
    /// ログエントリを1行の JSON 文字列にフォーマットする。
    /// </summary>
    /// <remarks>
    /// 出力形式: {"t":"...","l":"...","m":"...","s":"..."}
    /// "s" (stackTrace) フィールドは null の場合省略される。
    /// </remarks>
    public static string Format(LogEntry entry)
    {
        var sb = AcquireStringBuilder();

        int estimatedLength = JsonSkeletonSize
            + entry.Timestamp.Length
            + entry.Level.Length
            + entry.Message.Length
            + (entry.StackTrace?.Length ?? 0);
        sb.EnsureCapacity(estimatedLength);

        sb.Append("{\"t\":\"");
        EscapeJsonTo(entry.Timestamp, sb);
        sb.Append("\",\"l\":\"");
        EscapeJsonTo(entry.Level, sb);
        sb.Append("\",\"m\":\"");
        EscapeJsonTo(entry.Message, sb);
        sb.Append('"');

        if (entry.StackTrace is not null)
        {
            sb.Append(",\"s\":\"");
            EscapeJsonTo(entry.StackTrace, sb);
            sb.Append('"');
        }

        sb.Append('}');
        return sb.ToString();
    }

    /// <summary>
    /// 文字列を JSON 値として安全にエスケープし、<paramref name="sb"/> に直接追記する。
    /// エスケープ不要な文字列（タイムスタンプ・レベル名など）はゼロアロケーション。
    /// </summary>
    internal static void EscapeJsonTo(string value, StringBuilder sb)
    {
        ReadOnlySpan<char> span = value.AsSpan();

        // 高速パス: エスケープ対象文字なし → そのまま追記して終了
        if (span.IndexOfAny(JsonEscapeChars) < 0 && !ContainsControlChars(span))
        {
            sb.Append(value);
            return;
        }

        // 低速パス: 1文字ずつエスケープ
        foreach (char c in value)
        {
            switch (c)
            {
                case '"':
                    sb.Append("\\\"");
                    break;
                case '\\':
                    sb.Append("\\\\");
                    break;
                case '\n':
                    sb.Append("\\n");
                    break;
                case '\r':
                    sb.Append("\\r");
                    break;
                case '\t':
                    sb.Append("\\t");
                    break;
                default:
                    if (c < 0x20)
                    {
                        AppendUnicodeEscape(sb, c);
                    }
                    else
                    {
                        sb.Append(c);
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// 後方互換用ラッパー。新しいコードは <see cref="EscapeJsonTo"/> を使うこと。
    /// </summary>
    internal static string EscapeJson(string value)
    {
        var sb = new StringBuilder(value.Length);
        EscapeJsonTo(value, sb);
        return sb.ToString();
    }

    /// <summary>\uXXXX 形式のユニコードエスケープを SB に直接書き込む。</summary>
    private static void AppendUnicodeEscape(StringBuilder sb, char c)
    {
        sb.Append("\\u");
        int val = c;
        sb.Append(HexDigit((val >> 12) & 0xF));
        sb.Append(HexDigit((val >> 8) & 0xF));
        sb.Append(HexDigit((val >> 4) & 0xF));
        sb.Append(HexDigit(val & 0xF));
    }

    private static char HexDigit(int value) =>
        (char)(value < 10 ? '0' + value : 'a' + value - 10);

    /// <summary>
    /// \n, \r, \t 以外の制御文字 (0x00-0x1F) が含まれているか判定する。
    /// <see cref="JsonEscapeChars"/> の <c>IndexOfAny</c> で捕捉できない範囲のチェック。
    /// </summary>
    private static bool ContainsControlChars(ReadOnlySpan<char> span)
    {
        foreach (char c in span)
        {
            // \n (0x0A), \r (0x0D), \t (0x09) は IndexOfAny 側で検出済みなので除外
            if (c < 0x20 && c != '\n' && c != '\r' && c != '\t')
            {
                return true;
            }
        }
        return false;
    }

    private static StringBuilder AcquireStringBuilder()
    {
        var sb = t_cachedSb;
        if (sb is null)
        {
            sb = new StringBuilder(DefaultCapacity);
            t_cachedSb = sb;
            return sb;
        }

        sb.Clear();
        if (sb.Capacity > MaxRetainedCapacity)
        {
            sb.Capacity = DefaultCapacity;
        }
        return sb;
    }
}
