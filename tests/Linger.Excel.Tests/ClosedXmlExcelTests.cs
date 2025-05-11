using System;
using System.Data;
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
                Assert.Equal(0, importedList[i].Id);
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

        [Fact]
        public void DataSetToFile_GeneratesValidExcelFile()
        {
            // Arrange
            var service = GetExcelService();
            var dataSet = new DataSet("TestDataSet");
            
            // 添加第一个表，使用自定义表名以避免冲突
            var table1 = new DataTable("部门信息");
            table1.Columns.Add("Id", typeof(int));
            table1.Columns.Add("Name", typeof(string));
            table1.Columns.Add("Budget", typeof(decimal));
            for (int i = 1; i <= 5; i++)
            {
                table1.Rows.Add(i, $"部门 {i}", 10000 * i);
            }
            dataSet.Tables.Add(table1);
            
            // 添加第二个表，使用自定义表名以避免冲突
            var table2 = new DataTable("员工信息");
            table2.Columns.Add("Id", typeof(int));
            table2.Columns.Add("Name", typeof(string));
            table2.Columns.Add("Age", typeof(int));
            for (int i = 1; i <= 3; i++)
            {
                table2.Rows.Add(i, $"员工 {i}", 20 + i);
            }
            dataSet.Tables.Add(table2);
            
            // 添加第三个表（不设置表名，测试默认表名）
            var table3 = new DataTable();
            table3.Columns.Add("Id", typeof(int));
            table3.Columns.Add("Value", typeof(string));
            for (int i = 1; i <= 2; i++)
            {
                table3.Rows.Add(i, $"值 {i}");
            }
            dataSet.Tables.Add(table3);
            
            var filePath = Path.Combine(TestFilesDir, "ClosedXmlExport_DataSet.xlsx");

            // Act
            var result = service.DataSetToFile(dataSet, filePath);

            // Assert
            Assert.Equal(filePath, result);
            Assert.True(File.Exists(filePath));
            Assert.True(new FileInfo(filePath).Length > 0);
            
            // 验证可以重新读取该文件中的表
            var table1Data = service.ExcelToDataTable(filePath, "部门信息", headerRowIndex: 0);
            Assert.NotNull(table1Data);
            Assert.Equal(5, table1Data.Rows.Count);  // table1有5行数据
            
            var table2Data = service.ExcelToDataTable(filePath, "员工信息", headerRowIndex: 0);
            Assert.NotNull(table2Data);
            Assert.Equal(3, table2Data.Rows.Count);  // table2有3行数据
            
            var table3Data = service.ExcelToDataTable(filePath, "Sheet3", headerRowIndex: 0);
            Assert.NotNull(table3Data);
            Assert.Equal(2, table3Data.Rows.Count);  // table3有2行数据
        }

        [Fact]
        public void DataSetToFile_WithEmptyDataSet_CreatesSheetWithDefaultName()
        {
            // Arrange
            var service = GetExcelService();
            var dataSet = new DataSet();
            
            // 添加一个空表，确保文件中有工作表
            var emptyTable = new DataTable();
            dataSet.Tables.Add(emptyTable);
            
            var filePath = Path.Combine(TestFilesDir, "ClosedXmlExport_EmptyDataSet.xlsx");

            // Act
            var result = service.DataSetToFile(dataSet, filePath);

            // Assert
            Assert.Equal(filePath, result);
            Assert.True(File.Exists(filePath));
            Assert.True(new FileInfo(filePath).Length > 0);
            
            // 验证创建了默认Sheet1工作表
            var importedData = service.ExcelToDataTable(filePath, "Sheet1", headerRowIndex: 0);
            Assert.NotNull(importedData);
            Assert.Empty(importedData.Rows);
        }
        
        [Fact]
        public void DataSetToFile_WithCustomDefaultSheetName_UsesCorrectSheetNames()
        {
            // Arrange
            var service = GetExcelService();
            var dataSet = new DataSet("TestDataSet");
            
            // 添加两个没有表名的表，使用不同的结构避免冲突
            var table1 = new DataTable();
            table1.Columns.Add("Id", typeof(int));
            table1.Columns.Add("Name", typeof(string));
            for (int i = 1; i <= 2; i++)
            {
                table1.Rows.Add(i, $"项目 {i}");
            }
            dataSet.Tables.Add(table1);
            
            var table2 = new DataTable();
            table2.Columns.Add("Code", typeof(string));
            table2.Columns.Add("Value", typeof(decimal));
            for (int i = 1; i <= 2; i++)
            {
                table2.Rows.Add($"CODE_{i}", i * 1.5m);
            }
            dataSet.Tables.Add(table2);
            
            var filePath = Path.Combine(TestFilesDir, "ClosedXmlExport_CustomSheetName.xlsx");
            string customDefaultSheetName = "CustomSheet";

            // Act
            var result = service.DataSetToFile(dataSet, filePath, customDefaultSheetName);

            // Assert
            Assert.Equal(filePath, result);
            Assert.True(File.Exists(filePath));
            
            // 验证使用了自定义的表名前缀
            var sheet1Data = service.ExcelToDataTable(filePath, "CustomSheet1", headerRowIndex: 0);
            Assert.NotNull(sheet1Data);
            Assert.Equal(2, sheet1Data.Rows.Count);
            
            var sheet2Data = service.ExcelToDataTable(filePath, "CustomSheet2", headerRowIndex: 0);
            Assert.NotNull(sheet2Data);
            Assert.Equal(2, sheet2Data.Rows.Count);
        }
    }
}
