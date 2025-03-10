using System.Data;
using Linger.Excel.Contracts.Utils;
using Linger.Excel.Tests.Models;
using Xunit;

namespace Linger.Excel.Tests.Utils;

public class ExcelValueConverterTests
{
    [Fact]
    public void ConvertToDbValue_Null_ReturnsDbNull()
    {
        // Act
        var result = ExcelValueConverter.ConvertToDbValue(null);
        
        // Assert
        Assert.Equal(DBNull.Value, result);
    }
    
    [Fact]
    public void ConvertToDbValue_EmptyString_ReturnsDbNull()
    {
        // Act
        var result = ExcelValueConverter.ConvertToDbValue(string.Empty);
        
        // Assert
        Assert.Equal(DBNull.Value, result);
    }
    
    [Fact]
    public void ConvertToDbValue_String_ReturnsString()
    {
        // Arrange
        var testValue = "测试字符串";
        
        // Act
        var result = ExcelValueConverter.ConvertToDbValue(testValue);
        
        // Assert
        Assert.Equal(testValue, result);
    }
    
    [Fact]
    public void ConvertToDbValue_DateTime_ReturnsDateTime()
    {
        // Arrange
        var testDate = new DateTime(2023, 1, 1);
        
        // Act
        var result = ExcelValueConverter.ConvertToDbValue(testDate, true);
        
        // Assert
        Assert.Equal(testDate, result);
    }
    
    [Fact]
    public void ConvertToDbValue_NumericDateAsDouble_ReturnsDateTime()
    {
        // Arrange - 44927是Excel格式的2023-01-01日期
        double oaDate = 44927;
        DateTime expectedDate = DateTime.FromOADate(oaDate);
        
        // Act
        var result = ExcelValueConverter.ConvertToDbValue(oaDate, true);
        
        // Assert
        Assert.Equal(expectedDate, result);
    }
    
    [Theory]
    [InlineData(42.0, 42)]  // 整数格式的双精度浮点数
    [InlineData(42.42, 42.42)]  // 小数格式的双精度浮点数
    [InlineData(2147483647.0, 2147483647)]  // Int.MaxValue
    [InlineData(2147483648.0, 2147483648L)]  // Int.MaxValue+1 (转为long)
    public void ConvertToDbValue_Double_ConvertsCorrectly(double input, object expected)
    {
        // Act
        var result = ExcelValueConverter.ConvertToDbValue(input);
        
        // Assert
        Assert.Equal(expected, result);
        Assert.Equal(expected.GetType(), result.GetType());
    }
    
    [Theory]
    [InlineData("123", typeof(int), 123)]
    [InlineData("123.45", typeof(double), 123.45)]
    [InlineData("123.45", typeof(decimal), 123.45)]
    [InlineData("true", typeof(bool), true)]
    [InlineData("false", typeof(bool), false)]
    [InlineData("yes", typeof(bool), true)]
    [InlineData("no", typeof(bool), false)]
    [InlineData("y", typeof(bool), true)]
    [InlineData("n", typeof(bool), false)]
    [InlineData("2023-01-01", typeof(DateTime), "2023-01-01")]
    [InlineData("Medium", typeof(TestLevel), TestLevel.Medium)]
    [InlineData("1", typeof(TestLevel), TestLevel.Medium)]
    public void TryConvertValue_ValidValues_ConvertsCorrectly(string input, Type targetType, object expected)
    {
        // Act
        var result = ExcelValueConverter.TryConvertValue(input, targetType);
        
        // Assert
        Assert.NotNull(result);
        
        if (targetType == typeof(DateTime))
        {
            // 日期类型需要特殊比较
            Assert.Equal(DateTime.Parse((string)expected), (DateTime)result);
        }
        else
        {
            Assert.Equal(expected, result);
        }
    }
    
    [Fact]
    public void TryConvertValue_InvalidValues_ReturnsNull()
    {
        // Act & Assert - 测试各种无效转换
        Assert.Null(ExcelValueConverter.TryConvertValue("not a number", typeof(int)));
        Assert.Null(ExcelValueConverter.TryConvertValue("not a date", typeof(DateTime)));
        Assert.Null(ExcelValueConverter.TryConvertValue("invalid", typeof(TestLevel)));
    }
    
    [Theory]
    [InlineData(1, "A")]
    [InlineData(2, "B")]
    [InlineData(26, "Z")]
    [InlineData(27, "AA")]
    [InlineData(28, "AB")]
    [InlineData(52, "AZ")]
    [InlineData(53, "BA")]
    [InlineData(702, "ZZ")]
    [InlineData(703, "AAA")]
    public void GetExcelColumnName_ValidIndex_ReturnsCorrectName(int columnIndex, string expected)
    {
        // Act
        var result = ExcelValueConverter.GetExcelColumnName(columnIndex);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("A", 1)]
    [InlineData("B", 2)]
    [InlineData("Z", 26)]
    [InlineData("AA", 27)]
    [InlineData("AB", 28)]
    [InlineData("AZ", 52)]
    [InlineData("BA", 53)]
    [InlineData("ZZ", 702)]
    [InlineData("AAA", 703)]
    public void GetExcelColumnIndex_ValidName_ReturnsCorrectIndex(string columnName, int expected)
    {
        // Act
        var result = ExcelValueConverter.GetExcelColumnIndex(columnName);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public void GetExcelColumnIndex_EmptyString_ReturnsNegativeOne()
    {
        // Act
        var result = ExcelValueConverter.GetExcelColumnIndex(string.Empty);
        
        // Assert
        Assert.Equal(-1, result);
    }
}
