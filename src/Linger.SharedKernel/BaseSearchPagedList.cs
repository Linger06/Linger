namespace Linger.SharedKernel;

/// <summary>
/// 分页搜索类，继承基础搜索功能并添加分页支持
/// </summary>
public class BaseSearchPagedList : BaseSearch, IBaseSearchPagedList
{
    /// <summary>
    /// 是否启用分页
    /// </summary>
    public bool IsPage { get; set; }

    /// <summary>
    /// 当前页码（从0或1开始，具体取决于实现）
    /// </summary>
    public int PageIndex { get; set; }

    /// <summary>
    /// 每页记录数
    /// </summary>
    public int PageSize { get; set; }
}
