namespace Linger.Audit.Contracts;

public interface ISoftDelete
{
    DateTimeOffset? DeletionTime { get; set; }
    string? DeleterId { get; set; }
    bool? IsDeleted { get; set; }
}