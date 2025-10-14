namespace Linger.Audit.Contracts;

public interface IAuditUserProvider
{
    string? UserName { get; }

    string GetUser();
}
