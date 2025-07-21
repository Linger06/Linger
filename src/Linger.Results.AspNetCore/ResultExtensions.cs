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

        // 根据Result的Status确定HTTP状态码
        var statusCode = result.Status switch
        {
            ResultStatus.NotFound => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status400BadRequest
        };

        return new ObjectResult(result.Errors)
        {
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// 将Result转换为ActionResult，并指定成功和失败时的状态码
    /// </summary>
    /// <param name="result">要转换的Result对象</param>
    /// <param name="successStatusCode">成功时返回的HTTP状态码</param>
    /// <param name="failureStatusCode">失败时返回的HTTP状态码</param>
    /// <returns>对应的ActionResult对象</returns>
    public static ActionResult ToActionResult(
        this Result result,
        int successStatusCode = StatusCodes.Status200OK,
        int failureStatusCode = StatusCodes.Status400BadRequest)
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
            StatusCode = failureStatusCode
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
            // 注意: 这里直接返回值，而不是整个Result对象
            return new OkObjectResult(result.Value);
        }

        // 根据Result的Status确定HTTP状态码
        var statusCode = result.Status switch
        {
            ResultStatus.NotFound => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status400BadRequest
        };

        // 返回错误信息
        return new ObjectResult(new { errors = result.Errors })
        {
            StatusCode = statusCode
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
    public static ActionResult<T> ToActionResult<T>(
        this Result<T> result,
        int successStatusCode = StatusCodes.Status200OK,
        int failureStatusCode = StatusCodes.Status400BadRequest)
    {
        if (result.IsSuccess)
        {
            // 直接返回值，而不是整个Result对象
            return new ObjectResult(result.Value)
            {
                StatusCode = successStatusCode
            };
        }

        // 返回错误信息，而不是整个Result对象
        return new ObjectResult(new { errors = result.Errors })
        {
            StatusCode = failureStatusCode
        };
    }

    /// <summary>
    /// 将Result转换为ProblemDetails格式的ActionResult
    /// </summary>
    /// <param name="result">要转换的Result对象</param>
    /// <param name="failureStatusCode">失败时返回的HTTP状态码</param>
    /// <returns>成功时返回200 OK，失败时返回ProblemDetails</returns>
    public static ActionResult ToProblemDetails(
        this Result result,
        int failureStatusCode = StatusCodes.Status400BadRequest)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result);
        }

        var problemDetails = new ProblemDetails
        {
            Status = failureStatusCode,
            Title = "One or more errors occurred",
            Detail = string.Join("; ", result.Errors.Select(e => e.Message))
        };

        // 添加所有错误到扩展字典
        if (result.Errors.Count() > 1)
        {
            var errors = result.Errors.Select(e => e.Message).ToList();
            problemDetails.Extensions["errors"] = errors;
        }

        return new ObjectResult(problemDetails)
        {
            StatusCode = failureStatusCode
        };
    }

    /// <summary>
    /// 将Result{T}转换为ProblemDetails格式的ActionResult
    /// </summary>
    /// <typeparam name="T">结果值的类型</typeparam>
    /// <param name="result">要转换的Result{T}对象</param>
    /// <param name="failureStatusCode">失败时返回的HTTP状态码</param>
    /// <returns>成功时返回200 OK和结果值，失败时返回ProblemDetails</returns>
    public static ActionResult ToProblemDetails<T>(
        this Result<T> result,
        int failureStatusCode = StatusCodes.Status400BadRequest)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result.Value);
        }

        var problemDetails = new ProblemDetails
        {
            Status = failureStatusCode,
            Title = "One or more errors occurred",
            Detail = string.Join("; ", result.Errors.Select(e => e.Message))
        };

        // 添加所有错误到扩展字典
        if (result.Errors.Count() > 1)
        {
            var errors = result.Errors.Select(e => e.Message).ToList();
            problemDetails.Extensions["errors"] = errors;
        }

        return new ObjectResult(problemDetails)
        {
            StatusCode = failureStatusCode
        };
    }
}