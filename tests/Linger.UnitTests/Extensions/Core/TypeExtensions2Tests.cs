namespace Linger.UnitTests.Extensions.Core
{
    public class TypeExtensions2Tests
    {
        [Fact]
        public void IsEnum_ShouldReturnTrue_WhenTypeIsEnum()
        {
            // Arrange
            var type = typeof(DayOfWeek);

            // Act
            var result = type.IsEnum();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsEnum_ShouldReturnFalse_WhenTypeIsNotEnum()
        {
            // Arrange
            var type = typeof(int);

            // Act
            var result = type.IsEnum();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsEnumOrNullableEnum_ShouldReturnTrue_WhenTypeIsEnum()
        {
            // Arrange
            var type = typeof(DayOfWeek);

            // Act
            var result = type.IsEnumOrNullableEnum();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsEnumOrNullableEnum_ShouldReturnTrue_WhenTypeIsNullableEnum()
        {
            // Arrange
            var type = typeof(DayOfWeek?);

            // Act
            var result = type.IsEnumOrNullableEnum();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsEnumOrNullableEnum_ShouldReturnFalse_WhenTypeIsNotEnumOrNullableEnum()
        {
            // Arrange
            var type = typeof(int);

            // Act
            var result = type.IsEnumOrNullableEnum();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsBool_ShouldReturnTrue_WhenTypeIsBool()
        {
            // Arrange
            var type = typeof(bool);

            // Act
            var result = type.IsBool();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsBool_ShouldReturnFalse_WhenTypeIsNotBool()
        {
            // Arrange
            var type = typeof(int);

            // Act
            var result = type.IsBool();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsBoolOrNullableBool_ShouldReturnTrue_WhenTypeIsBool()
        {
            // Arrange
            var type = typeof(bool);

            // Act
            var result = type.IsBoolOrNullableBool();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsBoolOrNullableBool_ShouldReturnTrue_WhenTypeIsNullableBool()
        {
            // Arrange
            var type = typeof(bool?);

            // Act
            var result = type.IsBoolOrNullableBool();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsBoolOrNullableBool_ShouldReturnFalse_WhenTypeIsNotBoolOrNullableBool()
        {
            // Arrange
            var type = typeof(int);

            // Act
            var result = type.IsBoolOrNullableBool();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetPropertyInfo_ShouldReturnPropertyInfo_WhenPropertyExists()
        {
            // Arrange
            var type = typeof(SampleClass);
            var propertyName = "Name";

            // Act
            var result = type.GetPropertyInfo(propertyName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(propertyName, result.Name);
        }

        [Fact]
        public void GetPropertyInfo_ShouldThrowException_WhenPropertyDoesNotExist()
        {
            // Arrange
            var type = typeof(SampleClass);
            var propertyName = "NonExistentProperty";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => type.GetPropertyInfo(propertyName));
        }

        [Fact]
        public void GetColumnsInfo_ShouldReturnColumnInfoList_ForType()
        {
            // Arrange
            var type = typeof(SampleClass);

            // Act
            var result = type.GetColumnsInfo();

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Name", result[0].PropertyName);
            Assert.Equal("Age", result[1].PropertyName);
        }

        // Sample class and attribute for testing
        private class SampleClass
        {
            [UserDefinedTableTypeColumn(Name = "Name", Order = 1)]
            public string Name { get; set; }

            [UserDefinedTableTypeColumn(Name = "Age", Order = 2)]
            public int Age { get; set; }
        }

        private class UserDefinedTableTypeColumnAttribute : System.Attribute
        {
            public string Name { get; set; }
            public int Order { get; set; }
        }
    }
}
