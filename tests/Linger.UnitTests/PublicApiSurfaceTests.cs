namespace Linger.UnitTests;

public class PublicApiSurfaceTests
{
    [Fact]
    public void LingerAssembly_ShouldNotExposeTypesInSystemNamespaces()
    {
        var exposedTypes = typeof(StringExtensions).Assembly
            .GetExportedTypes()
            .Where(type => type.Namespace is "System" || type.Namespace?.StartsWith("System.", StringComparison.Ordinal) == true)
            .Select(type => type.FullName ?? type.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(exposedTypes);
    }
}
