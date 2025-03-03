namespace Linger.Audit.Contracts;

public interface ICreationAuditEntity : IHasCreationTime, IBaseAuditEntity
{
    string CreatorId { get; set; }
}