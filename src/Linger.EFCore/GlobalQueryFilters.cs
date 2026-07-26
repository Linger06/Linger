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
            if (typeof(TInterface).IsAssignableFrom(entityType.ClrType))
            {
                var filter = CreateEntityFilter(entityType.ClrType, expression);
                ApplyFilter(modelBuilder, entityType, filter);
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
            IMutableProperty? foundProperty = entityType.FindProperty(propertyName);
            if (foundProperty is not null && foundProperty.ClrType == typeof(T))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "entity");
                var filterBody = Expression.Equal(
                    Expression.Property(parameter, propertyName),
                    valueExpression.Body);
                var filter = Expression.Lambda(filterBody, parameter);
                ApplyFilter(modelBuilder, entityType, filter);
            }
        }
    }

    private static LambdaExpression CreateEntityFilter(Type entityType, LambdaExpression expression)
    {
        var parameter = Expression.Parameter(entityType, "entity");
        var body = ReplacingExpressionVisitor.Replace(
            expression.Parameters.Single(),
            parameter,
            expression.Body);

        return Expression.Lambda(body, parameter);
    }

    private static void ApplyFilter(
        ModelBuilder modelBuilder,
        IMutableEntityType entityType,
        LambdaExpression filter)
    {
#if NET10_0_OR_GREATER
        var existingFilter = entityType
            .GetDeclaredQueryFilters()
            .SingleOrDefault(queryFilter => queryFilter.IsAnonymous)
            ?.Expression;
#else
        var existingFilter = entityType.GetQueryFilter();
#endif
        if (existingFilter is null)
        {
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            return;
        }

        var parameter = Expression.Parameter(entityType.ClrType, "entity");
        var existingBody = ReplacingExpressionVisitor.Replace(
            existingFilter.Parameters.Single(),
            parameter,
            existingFilter.Body);
        var newBody = ReplacingExpressionVisitor.Replace(
            filter.Parameters.Single(),
            parameter,
            filter.Body);

        modelBuilder.Entity(entityType.ClrType).HasQueryFilter(
            Expression.Lambda(Expression.AndAlso(existingBody, newBody), parameter));
    }
}
