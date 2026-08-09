using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Linger.Results.AspNetCore;

/// <summary>
/// 提供Result和Result{T}类型到ActionResult类型的转换扩展方法，
/// 方便在ASP.NET Core API中使用Result模式
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// 将Result转换为ActionResult，可选指定成功与失败状态码
    /// </summary>
    /// <param name="result">要转换的Result对象</param>
    /// <param name="successStatusCode">成功时返回的HTTP状态码，null时默认200 OK</param>
    /// <param name="failureStatusCode">失败时返回的HTTP状态码，null时根据Result状态自动确定</param>
    /// <returns>对应的ActionResult对象</returns>
    public static ActionResult ToActionResult(this Result result, HttpStatusCode? successStatusCode = null, HttpStatusCode? failureStatusCode = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        var success = successStatusCode.HasValue ? (int)successStatusCode.Value : StatusCodes.Status200OK;
        var failure = failureStatusCode.HasValue ? (int)failureStatusCode.Value : GetFailureStatusCode(result.Status);

        if (result.IsSuccess)
        {
            if (success == StatusCodes.Status200OK)
            {
                return new OkResult();
            }

            return new StatusCodeResult(success);
        }

        return CreateProblemDetailsResult(result.Errors, failure);
    }

    /// <summary>
    /// 将Result{T}转换为ActionResult{T}，可选指定成功与失败状态码
    /// </summary>
    /// <typeparam name="T">结果值的类型</typeparam>
    /// <param name="result">要转换的Result{T}对象</param>
    /// <param name="successStatusCode">成功时返回的HTTP状态码，null时默认200 OK</param>
    /// <param name="failureStatusCode">失败时返回的HTTP状态码，null时根据Result状态自动确定</param>
    /// <returns>对应的ActionResult{T}对象</returns>
    public static ActionResult<T> ToActionResult<T>(this Result<T> result, HttpStatusCode? successStatusCode = null, HttpStatusCode? failureStatusCode = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        var success = successStatusCode.HasValue ? (int)successStatusCode.Value : StatusCodes.Status200OK;
        var failure = failureStatusCode.HasValue ? (int)failureStatusCode.Value : GetFailureStatusCode(result.Status);

        if (result.IsSuccess)
        {
            if (success == StatusCodes.Status200OK)
            {
                return new OkObjectResult(result.Value);
            }

            return new ObjectResult(result.Value)
            {
                StatusCode = success
            };
        }

        return CreateProblemDetailsResult(result.Errors, failure);
    }

    #region Minimal API Extensions

    /// <summary>
    /// 将Result转换为IResult（用于Minimal API），可选指定成功与失败状态码
    /// </summary>
    /// <param name="result">要转换的Result对象</param>
    /// <param name="successStatusCode">成功时返回的HTTP状态码，null时默认200 OK</param>
    /// <param name="failureStatusCode">失败时返回的HTTP状态码，null时根据Result状态自动确定</param>
    /// <returns>对应的IResult对象</returns>
    /// <example>
    /// <code>
    /// app.MapPost("/api/users", (CreateUserRequest request) =>
    /// {
    ///     var result = CreateUser(request);
    ///     return result.ToHttpResult(HttpStatusCode.Created);
    /// });
    /// </code>
    /// </example>
    public static IResult ToHttpResult(this Result result, HttpStatusCode? successStatusCode = null, HttpStatusCode? failureStatusCode = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        var success = successStatusCode.HasValue ? (int)successStatusCode.Value : StatusCodes.Status200OK;
        var failure = failureStatusCode.HasValue ? (int)failureStatusCode.Value : GetFailureStatusCode(result.Status);

        if (result.IsSuccess)
        {
            if (success == StatusCodes.Status200OK)
            {
                return Microsoft.AspNetCore.Http.Results.Ok();
            }

            return Microsoft.AspNetCore.Http.Results.StatusCode(success);
        }

        var problemDetails = CreateProblemDetails(result.Errors, failure);
        return Microsoft.AspNetCore.Http.Results.Problem(problemDetails);
    }

    /// <summary>
    /// 将Result{T}转换为IResult（用于Minimal API），可选指定成功与失败状态码
    /// </summary>
    /// <typeparam name="T">结果值的类型</typeparam>
    /// <param name="result">要转换的Result{T}对象</param>
    /// <param name="successStatusCode">成功时返回的HTTP状态码，null时默认200 OK</param>
    /// <param name="failureStatusCode">失败时返回的HTTP状态码，null时根据Result状态自动确定</param>
    /// <returns>对应的IResult对象</returns>
    /// <example>
    /// <code>
    /// app.MapPut("/api/users/{id}", (int id, UpdateUserRequest request) =>
    /// {
    ///     var result = UpdateUser(id, request);
    ///     return result.ToHttpResult(HttpStatusCode.OK, HttpStatusCode.NotFound);
    /// });
    /// </code>
    /// </example>
    public static IResult ToHttpResult<T>(this Result<T> result, HttpStatusCode? successStatusCode = null, HttpStatusCode? failureStatusCode = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        var success = successStatusCode.HasValue ? (int)successStatusCode.Value : StatusCodes.Status200OK;
        var failure = failureStatusCode.HasValue ? (int)failureStatusCode.Value : GetFailureStatusCode(result.Status);

        if (result.IsSuccess)
        {
            if (success == StatusCodes.Status200OK)
            {
                return Microsoft.AspNetCore.Http.Results.Ok(result.Value);
            }

            return CreateSuccessResult(result.Value, success);
        }

        var problemDetails = CreateProblemDetails(result.Errors, failure);
        return Microsoft.AspNetCore.Http.Results.Problem(problemDetails);
    }

    /// <summary>
    /// 将Result转换为Created类型的IResult，用于Minimal API
    /// </summary>
    /// <param name="result">要转换的Result对象</param>
    /// <param name="location">Created结果的位置URI</param>
    /// <returns>对应的IResult对象</returns>
    /// <example>
    /// <code>
    /// app.MapPost("/api/users", (CreateUserRequest request) =>
    /// {
    ///     var result = CreateUser(request);
    ///     return result.ToCreatedResult($"/api/users/{user.Id}");
    /// });
    /// </code>
    /// </example>
    public static IResult ToCreatedResult(this Result result, string location)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(location);

        if (result.IsSuccess)
        {
            return Microsoft.AspNetCore.Http.Results.Created(location, null);
        }

        var problemDetails = CreateProblemDetails(result.Errors, GetFailureStatusCode(result.Status));
        return Microsoft.AspNetCore.Http.Results.Problem(problemDetails);
    }

    /// <summary>
    /// 将Result{T}转换为Created类型的IResult，用于Minimal API
    /// </summary>
    /// <typeparam name="T">结果值的类型</typeparam>
    /// <param name="result">要转换的Result{T}对象</param>
    /// <param name="location">Created结果的位置URI</param>
    /// <returns>对应的IResult对象</returns>
    /// <example>
    /// <code>
    /// app.MapPost("/api/users", (CreateUserRequest request) =>
    /// {
    ///     var result = CreateUser(request);
    ///     return result.ToCreatedResult($"/api/users/{result.Value.Id}");
    /// });
    /// </code>
    /// </example>
    public static IResult ToCreatedResult<T>(this Result<T> result, string location)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(location);

        if (result.IsSuccess)
        {
            return Microsoft.AspNetCore.Http.Results.Created(location, result.Value);
        }

        var problemDetails = CreateProblemDetails(result.Errors, GetFailureStatusCode(result.Status));
        return Microsoft.AspNetCore.Http.Results.Problem(problemDetails);
    }

    /// <summary>
    /// 将Result转换为NoContent类型的IResult，用于Minimal API的删除操作
    /// </summary>
    /// <param name="result">要转换的Result对象</param>
    /// <returns>对应的IResult对象</returns>
    /// <example>
    /// <code>
    /// app.MapDelete("/api/users/{id}", (int id) =>
    /// {
    ///     var result = DeleteUser(id);
    ///     return result.ToNoContentResult();
    /// });
    /// </code>
    /// </example>
    public static IResult ToNoContentResult(this Result result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
        {
            return Microsoft.AspNetCore.Http.Results.NoContent();
        }

        var problemDetails = CreateProblemDetails(result.Errors, GetFailureStatusCode(result.Status));
        return Microsoft.AspNetCore.Http.Results.Problem(problemDetails);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// 根据 ResultStatus 获取对应的 HTTP 失败状态码
    /// </summary>
    /// <param name="status">Result 状态</param>
    /// <returns>对应的 HTTP 状态码</returns>
    private static int GetFailureStatusCode(ResultStatus status)
    {
        return status switch
        {
            ResultStatus.NotFound => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status400BadRequest
        };
    }

    /// <summary>
    /// 根据成功时的状态码创建含值的IResult，用于Minimal API
    /// </summary>
    /// <typeparam name="T">结果值的类型</typeparam>
    /// <param name="value">结果值</param>
    /// <param name="successStatusCode">成功时返回的HTTP状态码</param>
    /// <returns>对应的IResult对象</returns>
    private static IResult CreateSuccessResult<T>(T value, int successStatusCode)
    {
        return successStatusCode switch
        {
            StatusCodes.Status201Created => Microsoft.AspNetCore.Http.Results.Created(string.Empty, value),
            StatusCodes.Status202Accepted => Microsoft.AspNetCore.Http.Results.Accepted(string.Empty, value),
            _ => Microsoft.AspNetCore.Http.Results.Json(value, statusCode: successStatusCode)
        };
    }

    private static ObjectResult CreateProblemDetailsResult(IEnumerable<Error> errors, int statusCode)
    {
        return new ObjectResult(CreateProblemDetails(errors, statusCode))
        {
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// 创建ProblemDetails对象
    /// </summary>
    /// <param name="errors">错误集合</param>
    /// <param name="statusCode">HTTP状态码</param>
    /// <returns>ProblemDetails对象</returns>
    private static ProblemDetails CreateProblemDetails(IEnumerable<Error> errors, int statusCode)
    {
        var errorList = errors as IList<Error> ?? errors.ToList();
        // 1. 根据状态码决定通用的 Title 与 Detail 默认说明
        var title = statusCode switch
        {
            StatusCodes.Status400BadRequest => "One or more validation errors occurred",
            StatusCodes.Status401Unauthorized => "Unauthorized access",
            StatusCodes.Status403Forbidden => "Access forbidden",
            StatusCodes.Status404NotFound => "The requested resource was not found",
            StatusCodes.Status409Conflict => "A conflict occurred",
            StatusCodes.Status500InternalServerError => "Internal Server Error",
            _ => "An error occurred"
        };
        // 2. 优化 Detail 的逻辑。如果是 500 错误，展示安全提示并附带追踪 ID
        string detail;
        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            // 生产环境安全提示，TraceIdentifier 会帮助开发人员在日志系统中瞬间定位堆栈
            detail = "An unexpected server error occurred. Please contact the administrator.";
        }
        else
        {
            // 4xx 业务错误时：如果只有一个错误，直接用它作为 Detail；如果有多个，用一句话概括，具体细节留给 errors 字典
            detail = errorList.Count == 1
                ? errorList[0].Message
                : "Please refer to the errors property for additional details.";
        }
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };
        // 3. 核心改进：通过 GroupBy 将错误按 Code（字段名）分组，值转为 string[]
        // 这不仅解决了 Key 重复引发的崩溃，还完美匹配了客户端的 Dictionary<string, string[]> 数据类型
        if (errorList.Count > 0 && statusCode != StatusCodes.Status500InternalServerError)
        {
            problemDetails.Extensions["errors"] = errorList
                .GroupBy(e => e.Code ?? string.Empty)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.Message).ToArray()
                );
        }
        return problemDetails;
    }

    #endregion
}
