using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityDotNetSample.EditorConsoleLogProxy;
using Xunit;

namespace UnityDotNetSample.Tests.EditorConsoleLogProxy;

public class LogFileWriterTests : IDisposable
{
    private readonly string _tempDir;

    public LogFileWriterTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"LogFileWriterTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    [Fact]
    public void Write_CreatesFileAndWritesContent()
    {
        string filePath = Path.Combine(_tempDir, "test.log");

        using (var writer = new LogFileWriter(filePath))
        {
            writer.WriteLine("Hello");
            writer.WriteLine("World");
        }

        string[] lines = File.ReadAllLines(filePath);
        Assert.Equal(2, lines.Length);
        Assert.Equal("Hello", lines[0]);
        Assert.Equal("World", lines[1]);
    }

    [Fact]
    public void Write_CreatesParentDirectoryIfNeeded()
    {
        string filePath = Path.Combine(_tempDir, "subdir", "nested", "test.log");

        using (var writer = new LogFileWriter(filePath))
        {
            writer.WriteLine("test");
        }

        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public void Write_OverwritesExistingFile()
    {
        string filePath = Path.Combine(_tempDir, "test.log");
        File.WriteAllText(filePath, "old content\n");

        using (var writer = new LogFileWriter(filePath))
        {
            writer.WriteLine("new content");
        }

        string content = File.ReadAllText(filePath);
        Assert.DoesNotContain("old content", content);
        Assert.Contains("new content", content);
    }

    [Fact]
    public void Write_AllowsConcurrentRead()
    {
        string filePath = Path.Combine(_tempDir, "test.log");

        using var writer = new LogFileWriter(filePath);
        writer.WriteLine("test line");
        writer.Flush();

        // AI エージェントによる同時読み取りをシミュレート
        using var readStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(readStream);
        string? line = reader.ReadLine();

        Assert.Equal("test line", line);
    }

    [Fact]
    public async Task Write_ThreadSafe_NoConcurrencyIssues()
    {
        string filePath = Path.Combine(_tempDir, "test.log");
        int lineCount = 100;

        using (var writer = new LogFileWriter(filePath))
        {
            var tasks = new Task[lineCount];
            for (int i = 0; i < lineCount; i++)
            {
                int index = i;
                tasks[i] = Task.Run(() => writer.WriteLine($"Line {index}"));
            }
            await Task.WhenAll(tasks);
        }

        string[] lines = File.ReadAllLines(filePath);
        Assert.Equal(lineCount, lines.Length);
    }

    [Fact]
    public void Write_AfterDispose_DoesNotThrow()
    {
        string filePath = Path.Combine(_tempDir, "test.log");

        var writer = new LogFileWriter(filePath);
        writer.Dispose();

        // 例外を投げない — 黙って無視される
        writer.WriteLine("should be ignored");
    }
}
