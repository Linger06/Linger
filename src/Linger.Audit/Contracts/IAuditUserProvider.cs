namespace Linger.Audit.Contracts;

public interface IAuditUserProvider
{
    [Obsolete("UserName will be removed in 2.0.0. Use GetUser() instead.")]
    string? UserName { get; }

    string GetUser();
}
