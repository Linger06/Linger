using System;
using System.Data;
using System.IO;
using Linger.Excel.Contracts;
using Linger.Excel.Npoi;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Linger.Excel.Tests
{
    public class NpoiExcelTests : ExcelServiceTestBase, IDisposable
    {
        public NpoiExcelTests()
        {
            CleanupTestDir();
        }

        public void Dispose()
        {
            CleanupTestDir();
        }

        protected override IExcelService GetExcelService()
        {
            // 使用测试配置创建NPOI实现
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().AddDebug());
            var logger = loggerFactory.CreateLogger<NpoiExcel>();
            return new NpoiExcel(Options, logger);
        }

        [Fact]
        public void DataTableToFile_GeneratesValidExcelFile()
        {
            // Arrange
            var service = GetExcelService();
            var dataTable = GenerateTestDataTable(10);
            var filePath = Path.Combine(TestFilesDir, "NpoiExport_DataTable.xlsx");

            // Act
            var result = service.DataTableToFile(dataTable, filePath, "测试表", "NPOI导出测试");

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
            var filePath = Path.Combine(TestFilesDir, "NpoiExport_List.xlsx");

            // Act
            var result = service.ListToFile(list, filePath, "人员列表", "NPOI导出测试");

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
            using var stream = service.ConvertDataTableToMemoryStream(dataTable, "测试表", "NPOI内存流测试");

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
            var filePath = Path.Combine(TestFilesDir, "NpoiImport_DataTable.xlsx");

            // 先导出Excel
            service.DataTableToFile(originalData, filePath, "测试表", "NPOI导入测试");

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
            var filePath = Path.Combine(TestFilesDir, "NpoiImport_List.xlsx");

            // 先导出Excel
            service.ListToFile(originalList, filePath, "人员列表", "NPOI导入测试");

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
            var templatePath = Path.Combine(TestFilesDir, "NpoiTemplate.xlsx");

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
            const int TABLE1_ROWS = 5;
            const int TABLE2_ROWS = 3;
            const int TABLE3_ROWS = 2;

            // 使用辅助方法生成测试数据集
            var dataSet = GenerateTestDataSet(
                // 部门表
                ("部门信息", TABLE1_ROWS, table =>
                {
                    table.Columns.Add("Id", typeof(int));
                    table.Columns.Add("Name", typeof(string));
                    table.Columns.Add("Budget", typeof(decimal));
                }
            ),
                // 员工表
                ("员工信息", TABLE2_ROWS, table =>
                {
                    table.Columns.Add("Id", typeof(int));
                    table.Columns.Add("Name", typeof(string));
                    table.Columns.Add("Age", typeof(int));
                }
            ),
                // 未命名表（将使用默认表名）
                (null, TABLE3_ROWS, table =>
                {
                    table.Columns.Add("Id", typeof(int));
                    table.Columns.Add("Value", typeof(string));
                }
            )
            );

            var filePath = Path.Combine(TestFilesDir, "NpoiExport_DataSet.xlsx");

            // Act
            var result = service.DataSetToFile(dataSet, filePath);

            // Assert
            Assert.Equal(filePath, result);
            Assert.True(File.Exists(filePath));
            Assert.True(new FileInfo(filePath).Length > 0);

            // 验证可以重新读取该文件中的表并包含所有正确的数据
            var departmentData = service.ExcelToDataTable(filePath, "部门信息", headerRowIndex: 0);
            Assert.NotNull(departmentData);
            Assert.Equal(TABLE1_ROWS, departmentData.Rows.Count);
            Assert.Equal(3, departmentData.Columns.Count);  // 确保所有列都导出

            var employeeData = service.ExcelToDataTable(filePath, "员工信息", headerRowIndex: 0);
            Assert.NotNull(employeeData);
            Assert.Equal(TABLE2_ROWS, employeeData.Rows.Count);
            Assert.Equal(3, employeeData.Columns.Count);
            // 验证使用默认的表名称
            var unnamedData = service.ExcelToDataTable(filePath, "Table1", headerRowIndex: 0);
            Assert.NotNull(unnamedData);
            Assert.Equal(TABLE3_ROWS, unnamedData.Rows.Count);
            Assert.Equal(2, unnamedData.Columns.Count);
        }

        [Fact]
        public void DataSetToFile_WithEmptyDataSet_CreatesSheetWithDefaultName()
        {
            // Arrange
            var service = GetExcelService();

            // 测试三种情况：
            // 1. 完全空的DataSet（不包含任何表）
            var emptyDataSet = new DataSet();
            var emptyFilePath = Path.Combine(TestFilesDir, "NpoiExport_CompletelyEmptyDataSet.xlsx");

            // 2. 包含一个空的DataTable（无列无行）
            var emptyTable = GenerateCustomDataTable("", 0, _ => { });
            var dataSetWithEmptyTable = new DataSet();
            dataSetWithEmptyTable.Tables.Add(emptyTable);
            var emptyTableFilePath = Path.Combine(TestFilesDir, "NpoiExport_EmptyTableDataSet.xlsx");

            // 3. 包含有列但无行的DataTable
            var dataSetWithColumnsOnly = GenerateTestDataSet(
                ("", 0, table =>
                {
                    table.Columns.Add("Id", typeof(int));
                    table.Columns.Add("Name", typeof(string));
                }
            )
            );
            var columnsOnlyFilePath = Path.Combine(TestFilesDir, "NpoiExport_ColumnsOnlyDataSet.xlsx");

            // Act & Assert - 测试完全空的DataSet
            var result1 = service.DataSetToFile(emptyDataSet, emptyFilePath);
            Assert.Equal(emptyFilePath, result1);
            Assert.True(File.Exists(emptyFilePath));
            // 验证创建了默认工作表
            var importedData1 = service.ExcelToDataTable(emptyFilePath, "Table1", headerRowIndex: 0);
            Assert.Null(importedData1);

            // Act & Assert - 测试包含空表的DataSet
            var result2 = service.DataSetToFile(dataSetWithEmptyTable, emptyTableFilePath);
            Assert.Equal(emptyTableFilePath, result2);
            Assert.True(File.Exists(emptyTableFilePath));
            // 验证创建了默认工作表
            var importedData2 = service.ExcelToDataTable(emptyTableFilePath, "Table1", headerRowIndex: 0);
            Assert.Null(importedData2);

            // Act & Assert - 测试只有列定义的表
            var result3 = service.DataSetToFile(dataSetWithColumnsOnly, columnsOnlyFilePath);
            Assert.Equal(columnsOnlyFilePath, result3);
            Assert.True(File.Exists(columnsOnlyFilePath));
            // 验证创建了工作表并包含列头但没有数据行
            var importedData3 = service.ExcelToDataTable(columnsOnlyFilePath, "Table1", headerRowIndex: 0);
            Assert.NotNull(importedData3);
            Assert.Empty(importedData3.Rows);
            Assert.Equal(2, importedData3.Columns.Count);// 应该包含两列
        }
        [Fact]
        public void DataSetToFile_WithCustomDefaultSheetName_UsesCorrectSheetNames()
        {
            // Arrange
            var service = GetExcelService();
            const int TABLE_ROWS = 2;
            const string CUSTOM_PREFIX = "Table";

            // 使用辅助方法生成测试数据集，两个表都不设置表名
            var dataSet = GenerateTestDataSet(
                // 第一个表（不指定表名）
                (null, TABLE_ROWS, table =>
                {
                    table.Columns.Add("Id", typeof(int));
                    table.Columns.Add("Name", typeof(string));
                }
            ),
                // 第二个表（不指定表名）
                (null, TABLE_ROWS, table =>
                {
                    table.Columns.Add("Code", typeof(string));
                    table.Columns.Add("Value", typeof(decimal));
                }
            )
            );

            var filePath = Path.Combine(TestFilesDir, "NpoiExport_CustomSheetNames.xlsx");

            // Act - 使用自定义的默认表名前缀
            var result = service.DataSetToFile(dataSet, filePath, CUSTOM_PREFIX);

            // Assert
            Assert.Equal(filePath, result);
            Assert.True(File.Exists(filePath));
            Assert.True(new FileInfo(filePath).Length > 0);

            // 验证工作表使用了正确的自定义名称
            var sheet1Data = service.ExcelToDataTable(filePath, $"{CUSTOM_PREFIX}1", headerRowIndex: 0);
            Assert.NotNull(sheet1Data);
            Assert.Equal(TABLE_ROWS, sheet1Data.Rows.Count);
            Assert.Equal(2, sheet1Data.Columns.Count);

            var sheet2Data = service.ExcelToDataTable(filePath, $"{CUSTOM_PREFIX}2", headerRowIndex: 0);
            Assert.NotNull(sheet2Data);
            Assert.Equal(TABLE_ROWS, sheet2Data.Rows.Count);
            Assert.Equal(2, sheet2Data.Columns.Count);

            // 额外验证 - 确认不能用默认的"Sheet"前缀访问
            var nullData = service.ExcelToDataTable(filePath, "Sheet1", headerRowIndex: 0);
            Assert.Null(nullData); // 应该返回null，因为没有Sheet1这个工作表
        }
    }
}
