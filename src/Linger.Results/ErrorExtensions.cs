namespace Linger.Results;

/// <summary>
/// 为错误类型提供扩展方法
/// </summary>
public static class ErrorExtensions
{
    /// <summary>
    /// 将 Error 转换为 ErrorObj
    /// </summary>
    /// <param name="error">要转换的错误</param>
    /// <returns>等效的 ErrorObj</returns>
    public static ErrorObj ToErrorObj(this Error error)
    {
        var errorObj = new ErrorObj();
        if (!string.IsNullOrWhiteSpace(error.Message))
        {
            errorObj.Message.Add(error.Message);
        }

        if (!string.IsNullOrWhiteSpace(error.Code))
        {
            errorObj.Form[error.Code] = error.Message;
        }

        return errorObj;
    }

    /// <summary>
    /// 将多个 Error 转换为单个 ErrorObj
    /// </summary>
    /// <param name="errors">要转换的错误集合</param>
    /// <returns>包含所有错误信息的 ErrorObj</returns>
    public static ErrorObj ToErrorObj(this IEnumerable<Error> errors)
    {
        var errorObj = new ErrorObj();

        foreach (var error in errors)
        {
            if (!string.IsNullOrWhiteSpace(error.Message))
            {
                errorObj.Message.Add(error.Message);
            }

            if (!string.IsNullOrWhiteSpace(error.Code))
            {
                errorObj.Form[error.Code] = error.Message;
            }
        }

        return errorObj;
    }

    /// <summary>
    /// 将 Exception 转换为 Error
    /// </summary>
    /// <param name="exception">要转换的异常</param>
    /// <returns>包含异常信息的 Error</returns>
    public static Error ToError(this Exception exception)
    {
        return new Error("Exception." + exception.GetType().Name, exception.Message);
    }

    /// <summary>
    /// 将 Exception 转换为 Error，并添加自定义错误代码
    /// </summary>
    /// <param name="exception">要转换的异常</param>
    /// <param name="errorCode">自定义错误代码</param>
    /// <returns>包含异常信息和自定义错误代码的 Error</returns>
    public static Error ToError(this Exception exception, string errorCode)
    {
        return new Error(errorCode, exception.Message);
    }
}