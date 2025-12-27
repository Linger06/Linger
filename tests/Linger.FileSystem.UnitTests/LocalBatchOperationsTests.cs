using Linger.FileSystem;
using Linger.FileSystem.Local;
using Xunit;

namespace Linger.FileSystem.Tests.Local
{
    public class LocalBatchOperationsTests : IDisposable
    {
        private readonly string _root;
        private readonly string _sourceDir;
        private readonly string _targetDir;
        private readonly LocalFileSystem _fs;

        public LocalBatchOperationsTests()
        {
            _root = Path.Combine("TestTempDir", $"batch-{Guid.NewGuid():N}");
            _sourceDir = Path.Combine(_root, "src");
            _targetDir = Path.Combine(_root, "dst");
            Directory.CreateDirectory(_sourceDir);
            Directory.CreateDirectory(_targetDir);
            _fs = new LocalFileSystem(new LocalFileSystemOptions
            {
                RootDirectoryPath = _root,
                MaxDegreeOfParallelism = 2
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
        public async Task UploadFilesAsync_CopiesFilesToTargetDirectory()
        {
            var files = new[]
            {
                Path.Combine(_sourceDir, "a.txt"),
                Path.Combine(_sourceDir, "b.txt")
            };
            File.WriteAllText(files[0], "A");
            File.WriteAllText(files[1], "B");

            var result = await _fs.UploadFilesAsync(files, "dst", overwrite: true);

            Assert.True(result.SuccessCount == 2);
            Assert.True(File.Exists(Path.Combine(_targetDir, "a.txt")));
            Assert.True(File.Exists(Path.Combine(_targetDir, "b.txt")));
        }

        [Fact]
        public async Task DownloadFilesAsync_CopiesFilesFromRootToLocalDirectory()
        {
            var rootFile1 = Path.Combine(_root, "x.txt");
            var rootFile2 = Path.Combine(_root, "y.txt");
            File.WriteAllText(rootFile1, "X");
            File.WriteAllText(rootFile2, "Y");

            var result = await _fs.DownloadFilesAsync(new[] { "x.txt", "y.txt" }, Path.Combine(_root, "out"), overwrite: true);

            Assert.True(result.SuccessCount == 2);
            Assert.True(File.Exists(Path.Combine(_root, "out", "x.txt")));
            Assert.True(File.Exists(Path.Combine(_root, "out", "y.txt")));
        }

        [Fact]
        public async Task DeleteFilesAsync_DeletesExistingFiles_SucceedsForMissing()
        {
            var f1 = Path.Combine(_root, "d1.txt");
            var f2 = Path.Combine(_root, "d2.txt");
            File.WriteAllText(f1, "d1");
            // f2 intentionally missing

            var result = await _fs.DeleteFilesAsync(new[] { "d1.txt", "d2.txt" });

            Assert.True(result.SuccessCount == 2);
            Assert.False(File.Exists(f1));
        }

        [Fact]
        public async Task ListFilesAndDirectories_ReturnsNames()
        {
            Directory.CreateDirectory(Path.Combine(_root, "listDir"));
            File.WriteAllText(Path.Combine(_root, "listDir", "f1.txt"), "1");
            File.WriteAllText(Path.Combine(_root, "listDir", "f2.log"), "2");
            Directory.CreateDirectory(Path.Combine(_root, "listDir", "sub1"));
            Directory.CreateDirectory(Path.Combine(_root, "listDir", "sub2"));

            var files = await _fs.ListFilesAsync("listDir");
            var dirs = await _fs.ListDirectoriesAsync("listDir");

            Assert.Contains("f1.txt", files);
            Assert.Contains("f2.log", files);
            Assert.Contains("sub1", dirs);
            Assert.Contains("sub2", dirs);
        }
    }
}
