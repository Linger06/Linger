using Linger.Attributes;
using Linger.Extensions.Core;

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

        [Fact]
        public void GetColumnsInfo_ShouldUseDefaultOrdering_WithoutAttributes()
        {
            // Arrange
            var type = typeof(ClassWithoutAttributes);

            // Act
            var result = type.GetColumnsInfo();

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(3, result.Count);
            
            // 验证属性名称是否保持不变
            Assert.Equal("FirstProperty", result[0].PropertyName);
            Assert.Equal("SecondProperty", result[1].PropertyName);
            Assert.Equal("ThirdProperty", result[2].PropertyName);
            
            // 验证属性顺序是否按声明顺序递增
            Assert.Equal(1, result[0].PropertyOrder);
            Assert.Equal(2, result[1].PropertyOrder);
            Assert.Equal(3, result[2].PropertyOrder);
        }

        [Fact]
        public void GetColumnsInfo_ShouldHandleMixedAttributes()
        {
            // Arrange
            var type = typeof(ClassWithMixedAttributes);

            // Act
            var result = type.GetColumnsInfo();

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(3, result.Count);
            
            // 验证属性名称和顺序
            Assert.Equal("CustomName", result[0].PropertyName);  // 第一个属性有特性并设置了名称和顺序
            Assert.Equal("SecondProperty", result[1].PropertyName);  // 第二个属性没有特性
            Assert.Equal("ThirdProperty", result[2].PropertyName);  // 第三个属性有特性只设置了顺序
            
            // 验证属性顺序
            Assert.Equal(1, result[0].PropertyOrder);
            Assert.Equal(2, result[1].PropertyOrder);
            Assert.Equal(10, result[2].PropertyOrder);  // 自定义顺序
        }

        [Fact]
        public void GetColumnsInfo_ShouldHandleInheritedProperties()
        {
            // Arrange
            var type = typeof(ChildClass);

            // Act
            var result = type.GetColumnsInfo();

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(3, result.Count);  // 2个子类属性 + 1个父类属性
            
            // 验证父类和子类属性都被正确处理
            var propertyNames = result.Select(c => c.PropertyName).ToList();
            Assert.Contains("ParentProperty", propertyNames);
            Assert.Contains("ChildProperty1", propertyNames);
            Assert.Contains("ChildProperty2", propertyNames);
        }

        [Fact]
        public void GetColumnsInfo_ShouldReturnEmptyList_ForEmptyClass()
        {
            // Arrange
            var type = typeof(EmptyClass);

            // Act
            var result = type.GetColumnsInfo();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetColumnsInfo_ShouldHandleOrderOnlyAttribute()
        {
            // Arrange
            var type = typeof(ClassWithOrderOnly);

            // Act
            var result = type.GetColumnsInfo();

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count);
            
            // 验证第二个属性排在第一位
            Assert.Equal("SecondProperty", result[0].PropertyName);
            Assert.Equal("FirstProperty", result[1].PropertyName);
            
            // 验证顺序值
            Assert.Equal(5, result[0].PropertyOrder);
            Assert.Equal(10, result[1].PropertyOrder);
        }

        [Fact]
        public void GetColumnsInfo_ShouldHandleNameOnlyAttribute()
        {
            // Arrange
            var type = typeof(ClassWithNameOnly);

            // Act
            var result = type.GetColumnsInfo();

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count);
            
            // 验证名称被正确应用但保持默认顺序
            Assert.Equal("Custom1", result[0].PropertyName);
            Assert.Equal("Custom2", result[1].PropertyName);
            
            // 默认顺序应该依次递增
            Assert.Equal(1, result[0].PropertyOrder);
            Assert.Equal(2, result[1].PropertyOrder);
        }

        // Sample class and attribute for testing
        private class SampleClass
        {
            [UserDefinedTableTypeColumn(1, "Name")]
            public string Name { get; set; }

            [UserDefinedTableTypeColumn(2, "Age")]
            public int Age { get; set; }
        }

        // 用于测试的类定义

        // 不带特性的简单类
        private class ClassWithoutAttributes
        {
            public string FirstProperty { get; set; }
            public int SecondProperty { get; set; }
            public bool ThirdProperty { get; set; }
        }

        // 混合特性和非特性属性的类
        private class ClassWithMixedAttributes
        {
            [UserDefinedTableTypeColumn(1, "CustomName")]
            public string FirstProperty { get; set; }
            
            public int SecondProperty { get; set; }
            
            [UserDefinedTableTypeColumn(10)]
            public bool ThirdProperty { get; set; }
        }

        // 继承层次结构测试
        private class ParentClass
        {
            public string ParentProperty { get; set; }
        }

        private class ChildClass : ParentClass
        {
            public int ChildProperty1 { get; set; }
            public bool ChildProperty2 { get; set; }
        }

        // 空类测试
        private class EmptyClass
        {
        }

        // 只有顺序没有名称的特性测试
        private class ClassWithOrderOnly
        {
            [UserDefinedTableTypeColumn(10)]
            public string FirstProperty { get; set; }
            
            [UserDefinedTableTypeColumn(5)]
            public int SecondProperty { get; set; }
        }

        // 只有名称没有顺序的特性测试
        private class ClassWithNameOnly
        {
            [UserDefinedTableTypeColumn("Custom1")]
            public string FirstProperty { get; set; }
            
            [UserDefinedTableTypeColumn("Custom2")]
            public int SecondProperty { get; set; }
        }
    }
}
