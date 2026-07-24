namespace Linger.Extensions.Tests;

public partial class ArrayExtensionsTests
{
    [Fact]
    public void ToBase64String_ConvertsCorrectly()
    {
        byte[] value = { 1, 2, 3 };
        var result = value.ToBase64String();
        Assert.Equal("AQID", result);
    }

    [Fact]
    public void ToImageDataUri_WithPngMediaType_UsesSpecifiedMediaType()
    {
        byte[] value = { 1, 2, 3 };

        var result = value.ToImageDataUri("image/png");

        Assert.Equal("data:image/png;base64,AQID", result);
    }

    [Fact]
    public void ToImageDataUri_WithNonImageMediaType_ThrowsArgumentException()
    {
        byte[] value = { 1, 2, 3 };

        Assert.Throws<ArgumentException>(() => value.ToImageDataUri("application/octet-stream"));
    }

    [Fact]
    public void ToMemoryStream_ConvertsCorrectly()
    {
        byte[] value = { 1, 2, 3 };
        using var stream = value.ToMemoryStream();
        Assert.Equal(value, stream.ToArray());
    }

    [Fact]
    public void ToDataTableColumns_ConvertsCorrectly()
    {
        string[] columns = { "Column1", "Column2" };
        DataTable table = columns.ToDataTableColumns();
        Assert.Equal(2, table.Columns.Count);
        Assert.Equal("Column1", table.Columns[0].ColumnName);
        Assert.Equal("Column2", table.Columns[1].ColumnName);
    }

    [Fact]
    public void ToBase64UrlString_ConvertsCorrectly()
    {
        byte[] value = { 1, 2, 3 };
        var result = value.ToBase64UrlString();
        Assert.Equal("AQID", result);
    }

    [Fact]
    public void ToBase64UrlString_ReplacesSpecialCharacters()
    {
        // Bytes that produce '+' and '/' in standard Base64
        // 0xFB + 0xFF = standard Base64 would produce "++//"
        byte[] value = { 0xFB, 0xEF, 0xBE };
        var result = value.ToBase64UrlString();

        // Should not contain '+' or '/'
        Assert.DoesNotContain("+", result);
        Assert.DoesNotContain("/", result);
        // Should contain '-' or '_' instead
        Assert.True(result.Contains('-') || result.Contains('_') || !result.Contains('+') && !result.Contains('/'));
    }

    [Fact]
    public void ToBase64UrlString_RemovesPadding()
    {
        // Single byte produces Base64 with padding "AQ=="
        byte[] value = { 1 };
        var result = value.ToBase64UrlString();

        // Should not contain padding
        Assert.DoesNotContain("=", result);
        Assert.Equal("AQ", result);
    }
}
