namespace Linger.Audit.Contracts;

public interface IHasCreationTime
{
    DateTimeOffset CreationTime { get; set; }
}