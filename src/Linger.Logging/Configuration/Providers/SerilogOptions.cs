using Linger.Logging.Abstractions;

namespace Linger.Logging.Configuration.Providers;

/// <summary>
/// Serilog����ѡ��
/// </summary>
public class SerilogOptions
{
    /// <summary>
    /// ��С��־����
    /// </summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// �Ƿ����ýṹ����־
    /// </summary>
    public bool EnableStructuredLogging { get; set; } = true;

    /// <summary>
    /// �Ƿ�����Elasticsearch
    /// </summary>
    public bool EnableElasticLogging { get; set; }

    /// <summary>
    /// Elasticsearch�˵�
    /// </summary>
    public Uri? ElasticsearchEndpoint { get; set; }

    /// <summary>
    /// �Ƿ�����Seq
    /// </summary>
    public bool EnableSeqLogging { get; set; }

    /// <summary>
    /// Seq������URL
    /// </summary>
    public string? SeqServerUrl { get; set; }

    /// <summary>
    /// �Զ�������
    /// </summary>
    public Dictionary<string, string> CustomProperties { get; set; } = new();

    /// <summary>
    /// ����̨��������
    /// </summary>
    public ConsoleThemeSettings ConsoleTheme { get; set; } = new();
}
