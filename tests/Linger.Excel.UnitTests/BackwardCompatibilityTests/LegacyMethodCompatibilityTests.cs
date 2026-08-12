using Linger.Excel.Npoi;
using Xunit;

namespace Linger.Excel.Tests.BackwardCompatibilityTests;

public class LegacyMethodCompatibilityTests
{
    [Theory]
    [InlineData("ExcelToDataTableAsync")]
    [InlineData("ExcelToListAsync")]
    [InlineData("ExcelToDataSetAsync")]
    [InlineData("StreamToDataTableAsync")]
    [InlineData("StreamToListAsync")]
    [InlineData("StreamToDataSetAsync")]
    public void LegacyAsyncImportMethod_IsNotPublic(string methodName)
    {
        Assert.DoesNotContain(
            typeof(NpoiExcel).GetMethods(),
            method => method.Name == methodName);
    }

    [Fact]
    public void LegacyCommaSeparatedSheetSelection_IsNotPublic()
    {
        Assert.DoesNotContain(
            typeof(NpoiExcel).GetMethods(),
            method =>
            {
                if (method.Name != "ExcelToDataSet")
                {
                    return false;
                }

                var parameters = method.GetParameters();

                return parameters.Length == 3 &&
                       parameters[0].ParameterType == typeof(string) &&
                       parameters[1].ParameterType == typeof(string) &&
                       parameters[2].ParameterType == typeof(int);
            });
    }
}
