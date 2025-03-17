#if NETSTANDARD2_0 || NETSTANDARD2_1 || NETCOREAPP2_0_OR_GREATER || NET451_OR_GREATER
using System.ComponentModel;
#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Runtime.CompilerServices
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit { }
}
#endif

#if NETSTANDARD2_0 || NETFRAMEWORK

// These Annotations are Part of .NET Standard 2.1 and .NET Core 3.0+. Defining them here allows
// their usage to support developers using this library with nullable-reference-type-warnings enabled.
#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Diagnostics.CodeAnalysis
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    [AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class MaybeNullWhenAttribute(bool returnValue) : Attribute
    {
        public bool ReturnValue { get; } = returnValue;
    }
}

#endif

namespace Linger.Result
{
    public class Result
    {
        private static readonly Result s_success = new(ResultStatus.Ok);

        protected Result()
        {
        }

        protected Result(ResultStatus status)
        {
            Status = status;
        }

        public ResultStatus Status { get; protected set; } = ResultStatus.Ok;

        public bool IsSuccess => Status is ResultStatus.Ok;
        public bool IsFailure => !IsSuccess;
        public IEnumerable<Error> Errors { get; protected set; } = [];

        public static Result Success() => s_success;
        public static Result<TValue> Success<TValue>(TValue value) => new(value, ResultStatus.Ok);

        public static Result Failure() => new(ResultStatus.Error) { Errors = [Error.Default] };
        public static Result Failure(Error error) => new(ResultStatus.Error) { Errors = [error] };
        public static Result Failure(string message) => new(ResultStatus.Error) { Errors = [new Error(string.Empty, message)] };
        public static Result Failure(IEnumerable<Error> errors) => new(ResultStatus.Error) { Errors = errors };

        public static Result<TValue> Failure<TValue>(Error error) => new(default, ResultStatus.Error) { Errors = [error] };
        public static Result<TValue> Failure<TValue>(string message) => new(default, ResultStatus.Error) { Errors = [new Error(string.Empty, message)] };
        public static Result<TValue> Failure<TValue>(IEnumerable<Error> errors) => new(default, ResultStatus.Error) { Errors = errors };

        public static Result Create(bool condition) => condition ? Success() : Failure(Error.ConditionNotMet);
        public static Result<TValue> Create<TValue>(TValue? value) => value is not null ? Success(value) : Failure<TValue>(Error.NullValue);

        public static Result NotFound() => new(ResultStatus.NotFound) { Errors = [Error.NotFound] };
        public static Result NotFound(string errorMessage) => new(ResultStatus.NotFound) { Errors = [new Error(string.Empty, errorMessage)] };
        public static Result NotFound(Error error) => new(ResultStatus.NotFound) { Errors = [error] };
        public static Result NotFound(IEnumerable<Error> errors) => new(ResultStatus.NotFound) { Errors = errors };
    }

    public class Result<TValue> : Result
    {
        private readonly TValue? _value;

        protected internal Result(TValue? value, ResultStatus status) : base(status) => _value = value;

        public TValue Value => IsSuccess
            ? _value!
            : throw new InvalidOperationException("The value of a failure result can not be accessed.");

        public static implicit operator Result<TValue>(TValue? value) => Create(value);

        public new static Result<TValue> Failure() => new(default, ResultStatus.Error) { Errors = [Error.Default] };
        public new static Result<TValue> Failure(Error error) => new(default, ResultStatus.Error) { Errors = [error] };
        public new static Result<TValue> Failure(string message) => new(default, ResultStatus.Error) { Errors = [new Error(string.Empty, message)] };
        public new static Result<TValue> Failure(IEnumerable<Error> errors) => new(default, ResultStatus.Error) { Errors = errors };

        public new static Result<TValue> NotFound() => new(default, ResultStatus.NotFound) { Errors = [Error.NotFound] };
        public new static Result<TValue> NotFound(string errorMessage) => new(default, ResultStatus.NotFound) { Errors = [new Error(string.Empty, errorMessage)] };
        public new static Result<TValue> NotFound(Error error) => new(default, ResultStatus.NotFound) { Errors = [error] };
        public new static Result<TValue> NotFound(IEnumerable<Error> errors) => new(default, ResultStatus.NotFound) { Errors = errors };

        public bool TryGetValue([System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out TValue? value)
        {
            value = _value;
            return IsSuccess;
        }

        /// <summary>
        /// 获取值，如果结果失败则返回默认值
        /// </summary>
        public TValue? ValueOrDefault => IsSuccess ? _value : default;

        /// <summary>
        /// 获取值，如果结果失败则返回指定的默认值
        /// </summary>
        /// <param name="defaultValue">结果失败时返回的默认值</param>
        /// <returns>成功时返回结果值，失败时返回默认值</returns>
        public TValue GetValueOrDefault(TValue defaultValue) =>
            IsSuccess ? _value! : defaultValue;

        /// <summary>
        /// 根据结果状态映射到不同的返回值
        /// </summary>
        /// <typeparam name="TResult">映射后的返回类型</typeparam>
        /// <param name="onSuccess">成功时的映射函数</param>
        /// <param name="onFailure">失败时的映射函数</param>
        /// <returns>映射后的结果</returns>
        public TResult Match<TResult>(
            Func<TValue, TResult> onSuccess,
            Func<IEnumerable<Error>, TResult> onFailure)
        {
            return IsSuccess
                ? onSuccess(Value)
                : onFailure(Errors);
        }

        /// <summary>
        /// 根据结果状态执行不同的操作
        /// </summary>
        /// <param name="onSuccess">成功时执行的操作</param>
        /// <param name="onFailure">失败时执行的操作</param>
        public void Match(
            Action<TValue> onSuccess,
            Action<IEnumerable<Error>> onFailure)
        {
            if (IsSuccess)
                onSuccess(Value);
            else
                onFailure(Errors);
        }
    }

    public record Error(string Code, string Message)
    {
        public static readonly Error None = new(string.Empty, string.Empty);//None代表没有错误
        public static readonly Error Default = new("Error.Default", "No Detail Error.");//Default代表默认错误,不具体指定        
        public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.");
        public static readonly Error ConditionNotMet = new("Error.ConditionNotMet", "The specified condition was not met.");
        public static readonly Error NotFound = new("Error.NotFound", "The Service was unable to find a requested resource.");
    }
}

