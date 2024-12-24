using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;

namespace Linger.UnitTests;

public class ExtensionMethodSettingTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public void DefaultEncoding_IsUTF8()
    {
        Assert.Equal(Encoding.UTF8, ExtensionMethodSetting.DefaultEncoding);
    }

    [Fact]
    public void DefaultCulture_IsInvariantCulture()
    {
        outputHelper.WriteLine($"FirstDayOfWeek:{ExtensionMethodSetting.DefaultCulture.DateTimeFormat.FirstDayOfWeek.ToString()}");
        outputHelper.WriteLine($"CalendarWeekRule:{ExtensionMethodSetting.DefaultCulture.DateTimeFormat.CalendarWeekRule.ToString()}");
        Assert.Equal(CultureInfo.InvariantCulture, ExtensionMethodSetting.DefaultCulture);
    }

    [Fact]
    public void zhCNCulture_Test_InDiffFramwork()
    {
        outputHelper.WriteLine($"FirstDayOfWeek:{new CultureInfo("zh-CN").DateTimeFormat.FirstDayOfWeek.ToString()}");
        outputHelper.WriteLine($"CalendarWeekRule:{new CultureInfo("zh-CN").DateTimeFormat.CalendarWeekRule.ToString()}");
    }

    [Fact]
    public void DefaultJsonSerializerOptions_HasCorrectSettings()
    {
        JsonSerializerOptions? options = ExtensionMethodSetting.DefaultJsonSerializerOptions;
        Assert.True(options.WriteIndented);
        Assert.Equal(JavaScriptEncoder.UnsafeRelaxedJsonEscaping, options.Encoder);
        Assert.True(options.PropertyNameCaseInsensitive);
        Assert.Equal(JsonNamingPolicy.CamelCase, options.PropertyNamingPolicy);
        Assert.Equal(JsonNamingPolicy.CamelCase, options.DictionaryKeyPolicy);
        Assert.Equal(JsonIgnoreCondition.WhenWritingNull, options.DefaultIgnoreCondition);
        Assert.True(options.IgnoreReadOnlyProperties);
        Assert.True(options.IgnoreReadOnlyFields);
        Assert.False(options.AllowTrailingCommas);
        Assert.Equal(JsonCommentHandling.Disallow, options.ReadCommentHandling);
        Assert.Equal(JsonNumberHandling.AllowReadingFromString, options.NumberHandling);
        Assert.Equal(ReferenceHandler.IgnoreCycles, options.ReferenceHandler);
        Assert.Contains(options.Converters, c => c is JsonObjectConverter);
        Assert.Contains(options.Converters, c => c is DateTimeConverter);
        Assert.Contains(options.Converters, c => c is DateTimeNullConverter);
        Assert.Contains(options.Converters, c => c is DataTableJsonConverter);
    }

    [Fact]
    public void DefaultPostJsonOption_HasCorrectSettings()
    {
        JsonSerializerOptions? options = ExtensionMethodSetting.DefaultPostJsonOption;
        Assert.Null(options.PropertyNamingPolicy);
        Assert.Equal(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString, options.NumberHandling);
        Assert.True(options.AllowTrailingCommas);
        Assert.Contains(options.Converters, c => c is DateTimeConverter);
    }

    [Fact]
    public void DefaultBufferSize_Is4096()
    {
        Assert.Equal(4096, ExtensionMethodSetting.DefaultBufferSize);
    }
}