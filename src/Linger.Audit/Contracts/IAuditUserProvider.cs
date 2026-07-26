namespace Linger.Audit.Contracts;

/// <summary>
/// Provides the identifier of the user responsible for an audited operation.
/// </summary>
public interface IAuditUserProvider
{
    /// <summary>
    /// Gets the current user identifier, or <see langword="null"/> when no authenticated user is available.
    /// </summary>
    string? GetUser();
}
