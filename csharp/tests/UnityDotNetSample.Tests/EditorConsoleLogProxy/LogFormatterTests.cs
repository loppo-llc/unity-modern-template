using UnityDotNetSample.EditorConsoleLogProxy;
using Xunit;

namespace UnityDotNetSample.Tests.EditorConsoleLogProxy;

public class LogFormatterTests
{
    [Fact]
    public void Format_BasicEntry_ReturnsValidJsonWithAllFields()
    {
        var entry = new LogEntry("2025-02-06T23:45:12.345Z", "Log", "Hello World", null);

        string result = LogFormatter.Format(entry);

        Assert.Equal(
            "{\"t\":\"2025-02-06T23:45:12.345Z\",\"l\":\"Log\",\"m\":\"Hello World\"}",
            result);
    }

    [Fact]
    public void Format_WithStackTrace_IncludesStackField()
    {
        var entry = new LogEntry(
            "2025-02-06T23:45:12.345Z",
            "Error",
            "NullReferenceException",
            "at Foo.Bar() in Foo.cs:42");

        string result = LogFormatter.Format(entry);

        Assert.Contains("\"s\":\"at Foo.Bar() in Foo.cs:42\"", result);
    }

    [Fact]
    public void Format_WithoutStackTrace_OmitsStackField()
    {
        var entry = new LogEntry("2025-02-06T23:45:12.345Z", "Log", "Normal log", null);

        string result = LogFormatter.Format(entry);

        Assert.DoesNotContain("\"s\":", result);
    }

    [Fact]
    public void Format_MessageWithQuotes_EscapesCorrectly()
    {
        var entry = new LogEntry("2025-02-06T23:45:12.345Z", "Log", "He said \"hello\"", null);

        string result = LogFormatter.Format(entry);

        Assert.Contains("He said \\\"hello\\\"", result);
    }

    [Fact]
    public void Format_MessageWithNewlines_EscapesCorrectly()
    {
        var entry = new LogEntry("2025-02-06T23:45:12.345Z", "Warning", "Line1\nLine2\r\nLine3", null);

        string result = LogFormatter.Format(entry);

        Assert.Contains("Line1\\nLine2\\r\\nLine3", result);
    }

    [Fact]
    public void Format_MessageWithBackslash_EscapesCorrectly()
    {
        var entry = new LogEntry("2025-02-06T23:45:12.345Z", "Log", @"C:\Users\path", null);

        string result = LogFormatter.Format(entry);

        Assert.Contains("C:\\\\Users\\\\path", result);
    }

    [Fact]
    public void Format_JapaneseMessage_HandledCorrectly()
    {
        var entry = new LogEntry("2025-02-06T23:45:12.345Z", "Log", "初期化完了", null);

        string result = LogFormatter.Format(entry);

        Assert.Contains("初期化完了", result);
    }

    [Fact]
    public void Format_ControlCharacters_EscapedAsUnicode()
    {
        var entry = new LogEntry("2025-02-06T23:45:12.345Z", "Log", "tab\there", null);

        string result = LogFormatter.Format(entry);

        Assert.Contains("tab\\there", result);
    }

    [Theory]
    [InlineData("Log")]
    [InlineData("Warning")]
    [InlineData("Error")]
    [InlineData("Exception")]
    [InlineData("Assert")]
    public void Format_AllLogLevels_IncludedInOutput(string level)
    {
        var entry = new LogEntry("2025-02-06T23:45:12.345Z", level, "test", null);

        string result = LogFormatter.Format(entry);

        Assert.Contains($"\"l\":\"{level}\"", result);
    }
}
