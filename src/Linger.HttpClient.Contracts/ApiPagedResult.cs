namespace Linger.HttpClient.Contracts;

public class ApiPagedResult<T>
{
    public List<T> Data { get; set; } = default!;
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageCount { get; set; }
    public string? Msg { get; set; }
    public int Code { get; set; }
    
    /// <summary>
    /// 是否有数据
    /// </summary>
    public bool HasData => Data != null && Data.Count > 0;
    
    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNextPage => Page < PageCount;
}