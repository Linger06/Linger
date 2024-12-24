using System.Linq.Expressions;

namespace Linger.UnitTests.Helper
{
    public class PropertyHelperTests
    {
        private class TestClass
        {
            public string MyProperty { get; set; } = "InitialValue";
            public NestedClass Nested { get; set; } = new NestedClass();
            [Obsolete]
            public string IgnoredProperty { get; set; } = "IgnoredValue";
        }

        private class NestedClass
        {
            public string NestedProperty { get; set; } = "NestedValue";
        }

        [Fact]
        public void GetMemberExp_ShouldReturnCorrectMemberExpression()
        {
            var param = Expression.Parameter(typeof(TestClass), "x");
            var memberExp = param.GetMemberExp("MyProperty");

            Assert.NotNull(memberExp);
            Assert.IsAssignableFrom<MemberExpression>(memberExp);
            Assert.Equal("MyProperty", ((MemberExpression)memberExp).Member.Name);
        }

        [Fact]
        public void GetMemberExp_ShouldReturnCorrectNestedMemberExpression()
        {
            var param = Expression.Parameter(typeof(TestClass), "x");
            var memberExp = param.GetMemberExp("Nested,NestedProperty");

            Assert.NotNull(memberExp);
            Assert.IsAssignableFrom<MemberExpression>(memberExp);
            Assert.Equal("NestedProperty", ((MemberExpression)memberExp).Member.Name);
        }

        [Fact]
        public void GetPropertyName_SingleProperty_ReturnsPropertyName()
        {
            Expression<Func<TestClass, string>> expression = x => x.MyProperty;
            var propertyName = expression.GetPropertyName();
            Assert.Equal("MyProperty", propertyName);
        }

        [Fact]
        public void GetPropertyName_NestedProperty_ReturnsFullPropertyPath()
        {
            Expression<Func<TestClass, string>> expression = x => x.Nested.NestedProperty;
            var propertyName = expression.GetPropertyName();
            Assert.Equal("Nested.NestedProperty", propertyName);
        }

        [Fact]
        public void GetPropertyName_NullExpression_ReturnsEmptyString()
        {
            Expression expression = null;
            var propertyName = expression.GetPropertyName();
            Assert.Equal(string.Empty, propertyName);
        }

        [Fact]
        public void GetPropertyInfo_ShouldReturnCorrectPropertyInfo()
        {
            Expression<Func<TestClass, string>> expression = x => x.MyProperty;
            var propInfo = expression.GetPropertyInfo();

            Assert.NotNull(propInfo);
            Assert.Equal("MyProperty", propInfo?.Name);
        }

        [Fact]
        public void GetPropertyInfo_Generic_ShouldReturnCorrectPropertyInfo()
        {
            var propInfo = PropertyHelper.GetPropertyInfo<TestClass>(x => x.MyProperty);

            Assert.NotNull(propInfo);
            Assert.Equal("MyProperty", propInfo?.Name);
        }

        [Fact]
        public void GetPropertyInfo_ShouldReturnNullForInvalidExpression()
        {
            Expression<Func<TestClass, string>> expression = x => x.MyProperty + "Invalid";
            var propInfo = expression.GetPropertyInfo();

            Assert.Null(propInfo);
        }

        [Fact]
        public void TrySetProperty_ShouldSetPropertyValue()
        {
            var obj = new TestClass();
            PropertyHelper.TrySetProperty(obj, x => x.MyProperty, () => "NewValue");

            Assert.Equal("NewValue", obj.MyProperty);
        }

        [Fact]
        public void TrySetProperty_WithFactory_ShouldSetPropertyValue()
        {
            var obj = new TestClass();
            PropertyHelper.TrySetProperty(obj, x => x.MyProperty, o => "NewValue");

            Assert.Equal("NewValue", obj.MyProperty);
        }

        [Fact]
        public void TrySetProperty_ShouldNotSetPropertyValueForInvalidExpression()
        {
            var obj = new TestClass();
            PropertyHelper.TrySetProperty(obj, x => x.MyProperty + "Invalid", () => "NewValue");

            Assert.Equal("InitialValue", obj.MyProperty);
        }

        [Fact]
        public void TrySetProperty_ShouldNotSetPropertyValueForIgnoredAttribute()
        {
            var obj = new TestClass();
            PropertyHelper.TrySetProperty(obj, x => x.IgnoredProperty, () => "NewValue", typeof(ObsoleteAttribute));

            Assert.Equal("IgnoredValue", obj.IgnoredProperty);
        }
    }
}
