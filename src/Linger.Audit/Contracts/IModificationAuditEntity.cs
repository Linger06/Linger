namespace Linger.Audit.Contracts;

public interface IModificationAuditEntity : IBaseAuditEntity
{
    string? LastModifierId { get; set; }
    DateTimeOffset? LastModificationTime { get; set; }
}