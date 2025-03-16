using System;
using System.IO;
using Linger.Excel.ClosedXML;
using Linger.Excel.Contracts;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Linger.Excel.Tests
{
    public class ClosedXmlExcelTests : ExcelServiceTestBase, IDisposable
    {
        public ClosedXmlExcelTests()
        {
            CleanupTestDir();
        }

        public void Dispose()
        {
            CleanupTestDir();
        }

        protected override IExcelService GetExcelService()
        {
            // 使用测试配置创建ClosedXML实现
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().AddDebug());
            var logger = loggerFactory.CreateLogger<ClosedXmlExcel>();
            return new ClosedXmlExcel(Options, logger);
        }

        [Fact]
        public void DataTableToFile_GeneratesValidExcelFile()
        {
            // Arrange
            var service = GetExcelService();
            var dataTable = GenerateTestDataTable(10);
            var filePath = Path.Combine(TestFilesDir, "ClosedXmlExport_DataTable.xlsx");

            // Act
            var result = service.DataTableToFile(dataTable, filePath, "测试表", "ClosedXML导出测试");

            // Assert
            Assert.Equal(filePath, result);
            Assert.True(File.Exists(filePath));
            Assert.True(new FileInfo(filePath).Length > 0);
        }

        [Fact]
        public void ListToFile_GeneratesValidExcelFile()
        {
            // Arrange
            var service = GetExcelService();
            var list = GenerateTestPersonList(10);
            var filePath = Path.Combine(TestFilesDir, "ClosedXmlExport_List.xlsx");

            // Act
            var result = service.ListToFile(list, filePath, "人员列表", "ClosedXML导出测试");

            // Assert
            Assert.Equal(filePath, result);
            Assert.True(File.Exists(filePath));
            Assert.True(new FileInfo(filePath).Length > 0);
        }

        [Fact]
        public void ConvertDataTableToMemoryStream_ReturnsValidStream()
        {
            // Arrange
            var service = GetExcelService();
            var dataTable = GenerateTestDataTable(5);

            // Act
            using var stream = service.ConvertDataTableToMemoryStream(dataTable, "测试表", "ClosedXML内存流测试");

            // Assert
            Assert.NotNull(stream);
            Assert.True(stream.Length > 0);
        }

        [Fact]
        public void ExcelToDataTable_ReadsValidExcelFile()
        {
            // Arrange
            var service = GetExcelService();
            var originalData = GenerateTestDataTable(5);
            var filePath = Path.Combine(TestFilesDir, "ClosedXmlImport_DataTable.xlsx");

            // 先导出Excel
            service.DataTableToFile(originalData, filePath, "测试表", "ClosedXML导入测试");

            // Act - 从Excel读取回来
            var importedData = service.ExcelToDataTable(filePath, headerRowIndex: 1);

            // Assert
            Assert.NotNull(importedData);
            Assert.Equal(originalData.Rows.Count, importedData.Rows.Count);
            Assert.Equal(originalData.Columns.Count, importedData.Columns.Count);
        }

        [Fact]
        public void ExcelToList_ReadsAndConvertsToObjects()
        {
            // Arrange
            var service = GetExcelService();
            var originalList = GenerateTestPersonList(5);
            var filePath = Path.Combine(TestFilesDir, "ClosedXmlImport_List.xlsx");

            // 先导出Excel
            service.ListToFile(originalList, filePath, "人员列表", "ClosedXML导入测试");

            // Act - 从Excel读取回来
            var importedList = service.ExcelToList<TestPerson>(filePath, headerRowIndex: 1);

            // Assert
            Assert.NotNull(importedList);
            Assert.Equal(originalList.Count, importedList.Count);

            // 验证部分数据是否正确导入
            for (int i = 0; i < importedList.Count; i++)
            {
                Assert.Equal(originalList[i].Id, importedList[i].Id);
                Assert.Equal(originalList[i].Name, importedList[i].Name);
                // 对日期的比较只比较日期部分
                Assert.Equal(originalList[i].Birthday.Date, importedList[i].Birthday.Date);
                Assert.Equal(originalList[i].IsActive, importedList[i].IsActive);
            }
        }

        [Fact]
        public void CreateExcelTemplate_GeneratesValidTemplate()
        {
            // Arrange
            var service = GetExcelService();

            // Act
            using var templateStream = service.CreateExcelTemplate<TestPerson>();
            var templatePath = Path.Combine(TestFilesDir, "ClosedXmlTemplate.xlsx");

            // 保存模板到文件用于检验
            using (var fileStream = File.Create(templatePath))
            {
                templateStream.CopyTo(fileStream);
            }

            // Assert
            Assert.NotNull(templateStream);
            Assert.True(templateStream.Length > 0);
            Assert.True(File.Exists(templatePath));
        }
    }
}
