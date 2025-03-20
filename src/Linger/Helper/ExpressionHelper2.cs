using System.Linq.Expressions;
using System.Reflection;
using Linger.Enums;
using Linger.Extensions.Core;

namespace Linger.Helper;

/// <summary>  
/// Provides helper methods for building and compiling LINQ expressions.  
/// </summary>  
public static partial class ExpressionHelper
{
    /// <summary>  
    /// Generates a function to order a queryable collection based on a list of sort information.  
    /// </summary>  
    /// <typeparam name="T">The type of the elements in the queryable collection.</typeparam>  
    /// <param name="sortList">A list of <see cref="SortInfo"/> objects containing the sorting information.</param>  
    /// <returns>A function that orders a queryable collection, or null if the sort list is null or empty.</returns>  
    /// <example>  
    /// <code>  
    /// var sortList = new List&lt;SortInfo&gt; { new SortInfo { Property = "Name", Direction = SortDirection.Ascending } };  
    /// var orderByFunc = ExpressionHelper.GetOrderBy&lt;MyClass&gt;(sortList);  
    /// var orderedQueryable = orderByFunc(myQueryable);  
    /// </code>  
    /// </example>  
    public static Func<IQueryable<T>, IOrderedQueryable<T>>? GetOrderBy<T>(List<SortInfo>? sortList)
    {
        if (sortList == null)
            return null;

        if (sortList.Count == 0)
            return null;

        var propertyList = new List<string>();
        var dirList = new List<string>();
        foreach (SortInfo sortInfo in sortList)
        {
            var propertyName = sortInfo.Property;
            var dir = sortInfo.Direction.ToString();
            propertyList.Add(propertyName);
            dirList.Add(dir);
        }

        return GetOrderBy<T>(propertyList, dirList);
    }

