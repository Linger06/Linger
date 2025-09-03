using Linger.SourceGen;
namespace Linger;

/// <summary>
/// Represents sorting information.
/// </summary>
public class SortInfo
{
    /// <summary>
    /// Gets or sets the property name to sort by.
    /// </summary>
    /// <value>The property name to sort by.</value>
    /// <example>
    /// <code>
    /// var sortInfo = new SortInfo { Property = "Name", Direction = SortDir.Asc };
    /// // sets the property to sort by to "Name"
    /// </code>
    /// </example>
    public string Property { get; set; } = null!;

    /// <summary>
    /// Gets or sets the sort direction.
    /// </summary>
    /// <value>The sort direction.</value>
    /// <example>
    /// <code>
    /// var sortInfo = new SortInfo { Property = "Name", Direction = SortDir.Asc };
    /// // sets the sort direction to ascending
    /// </code>
    /// </example>
    public SortDir Direction { get; set; }
}

/// <summary>
/// Specifies the sort direction.
/// </summary>
[GenerateEnumExtensions]
public enum SortDir
{
    /// <summary>
    /// Sort in ascending order.
    /// </summary>
    Asc,

    /// <summary>
    /// Sort in descending order.
    /// </summary>
    Desc
}
