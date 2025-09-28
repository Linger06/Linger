namespace Linger.Excel.Contracts;

/// <summary>
/// Excel配置选项
/// </summary>
public class ExcelOptions
{
    /// <summary>
    /// 并行处理阈值，超过此数量的数据将使用并行处理
    /// </summary>
    public int ParallelProcessingThreshold { get; set; } = 1000;

    /// <summary>
    /// 是否自动调整列宽
    /// </summary>
    public bool AutoFitColumns { get; set; } = true;

    // 移除此处的DefaultDateFormat, 改为使用StyleOptions中的配置

    /// <summary>
    /// 是否使用批量写入优化
    /// </summary>
    public bool UseBatchWrite { get; set; } = true;

    /// <summary>
    /// 批量写入大小
    /// </summary>
    public int BatchSize { get; set; } = 5000;

    /// <summary>
    /// 是否启用性能监控
    /// </summary>
    public bool EnablePerformanceMonitoring { get; set; }

    /// <summary>
    /// 性能监控阈值(毫秒)，超过此阈值才记录日志
    /// </summary>
    public int PerformanceThreshold { get; set; } = 500;

    /// <summary>
    /// 样式配置选项
    /// </summary>
    public ExcelStyleOptions StyleOptions { get; set; } = new ExcelStyleOptions();
}
