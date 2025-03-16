using System.IO;
using System.Data;
using Xunit;
using Linger.Excel.Contracts;
using OfficeOpenXml;

namespace Linger.Excel.Tests
{
    public class EPPlusHeaderIndexTests : IDisposable
    {
        private readonly IExcelService _excelService;
        private readonly string _testFilePath;
        
        public EPPlusHeaderIndexTests()
        {
            // 设置许可证模式
            // ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            // 初始化测试用的Excel服务
            _excelService = new Linger.Excel.EPPlus.EPPlusExcel();
            
            // 创建测试文件
            _testFilePath = Path.Combine(Path.GetTempPath(), "epplusHeaderIndexTest.xlsx");
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
            // 使用EPPlus创建测试文件
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Sheet1");
            
            // 行1: 标题行 (EPPlus是1-based索引)
            sheet.Cells[1, 1].Value = "员工信息表";
            
            // 行2: 表头行
            sheet.Cells[2, 1].Value = "ID";
            sheet.Cells[2, 2].Value = "姓名";
            sheet.Cells[2, 3].Value = "年龄";
            sheet.Cells[2, 4].Value = "部门";
            
            // 行3-4: 数据行
            sheet.Cells[3, 1].Value = 1;
            sheet.Cells[3, 2].Value = "张三";
            sheet.Cells[3, 3].Value = 28;
            sheet.Cells[3, 4].Value = "技术部";
            
            sheet.Cells[4, 1].Value = 2;
            sheet.Cells[4, 2].Value = "李四";
            sheet.Cells[4, 3].Value = 32;
            sheet.Cells[4, 4].Value = "市场部";
            
            // 保存文件
            package.SaveAs(new FileInfo(_testFilePath));
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
