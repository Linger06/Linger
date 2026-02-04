using Linger.FileSystem.Local;
using Xunit;

namespace Linger.FileSystem.Tests.Local
{
    public class LocalBatchConcurrencyTests : IDisposable
    {
        private readonly string _root;
        private readonly string _sourceDir;
        private readonly string _targetDir;

        public LocalBatchConcurrencyTests()
        {
            _root = Path.Combine("TestTempDir", $"batch-concurrency-{Guid.NewGuid():N}");
            _sourceDir = Path.Combine(_root, "src");
            _targetDir = Path.Combine(_root, "dst");

            Directory.CreateDirectory(_sourceDir);
            Directory.CreateDirectory(_targetDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, true);
            }
        }

        [Fact]
        public async Task UploadFilesAsync_WithHighConcurrency_ReportsStableResults()
        {
            const int fileCount = 200;

            var files = new List<string>(fileCount);
            for (var i = 0; i < fileCount; i++)
            {
                var path = Path.Combine(_sourceDir, $"f{i:D4}.txt");
                File.WriteAllText(path, new string('x', 64));
                files.Add(path);
            }

            var fs = new LocalFileSystem(new LocalFileSystemOptions
            {
                RootDirectoryPath = _root,
                MaxDegreeOfParallelism = 8
            });

            var result = await fs.UploadFilesAsync(files, "dst", overwrite: true);

            Assert.True(result.SuccessCount == fileCount);
            Assert.True(result.FailureCount == 0);
            Assert.True(result.SucceededFiles.Count == fileCount);
            Assert.True(result.FailedFiles.Count == 0);

            var destFiles = Directory.GetFiles(_targetDir, "*.txt", SearchOption.TopDirectoryOnly);
            Assert.True(destFiles.Length == fileCount);
        }

        [Fact]
        public async Task DownloadFilesAsync_WithHighConcurrency_ReportsStableResults()
        {
            const int fileCount = 200;

            for (var i = 0; i < fileCount; i++)
            {
                File.WriteAllText(Path.Combine(_root, $"r{i:D4}.txt"), new string('y', 64));
            }

            var fs = new LocalFileSystem(new LocalFileSystemOptions
            {
                RootDirectoryPath = _root,
                MaxDegreeOfParallelism = 8
            });

            var outDir = Path.Combine(_root, "out");

            var remotePaths = Enumerable.Range(0, fileCount)
                .Select(i => $"r{i:D4}.txt")
                .ToArray();

            var result = await fs.DownloadFilesAsync(remotePaths, outDir, overwrite: true);

            Assert.True(result.SuccessCount == fileCount);
            Assert.True(result.FailureCount == 0);

            var outFiles = Directory.GetFiles(outDir, "*.txt", SearchOption.TopDirectoryOnly);
            Assert.True(outFiles.Length == fileCount);
        }

        [Fact]
        public async Task DeleteFilesAsync_WithHighConcurrency_ReportsStableResults()
        {
            const int existingCount = 150;
            const int missingCount = 50;
            const int total = existingCount + missingCount;

            for (var i = 0; i < existingCount; i++)
            {
                File.WriteAllText(Path.Combine(_root, $"d{i:D4}.txt"), "delete-me");
            }

            var fs = new LocalFileSystem(new LocalFileSystemOptions
            {
                RootDirectoryPath = _root,
                MaxDegreeOfParallelism = 8
            });

            var paths = Enumerable.Range(0, total)
                .Select(i => $"d{i:D4}.txt")
                .ToArray();

            var result = await fs.DeleteFilesAsync(paths);

            Assert.True(result.SuccessCount == total);
            Assert.True(result.FailureCount == 0);

            for (var i = 0; i < existingCount; i++)
            {
                Assert.False(File.Exists(Path.Combine(_root, $"d{i:D4}.txt")));
            }
        }
    }
}
