using Linger.Extensions.IO;

namespace Linger.UnitTests.Extensions.IO;

public class PathExtensionsTests : IDisposable
{
    private readonly string _testDirectory;

    public PathExtensionsTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"PathExtensionsTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }
        catch
        {
            // 忽略清理错误
        }
    }
}
