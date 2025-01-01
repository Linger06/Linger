using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace Linger.EFCore;

/// <summary>
/// Apply global query filters automatically to all entities which implement an interface or have a particular property
/// </summary>
public static class GlobalQueryFilters
{
    public static void ApplyGlobalFilters<TInterface>(this ModelBuilder modelBuilder, Expression<Func<TInterface, bool>> expression)
    {
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.GetInterface(typeof(TInterface).Name) != null)
            {
                ParameterExpression newParam = Expression.Parameter(entityType.ClrType);
                Expression newBody = ReplacingExpressionVisitor.
                    Replace(expression.Parameters.Single(), newParam, expression.Body);
                modelBuilder.Entity(entityType.ClrType).
                    HasQueryFilter(Expression.Lambda(newBody, newParam));
            }
        }
    }

    public static void ApplyGlobalFilters<T>(this ModelBuilder modelBuilder, string propertyName, T value)
    {
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            IMutableProperty? foundProperty = entityType.FindProperty(propertyName);
            if (foundProperty != null && foundProperty.ClrType == typeof(T))
            {
                ParameterExpression newParam = Expression.Parameter(entityType.ClrType);
                LambdaExpression filter = Expression.
                    Lambda(Expression.Equal(Expression.Property(newParam, propertyName),
                    Expression.Constant(value)), newParam);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }
}
