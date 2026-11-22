using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace Linger.EFCore;

/// <summary>
/// Applies global query filters to matching entity types.
/// </summary>
public static class GlobalQueryFilters
{
    /// <summary>
    /// Applies a filter to every entity type assignable to <typeparamref name="TInterface"/>.
    /// Existing filters are combined with the supplied filter.
    /// </summary>
    /// <typeparam name="TInterface">The interface or base type used to select entities.</typeparam>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="expression">The filter expression.</param>
    public static void ApplyGlobalFilters<TInterface>(this ModelBuilder modelBuilder, Expression<Func<TInterface, bool>> expression)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        ArgumentNullException.ThrowIfNull(expression);

        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(TInterface).IsAssignableFrom(entityType.ClrType) &&
                (entityType.BaseType is null || !typeof(TInterface).IsAssignableFrom(entityType.BaseType.ClrType)))
            {
                ApplyFilter(modelBuilder, entityType, expression);
            }
        }
    }

    /// <summary>
    /// Applies an equality filter to entities that contain a property with the specified name and type.
    /// Existing filters are combined with the generated filter.
    /// </summary>
    /// <typeparam name="T">The property value type.</typeparam>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="valueExpression">
    /// An expression that obtains the current filter value. Use a captured context member when the value varies by context instance.
    /// </param>
    public static void ApplyGlobalFilters<T>(
        this ModelBuilder modelBuilder,
        string propertyName,
        Expression<Func<T>> valueExpression)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        ArgumentNullException.ThrowIfNull(valueExpression);

        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            IMutableProperty? foundProperty = entityType.FindDeclaredProperty(propertyName);
            if (foundProperty is not null && foundProperty.ClrType == typeof(T))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "entity");
                var filterBody = Expression.Equal(
                    Expression.Call(typeof(EF), nameof(EF.Property), [typeof(T)], parameter, Expression.Constant(propertyName)),
                    valueExpression.Body);
                var filter = Expression.Lambda(filterBody, parameter);
                ApplyFilter(modelBuilder, entityType, filter);
            }
        }
    }

    private static void ApplyFilter(
        ModelBuilder modelBuilder,
        IMutableEntityType entityType,
        LambdaExpression filter)
    {
        // EF 只允许在根实体上设置过滤器，其他分支不应被派生类型的条件排除。
        var root = entityType.GetRootType();
        var parameter = Expression.Parameter(root.ClrType, "entity");
        Expression entity = entityType == root ? parameter : Expression.Convert(parameter, entityType.ClrType);
        var body = ReplacingExpressionVisitor.Replace(filter.Parameters.Single(), entity, filter.Body);
        if (entityType != root)
        {
            body = Expression.OrElse(Expression.Not(Expression.TypeIs(parameter, entityType.ClrType)), body);
        }

#if NET10_0_OR_GREATER
        var existingFilter = root
            .GetDeclaredQueryFilters()
            .SingleOrDefault(queryFilter => queryFilter.IsAnonymous)
            ?.Expression;
#else
        var existingFilter = root.GetQueryFilter();
#endif
        if (existingFilter is not null)
        {
            var existingBody = ReplacingExpressionVisitor.Replace(existingFilter.Parameters.Single(), parameter, existingFilter.Body);
            body = Expression.AndAlso(existingBody, body);
        }

        modelBuilder.Entity(root.ClrType).HasQueryFilter(Expression.Lambda(body, parameter));
    }
}
