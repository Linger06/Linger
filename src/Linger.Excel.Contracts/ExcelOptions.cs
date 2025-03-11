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

    /// <summary>
    /// 默认日期格式
    /// </summary>
    public string DefaultDateFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";

    /// <summary>
    /// 是否使用批量写入优化
    /// </summary>
    public bool UseBatchWrite { get; set; } = true;

    /// <summary>
    /// 批量写入大小
    /// </summary>
    public int BatchSize { get; set; } = 5000;

    /// <summary>
    /// 是否在错误时继续处理（不抛出异常）
    /// </summary>
    public bool ContinueOnError { get; set; } = true;

    /// <summary>
    /// 是否使用内存优化（处理大文件时）
    /// </summary>
    public bool UseMemoryOptimization { get; set; } = false;

    /// <summary>
    /// 内存优化缓冲区大小（行数）
    /// </summary>
    public int MemoryBufferSize { get; set; } = 1000;

    /// <summary>
    /// 是否启用性能监控
    /// </summary>
    public bool EnablePerformanceMonitoring { get; set; } = false;

    /// <summary>
    /// 性能监控阈值(毫秒)，超过此阈值才记录日志
    /// </summary>
    public int PerformanceThreshold { get; set; } = 500;
}
