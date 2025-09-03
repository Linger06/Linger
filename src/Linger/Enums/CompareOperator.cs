using System.ComponentModel;
using Linger.SourceGen;
namespace Linger.Enums;

/// <summary>
/// Specifies the comparison operators.
/// </summary>
[GenerateEnumExtensions]
public enum CompareOperator
{
    /// <summary>
    /// Represents the equality operator.
    /// </summary>
    [Description("Equals")]
    Equals = 0,

    /// <summary>
    /// Represents the inequality operator.
    /// </summary>
    [Description("NotEquals")]
    NotEquals = 1,

    /// <summary>
    /// Represents the greater than operator.
    /// </summary>
    [Description("GreaterThan")]
    GreaterThan = 2,

    /// <summary>
    /// Represents the greater than or equal to operator.
    /// </summary>
    [Description("GreaterThanOrEquals")]
    GreaterThanOrEquals = 3,

    /// <summary>
    /// Represents the less than operator.
    /// </summary>
    [Description("LessThan")]
    LessThan = 4,

    /// <summary>
    /// Represents the less than or equal to operator.
    /// </summary>
    [Description("LessThanOrEquals")]
    LessThanOrEquals = 5,

    /// <summary>
    /// Represents the standard input operator.
    /// </summary>
    [Description("StdIn")]
    StdIn = 6,

    /// <summary>
    /// Represents the standard not in operator.
    /// </summary>
    [Description("StdNotIn")]
    StdNotIn = 7,

    /// <summary>
    /// Represents the contains operator.
    /// </summary>
    [Description("Contains")]
    Contains = 8,

    /// <summary>
    /// Represents the not contains operator.
    /// </summary>
    [Description("NotContains")]
    NotContains = 9,

    /// <summary>
    /// Represents the starts with operator.
    /// </summary>
    [Description("StartsWith")]
    StartsWith = 10,

    /// <summary>
    /// Represents the ends with operator.
    /// </summary>
    [Description("EndsWith")]
    EndsWith = 11
}
