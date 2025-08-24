using System.Text;
#if !NETFRAMEWORK || NET462_OR_GREATER

using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using Linger.JsonConverter;

#endif

namespace Linger;

/// <summary>
/// Settings for extension methods.
/// Provides immutable default configurations to ensure thread safety.
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
        DefaultJsonSerializerOptions = CreateDefaultJsonOptions();
        DefaultPostJsonOption = CreateDefaultPostJsonOptions();
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
    /// Gets the default JSON serializer options.
    /// This is a read-only configuration to ensure thread safety.
    /// To customize options, create a new JsonSerializerOptions instance based on this one.
    /// </summary>
    /// <value>The default JSON serializer options.</value>
    public static JsonSerializerOptions DefaultJsonSerializerOptions { get; }

    /// <summary>
    /// Gets the default JSON serializer options for POST requests.
    /// This is a read-only configuration to ensure thread safety.
    /// To customize options, create a new JsonSerializerOptions instance based on this one.
    /// </summary>
    /// <value>The default JSON serializer options for POST requests.</value>
    public static JsonSerializerOptions DefaultPostJsonOption { get; }

    /// <summary>
    /// Creates a copy of the default JSON serializer options for customization.
    /// Use this method when you need to modify JSON options without affecting the global defaults.
    /// </summary>
    /// <returns>A new JsonSerializerOptions instance based on the defaults.</returns>
    public static JsonSerializerOptions CreateCustomJsonOptions()
    {
        return new JsonSerializerOptions(DefaultJsonSerializerOptions);
    }

    /// <summary>
    /// Creates a copy of the default POST JSON serializer options for customization.
    /// Use this method when you need to modify JSON options without affecting the global defaults.
    /// </summary>
    /// <returns>A new JsonSerializerOptions instance based on the POST defaults.</returns>
    public static JsonSerializerOptions CreateCustomPostJsonOptions()
    {
        return new JsonSerializerOptions(DefaultPostJsonOption);
    }

    /// <summary>
    /// Creates the default JSON serializer options.
    /// </summary>
    private static JsonSerializerOptions CreateDefaultJsonOptions()
    {
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

        return jsonOptions;
    }

    /// <summary>
    /// Creates the default JSON serializer options for POST requests.
    /// </summary>
    private static JsonSerializerOptions CreateDefaultPostJsonOptions()
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString,
            AllowTrailingCommas = true
        };

        jsonOptions.Converters.Add(new DateTimeConverter());
        return jsonOptions;
    }
#endif

    /// <summary>
    /// Gets the default buffer size.
    /// </summary>
    /// <value>The default buffer size.</value>
    public static int DefaultBufferSize => 4096;
}
