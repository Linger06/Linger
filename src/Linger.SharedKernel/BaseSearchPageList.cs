namespace Linger.SharedKernel;

public class BaseSearchPageList : BaseSearch, IBaseSearchPagedList
{
    public bool IsPage { get; set; }

    /// <summary>
    ///     当前页 值
    /// </summary>
    public int PageIndex { get; set; }

    /// <summary>
    ///     每页大小
    /// </summary>
    public int PageSize { get; set; }
}