using System.Globalization;
using System.Text;

#if !NETFRAMEWORK || NET462_OR_GREATER

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linger.JsonConverter;

#endif

namespace Linger;

/// <summary>
/// Settings for extension methods.
/// </summary>
public static class ExtensionMethodSetting
{
    /// <summary>
    /// Initializes a static instance of the ExtensionMethodSetting class.
    /// </summary>
    static ExtensionMethodSetting()
    {
        DefaultEncoding = Encoding.UTF8;
        DefaultCulture = CultureInfo.InvariantCulture;

#if !NETFRAMEWORK || NET462_OR_GREATER
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            IgnoreReadOnlyProperties = true,
            IgnoreReadOnlyFields = true,
            AllowTrailingCommas = false,
            ReadCommentHandling = JsonCommentHandling.Disallow,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        jsonOptions.Converters.Add(new JsonObjectConverter());
        jsonOptions.Converters.Add(new DateTimeConverter());
        jsonOptions.Converters.Add(new DateTimeNullConverter());
        jsonOptions.Converters.Add(new DataTableJsonConverter());

        DefaultJsonSerializerOptions = jsonOptions;

        var jsonOptions2 = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString,
            AllowTrailingCommas = true
        };

        jsonOptions2.Converters.Add(new DateTimeConverter());
        DefaultPostJsonOption = jsonOptions2;
#endif
    }

    /// <summary>
    /// Gets the default encoding, default is UTF8.
    /// </summary>
    /// <value>The default encoding.</value>
    public static Encoding DefaultEncoding { get; }

    /// <summary>
    /// Gets the default culture information, default is <see cref="CultureInfo.InvariantCulture"/>.
    /// </summary>
    /// <value >
    ///  <para>The default culture information is <see cref="CultureInfo.InvariantCulture"/>.</para>
    ///  <para>FirstDayOfWeek:Sunday</para>
    ///  <para>CalendarWeekRule:FirstDay</para>
    /// </value>
    public static CultureInfo DefaultCulture { get; }

#if !NETFRAMEWORK || NET462_OR_GREATER
    /// <summary>
    /// Gets or sets the default JSON serializer options.
    /// </summary>
    /// <value>The default JSON serializer options.</value>
    public static JsonSerializerOptions DefaultJsonSerializerOptions { get; set; }

    /// <summary>
    /// Gets or sets the default JSON serializer options for POST requests.
    /// </summary>
    /// <value>The default JSON serializer options for POST requests.</value>
    public static JsonSerializerOptions DefaultPostJsonOption { get; set; }
#endif

    /// <summary>
    /// Gets the default buffer size.
    /// </summary>
    /// <value>The default buffer size.</value>
    public static int DefaultBufferSize => 4096;
}
