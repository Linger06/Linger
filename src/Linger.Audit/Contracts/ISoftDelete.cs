namespace Linger.Audit.Contracts;

/// <summary>
/// Defines soft-delete audit information for an entity.
/// </summary>
public interface ISoftDelete
{
    /// <summary>
    /// Gets or sets the deletion timestamp.
    /// </summary>
    DateTimeOffset? DeletionTime { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who deleted the entity.
    /// </summary>
    string? DeleterId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity is deleted. A value of <see langword="null"/>
    /// indicates that no deletion state has been recorded.
    /// </summary>
    bool? IsDeleted { get; set; }
}
