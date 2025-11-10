using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using Linger.Json.JsonConverter;

namespace Linger.Json;

/// <summary>
/// 提供统一的 JSON 序列化选项配置
/// </summary>
public static class JsonDefaults
{
    /// <summary>
    /// 创建用于 HTTP 响应反序列化的 JSON 序列化选项。
    /// </summary>
    /// <returns>配置了宽容输入策略的 <see cref="JsonSerializerOptions"/> 实例。</returns>
    /// <remarks>
    /// <para>
    /// 该方法遵循"宽进严出"(Be liberal in what you accept)原则,在接收数据时保持宽容:
    /// </para>
    /// <list type="bullet">
    /// <item><description>允许将数字字符串反序列化为数字类型</description></item>
    /// <item><description>启用循环引用检测和处理</description></item>
    /// <item><description>保留 null 值</description></item>
    /// <item><description>包含所有自定义转换器(DateTime、DataTable 等)</description></item>
    /// </list>
    /// <para>
    /// 适用场景: HTTP 客户端接收 API 响应、WebAPI 控制器输入模型绑定。
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // 在 HTTP 客户端中使用
    /// var options = JsonDefaults.CreateResponseOptions();
    /// var response = await httpClient.GetStringAsync("/api/data");
    /// var data = JsonSerializer.Deserialize&lt;MyModel&gt;(response, options);
    /// </code>
    /// </example>
    public static JsonSerializerOptions CreateResponseOptions()
    {
        // 基于标准 Web 默认值,然后进行安全加固并启用对数字的只读宽容处理
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            // 安全加固和一致性
            Encoder = JavaScriptEncoder.Default,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            // 只读宽容性以实现互操作:在反序列化时接受数字字符串
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        // 保留项目特定的转换器
        jsonOptions.Converters.Add(new JsonObjectConverter());
        jsonOptions.Converters.Add(new DateTimeConverter());
        jsonOptions.Converters.Add(new DateTimeNullConverter());
        jsonOptions.Converters.Add(new DataTableJsonConverter());

        return jsonOptions;
    }

    /// <summary>
    /// 创建用于 HTTP 请求序列化的 JSON 序列化选项。
    /// </summary>
    /// <returns>配置了严格输出策略的 <see cref="JsonSerializerOptions"/> 实例。</returns>
    /// <remarks>
    /// <para>
    /// 该方法遵循"严出宽进"(Be conservative in what you send)原则,在发送数据时保持严格:
    /// </para>
    /// <list type="bullet">
    /// <item><description>使用规范的 JSON 格式(数字不带引号)</description></item>
    /// <item><description>忽略 null 值以减少请求体大小</description></item>
    /// <item><description>仅包含必要的转换器(DateTime)</description></item>
    /// </list>
    /// <para>
    /// 适用场景: HTTP 客户端发送 API 请求。
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // 在 HTTP 客户端中使用
    /// var options = JsonDefaults.CreateRequestOptions();
    /// var json = JsonSerializer.Serialize(requestData, options);
    /// var content = new StringContent(json, Encoding.UTF8, "application/json");
    /// await httpClient.PostAsync("/api/data", content);
    /// </code>
    /// </example>
    public static JsonSerializerOptions CreateRequestOptions()
    {
        // 基于标准 Web 默认值用于传出请求体。
        // 遵循"严进宽出"原则:发送数据时保持严格和标准,使用规范的 JSON 格式。
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Encoder = JavaScriptEncoder.Default,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // 仅添加必要的转换器,确保请求序列化的严格性
        jsonOptions.Converters.Add(new DateTimeConverter());
        jsonOptions.Converters.Add(new DateTimeNullConverter());
        return jsonOptions;
    }

    /// <summary>
    /// 将 WebAPI 优化的配置应用到现有的 <see cref="JsonSerializerOptions"/> 实例。
    /// </summary>
    /// <param name="options">要配置的 <see cref="JsonSerializerOptions"/> 实例。</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="options"/> 为 <see langword="null"/> 时抛出。</exception>
    /// <remarks>
    /// <para>
    /// 该方法将 WebAPI 优化配置应用到现有实例,平衡了输入宽容性和输出效率:
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>输入(反序列化请求)</strong>: 宽容模式,接受数字字符串等变体</description></item>
    /// <item><description><strong>输出(序列化响应)</strong>: 优化模式,忽略 null 值以减少响应大小</description></item>
    /// <item><description>启用循环引用检测</description></item>
    /// <item><description>包含所有自定义转换器</description></item>
    /// </list>
    /// <para>
    /// 适用场景: ASP.NET Core WebAPI 的 <c>AddJsonOptions</c> 配置、需要修改现有实例的场景。
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // 场景 1: 在 ASP.NET Core WebAPI 中配置(推荐)
    /// builder.Services.AddControllers()
    ///     .AddJsonOptions(options =>
    ///         JsonDefaults.ApplyDefaultConfiguration(options.JsonSerializerOptions));
    ///
    /// // 场景 2: 创建新实例并应用配置
    /// var webApiOptions = new JsonSerializerOptions();
    /// JsonDefaults.ApplyDefaultConfiguration(webApiOptions);
    ///
    /// // 场景 3: 先应用配置,再自定义
    /// builder.Services.AddControllers()
    ///     .AddJsonOptions(options =>
    ///     {
    ///         JsonDefaults.ApplyDefaultConfiguration(options.JsonSerializerOptions);
    ///         options.JsonSerializerOptions.WriteIndented = true; // 额外配置
    ///     });
    /// </code>
    /// </example>
    public static void ApplyDefaultConfiguration(JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // 应用 WebAPI 优化配置
        options.Encoder = JavaScriptEncoder.Default;
        options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

        // 添加自定义转换器(避免重复添加)
        var convertersToAdd = new System.Text.Json.Serialization.JsonConverter[]
        {
            new JsonObjectConverter(),
            new DateTimeConverter(),
            new DateTimeNullConverter(),
            new DataTableJsonConverter()
        };

        foreach (var converter in convertersToAdd)
        {
            if (options.Converters.All(c => c.GetType() != converter.GetType()))
            {
                options.Converters.Add(converter);
            }
        }
    }
}
