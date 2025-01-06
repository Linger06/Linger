namespace Linger.Logging.Configuration.Providers;

/// <summary>
/// Log4Net����ѡ��
/// </summary>
public class Log4NetOptions
{
    /// <summary>
    /// �����ļ�·��
    /// </summary>
    public string? ConfigFilePath { get; set; }

    /// <summary>
    /// �Ƿ���������ļ��仯
    /// </summary>
    public bool WatchConfig { get; set; } = true;

    /// <summary>
    /// �ֿ�����
    /// </summary>
    public string RepositoryName { get; set; } = "DefaultRepository";

    /// <summary>
    /// �Ƿ�ʹ��ȫ�޶�������Ϊ��־��
    /// </summary>
    public bool UseFullyQualifiedLoggerName { get; set; } = true;
}
