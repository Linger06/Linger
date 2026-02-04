using System.IO;
using System.Data;
using Xunit;
using Linger.Excel.Contracts;
using ClosedXML.Excel;

namespace Linger.Excel.Tests
{
    public class ClosedXmlHeaderIndexTests : IDisposable
    {
        private readonly IExcelService _excelService;
        private readonly string _testFilePath;
        
        public ClosedXmlHeaderIndexTests()
        {
            // 初始化测试用的Excel服务
            _excelService = new Linger.Excel.ClosedXML.ClosedXmlExcel();
            
            // 创建测试文件
            _testFilePath = Path.Combine(Path.GetTempPath(), $"closedxmlHeaderIndexTest_{Guid.NewGuid():N}.xlsx");
            CreateTestExcelFile();
        }
        
        public void Dispose()
        {
            try
            {
                if (File.Exists(_testFilePath))
                {
                    File.Delete(_testFilePath);
                }
            }
            catch
            {
                // 忽略删除异常
            }
        }
        
        private void CreateTestExcelFile()
        {
            // 使用ClosedXML创建测试文件
            using var workbook = new XLWorkbook();
            var sheet = workbook.AddWorksheet("Sheet1");
            
            // 行1: 标题行 (ClosedXML是1-based索引)
            sheet.Cell(1, 1).Value = "员工信息表";
            
            // 行2: 表头行
            sheet.Cell(2, 1).Value = "ID";
            sheet.Cell(2, 2).Value = "姓名";
            sheet.Cell(2, 3).Value = "年龄";
            sheet.Cell(2, 4).Value = "部门";
            
            // 行3-4: 数据行
            sheet.Cell(3, 1).Value = 1;
            sheet.Cell(3, 2).Value = "张三";
            sheet.Cell(3, 3).Value = 28;
            sheet.Cell(3, 4).Value = "技术部";
            
            sheet.Cell(4, 1).Value = 2;
            sheet.Cell(4, 2).Value = "李四";
            sheet.Cell(4, 3).Value = 32;
            sheet.Cell(4, 4).Value = "市场部";
            
            // 保存文件
            workbook.SaveAs(_testFilePath);
        }
        
        [Fact]
        public void TestHeaderAtFirstRow()
        {
            // 默认headerRowIndex=0，表头在第一行(会错过实际表头)
            var dt = _excelService.ExcelToDataTable(_testFilePath);
            
            // 验证结果 - 将标题行误认为是表头
            Assert.Equal("员工信息表", dt.Columns[0].ColumnName);
            Assert.Equal(3, dt.Rows.Count); // 包括表头行和两行数据
        }
        
        [Fact]
        public void TestHeaderAtSecondRow()
        {
            // 指定headerRowIndex=1，表头在第二行
            var dt = _excelService.ExcelToDataTable(_testFilePath, null, 1);
            
            // 验证结果
            Assert.Equal("ID", dt.Columns[0].ColumnName);
            Assert.Equal("姓名", dt.Columns[1].ColumnName);
            Assert.Equal(2, dt.Rows.Count); // 只包含两行数据
        }
        
        [Fact]
        public void TestNoHeader()
        {
            // 指定headerRowIndex=-1，无表头
            var dt = _excelService.ExcelToDataTable(_testFilePath, null, -1);
            
            // 验证结果
            Assert.Equal("Column1", dt.Columns[0].ColumnName); // 自动生成列名
            Assert.Equal("Column2", dt.Columns[1].ColumnName);
            Assert.Equal(4, dt.Rows.Count); // 所有行都作为数据
        }
    }
}
