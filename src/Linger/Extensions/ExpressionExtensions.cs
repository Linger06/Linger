using System.Linq.Expressions;

namespace Linger.Extensions;

/// <summary>
/// Provides extension methods for combining and manipulating expressions.
/// </summary>
public static class ExpressionExtensions
{
    /// <summary>
    /// Combines the original expression with a new expression using AndAlso if the condition is met.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="left">The original expression.</param>
    /// <param name="need">The condition to determine if the new expression should be combined.</param>
    /// <param name="right">The new expression to combine.</param>
    /// <returns>The combined expression if the condition is met; otherwise, the original expression.</returns>
    /// <example>
    /// <code>
    /// var expr1 = x => x > 5;
    /// var expr2 = x => x &lt; 10;
    /// var result = expr1.AndIf(true, expr2);
    /// // result: x => (x > 5) <![CDATA[&&]]> (x &lt; 10)
    /// </code>
    /// </example>
    public static Expression<Func<T, bool>> AndIf<T>(this Expression<Func<T, bool>> left, bool need,
        Expression<Func<T, bool>> right)
    {
        return need ? left.And(right) : left;
    }

    /// <summary>
    /// Combines the original expression with a new expression using OrElse if the condition is met.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="left">The original expression.</param>
    /// <param name="need">The condition to determine if the new expression should be combined.</param>
    /// <param name="right">The new expression to combine.</param>
    /// <returns>The combined expression if the condition is met; otherwise, the original expression.</returns>
    /// <example>
    /// <code>
    /// var expr1 = x => x > 5;
    /// var expr2 = x => x &lt; 10;
    /// var result = expr1.OrIf(true, expr2);
    /// // result: x => (x > 5) || (x &lt; 10)
    /// </code>
    /// </example>
    public static Expression<Func<T, bool>> OrIf<T>(this Expression<Func<T, bool>> left, bool need,
        Expression<Func<T, bool>> right)
    {
        return need ? left.Or(right) : left;
    }

    /// <summary>
    /// Combines two expressions using AndAlso.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="first">The first expression.</param>
    /// <param name="second">The second expression.</param>
    /// <returns>The combined expression.</returns>
    /// <example>
    /// <code>
    /// var expr1 = x => x > 5;
    /// var expr2 = x => x &lt; 10;
    /// var result = expr1.And(expr2);
    /// // result: x => (x > 5) <![CDATA[&&]]> (x &lt; 10)
    /// </code>
    /// </example>
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first,
        Expression<Func<T, bool>> second)
    {
        return first.CombineExpressions(second, Expression.AndAlso);
    }

    /// <summary>
    /// Combines two expressions using OrElse.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="first">The first expression.</param>
    /// <param name="second">The second expression.</param>
    /// <returns>The combined expression.</returns>
    /// <example>
    /// <code>
    /// var expr1 = x => x > 5;
    /// var expr2 = x => x &lt; 10;
    /// var result = expr1.Or(expr2);
    /// // result: x => (x > 5) || (x &lt; 10)
    /// </code>
    /// </example>
    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first,
        Expression<Func<T, bool>> second)
    {
        return first.CombineExpressions(second, Expression.OrElse);
    }

    /// <summary>
    /// 将两个表达式组合成一个新的表达式，使用指定的合并函数。
    /// </summary>
    /// <typeparam name="T">表达式委托的类型。</typeparam>
    /// <param name="firstExpression">第一个表达式。</param>
    /// <param name="secondExpression">第二个表达式。</param>
    /// <param name="combineFunction">用于组合表达式的合并函数。</param>
    /// <returns>组合后的表达式。</returns>
    /// <example>
    /// <code>
    /// var expr1 = x => x > 5;
    /// var expr2 = x => x &lt; 10;
    /// var result = expr1.CombineExpressions(expr2, Expression.AndAlso);
    /// // result: x => (x > 5) <![CDATA[&&]]> (x &lt; 10)
    /// </code>
    /// </example>
    public static Expression<T> CombineExpressions<T>(this Expression<T> firstExpression, Expression<T> secondExpression,
        Func<Expression, Expression, Expression> combineFunction)
    {
        var parameterMap = firstExpression.Parameters.Select((firstParam, i) => new { firstParam, secondParam = secondExpression.Parameters[i] })
            .ToDictionary(p => p.secondParam, p => p.firstParam);
        Expression mappedSecondBody = ParameterRebindVisitor.ReplaceParameters(parameterMap, secondExpression.Body);
        return Expression.Lambda<T>(combineFunction(firstExpression.Body, mappedSecondBody), firstExpression.Parameters);
    }

