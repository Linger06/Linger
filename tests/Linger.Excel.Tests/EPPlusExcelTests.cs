using System.Data;
using System.IO;
using Linger.Excel.Contracts;
using Linger.Excel.EPPlus;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Linger.Excel.Tests;

public class EPPlusExcelTests : ExcelServiceTestBase, IDisposable
{
    public EPPlusExcelTests()
    {
        CleanupTestDir();
    }

    public void Dispose()
    {
        CleanupTestDir();
        GC.SuppressFinalize(this);
    }

    protected override IExcelService GetExcelService()
    {
        // 使用测试配置创建EPPlus实现
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().AddDebug());
        var logger = loggerFactory.CreateLogger<EPPlusExcel>();
        return new EPPlusExcel(Options, logger);
    }

    [Fact]
    public void DataTableToFile_GeneratesValidExcelFile()
    {
        // Arrange
        var service = GetExcelService();
        var dataTable = GenerateTestDataTable(10);
        var filePath = Path.Combine(TestFilesDir, "EPPlusExport_DataTable.xlsx");

        // Act
        var result = service.DataTableToFile(dataTable, filePath, "测试表", "EPPlus导出测试");

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
        var filePath = Path.Combine(TestFilesDir, "EPPlusExport_List.xlsx");

        // Act
        var result = service.ListToFile(list, filePath, "人员列表", "EPPlus导出测试");

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
        using var stream = service.ConvertDataTableToMemoryStream(dataTable, "测试表", "EPPlus内存流测试");

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
        var filePath = Path.Combine(TestFilesDir, "EPPlusImport_DataTable.xlsx");

        // 先导出Excel
        service.DataTableToFile(originalData, filePath, "测试表", "EPPlus导入测试");

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
        var filePath = Path.Combine(TestFilesDir, "EPPlusImport_List.xlsx");

        // 先导出Excel
        service.ListToFile(originalList, filePath, "人员列表", "EPPlus导入测试");

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
        var templatePath = Path.Combine(TestFilesDir, "EPPlusTemplate.xlsx");

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
            }),
            // 员工表
            ("员工信息", TABLE2_ROWS, table =>
            {
                table.Columns.Add("Id", typeof(int));
                table.Columns.Add("Name", typeof(string));
                table.Columns.Add("Age", typeof(int));
            }),
            // 未命名表（将使用默认表名）
            (null, TABLE3_ROWS, table =>
            {
                table.Columns.Add("Id", typeof(int));
                table.Columns.Add("Value", typeof(string));
            })
        );

        var filePath = Path.Combine(TestFilesDir, "EPPlusExport_DataSet.xlsx");

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
        Assert.Equal(3, departmentData.Columns.Count);

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
        var emptyFilePath = Path.Combine(TestFilesDir, "EPPlusExport_CompletelyEmptyDataSet.xlsx");

        // 2. 包含一个空的DataTable（无列无行）
        var emptyTable = GenerateCustomDataTable("", 0, _ => { });
        var dataSetWithEmptyTable = new DataSet();
        dataSetWithEmptyTable.Tables.Add(emptyTable);
        var emptyTableFilePath = Path.Combine(TestFilesDir, "EPPlusExport_EmptyTableDataSet.xlsx");

        // 3. 包含有列但无行的DataTable
        var dataSetWithColumnsOnly = GenerateTestDataSet(
            ("", 0, table =>
            {
                table.Columns.Add("Id", typeof(int));
                table.Columns.Add("Name", typeof(string));
            })
        );
        var columnsOnlyFilePath = Path.Combine(TestFilesDir, "EPPlusExport_ColumnsOnlyDataSet.xlsx");

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
        Assert.Equal(2, importedData3.Columns.Count);
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
            }),
            // 第二个表（不指定表名）
            (null, TABLE_ROWS, table =>
            {
                table.Columns.Add("Code", typeof(string));
                table.Columns.Add("Value", typeof(decimal));
            })
        );

        var filePath = Path.Combine(TestFilesDir, "EPPlusExport_CustomSheetName.xlsx");

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
        Assert.Null(nullData);
    }

    [Fact]
    public void LargeDataSet_HandlesParallelProcessing()
    {
        // Arrange
        var service = GetExcelService();
        var largeList = GenerateTestPersonList(1000); // 超过并行处理阈值
        var filePath = Path.Combine(TestFilesDir, "EPPlusExport_LargeDataSet.xlsx");

        // Act
        var result = service.ListToFile(largeList, filePath, "大数据集", "EPPlus并行处理测试");

        // Assert
        Assert.Equal(filePath, result);
        Assert.True(File.Exists(filePath));
        Assert.True(new FileInfo(filePath).Length > 0);

        // 验证可以正确读取
        var importedList = service.ExcelToList<TestPerson>(filePath, headerRowIndex: 1);
        Assert.NotNull(importedList);
        Assert.Equal(largeList.Count, importedList.Count);
    }

    [Fact]
    public void ExcelWithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var service = GetExcelService();
        var dataTable = new DataTable("特殊字符测试");
        dataTable.Columns.Add("名称", typeof(string));
        dataTable.Columns.Add("描述", typeof(string));

        var row1 = dataTable.NewRow();
        row1["名称"] = "测试\"引号\"";
        row1["描述"] = "包含换行\n和制表符\t的文本";
        dataTable.Rows.Add(row1);

        var row2 = dataTable.NewRow();
        row2["名称"] = "特殊符号 < > & ' \"";
        row2["描述"] = "中文，English，123，!@#$%";
        dataTable.Rows.Add(row2);

        var filePath = Path.Combine(TestFilesDir, "EPPlusExport_SpecialChars.xlsx");

        // Act
        var result = service.DataTableToFile(dataTable, filePath);

        // Assert
        Assert.Equal(filePath, result);
        Assert.True(File.Exists(filePath));

        // 验证可以正确读回数据
        var importedData = service.ExcelToDataTable(filePath, headerRowIndex: 0);
        Assert.NotNull(importedData);
        Assert.Equal(2, importedData.Rows.Count);
    }

    #region ExcelToDataSet Tests

    [Fact]
    public void ExcelToDataSet_WithAllSheets_ImportsAllSheetsWithUniformHeader()
    {
        // Arrange
        var service = GetExcelService();
        const int TABLE1_ROWS = 5;
        const int TABLE2_ROWS = 3;
        const int TABLE3_ROWS = 7;

        var sourceDataSet = GenerateTestDataSet(
            ("部门信息", TABLE1_ROWS, table =>
            {
                table.Columns.Add("Id", typeof(int));
                table.Columns.Add("Name", typeof(string));
                table.Columns.Add("Budget", typeof(decimal));
            }),
            ("员工信息", TABLE2_ROWS, table =>
            {
                table.Columns.Add("EmployeeId", typeof(int));
                table.Columns.Add("EmployeeName", typeof(string));
                table.Columns.Add("Age", typeof(int));
            }),
            ("项目信息", TABLE3_ROWS, table =>
            {
                table.Columns.Add("ProjectId", typeof(int));
                table.Columns.Add("ProjectName", typeof(string));
            })
        );

        var filePath = Path.Combine(TestFilesDir, "EPPlusImport_AllSheets.xlsx");
        service.DataSetToFile(sourceDataSet, filePath);

        // Act
        var importedDataSet = service.ExcelToDataSet(filePath, headerRowIndex: 0, addEmptyRow: false);

        // Assert
        Assert.NotNull(importedDataSet);
        Assert.Equal(3, importedDataSet.Tables.Count);

        var table1 = importedDataSet.Tables["部门信息"];
        Assert.NotNull(table1);
        Assert.Equal(TABLE1_ROWS, table1.Rows.Count);
        Assert.Equal(3, table1.Columns.Count);

        var table2 = importedDataSet.Tables["员工信息"];
        Assert.NotNull(table2);
        Assert.Equal(TABLE2_ROWS, table2.Rows.Count);

        var table3 = importedDataSet.Tables["项目信息"];
        Assert.NotNull(table3);
        Assert.Equal(TABLE3_ROWS, table3.Rows.Count);
    }

    [Fact]
    public void ExcelToDataSet_WithSelectedSheets_ImportsOnlySpecifiedSheets()
    {
        // Arrange
        var service = GetExcelService();
        const int TABLE1_ROWS = 5;
        const int TABLE2_ROWS = 3;
        const int TABLE3_ROWS = 7;

        var sourceDataSet = GenerateTestDataSet(
            ("部门信息", TABLE1_ROWS, table =>
            {
                table.Columns.Add("Id", typeof(int));
                table.Columns.Add("Name", typeof(string));
            }),
            ("员工信息", TABLE2_ROWS, table =>
            {
                table.Columns.Add("EmployeeId", typeof(int));
                table.Columns.Add("EmployeeName", typeof(string));
            }),
            ("项目信息", TABLE3_ROWS, table =>
            {
                table.Columns.Add("ProjectId", typeof(int));
                table.Columns.Add("ProjectName", typeof(string));
            })
        );

        var filePath = Path.Combine(TestFilesDir, "EPPlusImport_SelectedSheets.xlsx");
        service.DataSetToFile(sourceDataSet, filePath);

        // Act
        var sheetsToImport = new[] { "部门信息", "项目信息" };
        var importedDataSet = service.ExcelToDataSet(filePath, sheetsToImport, headerRowIndex: 0, addEmptyRow: false);

        // Assert
        Assert.NotNull(importedDataSet);
        Assert.Equal(2, importedDataSet.Tables.Count);
        Assert.NotNull(importedDataSet.Tables["部门信息"]);
        Assert.NotNull(importedDataSet.Tables["项目信息"]);
        Assert.Null(importedDataSet.Tables["员工信息"]);
    }

    [Fact]
    public void ExcelToDataSet_WithFlexibleHeaderSelector_UsesPerSheetHeaderRows()
    {
        // Arrange
        var service = GetExcelService();
        
        var sourceDataSet = GenerateTestDataSet(
            ("标准表头", 5, table =>
            {
                table.Columns.Add("Id", typeof(int));
                table.Columns.Add("Name", typeof(string));
            }),
            ("无表头", 3, table =>
            {
                table.Columns.Add("Column1", typeof(int));
                table.Columns.Add("Column2", typeof(string));
            })
        );

        var filePath = Path.Combine(TestFilesDir, "EPPlusImport_FlexibleHeaders.xlsx");
        service.DataSetToFile(sourceDataSet, filePath);

        // Act
        var importedDataSet = service.ExcelToDataSet(filePath, sheetName =>
        {
            return sheetName switch
            {
                "标准表头" => 0,
                "无表头" => null,
                _ => 0
            };
        }, addEmptyRow: false);

        // Assert
        Assert.NotNull(importedDataSet);
        Assert.Equal(2, importedDataSet.Tables.Count);

        var table1 = importedDataSet.Tables["标准表头"];
        Assert.NotNull(table1);
        Assert.Equal(5, table1.Rows.Count);

        var table2 = importedDataSet.Tables["无表头"];
        Assert.NotNull(table2);
        Assert.Equal(3, table2.Rows.Count); // 不包含表头行，因为 headerRowIndex = null 时会从第一行开始读取数据
    }

    [Fact]
    public async Task ExcelToDataSetAsync_WithSelectedSheetsAndFlexibleHeaders_WorksCorrectly()
    {
        // Arrange
        var service = GetExcelService();
        
        var sourceDataSet = GenerateTestDataSet(
            ("表一", 3, table =>
            {
                table.Columns.Add("Id", typeof(int));
                table.Columns.Add("Name", typeof(string));
            }),
            ("表二", 2, table =>
            {
                table.Columns.Add("Code", typeof(string));
                table.Columns.Add("Value", typeof(decimal));
            }),
            ("表三", 4, table =>
            {
                table.Columns.Add("X", typeof(int));
                table.Columns.Add("Y", typeof(int));
            })
        );

        var filePath = Path.Combine(TestFilesDir, "EPPlusImport_AsyncSelectedAndFlexible.xlsx");
        service.DataSetToFile(sourceDataSet, filePath);

        // Act
        var sheetsToImport = new[] { "表一", "表三" };
        var importedDataSet = await service.ExcelToDataSetAsync(filePath, sheetsToImport, sheetName =>
        {
            return sheetName switch
            {
                "表一" => 0,
                "表三" => null,
                _ => 0
            };
        }, addEmptyRow: false);

        // Assert
        Assert.NotNull(importedDataSet);
        Assert.Equal(2, importedDataSet.Tables.Count);
        Assert.NotNull(importedDataSet.Tables["表一"]);
        Assert.NotNull(importedDataSet.Tables["表三"]);
        Assert.Null(importedDataSet.Tables["表二"]);
    }

    #endregion
}
