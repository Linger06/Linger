namespace Linger.Results;

public class ExecuteResult
{
    public ExecuteResult(bool isSucceed, string message)
    {
        Set(isSucceed, message);
    }

    /// <summary>
    ///     如果是给字符串，表示有错误信息，默认IsSucceed=false
    /// </summary>
    /// <param name="message">错误信息</param>
    public ExecuteResult(string message)
    {
        Set(false, message);
    }

    /// <summary>
    ///     如果是空的，没有信息，默认IsSucceed=true
    /// </summary>
    public ExecuteResult()
    {
        // 显式初始化，使意图更清晰
        Set(true, string.Empty);
    }

    /// <summary>
    ///     执行是否成功
    ///     默认为True
    /// </summary>
    public bool IsSucceed { get; set; } = true;

    /// <summary>
    ///     执行信息（一般是错误信息）
    ///     默认置空
    /// </summary>
    public string Message { get; set; } = string.Empty;

    public ErrorObj? ErrorList { get; set; }

    public string? Action { get; set; }

    /// <summary>
    ///     设置执行结果的状态和消息
    /// </summary>
    /// <param name="isSucceed">是否成功</param>
    /// <param name="message">相关消息</param>
    /// <returns>当前对象实例，支持链式调用</returns>
    public virtual ExecuteResult Set(bool isSucceed, string message)
    {
        IsSucceed = isSucceed;
        Message = message ?? string.Empty; // 防止null
        return this;
    }

    /// <summary>
    ///     设定错误信息
    /// </summary>
    /// <param name="message">错误信息</param>
    /// <returns>当前对象实例，支持链式调用</returns>
    public virtual ExecuteResult SetFailMessage(string message)
    {
        return Set(false, message);
    }

    /// <summary>
    ///     将结果标记为失败，但不提供错误消息
    /// </summary>
    /// <returns>当前对象实例，支持链式调用</returns>
    public virtual ExecuteResult SetFail()
    {
        return Set(false, string.Empty);
    }

    /// <summary>
    ///     获取格式化的错误对象
    /// </summary>
    /// <returns>包含错误信息的对象</returns>
    public ErrorObj GetErrorJson()
    {
        var mse = new ErrorObj();
        if (IsSucceed)
        {
            return mse;
        }

        mse.Form = [];
        mse.Message = [];

        var err = Message;
        if (!string.IsNullOrEmpty(err))
        {
            mse.Message.Add(err);
        }

        return mse;
    }

    /// <summary>
    ///     创建一个成功的执行结果
    /// </summary>
    /// <returns>表示成功的ExecuteResult实例</returns>
    public static ExecuteResult Success() => new();

    /// <summary>
    ///     创建一个失败的执行结果
    /// </summary>
    /// <param name="message">错误信息</param>
    /// <returns>表示失败的ExecuteResult实例</returns>
    public static ExecuteResult Fail(string message) => new(false, message);
}

/// <summary>
///     执行返回结果
/// </summary>
/// <typeparam name="TValue">结果值的类型</typeparam>
public class ExecuteResult<TValue> : ExecuteResult
{
    public ExecuteResult() : base()
    {
    }

    public ExecuteResult(string message) : base(message)
    {
    }

    public ExecuteResult(bool isSucceed, string message) : base(isSucceed, message)
    {
    }

    public ExecuteResult(TValue result)
    {
        SetData(result);
    }

    /// <summary>
    ///     结果的值，只有在IsSucceed=true时才能访问
    /// </summary>
    public TValue Value { get; private set; } = default!;

    /// <summary>
    ///     设置执行结果的状态、消息和数据
    /// </summary>
    /// <param name="isSucceed">是否成功</param>
    /// <param name="message">相关消息</param>
    /// <param name="result">结果数据</param>
    /// <returns>当前对象实例，支持链式调用</returns>
    public ExecuteResult<TValue> Set(bool isSucceed, string message, TValue? result)
    {
        base.Set(isSucceed, message);
        if (isSucceed && result != null)
        {
            Value = result;
        }
        return this;
    }

    /// <summary>
    ///     设置成功结果数据
    /// </summary>
    /// <param name="data">结果数据</param>
    /// <returns>当前对象实例，支持链式调用</returns>
    public ExecuteResult<TValue> SetData(TValue data)
    {
        return Set(true, string.Empty, data);
    }

    public new ExecuteResult<TValue> SetFail()
    {
        base.SetFail();
        return this;
    }

    /// <summary>
    ///     设定错误信息
    ///     如果TValue正好也是string类型，可能set方法会存在用错的时候，所以取名SetFailMessage更明确
    /// </summary>
    /// <param name="message">错误信息</param>
    /// <returns>当前对象实例，支持链式调用</returns>
    public new ExecuteResult<TValue> SetFailMessage(string message)
    {
        base.SetFailMessage(message);
        return this;
    }

    /// <summary>
    ///     安全获取结果值，如果失败返回默认值
    /// </summary>
    /// <returns>成功时返回Value，失败时返回default</returns>
    public TValue? GetValueOrDefault()
    {
        return IsSucceed ? Value : default;
    }

    /// <summary>
    ///     创建一个包含数据的成功结果
    /// </summary>
    /// <param name="data">结果数据</param>
    /// <returns>包含指定数据的成功结果</returns>
    public static ExecuteResult<TValue> Success(TValue data) => new(data);

    /// <summary>
    ///     创建一个失败的结果
    /// </summary>
    /// <param name="message">错误信息</param>
    /// <returns>包含错误信息的失败结果</returns>
    public static new ExecuteResult<TValue> Fail(string message) => new(false, message);
}