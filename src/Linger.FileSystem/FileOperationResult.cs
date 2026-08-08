namespace Linger.FileSystem;

/// <summary>
/// 文件操作结果（不可变）
/// </summary>
/// <remarks>
/// <para>此类使用不可变模式设计，所有属性在创建后不可修改。</para>
/// <para>请使用 <see cref="CreateSuccess(string)"/> 或 <see cref="CreateFailure(string, Exception?)"/> 工厂方法创建实例。</para>
/// </remarks>
public class FileOperationResult
{
    /// <summary>
    /// 操作是否成功
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// 错误信息（如果操作失败）
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// 异常信息（如果操作失败）
    /// </summary>
    public Exception? Exception { get; init; }

    /// <summary>
    /// 文件路径
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// 创建成功的操作结果
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>表示成功的操作结果</returns>
    public static FileOperationResult CreateSuccess(string filePath)
    {
        return new FileOperationResult
        {
            Success = true,
            FilePath = filePath
        };
    }

    /// <summary>
    /// 创建成功的操作结果（无附加信息）
    /// </summary>
    /// <returns>表示成功的操作结果</returns>
    public static FileOperationResult CreateSuccess()
    {
        return new FileOperationResult
        {
            Success = true
        };
    }

    /// <summary>
    /// 创建失败的操作结果
    /// </summary>
    /// <param name="errorMessage">错误信息</param>
    /// <param name="exception">异常信息（可选）</param>
    /// <returns>表示失败的操作结果</returns>
    public static FileOperationResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new FileOperationResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}
