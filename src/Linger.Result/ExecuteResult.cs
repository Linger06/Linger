namespace Linger.Result;

public class ExecuteResult
{
    public ExecuteResult(bool isSucceed, string message)
    {
        Set(isSucceed, message);
    }

    /// <summary>
    ///     如果是给字符串，表示有错误信息，默认IsSucceed=false
    /// </summary>
    /// <param name="message"></param>
    public ExecuteResult(string message)
    {
        Set(false, message);
    }

    /// <summary>
    ///     如果是空的，没有信息，默认IsSucceed=true
    /// </summary>
    public ExecuteResult()
    {
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

    public virtual ExecuteResult Set(bool isSucceed, string message)
    {
        IsSucceed = isSucceed;
        Message = message;
        return this;
    }

    /// <summary>
    ///     设定错误信息
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public virtual ExecuteResult SetFailMessage(string message)
    {
        return Set(false, message);
    }

    public virtual ExecuteResult SetFail()
    {
        return Set(false, string.Empty);
    }

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
}

/// <summary>
///     执行返回结果
/// </summary>
/// <typeparam name="TValue"></typeparam>
public class ExecuteResult<TValue> : ExecuteResult
{
    public ExecuteResult()
    {
    }

    public ExecuteResult(string message)
    {
        Set(false, message);
    }

    public ExecuteResult(bool isSucceed, string message)
    {
        Set(isSucceed, message);
    }

    public ExecuteResult(TValue result)
    {
        SetData(result);
    }

    public TValue Value { get; set; } = default!;

    public ExecuteResult<TValue> Set(bool isSucceed, string message, TValue? result)
    {
        IsSucceed = isSucceed;
        Message = message;
        Value = isSucceed ? result! : throw new InvalidOperationException("The value of a failure result can not be accessed.");
        return this;
    }

    public ExecuteResult<TValue> SetData(TValue data)
    {
        return Set(true, string.Empty, data);
    }

    public new ExecuteResult<TValue> SetFail()
    {
        return Set(false, string.Empty, default);
    }

    /// <summary>
    ///     设定错误信息
    ///     如果TValue正好也是string类型，可能set方法会存在用错的时候，所以取名SetMessage更明确
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public new ExecuteResult<TValue> SetFailMessage(string message)
    {
        return Set(false, message, default);
    }
}