using Xunit;

namespace Linger.Excel.Tests.Collections
{
    [CollectionDefinition("Sequential")]
    public class SequentialTestCollection : ICollectionFixture<object>
    {
        // 这个类不需要实际的成员，它只是用来标记集合
    }
    
    [CollectionDefinition("ExcelValueConverter")]
    public class ExcelValueConverterTestCollection : ICollectionFixture<object>
    {
        // 用于测试值转换器的集合
    }
}
