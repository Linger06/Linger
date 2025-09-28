using System.Linq.Expressions;
using System.Reflection;
using Linger.Extensions;
using Linger.Extensions.Core;
using Linger.Helper;

namespace Linger.SharedKernel;

public class BaseSearch : IBaseSearch
{
    public string? SearchText { get; set; }

    /// <summary>
    ///     复杂查询条件
    /// </summary>
    public List<Condition> SearchParameters { get; set; } = [];

    public List<Condition> SearchOrParameters { get; set; } = [];
    public string? Sorting { get; set; }
    public List<SortInfo>? SortList { get; set; }

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
                if (value == null)
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
                condition2 = condition2 == null ? be : Expression.And(condition2, be);
            }
        }

        Expression<Func<TEntity, bool>> pre2 = condition2 == null ? Expression.Lambda<Func<TEntity, bool>>(Expression.Constant(true), param) : Expression.Lambda<Func<TEntity, bool>>(condition2, param);

        return pre2;
    }

    public Func<IQueryable<T>, IOrderedQueryable<T>>? GetOrderBy<T>()
    {
        return ExpressionHelper.GetOrderBy<T>(SortList);
    }

    public Expression<Func<TEntity, bool>> GetSearchTextExpression<TEntity>()
    {
        PropertyInfo[] properties = typeof(TEntity).GetProperties();
        var text = SearchText;
        Expression? condition = null;
        ParameterExpression param = Expression.Parameter(typeof(TEntity), "x");

        Expression<Func<TEntity, bool>> pre;

        if (text != null)
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
                MethodInfo? method = typeof(string).GetMethod("Contains", [typeof(string)]);
                ConstantExpression right = Expression.Constant(text, property.PropertyType); //value
                MethodCallExpression be = Expression.Call(left, method!, right);

                if (condition == null)
                {
                    condition = be;
                }
                else
                {
                    condition = Expression.Or(condition, be);
                }

                //if (property.PropertyType == typeof(int))
                //{
                //    //https://www.cnblogs.com/loverwangshan/p/10254730.html
                //    var left = Expression.Property(param, property); //x.name
                //    //得到ToString方法
                //    MethodInfo toStringWay = typeof(int).GetMethod("ToString", new Type[] { });
                //    //得到IndexOf的方法，然后new Type[]这个代表是得到参数为string的一个方法
                //    MethodInfo indexOfWay = typeof(string).GetMethod("IndexOf", new Type[] { typeof(string) });
                //    //通过下面方法得到x.Id.ToString()
                //    MethodCallExpression toStringResult = Expression.Call(left, toStringWay, new Expression[] { });

                //    var right = Expression.Constant(text); //value
                //    //通过下面方法得到x.Id.ToString().IndexOf("5") ,MethodCallExpression继承于Expression
                //    MethodCallExpression indexOfResult = Expression.Call(toStringResult, indexOfWay, new Expression[] { right });

                //    //x.Id.ToString().IndexOf("5")>=0
                //    var lambdaBody = Expression.GreaterThanOrEqual(indexOfResult, Expression.Constant(0));

                //    if (condition == null)
                //    {
                //        condition = lambdaBody;
                //    }
                //    else
                //    {
                //        condition = Expression.Or(condition, lambdaBody);
                //    }
                //}
            }

            pre = Expression.Lambda<Func<TEntity, bool>>(condition!, param);
        }
        else
        {
            pre = Expression.Lambda<Func<TEntity, bool>>(Expression.Constant(true), param);
        }

        return pre;
    }

    public Expression<Func<TEntity, bool>> GetSearchExpression<TEntity>()
    {
        Expression<Func<TEntity, bool>> pre = GetSearchTextExpression<TEntity>();
        Expression<Func<TEntity, bool>> pre2 = GetSearchModelExpression<TEntity>();

        return pre.And(pre2);
    }
}