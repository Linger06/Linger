namespace Linger.Attributes;
/// <summary>
/// This attribute can customize column name and order of converted DataTable.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class UserDefinedTableTypeColumnAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserDefinedTableTypeColumnAttribute"/> class with the specified order.
    /// </summary>
    /// <param name="order">Order of column in DataTable.</param>
    public UserDefinedTableTypeColumnAttribute(int order)
    {
        Order = order;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserDefinedTableTypeColumnAttribute"/> class with the specified name.
    /// </summary>
    /// <param name="name">Name of column in DataTable.</param>
    public UserDefinedTableTypeColumnAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserDefinedTableTypeColumnAttribute"/> class with the specified order and name.
    /// </summary>
    /// <param name="order">Order of column in DataTable.</param>
    /// <param name="name">Name of column in DataTable.</param>
    public UserDefinedTableTypeColumnAttribute(int order, string name)
    {
        Order = order;
        Name = name;
    }

    /// <summary>
    /// Gets or sets the order of the column.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the name of the column.
    /// </summary>
    public string? Name { get; set; }
}
