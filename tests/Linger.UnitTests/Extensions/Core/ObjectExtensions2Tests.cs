namespace Linger.UnitTests.Extensions.Core
{
    public class ObjectExtensions2Tests
    {
        [Fact]
        public void ForIn_ShouldExecuteActionOnEachProperty()
        {
            var obj = new { Name = "John", Age = 30 };
            var properties = new Dictionary<string, object?>();

            obj.ForIn((name, val) => properties[name] = val);

            Assert.Equal(2, properties.Count);
            Assert.Equal("John", properties["Name"]);
            Assert.Equal(30, properties["Age"]);
        }

        [Fact]
        public void ForIn_ShouldNotThrow_WhenObjectIsNull()
        {
            object? obj = null;
            var properties = new Dictionary<string, object?>();

            obj.ForIn((name, val) => properties[name] = val);

            Assert.Empty(properties);
        }

        [Fact]
        public void GetPropertyInfo_ShouldReturnPropertyInfo_WhenPropertyExists()
        {
            var obj = new { Name = "John" };
            var propertyInfo = obj.GetPropertyInfo("Name");

            Assert.NotNull(propertyInfo);
            Assert.Equal("Name", propertyInfo.Name);
        }

        [Fact]
        public void GetPropertyInfo_ShouldThrowArgumentException_WhenPropertyDoesNotExist()
        {
            var obj = new { Name = "John" };

            Assert.Throws<InvalidOperationException>(() => obj.GetPropertyInfo("Age"));
        }

        [Fact]
        public void GetPropertyValue_ShouldReturnValue_WhenPropertyExists()
        {
            var obj = new { Name = "John" };
            var value = obj.GetPropertyValue("Name");

            Assert.Equal("John", value);
        }

        [Fact]
        public void GetPropertyValue_ShouldThrowArgumentException_WhenPropertyDoesNotExist()
        {
            var obj = new { Name = "John" };

            Assert.Throws<InvalidOperationException>(() => obj.GetPropertyValue("Age"));
        }

        [Fact]
        public void IsString_ShouldReturnTrue_WhenObjectIsString()
        {
            object str = "Hello";
            Assert.True(str.IsString());
        }

        [Fact]
        public void IsString_ShouldReturnFalse_WhenObjectIsNotString()
        {
            object num = 123;
            Assert.False(num.IsString());
        }

        [Fact]
        public void IsInt16_ShouldReturnTrue_WhenObjectIsInt16()
        {
            object num = (short)123;
            Assert.True(num.IsInt16());
        }

        [Fact]
        public void IsInt16_ShouldReturnFalse_WhenObjectIsNotInt16()
        {
            object num = 123;
            Assert.False(num.IsInt16());
        }

        [Fact]
        public void IsInt_ShouldReturnTrue_WhenObjectIsInt()
        {
            object num = 123;
            Assert.True(num.IsInt());
        }

        [Fact]
        public void IsInt_ShouldReturnFalse_WhenObjectIsNotInt()
        {
            object num = (short)123;
            Assert.False(num.IsInt());
        }

        [Fact]
        public void IsInt64_ShouldReturnTrue_WhenObjectIsInt64()
        {
            object num = 123L;
            Assert.True(num.IsInt64());
        }

        [Fact]
        public void IsInt64_ShouldReturnFalse_WhenObjectIsNotInt64()
        {
            object num = 123;
            Assert.False(num.IsInt64());
        }

        [Fact]
        public void IsDecimal_ShouldReturnTrue_WhenObjectIsDecimal()
        {
            object num = 123.45M;
            Assert.True(num.IsDecimal());
        }

        [Fact]
        public void IsDecimal_ShouldReturnFalse_WhenObjectIsNotDecimal()
        {
            object num = 123.45;
            Assert.False(num.IsDecimal());
        }

        [Fact]
        public void IsSingle_ShouldReturnTrue_WhenObjectIsSingle()
        {
            object num = 123.45F;
            Assert.True(num.IsSingle());
        }

        [Fact]
        public void IsSingle_ShouldReturnFalse_WhenObjectIsNotSingle()
        {
            object num = 123.45;
            Assert.False(num.IsSingle());
        }

        [Fact]
        public void IsFloat_ShouldReturnTrue_WhenObjectIsFloat()
        {
            object num = 123.45F;
            Assert.True(num.IsFloat());
        }

        [Fact]
        public void IsFloat_ShouldReturnFalse_WhenObjectIsNotFloat()
        {
            object num = "Hello";
            Assert.False(num.IsFloat());
        }

        [Fact]
        public void IsDouble_ShouldReturnTrue_WhenObjectIsDouble()
        {
            object num = 123.45;
            Assert.True(num.IsDouble());
        }

        [Fact]
        public void IsDouble_ShouldReturnFalse_WhenObjectIsNotDouble()
        {
            object num = 123;
            Assert.False(num.IsDouble());
        }

        [Fact]
        public void IsDateTime_ShouldReturnTrue_WhenObjectIsDateTime()
        {
            object date = DateTime.Now;
            Assert.True(date.IsDateTime());
        }

        [Fact]
        public void IsDateTime_ShouldReturnFalse_WhenObjectIsNotDateTime()
        {
            object num = 123;
            Assert.False(num.IsDateTime());
        }

        [Fact]
        public void IsBoolean_ShouldReturnTrue_WhenObjectIsBoolean()
        {
            object flag = true;
            Assert.True(flag.IsBoolean());
        }

        [Fact]
        public void IsBoolean_ShouldReturnFalse_WhenObjectIsNotBoolean()
        {
            object num = 123;
            Assert.False(num.IsBoolean());
        }
    }
}