#if !NETFRAMEWORK || NET40_OR_GREATER

    /// <summary>
    /// Determines whether the specified <see cref="IQueryable{T}"/> has an OrderBy clause.    
    /// </summary>
    /// <typeparam name="T">The type of the elements of source.</typeparam>
    /// <param name="query">The query to check.</param>
    /// <returns><c>true</c> if the query has an OrderBy clause; otherwise, <c>false</c>.</returns>
    /// <example>
    /// <code>
    /// IQueryable&lt;int> query = new List&lt;int>().AsQueryable();
    /// bool hasOrderBy = query.HasOrderBy();
    /// // hasOrderBy: false
    /// </code>
    /// </example>
    public static bool HasOrderBy<T>(this IQueryable<T> query)
    {
        if (query == null)
        {
            return false;
        }
        
        // 不能直接依赖IOrderedQueryable接口检查，因为许多LINQ提供程序都实现了这个接口
        // 即使查询没有排序操作
        // if (query is IOrderedQueryable)
        // {
        //     return true;
        // }

        // 通过表达式树分析，查找排序方法调用
        var visitor = new OrderByVisitor();
        visitor.Visit(query.Expression);
        return visitor.HasOrderBy;
    }

    /// <summary>
    /// Replaces parameters in an expression with the specified map.
    /// </summary>
    private class ParameterRebindVisitor : ExpressionVisitor
    {
        /// <summary>
        /// The ParameterExpression map.
        /// </summary>
        private readonly Dictionary<ParameterExpression, ParameterExpression> _map;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterRebindVisitor"/> class.
        /// </summary>
        /// <param name="map">The map of parameters to replace.</param>
        private ParameterRebindVisitor(Dictionary<ParameterExpression, ParameterExpression>? map)
        {
            _map = map ?? [];
        }

        /// <summary>
        /// Replaces the parameters in the specified expression.
        /// </summary>
        /// <param name="map">The map of parameters to replace.</param>
        /// <param name="exp">The expression in which to replace parameters.</param>
        /// <returns>The expression with replaced parameters.</returns>
        /// <example>
        /// <code>
        /// var map = new Dictionary&lt;ParameterExpression, ParameterExpression&gt;();
        /// Expression exp = Expression.Constant(5);
        /// var result = ParameterRebindVisitor.ReplaceParameters(map, exp);
        /// // result: 5
        /// </code>
        /// </example>
        public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map,
            Expression exp)
        {
            return new ParameterRebindVisitor(map).Visit(exp);
        }

        /// <summary>
        /// Visits the parameter and replaces it if it exists in the map.
        /// </summary>
        /// <param name="p">The parameter to visit.</param>
        /// <returns>The visited expression.</returns>
        protected override Expression VisitParameter(ParameterExpression p)
        {
            if (_map.TryGetValue(p, out ParameterExpression? replacement))
            {
                p = replacement;
            }

            return base.VisitParameter(p);
        }
    }

    /// <summary>
    /// Visits expressions to determine if an OrderBy clause is present.
    /// </summary>
    private class OrderByVisitor : ExpressionVisitor
    {
        /// <summary>
        /// Gets a value indicating whether the query has an OrderBy clause.
        /// </summary>
        /// <value><c>true</c> if the query has an OrderBy clause; otherwise, <c>false</c>.</value>
        public bool HasOrderBy { get; private set; }

        /// <summary>
        /// Visits the method call expression and checks for OrderBy methods.
        /// </summary>
        /// <param name="node">The method call expression to visit.</param>
        /// <returns>The visited expression.</returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            // 如果已经找到排序操作，就不再继续查找
            if (HasOrderBy)
            {
                return node;
            }

            // 判断是否为 Queryable 的排序方法
            if (node.Method.DeclaringType == typeof(Queryable))
            {
                if (IsOrderByMethod(node.Method.Name))
                {
                    HasOrderBy = true;
                    return node;
                }
            }
            
            // 检查自定义扩展方法或 EF Core 扩展
            else if (node.Method.Name.Contains("OrderBy"))
            {
                HasOrderBy = true;
                return node;
            }

            return base.VisitMethodCall(node);
        }

        /// <summary>
        /// 判断方法名是否为排序相关方法
        /// </summary>
        /// <param name="methodName">方法名</param>
        /// <returns>如果是排序方法则返回 true，否则返回 false</returns>
        private static bool IsOrderByMethod(string methodName)
        {
            return methodName is "OrderBy" or "OrderByDescending" or "ThenBy" or "ThenByDescending";
        }
    }

#endif
}
