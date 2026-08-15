namespace Linger.HttpClient.Contracts.Core;

/// <summary>
/// HTTP方法枚举
/// </summary>
[Obsolete("HttpMethodEnum will be removed in 2.0.0. Use the BCL HttpMethod type instead. See the migration guide in the Linger repository for guidance.")]
public enum HttpMethodEnum
{
    Get,
    Post,
    Put,
    Delete
}
