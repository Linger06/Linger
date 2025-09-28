using System.Linq.Expressions;
using Linger.Helper;

namespace Linger.SharedKernel;

public interface IBaseSearch
{
    string? SearchText { get; set; }
    string? Sorting { get; set; }
    List<SortInfo>? SortList { get; set; }

    List<Condition> SearchParameters { get; set; }

    List<Condition> SearchOrParameters { get; set; }
    Expression<Func<T, bool>> GetSearchModelExpression<T>();
}