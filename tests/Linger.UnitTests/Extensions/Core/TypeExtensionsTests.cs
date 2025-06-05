using System.Reflection;

namespace Linger.UnitTests.Extensions.Core
{
    public partial class TypeExtensionsTests
    {
        [Fact]
        public void IsGeneric_ShouldReturnTrue_WhenTypeIsGeneric()
        {
            // Arrange
            var type = typeof(List<int>);

            // Act
            var result = type.IsGeneric(typeof(List<>));

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsGeneric_ShouldReturnFalse_WhenTypeIsNotGeneric()
        {
            // Arrange
            var type = typeof(int);

            // Act
            var result = type.IsGeneric(typeof(List<>));

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsNullable_ShouldReturnTrue_WhenTypeIsNullable()
        {
            // Arrange
            var type = typeof(int?);

            // Act
            var result = type.IsNullable();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsNullable_ShouldReturnFalse_WhenTypeIsNotNullable()
        {
            // Arrange
            var type = typeof(int);

            // Act
            var result = type.IsNullable();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsInherits_ShouldReturnTrue_WhenTypeInheritsFromBaseType()
        {
            // Arrange
            var type = typeof(List<int>);
            var baseType = typeof(IEnumerable<int>);

            // Act
            var result = type.IsInherits(baseType);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsInherits_ShouldReturnFalse_WhenTypeDoesNotInheritFromBaseType()
        {
            // Arrange
            var type = typeof(int);
            var baseType = typeof(IEnumerable<int>);

            // Act
            var result = type.IsInherits(baseType);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsList_ShouldReturnTrue_WhenTypeIsList()
        {
            // Arrange
            var type = typeof(List<int>);

            // Act
            var result = type.IsList();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsList_ShouldReturnFalse_WhenTypeIsNotList()
        {
            // Arrange
            var type = typeof(int);

            // Act
            var result = type.IsList();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsPrimitive_ShouldReturnTrue_WhenTypeIsPrimitive()
        {
            // Arrange
            var type = typeof(int);

            // Act
            var result = type.IsPrimitive();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsPrimitive_ShouldReturnTrue_WhenTypeIsDecimal()
        {
            // Arrange
            var type = typeof(decimal);

            // Act
            var result = type.IsPrimitive();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsPrimitive_ShouldReturnFalse_WhenTypeIsNotPrimitive()
        {
            // Arrange
            var type = typeof(string);

            // Act
            var result = type.IsPrimitive();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsNumber_ShouldReturnTrue_WhenTypeIsNumber()
        {
            // Arrange
            var type = typeof(int);

            // Act
            var result = type.IsNumber();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsNumber_ShouldReturnFalse_WhenTypeIsNotNumber()
        {
            // Arrange
            var type = typeof(string);

            // Act
            var result = type.IsNumber();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetSingleProperty_ShouldReturnPropertyInfo_WhenPropertyExists()
        {
            // Arrange
            var type = typeof(SampleClass);
            var propertyName = "Name";

            // Act
            var result = type.GetSingleProperty(propertyName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(propertyName, result.Name);
        }

        [Fact]
        public void GetSingleProperty_ShouldReturnNull_WhenPropertyDoesNotExist()
        {
            // Arrange
            var type = typeof(SampleClass);
            var propertyName = "NonExistentProperty";

            // Act
            var result = type.GetSingleProperty(propertyName);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetTypeName_ShouldReturnCorrectTypeName_ForNonGenericType()
        {
            // Arrange
            var type = typeof(int);

            // Act
            var result = type.GetTypeName();

            // Assert
            Assert.Equal("Int32", result);
        }

        [Fact]
        public void GetTypeName_ShouldReturnCorrectTypeName_ForGenericType()
        {
            // Arrange
            var type = typeof(List<int>);

            // Act
            var result = type.GetTypeName();

            // Assert
            Assert.Equal("List<Int32>", result);
        }

        [Fact]
        public void GetTypeName_ShouldReturnCorrectTypeName_ForNullableType()
        {
            // Arrange
            var type = typeof(int?);

            // Act
            var result = type.GetTypeName();

            // Assert
            Assert.Equal("Int32?", result);
        }

        [Fact]
        public void GetTypeName_ShouldReturnCorrectTypeName_ForNullableType2()
        {
            // Arrange
            var type = typeof(int?);

            // Act
            var result = type.GetTypeName(false);

            // Assert
            Assert.Equal("Int32", result);
        }

        [Fact]
        public void GetTypeName_ShouldReturnCorrectTypeName_ForArrayType()
        {
            // Arrange
            var type = typeof(string[]);

            // Act
            var result = type.GetTypeName();

            // Assert
            Assert.Equal("String[]", result);
        }

        [Fact]
        public void GetDefaultValue_ShouldReturnDefaultValue_ForValueType()
        {
            // Arrange
            var type = typeof(int);

            // Act
            var result = type.GetDefaultValue();

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetDefaultValue_ShouldReturnNull_ForReferenceType()
        {
            // Arrange
            var type = typeof(string);

            // Act
            var result = type.GetDefaultValue();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetDescriptionValue_ShouldReturnAttribute_WhenAttributeExists()
        {
            // Arrange
            var type = typeof(SampleClass);

            // Act
            var result = type.GetDescriptionValue<SampleAttribute>();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetDescriptionValue_ShouldReturnNull_WhenAttributeDoesNotExist()
        {
            // Arrange
            var type = typeof(SampleClass);

            // Act
            var result = type.GetDescriptionValue<ObsoleteAttribute>();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Props_ShouldReturnProperties_ForType()
        {
            // Arrange
            var type = typeof(SampleClass);

            // Act
            var result = type.Props();

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void Props_ShouldReturnProperties_ForObject()
        {
            // Arrange
            var obj = new SampleClass();

            // Act
            var result = obj.Props();

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void HasAttribute_ShouldReturnTrue_WhenTypeHasAttribute()
        {
            // Arrange
            var type = typeof(SampleClass);

            // Act
            var result = type.HasAttribute<SampleAttribute>();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasAttribute_ShouldReturnFalse_WhenTypeDoesNotHaveAttribute()
        {
            // Arrange
            var type = typeof(SampleClass);

            // Act
            var result = type.HasAttribute<ObsoleteAttribute>();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetAttribute_ShouldReturnAttribute_WhenTypeHasAttribute()
        {
            // Arrange
            var type = typeof(SampleClass);

            // Act
            var result = type.GetAttribute<SampleAttribute>();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void HasAttribute_WithPredicate_ShouldReturnTrue_WhenTypeHasMatchingAttribute()
        {
            // Arrange
            var type = typeof(SampleClass);

            // Act
            var result = type.HasAttribute<SampleAttribute>(attr => attr.Name == "Test");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasAttribute_WithPredicate_ShouldReturnFalse_WhenTypeDoesNotHaveMatchingAttribute()
        {
            // Arrange
            var type = typeof(SampleClass);

            // Act
            var result = type.HasAttribute<SampleAttribute>(attr => attr.Name == "NonExistent");

            // Assert
            Assert.False(result);
        }

#if !NETFRAMEWORK || NET462_OR_GREATER

        [Fact]
        public void AttrProps_ShouldReturnPropertiesWithAttributes()
        {
            // Arrange
            var type = typeof(SampleClass);

            // Act
            var result = type.AttrProps(typeof(SampleAttribute));

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void AttrProps_Generic_ShouldReturnPropertiesWithAttributes()
        {
            // Arrange
            var type = typeof(SampleClass);

            // Act
            var result = type.AttrProps<SampleAttribute>();

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void HasAttribute_MemberInfo_ShouldReturnTrue_WhenMemberHasAttribute()
        {
            // Arrange
            var member = typeof(SampleClass).GetProperty("Name");

            // Act
            var result = member.HasAttribute<SampleAttribute>();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasAttribute_Type_ShouldReturnTrue_WhenTypeHasAttributes()
        {
            // Arrange
            var type = typeof(SampleClass);

            // Act
            var result = type.HasAttribute(typeof(SampleAttribute));

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasAttribute_MemberInfo_ShouldReturnTrue_WhenMemberHasAttributes()
        {
            // Arrange
            var member = typeof(SampleClass).GetProperty("Name");

            // Act
            var result = member.HasAttribute(typeof(SampleAttribute));

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void AttrValues_ShouldReturnPropertiesWithAttributeValues()
        {
            // Arrange
            var type = typeof(SampleClass);

            // Act
            var result = type.AttrValues<SampleAttribute>();

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void AttrPropValues_ShouldReturnPropertiesWithAttributeValues()
        {
            // Arrange
            var type = typeof(SampleClass);

            // Act
            var result = type.AttrPropValues<SampleAttribute>();

            // Assert
            Assert.NotEmpty(result);
        }

#endif

        [Fact]
        public void GetDescriptionValue_FieldInfo_ShouldReturnAttribute_WhenAttributeExists()
        {
            // Arrange
            var field = typeof(SampleClass).GetField("SampleField", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var result = field.GetDescriptionValue<SampleAttribute>();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetDescriptionValue_FieldInfo_ShouldReturnNull_WhenAttributeDoesNotExist()
        {
            // Arrange
            var field = typeof(SampleClass).GetField("SampleField", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var result = field.GetDescriptionValue<ObsoleteAttribute>();

            // Assert
            Assert.Null(result);
        }

        // Sample class and attribute for testing
        [Sample(Name = "Test")]
        private class SampleClass
        {
            [Sample(Name = "NameProperty")]
            public string Name { get; set; }

            [Sample(Name = "FieldProperty")]
            private int SampleField;
        }

        private class SampleAttribute : System.Attribute
        {
            public string Name { get; set; }
        }
    }
}
