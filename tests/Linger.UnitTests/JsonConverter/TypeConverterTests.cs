#if NET462_OR_GREATER
using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.Json;
using Linger.JsonConverter;
using Xunit;

namespace Linger.UnitTests;

public class TypeConverterTests
{
    [Fact]
    public void Read_ReturnsNull_ForJsonNull()
    {
        var json = "null";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        var converter = new Linger.JsonConverter.TypeConverter();
        var options = new JsonSerializerOptions();
        Type? result = converter.Read(ref reader, typeof(Type), options);
        Assert.Null(result);
    }

    [Fact]
    public void Write_WritesNullValue_ForAnyType()
    {
        var converter = new Linger.JsonConverter.TypeConverter();
        var options = new JsonSerializerOptions();
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        converter.Write(writer, typeof(string), options);
        writer.Flush();
        var json = Encoding.UTF8.GetString(stream.ToArray());
        Assert.Equal("null", json);
    }
}
#endif