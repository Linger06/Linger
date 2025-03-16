using System.IO;
using System.Data;
using Xunit;
using Linger.Excel.Contracts;
using NPOI.XSSF.UserModel;

namespace Linger.Excel.Tests
{
    public class NpoiHeaderIndexTests : IDisposable
    {
        private readonly IExcelService _excelService;
        private readonly string _testFilePath;
        
        public NpoiHeaderIndexTests()
        {
            // 初始化测试用的Excel服务
            _excelService = new Linger.Excel.Npoi.NpoiExcel();
            
            // 创建测试文件
            _testFilePath = Path.Combine(Path.GetTempPath(), "npoiHeaderIndexTest.xlsx");
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
            // 创建一个有标题行和表头行的Excel文件
            using var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Sheet1");
            
            // 行0: 标题行
            var titleRow = sheet.CreateRow(0);
            titleRow.CreateCell(0).SetCellValue("员工信息表");
            
            // 行1: 表头行
            var headerRow = sheet.CreateRow(1);
            headerRow.CreateCell(0).SetCellValue("ID");
            headerRow.CreateCell(1).SetCellValue("姓名");
            headerRow.CreateCell(2).SetCellValue("年龄");
            headerRow.CreateCell(3).SetCellValue("部门");
            
            // 行2-3: 数据行
            var dataRow1 = sheet.CreateRow(2);
            dataRow1.CreateCell(0).SetCellValue(1);
            dataRow1.CreateCell(1).SetCellValue("张三");
            dataRow1.CreateCell(2).SetCellValue(28);
            dataRow1.CreateCell(3).SetCellValue("技术部");
            
            var dataRow2 = sheet.CreateRow(3);
            dataRow2.CreateCell(0).SetCellValue(2);
            dataRow2.CreateCell(1).SetCellValue("李四");
            dataRow2.CreateCell(2).SetCellValue(32);
            dataRow2.CreateCell(3).SetCellValue("市场部");
            
            // 保存文件
            using var fs = new FileStream(_testFilePath, FileMode.Create);
            workbook.Write(fs, true);
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
