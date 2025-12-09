using Linger.Attributes;

namespace Linger.UnitTests.Attributes;

public class UserDefinedTableTypeColumnAttributeTests
{
    [Fact]
    public void Constructor_WithOrder_SetsOrder()
    {
        // Arrange & Act
        var attribute = new UserDefinedTableTypeColumnAttribute(5);

        // Assert
        Assert.Equal(5, attribute.Order);
        Assert.Null(attribute.Name);
    }

    [Fact]
    public void Constructor_WithName_SetsName()
    {
        // Arrange & Act
        var attribute = new UserDefinedTableTypeColumnAttribute("ColumnName");

        // Assert
        Assert.Equal("ColumnName", attribute.Name);
        Assert.Equal(0, attribute.Order);
    }

    [Fact]
    public void Constructor_WithOrderAndName_SetsBoth()
    {
        // Arrange & Act
        var attribute = new UserDefinedTableTypeColumnAttribute(3, "TestColumn");

        // Assert
        Assert.Equal(3, attribute.Order);
        Assert.Equal("TestColumn", attribute.Name);
    }

    [Fact]
    public void Order_CanBeSet()
    {
        // Arrange
        var attribute = new UserDefinedTableTypeColumnAttribute(1);

        // Act
        attribute.Order = 10;

        // Assert
        Assert.Equal(10, attribute.Order);
    }

    [Fact]
    public void Name_CanBeSet()
    {
        // Arrange
        var attribute = new UserDefinedTableTypeColumnAttribute(1);

        // Act
        attribute.Name = "NewName";

        // Assert
        Assert.Equal("NewName", attribute.Name);
    }

    [Fact]
    public void Attribute_CanBeAppliedToProperty()
    {
        // Arrange
        var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.TestProperty));

        // Act
        var attribute = propertyInfo?.GetCustomAttributes(typeof(UserDefinedTableTypeColumnAttribute), false)
            .FirstOrDefault() as UserDefinedTableTypeColumnAttribute;

        // Assert
        Assert.NotNull(attribute);
        Assert.Equal(1, attribute.Order);
        Assert.Equal("CustomColumn", attribute.Name);
    }

    [Fact]
    public void Attribute_WithNegativeOrder_IsAllowed()
    {
        // Arrange & Act
        var attribute = new UserDefinedTableTypeColumnAttribute(-1);

        // Assert
        Assert.Equal(-1, attribute.Order);
    }

    [Fact]
    public void Attribute_WithEmptyName_IsAllowed()
    {
        // Arrange & Act
        var attribute = new UserDefinedTableTypeColumnAttribute(string.Empty);

        // Assert
        Assert.Equal(string.Empty, attribute.Name);
    }

    [Fact]
    public void Attribute_WithNullName_IsAllowed()
    {
        // Arrange & Act
        var attribute = new UserDefinedTableTypeColumnAttribute(1);
        attribute.Name = null;

        // Assert
        Assert.Null(attribute.Name);
    }

    private class TestClass
    {
        [UserDefinedTableTypeColumn(1, "CustomColumn")]
        public string? TestProperty { get; set; }
    }
}
