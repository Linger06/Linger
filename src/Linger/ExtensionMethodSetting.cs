using System.Text;
#if !NETFRAMEWORK || NET462_OR_GREATER

using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using Linger.Json.JsonConverter;

#endif

namespace Linger;

/// <summary>
/// Settings for extension methods.
/// Provides immutable default configurations to ensure thread safety.
/// </summary>
[Obsolete("ExtensionMethodSetting is deprecated. For JSON-related configurations, use JsonDefaults from Linger.Json namespace instead.")]
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
        DefaultRequestJsonOptions = CreateDefaultRequestJsonOptions();
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
    [Obsolete("Use JsonDefaults.CreateResponseOptions() from Linger.Json namespace instead.")]
    public static JsonSerializerOptions DefaultJsonSerializerOptions { get; }

    /// <summary>
    /// Gets the default JSON serializer options for POST requests.
    /// This is a read-only configuration to ensure thread safety.
    /// To customize options, create a new JsonSerializerOptions instance based on this one.
    /// </summary>
    /// <value>The default JSON serializer options for POST requests.</value>
    [Obsolete("Use JsonDefaults.CreateRequestOptions() from Linger.Json namespace instead.")]
    public static JsonSerializerOptions DefaultRequestJsonOptions { get; }

    [Obsolete("Use JsonDefaults.CreateRequestOptions() from Linger.Json namespace instead.")]
    public static JsonSerializerOptions DefaultPostJsonOption => DefaultRequestJsonOptions;

    /// <summary>
    /// Creates the default JSON serializer options.
    /// </summary>
    private static JsonSerializerOptions CreateDefaultJsonOptions()
    {
        // Base on standard Web defaults, then harden and enable read-only leniency for numbers
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            // Security hardening and consistency
            Encoder = JavaScriptEncoder.Default,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            // Read-only leniency for interop: accept numeric strings on deserialization
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        // Keep project-specific converters
        jsonOptions.Converters.Add(new JsonObjectConverter());
        jsonOptions.Converters.Add(new DateTimeConverter());
        jsonOptions.Converters.Add(new DateTimeNullConverter());
        jsonOptions.Converters.Add(new DataTableJsonConverter());

        return jsonOptions;
    }

    /// <summary>
    /// Creates the default JSON serializer options for POST requests.
    /// </summary>
    private static JsonSerializerOptions CreateDefaultRequestJsonOptions()
    {
        // Base on standard Web defaults for outgoing request bodies.
        // Do NOT globally write numbers as strings; keep standard numeric outputs.
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Encoder = JavaScriptEncoder.Default
        };

        // Keep only essential converters for request serialization
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
