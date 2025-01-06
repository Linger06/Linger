namespace Linger.Logging.Configuration.Providers;

/// <summary>
/// NLog����ѡ��
/// </summary>
public class NLogOptions
{
    /// <summary>
    /// �����ļ�·��
    /// </summary>
    public string? ConfigFilePath { get; set; }

    /// <summary>
    /// �Ƿ��Զ���������
    /// </summary>
    public bool AutoReload { get; set; } = true;

    /// <summary>
    /// �׳��쳣ʱ�Ƿ��������ջ
    /// </summary>
    public bool IncludeCallSite { get; set; } = true;

    /// <summary>
    /// �Ƿ������ڲ���־
    /// </summary>
    public bool EnableInternalLog { get; set; }

    /// <summary>
    /// �ڲ���־����
    /// </summary>
    public string InternalLogLevel { get; set; } = "Error";

    /// <summary>
    /// �ڲ���־�ļ�
    /// </summary>
    public string? InternalLogFile { get; set; }
}
