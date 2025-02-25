#if NETSTANDARD2_0 || NETSTANDARD2_1 || NETCOREAPP2_0_OR_GREATER || NET451_OR_GREATER
using System.ComponentModel;
namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit { }
}
#endif

namespace Linger.Result
{
    public class Result
    {
        protected Result()
        {
        }
        
        protected Result(ResultStatus status)
        {
            Status = status;
        }

        private ResultStatus Status { get; set; } = ResultStatus.Ok;


        public bool IsSuccess => Status is ResultStatus.Ok;
        public bool IsFailure => !IsSuccess;
        public IEnumerable<Error> Errors { get; protected set; } = [];
        public static Result Success() => new(ResultStatus.Ok);
        public static Result<TValue> Success<TValue>(TValue value) => new(value, ResultStatus.Ok);

        public static Result Failure() => new(ResultStatus.Error) { Errors = new[] { Error.Default } };
        public static Result Failure(Error error) => new(ResultStatus.Error) { Errors = new[] { error } };
        public static Result Failure(string message) => new(ResultStatus.Error) { Errors = new[] { new Error(string.Empty, message) } };
        public static Result Failure(IEnumerable<Error> errors) => new(ResultStatus.Error) { Errors = errors };

        public static Result<TValue> Failure<TValue>(Error error) => new(default, ResultStatus.Error) { Errors = new[] { error } };
        public static Result<TValue> Failure<TValue>(string message) => new(default, ResultStatus.Error) { Errors = new[] { new Error(string.Empty, message) } };
        public static Result<TValue> Failure<TValue>(IEnumerable<Error> errors) => new(default, ResultStatus.Error) { Errors = errors };

        public static Result Create(bool condition) => condition ? Success() : Failure(Error.ConditionNotMet);
        public static Result<TValue> Create<TValue>(TValue? value) => value is not null ? Success(value) : Failure<TValue>(Error.NullValue);

        public static Result NotFound() => new(ResultStatus.NotFound) { Errors = new[] { Error.NotFound } };
        public static Result NotFound(string errorMessage) => new(ResultStatus.NotFound) { Errors = new[] { new Error(string.Empty, errorMessage) } };
        public static Result NotFound(Error error) => new(ResultStatus.NotFound) { Errors = new[] { error } };
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
        public new static Result<TValue> Failure(string message) => new(default, ResultStatus.Error) { Errors = [new Error(string.Empty, message)
            ]
        };
        public new static Result<TValue> Failure(IEnumerable<Error> errors) => new(default, ResultStatus.Error) { Errors = errors };

        public new static Result<TValue> NotFound() => new(default, ResultStatus.NotFound) { Errors = [Error.NotFound] };
        public new static Result<TValue> NotFound(string errorMessage) => new(default, ResultStatus.NotFound) { Errors =
            [new Error(string.Empty, errorMessage)]
        };
        public new static Result<TValue> NotFound(Error error) => new(default, ResultStatus.NotFound) { Errors = [error]
        };
        public new static Result<TValue> NotFound(IEnumerable<Error> errors) => new(default, ResultStatus.NotFound) { Errors = errors };
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

