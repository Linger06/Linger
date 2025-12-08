using System.Linq.Expressions;
using System.Reflection;
using Linger.Extensions;
using Linger.Extensions.Core;
using Linger.Helper;

namespace Linger.SharedKernel;

/// <summary>
/// 基础搜索类，提供通用的搜索和筛选功能
/// </summary>
public class BaseSearch : IBaseSearch
{
    /// <summary>
    /// 搜索文本，用于在实体的字符串属性中进行模糊匹配
    /// </summary>
    public string? SearchText { get; set; }

    /// <summary>
    /// 复杂查询条件，使用 AND 逻辑组合
    /// </summary>
    public List<Condition> SearchParameters { get; set; } = [];

    /// <summary>
    /// 复杂查询条件，使用 OR 逻辑组合
    /// </summary>
    public List<Condition> SearchOrParameters { get; set; } = [];

    /// <summary>
    /// 排序字符串，格式为 "PropertyName asc" 或 "PropertyName desc"
    /// </summary>
    public string? Sorting { get; set; }

    /// <summary>
    /// 排序信息列表，支持多字段排序
    /// </summary>
    public List<SortInfo>? SortList { get; set; }

    /// <summary>
    /// 获取基于模型属性匹配的搜索表达式。
    /// 将搜索模型中与实体类型匹配的属性值转换为相等比较表达式
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <returns>用于筛选实体的表达式树</returns>
    public Expression<Func<TEntity, bool>> GetSearchModelExpression<TEntity>()
    {
        PropertyInfo[] properties = typeof(TEntity).GetProperties();
        //var entityPropertiesNames = properties.Select(x => x.Name).ToList();
        PropertyInfo[] propertiesSVm = GetType().GetProperties();
        BinaryExpression? condition2 = null;
        ParameterExpression param = Expression.Parameter(typeof(TEntity), "x");

        foreach (PropertyInfo property in properties)
        {
            foreach (PropertyInfo propertySVm in propertiesSVm)
            {
                // 跳过自定义属性和空值属性，自定义属性可在查询前委托中定义查询条件
                if (property.Name != propertySVm.Name || property.PropertyType != propertySVm.PropertyType)
                {
                    continue;
                }

                var defaultValue = propertySVm.PropertyType.GetDefaultValue();

                var value = propertySVm.GetValue(this);

                //空值
                if (value is null)
                {
                    continue;
                }

                //默认值
                if (Equals(value, defaultValue))
                {
                    continue;
                }

                MemberExpression left = Expression.Property(param, property); //x.name
                ConstantExpression right = Expression.Constant(value, property.PropertyType); //value
                BinaryExpression be = Expression.Equal(left, right); //x=>x.name == value
                condition2 = condition2 is null ? be : Expression.And(condition2, be);
            }
        }

        Expression<Func<TEntity, bool>> pre2 = condition2 is null ? Expression.Lambda<Func<TEntity, bool>>(Expression.Constant(true), param) : Expression.Lambda<Func<TEntity, bool>>(condition2, param);

        return pre2;
    }

    /// <summary>
    /// 获取排序函数，基于 SortList 构建排序表达式
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <returns>用于对查询结果排序的函数，如果没有排序条件则返回 null</returns>
    public Func<IQueryable<T>, IOrderedQueryable<T>>? GetOrderBy<T>()
    {
        return ExpressionHelper.GetOrderBy<T>(SortList);
    }

    /// <summary>
    /// 获取基于搜索文本的表达式。
    /// 在实体的所有字符串属性中进行模糊匹配（使用 Contains）
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <returns>用于文本搜索的表达式树</returns>
    public Expression<Func<TEntity, bool>> GetSearchTextExpression<TEntity>()
    {
        PropertyInfo[] properties = typeof(TEntity).GetProperties();
        var text = SearchText;
        Expression? condition = null;
        ParameterExpression param = Expression.Parameter(typeof(TEntity), "x");

        Expression<Func<TEntity, bool>> pre;

        if (text is not null)
        {
            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType != typeof(string))
                {
                    continue;
                }

                if (property.Name == "Id")
                {
                    continue;
                }

                MemberExpression left = Expression.Property(param, property); //x.name
                MethodInfo? method = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)]);
                ConstantExpression right = Expression.Constant(text, property.PropertyType); //value
                MethodCallExpression be = Expression.Call(left, method!, right);

                if (condition is null)
                {
                    condition = be;
                }
                else
                {
                    condition = Expression.Or(condition, be);
                }
            }

            // If no string properties were found, return a true condition
            pre = condition is not null
                ? Expression.Lambda<Func<TEntity, bool>>(condition, param)
                : Expression.Lambda<Func<TEntity, bool>>(Expression.Constant(true), param);
        }
        else
        {
            pre = Expression.Lambda<Func<TEntity, bool>>(Expression.Constant(true), param);
        }

        return pre;
    }

    /// <summary>
    /// 获取组合的搜索表达式。
    /// 将文本搜索表达式和属性匹配表达式使用 AND 逻辑组合
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <returns>组合后的搜索表达式树</returns>
    public Expression<Func<TEntity, bool>> GetSearchExpression<TEntity>()
    {
        Expression<Func<TEntity, bool>> pre = GetSearchTextExpression<TEntity>();
        Expression<Func<TEntity, bool>> pre2 = GetSearchModelExpression<TEntity>();

        return pre.And(pre2);
    }
}