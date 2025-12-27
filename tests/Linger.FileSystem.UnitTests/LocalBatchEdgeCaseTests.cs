using Linger.FileSystem.Local;
using Xunit;

namespace Linger.FileSystem.Tests.Local
{
    public class LocalBatchEdgeCaseTests : IDisposable
    {
        private readonly string _root;
        private readonly LocalFileSystem _fs;

        public LocalBatchEdgeCaseTests()
        {
            _root = Path.Combine("TestTempDir", $"edge-{Guid.NewGuid():N}");
            Directory.CreateDirectory(_root);
            _fs = new LocalFileSystem(new LocalFileSystemOptions
            {
                RootDirectoryPath = _root,
                MaxDegreeOfParallelism = 4
            });
        }

        public void Dispose()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, true);
            }
        }

        #region Cancellation Tests

        [Fact]
        public async Task UploadFilesAsync_WithCancelledToken_ThrowsOperationCancelledException()
        {
            var sourceDir = Path.Combine(_root, "src");
            Directory.CreateDirectory(sourceDir);

            var files = new List<string>();
            for (var i = 0; i < 10; i++)
            {
                var path = Path.Combine(sourceDir, $"f{i}.txt");
                File.WriteAllText(path, "content");
                files.Add(path);
            }

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await _fs.UploadFilesAsync(files, "dst", overwrite: true, cts.Token));
        }

        [Fact]
        public async Task DownloadFilesAsync_WithCancelledToken_ThrowsOperationCancelledException()
        {
            for (var i = 0; i < 10; i++)
            {
                File.WriteAllText(Path.Combine(_root, $"r{i}.txt"), "content");
            }

            var remotePaths = Enumerable.Range(0, 10).Select(i => $"r{i}.txt").ToArray();

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await _fs.DownloadFilesAsync(remotePaths, Path.Combine(_root, "out"), overwrite: true, cts.Token));
        }

        [Fact]
        public async Task DeleteFilesAsync_WithCancelledToken_ThrowsOperationCancelledException()
        {
            for (var i = 0; i < 10; i++)
            {
                File.WriteAllText(Path.Combine(_root, $"d{i}.txt"), "content");
            }

            var paths = Enumerable.Range(0, 10).Select(i => $"d{i}.txt").ToArray();

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await _fs.DeleteFilesAsync(paths, cts.Token));
        }

        #endregion

        #region Overwrite=false Tests

        [Fact]
        public async Task UploadFilesAsync_OverwriteFalse_FailsWhenTargetExists()
        {
            var sourceDir = Path.Combine(_root, "src");
            var destDir = Path.Combine(_root, "dst");
            Directory.CreateDirectory(sourceDir);
            Directory.CreateDirectory(destDir);

            var sourcePath = Path.Combine(sourceDir, "exist.txt");
            File.WriteAllText(sourcePath, "new content");

            var existingPath = Path.Combine(destDir, "exist.txt");
            File.WriteAllText(existingPath, "old content");

            var result = await _fs.UploadFilesAsync(new[] { sourcePath }, "dst", overwrite: false);

            Assert.True(result.FailureCount == 1);
            Assert.True(result.SuccessCount == 0);
            Assert.Equal("old content", File.ReadAllText(existingPath));
        }

        [Fact]
        public async Task DownloadFilesAsync_OverwriteFalse_FailsWhenLocalExists()
        {
            var outDir = Path.Combine(_root, "out");
            Directory.CreateDirectory(outDir);

            File.WriteAllText(Path.Combine(_root, "remote.txt"), "remote content");
            File.WriteAllText(Path.Combine(outDir, "remote.txt"), "local content");

            var result = await _fs.DownloadFilesAsync(new[] { "remote.txt" }, outDir, overwrite: false);

            Assert.True(result.FailureCount == 1);
            Assert.True(result.SuccessCount == 0);
            Assert.Equal("local content", File.ReadAllText(Path.Combine(outDir, "remote.txt")));
        }

        #endregion

        #region Empty / Missing Input Tests

        [Fact]
        public async Task UploadFilesAsync_EmptyList_ReturnsEmptyResult()
        {
            var result = await _fs.UploadFilesAsync(Array.Empty<string>(), "dst", overwrite: true);

            Assert.True(result.SuccessCount == 0);
            Assert.True(result.FailureCount == 0);
        }

        [Fact]
        public async Task UploadFilesAsync_MissingSourceFile_ReportsFailure()
        {
            var result = await _fs.UploadFilesAsync(new[] { "/nonexistent/file.txt" }, "dst", overwrite: true);

            Assert.True(result.FailureCount == 1);
            Assert.True(result.SuccessCount == 0);
            Assert.Contains("本地文件不存在", result.FailedFiles[0].ErrorMessage);
        }

        [Fact]
        public async Task DownloadFilesAsync_MissingRemoteFile_ReportsFailure()
        {
            var outDir = Path.Combine(_root, "out");

            var result = await _fs.DownloadFilesAsync(new[] { "nonexistent.txt" }, outDir, overwrite: true);

            Assert.True(result.FailureCount == 1);
            Assert.True(result.SuccessCount == 0);
            Assert.Contains("源文件不存在", result.FailedFiles[0].ErrorMessage);
        }

        [Fact]
        public async Task ListFilesAsync_NonExistentDirectory_ReturnsEmptyList()
        {
            var files = await _fs.ListFilesAsync("nonexistent");

            Assert.Empty(files);
        }

        [Fact]
        public async Task ListDirectoriesAsync_NonExistentDirectory_ReturnsEmptyList()
        {
            var dirs = await _fs.ListDirectoriesAsync("nonexistent");

            Assert.Empty(dirs);
        }

        #endregion
    }
}