    /// <summary>  
    /// Generates a function to order a queryable collection based on specified columns and directions.  
    /// </summary>  
    /// <typeparam name="T">The type of the elements in the queryable collection.</typeparam>  
    /// <param name="orderColumn">A list of column names to sort by.</param>  
    /// <param name="orderDir">A list of sort directions corresponding to the columns.</param>  
    /// <returns>A function that orders a queryable collection.</returns>  
    /// <example>  
    /// <code>  
    /// var orderColumns = new List&lt;string&gt; { "Name", "Age" };  
    /// var orderDirs = new List&lt;string&gt; { "asc", "desc" };  
    /// var orderByFunc = ExpressionHelper.GetOrderBy&lt;MyClass&gt;(orderColumns, orderDirs);  
    /// var orderedQueryable = orderByFunc(myQueryable);  
    /// </code>  
    /// </example>  
    public static Func<IQueryable<T>, IOrderedQueryable<T>>? GetOrderBy<T>(List<string> orderColumn, List<string> orderDir)
    {
        var ascKey = "OrderBy";
        var descKey = "OrderByDescending";

        Type typeQueryable = typeof(IQueryable<T>);
        ParameterExpression argQueryable = Expression.Parameter(typeQueryable, "jk");
        LambdaExpression outerExpression = Expression.Lambda(argQueryable, argQueryable);

        for (var i = 0; i < orderColumn.Count; i++)
        {
            var columnName = orderColumn[i];
            var dirKey = orderDir[i].ToLower();
            string[] props = columnName.Split('.');
            Type type = typeof(T);
            ParameterExpression arg = Expression.Parameter(type, "uf");
            Expression expr = arg;

            foreach (var prop in props)
            {
                PropertyInfo? pi = type.GetProperty(prop, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (pi == null)
                {
                    throw new InvalidOperationException(nameof(pi));
                }

                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }

            LambdaExpression lambda = Expression.Lambda(expr, arg);
            var methodName = dirKey == "asc" ? ascKey : descKey;
            MethodCallExpression resultExp = Expression.Call(typeof(Queryable), methodName, [typeof(T), type], outerExpression.Body, Expression.Quote(lambda));

            outerExpression = Expression.Lambda(resultExp, argQueryable);

            ascKey = "ThenBy";
            descKey = "ThenByDescending";
        }

        return (Func<IQueryable<T>, IOrderedQueryable<T>>?)outerExpression.Compile();
    }

    /// <summary>  
    /// Builds a lambda expression for filtering a queryable collection based on a set of conditions.  
    /// </summary>  
    /// <typeparam name="T">The type of the elements in the queryable collection.</typeparam>  
    /// <param name="conditions">A collection of <see cref="Condition"/> objects representing the filter conditions.</param>  
    /// <returns>A lambda expression for filtering the collection.</returns>  
    /// <example>  
    /// <code>  
    /// var conditions = new List&lt;Condition&gt; { new Condition { Field = "Name", Op = CompareOperator.Equals, Value = "John" } };  
    /// var filterLambda = ExpressionHelper.BuildLambda&lt;MyClass&gt;(conditions);  
    /// var filteredQueryable = myQueryable.Where(filterLambda);  
    /// </code>  
    /// </example>  
    public static Expression<Func<T, bool>> BuildLambda<T>(IEnumerable<Condition>? conditions)
    {
        if (conditions.IsNull())
        {
            return x => true;
        }

        IEnumerable<Condition> iEnumerable = conditions.ToList();
        if (!iEnumerable.Any())
        {
            return x => true;
        }

        ParameterExpression parameter = Expression.Parameter(typeof(T), "x");

        var simpleExps = iEnumerable
            .ToList()
            .FindAll(c => string.IsNullOrEmpty(c.OrGroup))
            .Select(c => GetExpression(parameter, c))
            .ToList();

        var complexExps = iEnumerable
            .ToList()
            .FindAll(c => !string.IsNullOrEmpty(c.OrGroup))
            .GroupBy(x => x.OrGroup)
            .Select(g => GetGroupExpression(parameter, g.ToList()))
            .ToList();

        Expression exp = simpleExps.Concat(complexExps)
            .Aggregate((left, right) => left.IsNull() ? right : Expression.AndAlso(left, right));
        return Expression.Lambda<Func<T, bool>>(exp, parameter);
    }

    /// <summary>  
    /// Builds a lambda expression for filtering a queryable collection using the AndAlso operator.  
    /// </summary>  
    /// <typeparam name="T">The type of the elements in the queryable collection.</typeparam>  
    /// <param name="conditions">A collection of <see cref="Condition"/> objects representing the filter conditions.</param>  
    /// <returns>A lambda expression for filtering the collection using the AndAlso operator.</returns>  
    /// <example>  
    /// <code>  
    /// var conditions = new List&lt;Condition&gt; { new Condition { Field = "Age", Op = CompareOperator.GreaterThan, Value = 30 } };  
    /// var filterLambda = ExpressionHelper.BuildAndAlsoLambda&lt;MyClass&gt;(conditions);  
    /// var filteredQueryable = myQueryable.Where(filterLambda);  
    /// </code>  
    /// </example>  
    public static Expression<Func<T, bool>> BuildAndAlsoLambda<T>(IEnumerable<Condition>? conditions)
    {
        if (conditions.IsNull())
        {
            return x => true;
        }

        IEnumerable<Condition> iEnumerable = conditions.ToList();
        if (!iEnumerable.Any())
        {
            return x => true;
        }

        ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
        var simpleExps = iEnumerable
            .ToList()
            .Select(c => GetExpression(parameter, c))
            .ToList();

        Expression exp = simpleExps.Aggregate((left, right) => left.IsNull() ? right : Expression.AndAlso(left, right));
        return Expression.Lambda<Func<T, bool>>(exp, parameter);
    }

    /// <summary>  
    /// Builds a lambda expression for filtering a queryable collection using the OrElse operator.  
    /// </summary>  
    /// <typeparam name="T">The type of the elements in the queryable collection.</typeparam>  
    /// <param name="conditions">A collection of <see cref="Condition"/> objects representing the filter conditions.</param>  
    /// <returns>A lambda expression for filtering the collection using the OrElse operator.</returns>  
    /// <example>  
    /// <code>  
    /// var conditions = new List&lt;Condition&gt; { new Condition { Field = "Age", Op = CompareOperator.LessThan, Value = 20 } };  
    /// var filterLambda = ExpressionHelper.BuildOrElseLambda&lt;MyClass&gt;(conditions);  
    /// var filteredQueryable = myQueryable.Where(filterLambda);  
    /// </code>  
    /// </example>  
    public static Expression<Func<T, bool>> BuildOrElseLambda<T>(IEnumerable<Condition>? conditions)
    {
        if (conditions.IsNull())
        {
            return x => true;
        }

        IEnumerable<Condition> iEnumerable = conditions.ToList();
        if (!iEnumerable.Any())
        {
            return x => true;
        }

        ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
        var simpleExps = iEnumerable
            .ToList()
            .Select(c => GetExpression(parameter, c))
            .ToList();

        Expression exp = simpleExps.Aggregate((left, right) => left.IsNull() ? right : Expression.OrElse(left, right));
        return Expression.Lambda<Func<T, bool>>(exp, parameter);
    }

    /// <summary>  
    /// Generates an expression for a single condition.  
    /// </summary>  
    /// <param name="parameter">The parameter expression representing the element.</param>  
    /// <param name="condition">The condition to be applied.</param>  
    /// <returns>An expression representing the condition.</returns>  
    private static Expression GetExpression(Expression parameter, Condition condition)
    {
        MemberExpression propertyParam = Expression.Property(parameter, condition.Field);

        var propertyInfo = propertyParam.Member as PropertyInfo;
        if (propertyInfo == null)
        {
            throw new MissingMemberException(nameof(Condition), condition.Field);
        }

        Type realPropertyType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
        if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            propertyParam = Expression.Property(propertyParam, "Value");
        }

        if (condition.Op is not CompareOperator.StdIn and not CompareOperator.StdNotIn)
        {
            condition.Value = Convert.ChangeType(condition.Value, realPropertyType);
        }
        else
        {
            if (condition.Value != null)
            {
                Type typeOfValue = condition.Value.GetType();
                Type typeOfList = typeof(IEnumerable<>).MakeGenericType(realPropertyType);
                if (typeOfValue.IsGenericType && typeOfList.IsAssignableFrom(typeOfValue))
                {
                    condition.Value = typeof(Enumerable)
                        .GetMethod("ToArray", BindingFlags.Public | BindingFlags.Static)
                        ?.MakeGenericMethod(realPropertyType)
                        .Invoke(null, [condition.Value]);
                }
            }
        }

        ConstantExpression constantParam = Expression.Constant(condition.Value);
        return condition.Op switch
        {
            CompareOperator.Equals => Expression.Equal(propertyParam, constantParam),
            CompareOperator.NotEquals => Expression.NotEqual(propertyParam, constantParam),
            CompareOperator.Contains => Expression.Call(propertyParam, "Contains", null, constantParam),
            CompareOperator.NotContains => Expression.Not(Expression.Call(propertyParam, "Contains", null, constantParam)),
            CompareOperator.StartsWith => Expression.Call(propertyParam, "StartsWith", null, constantParam),
            CompareOperator.EndsWith => Expression.Call(propertyParam, "EndsWith", null, constantParam),
            CompareOperator.GreaterThan => Expression.GreaterThan(propertyParam, constantParam),
            CompareOperator.GreaterThanOrEquals => Expression.GreaterThanOrEqual(propertyParam, constantParam),
            CompareOperator.LessThan => Expression.LessThan(propertyParam, constantParam),
            CompareOperator.LessThanOrEquals => Expression.LessThanOrEqual(propertyParam, constantParam),
            CompareOperator.StdIn => Expression.Call(typeof(Enumerable), "Contains", [realPropertyType], constantParam, propertyParam),
            CompareOperator.StdNotIn => Expression.Not(Expression.Call(typeof(Enumerable), "Contains", [realPropertyType
            ], constantParam, propertyParam)),
            _ => throw new NotSupportedException($"{condition.Op} Not Supported")
        };
    }

    /// <summary>  
    /// Generates an expression for a group of conditions combined with the OrElse operator.  
    /// </summary>  
    /// <param name="parameter">The parameter expression representing the element.</param>  
    /// <param name="orConditions">A collection of conditions to be combined.</param>  
    /// <returns>An expression representing the combined conditions.</returns>  
    private static Expression GetGroupExpression(Expression parameter, IEnumerable<Condition> orConditions)
    {
        var exps = orConditions.Select(c => GetExpression(parameter, c)).ToList();
        return exps.Aggregate((left, right) => left.IsNull() ? right : Expression.OrElse(left, right));
    }
}
