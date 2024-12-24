namespace Linger.UnitTests.Extensions.Core
{
    public class ObjectExtensionsTests
    {
        [Fact]
        public void IsNotNull_ShouldReturnTrue_WhenObjectIsNotNull()
        {
            var obj = new object();
            Assert.True(obj.IsNotNull());
        }

        [Fact]
        public void IsNotNull_ShouldReturnFalse_WhenObjectIsNull()
        {
            object? obj = null;
            Assert.False(obj.IsNotNull());
        }

        [Fact]
        public void IsNull_ShouldReturnTrue_WhenObjectIsNull()
        {
            object? obj = null;
            Assert.True(obj.IsNull());
        }

        [Fact]
        public void IsNull_ShouldReturnFalse_WhenObjectIsNotNull()
        {
            var obj = new object();
            Assert.False(obj.IsNull());
        }

        [Fact]
        public void IsNotNullAndEmpty_ShouldReturnTrue_WhenObjectIsNotNullAndNotEmpty()
        {
            object obj = "Hello";
            Assert.True(obj.IsNotNullAndEmpty());
        }

        [Fact]
        public void IsNotNullAndEmpty_ShouldReturnFalse_WhenObjectIsNull()
        {
            object? obj = null;
            Assert.False(obj.IsNotNullAndEmpty());
        }

        [Fact]
        public void IsNotNullAndEmpty_ShouldReturnFalse_WhenObjectIsEmptyString()
        {
            object obj = "";
            Assert.False(obj.IsNotNullAndEmpty());
        }

        [Fact]
        public void IsNullOrEmpty_ShouldReturnTrue_WhenObjectIsNull()
        {
            object? obj = null;
            Assert.True(obj.IsNullOrEmpty());
        }

        [Fact]
        public void IsNullOrEmpty_ShouldReturnTrue_WhenObjectIsEmptyString()
        {
            object obj = "";
            Assert.True(obj.IsNullOrEmpty());
        }

        [Fact]
        public void IsNullOrEmpty_ShouldReturnFalse_WhenObjectIsNotNullAndNotEmpty()
        {
            object obj = "Hello";
            Assert.False(obj.IsNullOrEmpty());
        }

        [Fact]
        public void IsNullOrDbNull_ShouldReturnTrue_WhenObjectIsNull()
        {
            object? obj = null;
            Assert.True(obj.IsNullOrDbNull());
        }

        [Fact]
        public void IsNullOrDbNull_ShouldReturnTrue_WhenObjectIsDbNull()
        {
            object obj = DBNull.Value;
            Assert.True(obj.IsNullOrDbNull());
        }

        [Fact]
        public void IsNullOrDbNull_ShouldReturnFalse_WhenObjectIsNotNullAndNotDbNull()
        {
            object obj = "Hello";
            Assert.False(obj.IsNullOrDbNull());
        }
    }
}
