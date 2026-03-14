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
    /// 兼容旧用法：将Result转换为ActionResult，并指定成功和失败时的状态码（已过时，推荐使用分开重载）。
    /// </summary>
    /// <param name="result">要转换的Result对象</param>
    /// <param name="successStatusCode">成功时返回的HTTP状态码</param>
    /// <param name="failureStatusCode">失败时返回的HTTP状态码，null时自动映射</param>
    /// <returns>对应的ActionResult对象</returns>
    [Obsolete("请使用 ToActionResult(successStatusCode) 或 ToActionResult(successStatusCode, failureStatusCode) 重载。此方法将在后续版本移除。", false)]
    public static ActionResult ToActionResult(
        this Result result,
        int successStatusCode = StatusCodes.Status200OK,
        int? failureStatusCode = null)
    {
        if (result.IsSuccess)
        {
            return new ObjectResult(result)
            {
                StatusCode = successStatusCode
            };
        }

        return new ObjectResult(result.Errors)
        {
            StatusCode = failureStatusCode ?? GetFailureStatusCode(result.Status)
        };
    }

    /// <summary>
    /// 兼容旧用法：将Result{T}转换为ActionResult{T}，并指定成功和失败时的状态码（已过时，推荐使用分开重载）。
    /// </summary>
    /// <typeparam name="T">结果值的类型</typeparam>
    /// <param name="result">要转换的Result{T}对象</param>
    /// <param name="successStatusCode">成功时返回的HTTP状态码</param>
    /// <param name="failureStatusCode">失败时返回的HTTP状态码，null时自动映射</param>
    /// <returns>对应的ActionResult{T}对象</returns>
    [Obsolete("请使用 ToActionResult<T>(successStatusCode) 或 ToActionResult<T>(successStatusCode, failureStatusCode) 重载。此方法将在后续版本移除。", false)]
    public static ActionResult<T> ToActionResult<T>(
        this Result<T> result,
        int successStatusCode = StatusCodes.Status200OK,
        int? failureStatusCode = null)
    {
        if (result.IsSuccess)
        {
            return new ObjectResult(result.Value)
            {
                StatusCode = successStatusCode
            };
        }

        return new ObjectResult(result.Errors)
        {
            StatusCode = failureStatusCode ?? GetFailureStatusCode(result.Status)
        };
    }

    /// <summary>
    /// 兼容旧用法：将Result转换为IResult，并指定成功和失败时的状态码（已过时，推荐使用分开重载）。
    /// </summary>
    /// <param name="result">要转换的Result对象</param>
    /// <param name="successStatusCode">成功时返回的HTTP状态码</param>
    /// <param name="failureStatusCode">失败时返回的HTTP状态码，null时自动映射</param>
    /// <returns>对应的IResult对象</returns>
    [Obsolete("请使用 ToHttpResult(successStatusCode) 或 ToHttpResult(successStatusCode, failureStatusCode) 重载。此方法将在后续版本移除。", false)]
    public static IResult ToHttpResult(
        this Result result,
        int successStatusCode = StatusCodes.Status200OK,
        int? failureStatusCode = null)
    {
        if (result.IsSuccess)
        {
            return Microsoft.AspNetCore.Http.Results.StatusCode(successStatusCode);
        }

        var statusCode = failureStatusCode ?? GetFailureStatusCode(result.Status);
        var problemDetails = CreateProblemDetails(result.Errors, statusCode);
        return Microsoft.AspNetCore.Http.Results.Problem(problemDetails);
    }

    /// <summary>
    /// 兼容旧用法：将Result{T}转换为IResult，并指定成功和失败时的状态码（已过时，推荐使用分开重载）。
    /// </summary>
    /// <typeparam name="T">结果值的类型</typeparam>
    /// <param name="result">要转换的Result{T}对象</param>
    /// <param name="successStatusCode">成功时返回的HTTP状态码</param>
    /// <param name="failureStatusCode">失败时返回的HTTP状态码，null时自动映射</param>
    /// <returns>对应的IResult对象</returns>
    [Obsolete("请使用 ToHttpResult<T>(successStatusCode) 或 ToHttpResult<T>(successStatusCode, failureStatusCode) 重载。此方法将在后续版本移除。", false)]
    public static IResult ToHttpResult<T>(
        this Result<T> result,
        int successStatusCode = StatusCodes.Status200OK,
        int? failureStatusCode = null)
    {
        if (result.IsSuccess)
        {
            return CreateSuccessResult(result.Value, successStatusCode);
        }

        var statusCode = failureStatusCode ?? GetFailureStatusCode(result.Status);
        var problemDetails = CreateProblemDetails(result.Errors, statusCode);
        return Microsoft.AspNetCore.Http.Results.Problem(problemDetails);
    }

    /// <summary>
    /// 兼容旧用法：Minimal API 下的 ToResult，等价于 ToHttpResult（已过时，推荐使用 ToHttpResult）。
    /// </summary>
    /// <param name="result">要转换的Result对象</param>
    /// <returns>对应的IResult对象</returns>
    [Obsolete("请使用 ToHttpResult 替代 ToResult。此方法将在后续版本移除。", false)]
    public static IResult ToResult(this Result result)
    {
        return ToHttpResult(result);
    }

    /// <summary>
    /// 兼容旧用法：Minimal API 下的 ToResult<T>，等价于 ToHttpResult<T>（已过时，推荐使用 ToHttpResult）。
    /// </summary>
    /// <typeparam name="T">结果值的类型</typeparam>
    /// <param name="result">要转换的Result{T}对象</param>
    /// <returns>对应的IResult对象</returns>
    [Obsolete("请使用 ToHttpResult 替代 ToResult。此方法将在后续版本移除。", false)]
    public static IResult ToResult<T>(this Result<T> result)
    {
        return ToHttpResult(result);
    }

    /// <summary>
    /// 将Result转换为ActionResult
    /// </summary>
    /// <param name="result">要转换的Result对象</param>
    /// <returns>对应的ActionResult对象，成功返回200 OK，失败返回400 Bad Request</returns>
    public static ActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result);
        }

        return new ObjectResult(result.Errors)
        {
            StatusCode = GetFailureStatusCode(result.Status)
        };
    }

    /// <summary>
    /// 将Result转换为ActionResult，并指定成功时的状态码
    /// </summary>
    /// <param name="result">要转换的Result对象</param>
    /// <param name="successStatusCode">成功时返回的HTTP状态码</param>
    /// <returns>对应的ActionResult对象，失败时根据Result状态自动确定状态码</returns>
    public static ActionResult ToActionResult(this Result result, HttpStatusCode successStatusCode)
    {
        if (result.IsSuccess)
        {
            return new ObjectResult(result)
            {
                StatusCode = (int)successStatusCode
            };
        }

        return new ObjectResult(result.Errors)
        {
            StatusCode = GetFailureStatusCode(result.Status)
        };
    }

    /// <summary>
    /// 将Result转换为ActionResult，并指定成功和失败时的状态码
    /// </summary>
    /// <param name="result">要转换的Result对象</param>
    /// <param name="successStatusCode">成功时返回的HTTP状态码</param>
    /// <param name="failureStatusCode">失败时返回的HTTP状态码</param>
    /// <returns>对应的ActionResult对象</returns>
    public static ActionResult ToActionResult(this Result result, HttpStatusCode successStatusCode, HttpStatusCode failureStatusCode)
    {
        if (result.IsSuccess)
        {
            return new ObjectResult(result)
            {
                StatusCode = (int)successStatusCode
            };
        }

        return new ObjectResult(result.Errors)
        {
            StatusCode = (int)failureStatusCode
        };
    }

    /// <summary>
    /// 将Result{T}转换为ActionResult{T}
    /// </summary>
    /// <typeparam name="T">结果值的类型</typeparam>
    /// <param name="result">要转换的Result{T}对象</param>
    /// <returns>对应的ActionResult{T}对象，成功时返回200 OK和内部值，失败时根据状态返回对应的HTTP状态码</returns>
    public static ActionResult<T> ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result.Value);
        }

        return new ObjectResult(result.Errors)
        {
            StatusCode = GetFailureStatusCode(result.Status)
        };
    }

    /// <summary>
    /// 将Result{T}转换为ActionResult{T}，并指定成功时的状态码
    /// </summary>
    /// <typeparam name="T">结果值的类型</typeparam>
    /// <param name="result">要转换的Result{T}对象</param>
    /// <param name="successStatusCode">成功时返回的HTTP状态码</param>
    /// <returns>对应的ActionResult{T}对象，失败时根据Result状态自动确定状态码</returns>
    public static ActionResult<T> ToActionResult<T>(this Result<T> result, HttpStatusCode successStatusCode)
    {
        if (result.IsSuccess)
        {
            return new ObjectResult(result.Value)
            {
                StatusCode = (int)successStatusCode
            };
        }

        return new ObjectResult(result.Errors)
        {
            StatusCode = GetFailureStatusCode(result.Status)
        };
    }

    /// <summary>
    /// 将Result{T}转换为ActionResult{T}，并指定成功和失败时的状态码
    /// </summary>
    /// <typeparam name="T">结果值的类型</typeparam>
    /// <param name="result">要转换的Result{T}对象</param>
    /// <param name="successStatusCode">成功时返回的HTTP状态码</param>
    /// <param name="failureStatusCode">失败时返回的HTTP状态码</param>
    /// <returns>对应的ActionResult{T}对象</returns>
    public static ActionResult<T> ToActionResult<T>(this Result<T> result, HttpStatusCode successStatusCode, HttpStatusCode failureStatusCode)
    {
        if (result.IsSuccess)
        {
            return new ObjectResult(result.Value)
            {
                StatusCode = (int)successStatusCode
            };
        }

        return new ObjectResult(result.Errors)
        {
            StatusCode = (int)failureStatusCode
        };
    }

    /// <summary>
    /// 将Result转换为ProblemDetails格式的ActionResult
    /// </summary>
    /// <param name="result">要转换的Result对象</param>
    /// <returns>成功时返回200 OK，失败时根据Result状态返回对应的ProblemDetails</returns>
    public static ActionResult ToProblemDetails(this Result result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result);
        }

        var statusCode = GetFailureStatusCode(result.Status);
        var problemDetails = CreateProblemDetails(result.Errors, statusCode);
        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// 将Result转换为ProblemDetails格式的ActionResult，并指定失败时的状态码
    /// </summary>
    /// <param name="result">要转换的Result对象</param>
    /// <param name="failureStatusCode">失败时返回的HTTP状态码</param>
    /// <returns>成功时返回200 OK，失败时返回ProblemDetails</returns>
    public static ActionResult ToProblemDetails(this Result result, HttpStatusCode failureStatusCode)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result);
        }

        var statusCode = (int)failureStatusCode;
        var problemDetails = CreateProblemDetails(result.Errors, statusCode);
        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// 将Result{T}转换为ProblemDetails格式的ActionResult
    /// </summary>
    /// <typeparam name="T">结果值的类型</typeparam>
    /// <param name="result">要转换的Result{T}对象</param>
    /// <returns>成功时返回200 OK和结果值，失败时根据Result状态返回对应的ProblemDetails</returns>
    public static ActionResult ToProblemDetails<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result.Value);
        }

        var statusCode = GetFailureStatusCode(result.Status);
        var problemDetails = CreateProblemDetails(result.Errors, statusCode);
        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// 将Result{T}转换为ProblemDetails格式的ActionResult，并指定失败时的状态码
    /// </summary>
    /// <typeparam name="T">结果值的类型</typeparam>
    /// <param name="result">要转换的Result{T}对象</param>
    /// <param name="failureStatusCode">失败时返回的HTTP状态码</param>
    /// <returns>成功时返回200 OK和结果值，失败时返回ProblemDetails</returns>
    public static ActionResult ToProblemDetails<T>(this Result<T> result, HttpStatusCode failureStatusCode)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result.Value);
        }

        var statusCode = (int)failureStatusCode;
        var problemDetails = CreateProblemDetails(result.Errors, statusCode);
        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
    }

    #region Minimal API Extensions

    /// <summary>
    /// 将Result转换为IResult，用于Minimal API
    /// </summary>
    /// <param name="result">要转换的Result对象</param>
    /// <returns>对应的IResult对象</returns>
    /// <example>
    /// <code>
    /// app.MapGet("/api/status", () =>
    /// {
    ///     var result = SomeOperation();
    ///     return result.ToHttpResult();
    /// });
    /// </code>
    /// </example>
    public static IResult ToHttpResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return Microsoft.AspNetCore.Http.Results.Ok();
        }

        var problemDetails = CreateProblemDetails(result.Errors, GetFailureStatusCode(result.Status));
        return Microsoft.AspNetCore.Http.Results.Problem(problemDetails);
    }

    /// <summary>
    /// 将Result{T}转换为IResult，用于Minimal API
    /// </summary>
    /// <typeparam name="T">结果值的类型</typeparam>
    /// <param name="result">要转换的Result{T}对象</param>
    /// <returns>对应的IResult对象</returns>
    /// <example>
    /// <code>
    /// app.MapGet("/api/users/{id}", (int id) =>
    /// {
    ///     var result = GetUser(id);
    ///     return result.ToHttpResult();
    /// });
    /// </code>
    /// </example>
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Microsoft.AspNetCore.Http.Results.Ok(result.Value);
        }

        var problemDetails = CreateProblemDetails(result.Errors, GetFailureStatusCode(result.Status));
        return Microsoft.AspNetCore.Http.Results.Problem(problemDetails);
    }

    /// <summary>
    /// 将Result转换为IResult，并指定成功时的状态码，用于Minimal API
    /// </summary>
    /// <param name="result">要转换的Result对象</param>
    /// <param name="successStatusCode">成功时返回的HTTP状态码</param>
    /// <returns>对应的IResult对象，失败时根据Result状态自动确定状态码</returns>
    /// <example>
    /// <code>
    /// app.MapPost("/api/users", (CreateUserRequest request) =>
    /// {
    ///     var result = CreateUser(request);
    ///     return result.ToHttpResult(HttpStatusCode.Created);
    /// });
    /// </code>
    /// </example>
    public static IResult ToHttpResult(this Result result, HttpStatusCode successStatusCode)
    {
        if (result.IsSuccess)
        {
            return Microsoft.AspNetCore.Http.Results.StatusCode((int)successStatusCode);
        }

        var statusCode = GetFailureStatusCode(result.Status);
        var problemDetails = CreateProblemDetails(result.Errors, statusCode);
        return Microsoft.AspNetCore.Http.Results.Problem(problemDetails);
    }

    /// <summary>
    /// 将Result转换为IResult，并指定成功和失败时的状态码，用于Minimal API
    /// </summary>
    /// <param name="result">要转换的Result对象</param>
    /// <param name="successStatusCode">成功时返回的HTTP状态码</param>
    /// <param name="failureStatusCode">失败时返回的HTTP状态码</param>
    /// <returns>对应的IResult对象</returns>
    public static IResult ToHttpResult(this Result result, HttpStatusCode successStatusCode, HttpStatusCode failureStatusCode)
    {
        if (result.IsSuccess)
        {
            return Microsoft.AspNetCore.Http.Results.StatusCode((int)successStatusCode);
        }

        var problemDetails = CreateProblemDetails(result.Errors, (int)failureStatusCode);
        return Microsoft.AspNetCore.Http.Results.Problem(problemDetails);
    }

    /// <summary>
    /// 将Result{T}转换为IResult，并指定成功时的状态码，用于Minimal API
    /// </summary>
    /// <typeparam name="T">结果值的类型</typeparam>
    /// <param name="result">要转换的Result{T}对象</param>
    /// <param name="successStatusCode">成功时返回的HTTP状态码</param>
    /// <returns>对应的IResult对象，失败时根据Result状态自动确定状态码</returns>
    /// <example>
    /// <code>
    /// app.MapPut("/api/users/{id}", (int id, UpdateUserRequest request) =>
    /// {
    ///     var result = UpdateUser(id, request);
    ///     return result.ToHttpResult(HttpStatusCode.OK);
    /// });
    /// </code>
    /// </example>
    public static IResult ToHttpResult<T>(this Result<T> result, HttpStatusCode successStatusCode)
    {
        if (result.IsSuccess)
        {
            return CreateSuccessResult(result.Value, (int)successStatusCode);
        }

        var statusCode = GetFailureStatusCode(result.Status);
        var problemDetails = CreateProblemDetails(result.Errors, statusCode);
        return Microsoft.AspNetCore.Http.Results.Problem(problemDetails);
    }

    /// <summary>
    /// 将Result{T}转换为IResult，并指定成功和失败时的状态码，用于Minimal API
    /// </summary>
    /// <typeparam name="T">结果值的类型</typeparam>
    /// <param name="result">要转换的Result{T}对象</param>
    /// <param name="successStatusCode">成功时返回的HTTP状态码</param>
    /// <param name="failureStatusCode">失败时返回的HTTP状态码</param>
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
    public static IResult ToHttpResult<T>(this Result<T> result, HttpStatusCode successStatusCode, HttpStatusCode failureStatusCode)
    {
        if (result.IsSuccess)
        {
            return CreateSuccessResult(result.Value, (int)successStatusCode);
        }

        var problemDetails = CreateProblemDetails(result.Errors, (int)failureStatusCode);
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
            StatusCodes.Status200OK => Microsoft.AspNetCore.Http.Results.Ok(value),
            StatusCodes.Status201Created => Microsoft.AspNetCore.Http.Results.Created(string.Empty, value),
            StatusCodes.Status202Accepted => Microsoft.AspNetCore.Http.Results.Accepted(string.Empty, value),
            _ => Microsoft.AspNetCore.Http.Results.Json(value, statusCode: successStatusCode)
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

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode switch
            {
                StatusCodes.Status400BadRequest => "One or more validation errors occurred",
                StatusCodes.Status401Unauthorized => "Unauthorized access",
                StatusCodes.Status403Forbidden => "Access forbidden",
                StatusCodes.Status404NotFound => "The requested resource was not found",
                StatusCodes.Status409Conflict => "A conflict occurred",
                _ => "An error occurred"
            },
            Detail = string.Join("; ", errorList.Select(e => e.Message))
        };

        // 说明：ProblemDetails.Extensions 使用 JsonExtensionData，序列化时会将键值对展开为顶层属性。
        // 因此设置 Extensions["errors"] 会在 JSON 顶层得到 "errors": {...} 而非嵌套在 "extensions" 下。
        if (errorList.Count > 0)
        {
            problemDetails.Extensions["errors"] = errorList.ToDictionary(e => e.Code, e => e.Message);
        }

        return problemDetails;
    }

    #endregion
}
