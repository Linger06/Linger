using Linger.FileSystem.Local;
using Xunit;

namespace Linger.FileSystem.Tests.Local;

/// <summary>
/// Synchronous progress reporter for testing (avoids async callback timing issues).
/// </summary>
internal sealed class SynchronousProgress<T> : IProgress<T>
{
    private readonly Action<T> _handler;

    public SynchronousProgress(Action<T> handler) => _handler = handler;

    public void Report(T value) => _handler(value);
}

/// <summary>
/// Tests for batch operation progress reporting.
/// </summary>
public class LocalBatchProgressTests : IDisposable
{
    private readonly string _root;
    private readonly string _sourceDir;
    private readonly LocalFileSystem _fs;

    public LocalBatchProgressTests()
    {
        _root = Path.Combine("TestTempDir", $"batch-progress-{Guid.NewGuid():N}");
        _sourceDir = Path.Combine(_root, "src");
        Directory.CreateDirectory(_sourceDir);
        Directory.CreateDirectory(_root);
        _fs = new LocalFileSystem(new LocalFileSystemOptions
        {
            RootDirectoryPath = _root,
            MaxDegreeOfParallelism = 1 // Serial for predictable order
        });
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }
    }

    [Fact]
    public async Task UploadFilesAsync_ReportsProgressAfterEachFile()
    {
        // Arrange
        var files = new[]
        {
            Path.Combine(_sourceDir, "a.txt"),
            Path.Combine(_sourceDir, "b.txt"),
            Path.Combine(_sourceDir, "c.txt")
        };
        foreach (var file in files)
        {
            File.WriteAllText(file, "content");
        }

        var progressReports = new List<BatchProgress>();
        var progress = new SynchronousProgress<BatchProgress>(p => progressReports.Add(p));

        // Act
        await _fs.UploadFilesAsync(files, "dst", overwrite: true, progress);

        // Assert: should have 4 reports (3 files + 1 final)
        Assert.True(progressReports.Count >= 3, $"Expected at least 3 progress reports, got {progressReports.Count}");

        // Verify completed counts are accurate (1, 2, 3, ...)
        for (var i = 0; i < progressReports.Count - 1; i++)
        {
            Assert.True(progressReports[i].Completed >= i + 1,
                $"Report {i}: Expected Completed >= {i + 1}, got {progressReports[i].Completed}");
        }

        // Verify final report
        var final = progressReports[progressReports.Count - 1];
        Assert.Equal(3, final.Total);
        Assert.Equal(3, final.Completed);
    }

    [Fact]
    public async Task UploadFilesAsync_Parallel_ReportsProgressAfterCompletion()
    {
        // Arrange: Use parallel mode
        var parallelFs = new LocalFileSystem(new LocalFileSystemOptions
        {
            RootDirectoryPath = _root,
            MaxDegreeOfParallelism = 4
        });

        var files = new List<string>();
        for (var i = 0; i < 10; i++)
        {
            var path = Path.Combine(_sourceDir, $"p{i}.txt");
            File.WriteAllText(path, $"content-{i}");
            files.Add(path);
        }

        var progressReports = new List<BatchProgress>();
        // Use SynchronousProgress to avoid async callback issues in tests
        var progress = new SynchronousProgress<BatchProgress>(p =>
        {
            lock (progressReports)
            {
                progressReports.Add(p);
            }
        });

        // Act
        await parallelFs.UploadFilesAsync(files, "dst", overwrite: true, progress);

        // Assert: Each report should have Completed > 0 (reported after completion)
        Assert.True(progressReports.Count >= 10, $"Expected at least 10 progress reports, got {progressReports.Count}");

        foreach (var report in progressReports.Where(r => !string.IsNullOrEmpty(r.CurrentFile)))
        {
            Assert.True(report.Completed > 0,
                $"Progress for {report.CurrentFile} should have Completed > 0, got {report.Completed}");
        }

        // Verify final report
        var final = progressReports[progressReports.Count - 1];
        Assert.Equal(10, final.Total);
        Assert.Equal(10, final.Completed);
    }

    [Fact]
    public async Task DownloadFilesAsync_ReportsProgressAfterEachFile()
    {
        // Arrange
        for (var i = 0; i < 3; i++)
        {
            File.WriteAllText(Path.Combine(_root, $"d{i}.txt"), $"download-{i}");
        }

        var progressReports = new List<BatchProgress>();
        var progress = new SynchronousProgress<BatchProgress>(p => progressReports.Add(p));

        // Act
        var outDir = Path.Combine(_root, "downloaded");
        await _fs.DownloadFilesAsync(new[] { "d0.txt", "d1.txt", "d2.txt" }, outDir, overwrite: true, progress);

        // Assert
        Assert.True(progressReports.Count >= 3, $"Expected at least 3 progress reports, got {progressReports.Count}");

        var final = progressReports[progressReports.Count - 1];
        Assert.Equal(3, final.Total);
        Assert.Equal(3, final.Completed);
    }

    [Fact]
    public async Task DeleteFilesAsync_ReportsProgressAfterEachFile()
    {
        // Arrange
        for (var i = 0; i < 3; i++)
        {
            File.WriteAllText(Path.Combine(_root, $"del{i}.txt"), $"delete-{i}");
        }

        var progressReports = new List<BatchProgress>();
        var progress = new SynchronousProgress<BatchProgress>(p => progressReports.Add(p));

        // Act
        await _fs.DeleteFilesAsync(new[] { "del0.txt", "del1.txt", "del2.txt" }, progress);

        // Assert
        Assert.True(progressReports.Count >= 3, $"Expected at least 3 progress reports, got {progressReports.Count}");

        var final = progressReports[progressReports.Count - 1];
        Assert.Equal(3, final.Total);
        Assert.Equal(3, final.Completed);
    }

    [Fact]
    public async Task UploadFilesAsync_WithMissingFile_ReportsProgressIncludingFailure()
    {
        // Arrange
        var existingFile = Path.Combine(_sourceDir, "exists.txt");
        File.WriteAllText(existingFile, "content");
        var missingFile = Path.Combine(_sourceDir, "missing.txt"); // Does not exist

        var progressReports = new List<BatchProgress>();
        var progress = new SynchronousProgress<BatchProgress>(p => progressReports.Add(p));

        // Act
        await _fs.UploadFilesAsync(new[] { existingFile, missingFile }, "dst", overwrite: true, progress);

        // Assert: Both files should be reported (1 success, 1 failure)
        Assert.True(progressReports.Count >= 2, $"Expected at least 2 progress reports, got {progressReports.Count}");

        var final = progressReports[progressReports.Count - 1];
        Assert.Equal(2, final.Total);
        Assert.Equal(2, final.Completed);
        Assert.Equal(1, final.Succeeded);
        Assert.Equal(1, final.Failed);
    }

    [Fact]
    public void BatchProgress_PercentComplete_CalculatesCorrectly()
    {
        var progress = new BatchProgress(5, 10, "test.txt", 4, 1);

        Assert.Equal(50.0, progress.PercentComplete);
        Assert.Equal(5, progress.Completed);
        Assert.Equal(10, progress.Total);
        Assert.Equal("test.txt", progress.CurrentFile);
        Assert.Equal(4, progress.Succeeded);
        Assert.Equal(1, progress.Failed);
    }

    [Fact]
    public void BatchProgress_PercentComplete_HandlesZeroTotal()
    {
        var progress = new BatchProgress(0, 0, string.Empty, 0, 0);

        Assert.Equal(0.0, progress.PercentComplete);
    }
}
