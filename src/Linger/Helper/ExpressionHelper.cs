using System.Linq.Expressions;
using System.Reflection;
using Linger.Enums;
using Linger.Extensions.Core;

namespace Linger.Helper;

/// <summary>
/// Helper class for creating and manipulating expressions.
/// <example>
/// <code>
/// var expression = ExpressionHelper.True&lt;MyClass&gt;();
/// // Returns: p => true
/// </code>
/// </example>
/// </summary>
public static class ExpressionHelper
{
    /// <summary>
    /// Creates a lambda expression that always returns true.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <returns>A lambda expression.</returns>
    /// <example>
    /// <code>
    /// var expression = ExpressionHelper.True&lt;MyClass&gt;();
    /// // Returns: p => true
    /// </code>
    /// </example>
    public static Expression<Func<T, bool>> True<T>()
    {
        return p => true;
    }

    /// <summary>
    /// Creates a lambda expression that always returns false.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <returns>A lambda expression.</returns>
    /// <example>
    /// <code>
    /// var expression = ExpressionHelper.False&lt;MyClass&gt;();
    /// // Returns: p => false
    /// </code>
    /// </example>
    public static Expression<Func<T, bool>> False<T>()
    {
        return p => false;
    }

    /// <summary>
    /// Creates a lambda expression for ordering by a specified property.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <typeparam name="TKey">The type of the property.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>A lambda expression.</returns>
    /// <example>
    /// <code>
    /// var expression = ExpressionHelper.GetOrderExpression&lt;MyClass, int&gt;("MyProperty");
    /// // Returns: p => p.MyProperty
    /// </code>
    /// </example>
    public static Expression<Func<T, TKey>> GetOrderExpression<T, TKey>(string propertyName)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "p");
        return Expression.Lambda<Func<T, TKey>>(Expression.Property(parameter, propertyName), parameter);
    }

    /// <summary>
    /// Creates a lambda expression for ordering by a specified property.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>A lambda expression.</returns>
    /// <example>
    /// <code>
    /// var expression = ExpressionHelper.GetOrderExpression&lt;MyClass&gt;("MyProperty");
    /// // Returns: p => p.MyProperty
    /// </code>
    /// </example>
    private static LambdaExpression GetOrderExpression<T>(string propertyName)
    {
        ParameterExpression paramExpr = Expression.Parameter(typeof(T));
        MemberExpression propAccess = Expression.PropertyOrField(paramExpr, propertyName);
        LambdaExpression expr = Expression.Lambda(propAccess, paramExpr);
        return expr;
    }

    /// <summary>
    /// Orders an IEnumerable by a specified property and sort direction.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the IEnumerable.</typeparam>
    /// <param name="query">The IEnumerable to order.</param>
    /// <param name="name">The property name and sort direction.</param>
    /// <returns>An ordered IEnumerable.</returns>
    /// <example>
    /// <code>
    /// var orderedList = myList.OrderBy("MyProperty Asc");
    /// // Returns: Ordered list by MyProperty in ascending order
    /// </code>
    /// </example>
    public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> query, string name)
    {
        var sort = "OrderBy";
        string propertyName;
        if (name.Contains(' '))
        {
            var splitName = name.Split(' ');
            propertyName = splitName[0];
            sort = splitName[1];
            if (sort == "Asc")
            {
                sort = "OrderBy";
            }
            else
            {
                sort = "OrderByDescending";
            }
        }
        else
        {
            propertyName = name;
        }

        return query.OrderBy(propertyName, sort);
    }

    /// <summary>
    /// Orders an IEnumerable by a specified property and sort direction.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the IEnumerable.</typeparam>
    /// <param name="query">The IEnumerable to order.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="sort">The sort direction ("OrderBy" or "OrderByDescending").</param>
    /// <returns>An ordered IEnumerable.</returns>
    /// <example>
    /// <code>
    /// var orderedList = myList.OrderBy("MyProperty", "OrderByDescending");
    /// // Returns: Ordered list by MyProperty in descending order
    /// </code>
    /// </example>
    public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> query, string propertyName, string sort)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (string.IsNullOrEmpty(propertyName)) throw new ArgumentException("Property name cannot be null or empty", nameof(propertyName));
        if (string.IsNullOrEmpty(sort)) throw new ArgumentException("Sort direction cannot be null or empty", nameof(sort));

        try
        {
            LambdaExpression expr = GetOrderExpression<T>(propertyName);
            PropertyInfo propInfo = typeof(T).GetPropertyInfo(propertyName);
            MethodInfo method = typeof(Enumerable).GetMethods().First(m => m.Name == sort && m.GetParameters().Length == 2);
            MethodInfo genericMethod = method.MakeGenericMethod(typeof(T), propInfo.PropertyType);
            var orderBy = genericMethod.Invoke(null, [query, expr.Compile()]);

            return orderBy as IEnumerable<T> ?? throw new InvalidOperationException(
                $"Failed to sort by property '{propertyName}' using direction '{sort}'.");
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Failed to perform ordering operation on property '{propertyName}'.", ex);
        }
    }

    /// <summary>
    /// Creates a lambda expression for equality comparison.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="propertyValue">The value to compare.</param>
    /// <returns>A lambda expression.</returns>
    /// <example>
    /// <code>
    /// var expression = ExpressionHelper.CreateEqual&lt;MyClass&gt;("MyProperty", 5);
    /// // Returns: p => p.MyProperty == 5
    /// </code>
    /// </example>
    public static Expression<Func<T, bool>> CreateEqual<T>(string propertyName, object propertyValue)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "p");
        Expression expression = GetExpression(parameter, new Condition { Field = propertyName, Op = CompareOperator.Equals, Value = propertyValue });
        return Expression.Lambda<Func<T, bool>>(expression, parameter);
    }

    /// <summary>
    /// Creates a lambda expression for equality comparison between two properties.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="propertyName">The name of the first property.</param>
    /// <param name="propertyName2">The name of the second property.</param>
    /// <returns>A lambda expression.</returns>
    /// <example>
    /// <code>
    /// var expression = ExpressionHelper.CreateEqualProperty&lt;MyClass&gt;("Property1", "Property2");
    /// // Returns: p => p.Property1 == p.Property2
    /// </code>
    /// </example>
    public static Expression<Func<T, bool>> CreateEqualProperty<T>(string propertyName, string propertyName2)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "p");
        MemberExpression member = Expression.PropertyOrField(parameter, propertyName);
        MemberExpression member2 = Expression.PropertyOrField(parameter, propertyName2);

        return Expression.Lambda<Func<T, bool>>(Expression.Equal(member, member2), parameter);
    }

    /// <summary>
    /// Creates a lambda expression for inequality comparison between two properties.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="propertyName">The name of the first property.</param>
    /// <param name="propertyName2">The name of the second property.</param>
    /// <returns>A lambda expression.</returns>
    /// <example>
    /// <code>
    /// var expression = ExpressionHelper.CreateNotEqualProperty&lt;MyClass&gt;("Property1", "Property2");
    /// // Returns: p => p.Property1 != p.Property2
    /// </code>
    /// </example>
    public static Expression<Func<T, bool>> CreateNotEqualProperty<T>(string propertyName, string propertyName2)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "p");
        MemberExpression member = Expression.PropertyOrField(parameter, propertyName);
        MemberExpression member2 = Expression.PropertyOrField(parameter, propertyName2);

        return Expression.Lambda<Func<T, bool>>(Expression.NotEqual(member, member2), parameter);
    }

    /// <summary>
    /// Creates a lambda expression for inequality comparison.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="propertyValue">The value to compare.</param>
    /// <returns>A lambda expression.</returns>
    /// <example>
    /// <code>
    /// var expression = ExpressionHelper.CreateNotEqual&lt;MyClass&gt;("MyProperty", "value");
    /// // Returns: p => p.MyProperty != "value"
    /// </code>
    /// </example>
    public static Expression<Func<T, bool>> CreateNotEqual<T>(string propertyName, string propertyValue)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "p");
        Expression expression = GetExpression(parameter, new Condition { Field = propertyName, Op = CompareOperator.NotEquals, Value = propertyValue });
        return Expression.Lambda<Func<T, bool>>(expression, parameter);
    }

    /// <summary>
    /// Creates a lambda expression for greater than comparison.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="propertyValue">The value to compare.</param>
    /// <returns>A lambda expression.</returns>
    /// <example>
    /// <code>
    /// var expression = ExpressionHelper.CreateGreaterThan&lt;MyClass&gt;("MyProperty", "value");
    /// // Returns: p => p.MyProperty > "value"
    /// </code>
    /// </example>
    public static Expression<Func<T, bool>> CreateGreaterThan<T>(string propertyName, string propertyValue)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "p");
        Expression expression = GetExpression(parameter, new Condition { Field = propertyName, Op = CompareOperator.GreaterThan, Value = propertyValue });
        return Expression.Lambda<Func<T, bool>>(expression, parameter);
    }

    /// <summary>
    /// Creates a lambda expression for less than comparison.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="propertyValue">The value to compare.</param>
    /// <returns>A lambda expression.</returns>
    /// <example>
    /// <code>
    /// var expression = ExpressionHelper.CreateLessThan&lt;MyClass&gt;("MyProperty", "value");
    /// // Returns: p => p.MyProperty &lt; "value"
    /// </code>
    /// </example>
    public static Expression<Func<T, bool>> CreateLessThan<T>(string propertyName, string propertyValue)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "p");
        Expression expression = GetExpression(parameter, new Condition { Field = propertyName, Op = CompareOperator.LessThan, Value = propertyValue });
        return Expression.Lambda<Func<T, bool>>(expression, parameter);
    }

    /// <summary>
    /// Creates a lambda expression for greater than or equal comparison.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="propertyValue">The value to compare.</param>
    /// <returns>A lambda expression.</returns>
    /// <example>
    /// <code>
    /// var expression = ExpressionHelper.CreateGreaterThanOrEqual&lt;MyClass&gt;("MyProperty", "value");
    /// // Returns: p => p.MyProperty >= "value"
    /// </code>
    /// </example>
    public static Expression<Func<T, bool>> CreateGreaterThanOrEqual<T>(string propertyName, string propertyValue)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "p");
        Expression expression = GetExpression(parameter, new Condition { Field = propertyName, Op = CompareOperator.GreaterThanOrEquals, Value = propertyValue });
        return Expression.Lambda<Func<T, bool>>(expression, parameter);
    }

    /// <summary>
    /// Creates a lambda expression for less than or equal comparison.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="propertyValue">The value to compare.</param>
    /// <returns>A lambda expression.</returns>
    /// <example>
    /// <code>
    /// var expression = ExpressionHelper.CreateLessThanOrEqual&lt;MyClass&gt;("MyProperty", "value");
    /// // Returns: p => p.MyProperty <![CDATA[<=]]> "value"
    /// </code>
    /// </example>
    public static Expression<Func<T, bool>> CreateLessThanOrEqual<T>(string propertyName, string propertyValue)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "p");
        Expression expression = GetExpression(parameter, new Condition { Field = propertyName, Op = CompareOperator.LessThanOrEquals, Value = propertyValue });
        return Expression.Lambda<Func<T, bool>>(expression, parameter);
    }

    /// <summary>
    /// Creates a lambda expression for string containment.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="propertyValue">The value to check for containment.</param>
    /// <returns>A lambda expression.</returns>
    /// <example>
    /// <code>
    /// var expression = ExpressionHelper.GetContains&lt;MyClass&gt;("MyProperty", "value");
    /// // Returns: p => p.MyProperty.Contains("value")
    /// </code>
    /// </example>
    public static Expression<Func<T, bool>> GetContains<T>(string propertyName, string propertyValue)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "p");
        Expression expression = GetExpression(parameter, new Condition { Field = propertyName, Op = CompareOperator.Contains, Value = propertyValue });
        return Expression.Lambda<Func<T, bool>>(expression, parameter);
    }

    /// <summary>
    /// Creates a lambda expression for string non-containment.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="propertyValue">The value to check for non-containment.</param>
    /// <returns>A lambda expression.</returns>
    /// <example>
    /// <code>
    /// var expression = ExpressionHelper.GetNotContains&lt;MyClass&gt;("MyProperty", "value");
    /// // Returns: p => !p.MyProperty.Contains("value")
    /// </code>
    /// </example>
    public static Expression<Func<T, bool>> GetNotContains<T>(string propertyName, string propertyValue)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "p");
        Expression expression = GetExpression(parameter, new Condition { Field = propertyName, Op = CompareOperator.NotContains, Value = propertyValue });
        return Expression.Lambda<Func<T, bool>>(expression, parameter);
    }

    /// <summary>
    /// Creates a lambda expression for array containment.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <typeparam name="TKey">The type of the array elements.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="arrayValue">The array to check for containment.</param>
    /// <returns>A lambda expression.</returns>
    /// <example>
    /// <code>
    /// var expression = ExpressionHelper.GetContains&lt;MyClass, int&gt;("MyProperty", new int[] {1, 2, 3});
    /// // Returns: p => new int[] {1, 2, 3}.Contains(p.MyProperty)
    /// </code>
    /// </example>
    public static Expression<Func<T, bool>> GetIn<T, TKey>(string propertyName, TKey[] arrayValue)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "p");
        Expression expression = GetExpression(parameter, new Condition { Field = propertyName, Op = CompareOperator.StdIn, Value = arrayValue });
        return Expression.Lambda<Func<T, bool>>(expression, parameter);
    }

    /// <summary>
    /// Creates a lambda expression for array non-containment.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <typeparam name="TKey">The type of the array elements.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="arrayValue">The array to check for non-containment.</param>
    /// <returns>A lambda expression.</returns>
    /// <example>
    /// <code>
    /// var expression = ExpressionHelper.GetNotIn&lt;MyClass, int&gt;("MyProperty", new int[] {1, 2, 3});
    /// // Returns: p => !new int[] {1, 2, 3}.Contains(p.MyProperty)
    /// </code>
    /// </example>
    public static Expression<Func<T, bool>> GetNotIn<T, TKey>(string propertyName, TKey[] arrayValue)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "p");
        Expression expression = GetExpression(parameter, new Condition { Field = propertyName, Op = CompareOperator.StdNotIn, Value = arrayValue });
        return Expression.Lambda<Func<T, bool>>(expression, parameter);
    }

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
            var props = columnName.Split('.');
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

        if (!conditions.Any())
        {
            return x => true;
        }

        ParameterExpression parameter = Expression.Parameter(typeof(T), "x");

        var simpleExps = conditions
            .ToList()
            .FindAll(c => string.IsNullOrEmpty(c.OrGroup))
            .Select(c => GetExpression(parameter, c))
            .ToList();

        var complexExps = conditions
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

        if (!conditions.Any())
        {
            return x => true;
        }

        ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
        var simpleExps = conditions
            .Select(c => GetExpression(parameter, c));

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

        if (!conditions.Any())
        {
            return x => true;
        }

        ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
        var simpleExps = conditions
            .Select(c => GetExpression(parameter, c));

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

        var propertyInfo = propertyParam.Member as PropertyInfo ?? throw new MissingMemberException(nameof(Condition), condition.Field);
        Type realPropertyType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
        if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            propertyParam = Expression.Property(propertyParam, "Value");
        }

        if (condition.Op is not CompareOperator.StdIn and not CompareOperator.StdNotIn)
        {
            condition.Value = Convert.ChangeType(condition.Value, realPropertyType, ExtensionMethodSetting.DefaultCulture);
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

/// <summary>
/// Represents a query condition.
/// </summary>
/// <example>
/// <code>
/// var condition = new Condition { Field = "MyField", Op = CompareOperator.Equals, Value = "MyValue" };
/// </code>
/// </example>
[Serializable]
public class Condition
{
    /// <summary>
    /// Gets or sets the field name.
    /// </summary>
    /// <value>The name of the field.</value>
    public string Field { get; set; } = null!;

    /// <summary>
    /// Gets or sets the comparison operator.
    /// </summary>
    /// <value>The comparison operator.</value>
    public CompareOperator Op { get; set; }

    /// <summary>
    /// Gets or sets the field value.
    /// </summary>
    /// <value>The value of the field.</value>
    public object? Value { get; set; }

    /// <summary>
    /// Gets or sets the group name for OR conditions.
    /// </summary>
    /// <value>The group name for OR conditions.</value>
    public string? OrGroup { get; set; }
}
