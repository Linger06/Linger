using System.Linq.Expressions;
using Linger.Helper;

namespace Linger.SharedKernel;

/// <summary>
/// 基础搜索接口，定义通用搜索功能
/// </summary>
public interface IBaseSearch
{
    /// <summary>
    /// 搜索文本，用于在实体的字符串属性中进行模糊匹配
    /// </summary>
    string? SearchText { get; set; }

    /// <summary>
    /// 排序字符串，格式为 "PropertyName asc" 或 "PropertyName desc"
    /// </summary>
    string? Sorting { get; set; }

    /// <summary>
    /// 排序信息列表，支持多字段排序
    /// </summary>
    List<SortInfo>? SortList { get; set; }

    /// <summary>
    /// 搜索参数列表，使用 AND 逻辑组合
    /// </summary>
    List<Condition> SearchParameters { get; set; }

    /// <summary>
    /// 搜索参数列表，使用 OR 逻辑组合
    /// </summary>
    List<Condition> SearchOrParameters { get; set; }

    /// <summary>
    /// 获取基于模型属性匹配的搜索表达式
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <returns>用于筛选实体的表达式树</returns>
    Expression<Func<T, bool>> GetSearchModelExpression<T>();
}