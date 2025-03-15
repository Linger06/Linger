using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using ExcelExample.Models;
using Linger.Excel.ClosedXML;
using Linger.Excel.Contracts;
using Linger.Excel.Contracts.Utils;
using Linger.Excel.EPPlus;
using Linger.Excel.Npoi;
using Microsoft.Extensions.Logging;

namespace ExcelExample
{
    class Program
    {
        static void Main(string[] args)
        {
            // 创建日志记录器(实际项目中应该使用DI)
            using var loggerFactory = LoggerFactory.Create(builder => 
                builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            
            var logger = loggerFactory.CreateLogger<Program>();
            
            // 创建Excel选项
            var options = new ExcelOptions
            {
                ParallelProcessingThreshold = 500, // 500行以上启用并行处理
                AutoFitColumns = true,
                DefaultDateFormat = "yyyy-MM-dd",
                UseMemoryOptimization = true
            };
            
            // 创建一个测试目录
            var directory = Path.Combine(Environment.CurrentDirectory, "ExcelTests");
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
                
            try
            {
                // 演示不同的Excel实现
                logger.LogInformation("=== 使用ClosedXML实现 ===");
                DemoExcelImplementation(new ClosedXmlExcel(options, loggerFactory.CreateLogger<ClosedXmlExcel>()), directory);
                
                logger.LogInformation("\n=== 使用EPPlus实现 ===");
                DemoExcelImplementation(new EPPlusExcel(options, loggerFactory.CreateLogger<EPPlusExcel>()), directory);
                
                logger.LogInformation("\n=== 使用NPOI实现 ===");
                DemoExcelImplementation(new NpoiExcel(options, loggerFactory.CreateLogger<NpoiExcel>()), directory);
                
                // 演示流式处理大文件
                logger.LogInformation("\n=== 流式处理大文件示例 ===");
                DemoStreamProcessing(new ClosedXmlExcel(options), directory);
                
                // 演示模板生成
                logger.LogInformation("\n=== 生成Excel模板 ===");
                DemoTemplateGeneration(new EPPlusExcel(options), directory);
                
                logger.LogInformation("\n所有示例完成，文件保存在: {0}", directory);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "示例运行失败");
            }
        }
        
        static void DemoExcelImplementation(IExcel excel, string directory)
        {
            // 1. 生成测试数据
            var employees = GenerateTestData(100);
            
            // 2. 导出员工数据到Excel
            string filePath = Path.Combine(directory, $"Employees_{excel.GetType().Name}.xlsx");
            excel.ListToFile(employees, filePath, "员工信息", "公司员工信息表");
            Console.WriteLine($"员工数据已导出到: {filePath}");
            
            // 3. 读取Excel文件转换回对象列表
            var importedEmployees = excel.ExcelToList<Employee>(filePath);
            Console.WriteLine($"成功导入 {importedEmployees?.Count ?? 0} 名员工信息");
            
            // 4. 验证数据一致性
            bool isConsistent = true;
            if (importedEmployees != null)
            {
                for (int i = 0; i < Math.Min(employees.Count, importedEmployees.Count); i++)
                {
                    if (employees[i].Id != importedEmployees[i].Id ||
                        employees[i].Name != importedEmployees[i].Name)
                    {
                        isConsistent = false;
                        break;
                    }
                }
            }
            Console.WriteLine($"数据一致性检查: {(isConsistent ? "通过" : "失败")}");
            
            // 5. 导出为DataTable
            var dataTable = new DataTable("员工信息");
            dataTable.Columns.Add("员工编号", typeof(string));
            dataTable.Columns.Add("姓名", typeof(string));
            dataTable.Columns.Add("部门", typeof(string));
            dataTable.Columns.Add("职位", typeof(string));
            
            foreach (var employee in employees.Take(10))
            {
                var row = dataTable.NewRow();
                row["员工编号"] = employee.Id;
                row["姓名"] = employee.Name;
                row["部门"] = employee.Department;
                row["职位"] = employee.Position;
                dataTable.Rows.Add(row);
            }
            
            filePath = Path.Combine(directory, $"简化员工表_{excel.GetType().Name}.xlsx");
            excel.DataTableToFile(dataTable, filePath, "简化员工信息", "公司员工简表");
            Console.WriteLine($"DataTable导出到: {filePath}");
        }
        
        static void DemoStreamProcessing(ClosedXmlExcel excel, string directory)
        {
            // 1. 生成大量测试数据
            var largeDataset = GenerateTestData(10000);
            
            // 2. 导出到Excel
            string filePath = Path.Combine(directory, "LargeEmployeeDataset.xlsx");
            excel.ListToFile(largeDataset, filePath, "大数据集");
            Console.WriteLine($"大数据集已导出到: {filePath}");
            
            // 3. 使用流式处理读取
            using (var stream = File.OpenRead(filePath))
            {
                Console.WriteLine("开始流式处理大文件...");
                int count = 0;
                
                foreach (var employee in excel.StreamReadExcel<Employee>(stream))
                {
                    // 在实际应用中，可以在此处处理每条记录
                    // 例如写入数据库、进行验证等
                    count++;
                    
                    // 为了演示，只处理前10条并显示
                    if (count <= 10)
                    {
                        Console.WriteLine($"处理员工: {employee.Id} - {employee.Name} ({employee.Department})");
                    }
                }
                
                Console.WriteLine($"流式处理完成，共处理 {count} 条记录");
            }
        }
        
        static void DemoTemplateGeneration(EPPlusExcel excel, string directory)
        {
            // 获取Employee类的列信息
            var columnInfos = ExcelTemplateGenerator.CreateColumnInfos<Employee>();
            
            Console.WriteLine("Employee模板列信息:");
            foreach (var column in columnInfos)
            {
                Console.WriteLine($"列名: {column.DisplayName} (属性: {column.PropertyName})");
                Console.WriteLine($"  类型: {column.PropertyType.Name}");
                Console.WriteLine($"  必填: {column.Required}");
                Console.WriteLine($"  顺序: {column.Order}");
                Console.WriteLine();
            }
            
            // 生成模板
            var templateStream = excel.CreateExcelTemplate<Employee>();
            var templatePath = Path.Combine(directory, "EmployeeTemplate.xlsx");
            
            using (var fileStream = File.Create(templatePath))
            {
                templateStream.CopyTo(fileStream);
            }
            
            Console.WriteLine($"模板已生成: {templatePath}");
        }
        
        static List<Employee> GenerateTestData(int count)
        {
            var departments = new[] { "技术部", "销售部", "市场部", "人力资源部", "财务部" };
            var positions = new[] { "经理", "主管", "专员", "助理" };
            var genders = new[] { "男", "女" };
            
            var random = new Random(42); // 固定种子，确保可重复结果
            var employees = new List<Employee>(count);
            
            for (int i = 0; i < count; i++)
            {
                var employee = new Employee
                {
                    Id = $"EMP{i+1:D5}",
                    Name = $"员工{i+1}",
                    Age = random.Next(22, 60),
                    Gender = genders[random.Next(genders.Length)],
                    Department = departments[random.Next(departments.Length)],
                    Position = positions[random.Next(positions.Length)],
                    JoinDate = DateTime.Now.AddDays(-random.Next(0, 3650)), // 过去10年内
                    BaseSalary = Math.Round((decimal)(random.NextDouble() * 15000 + 5000), 2),
                    IsActive = random.Next(10) > 1, // 90%概率在职
                    Remarks = $"备注信息{i+1}"
                };
                
                employees.Add(employee);
            }
            
            return employees;
        }
    }
    
    // 非泛型接口使用示例
    IExcelService excelService = new NpoiExcel();
    var data = new List<Employee> { /* ... */ };
    excelService.ListToFile(data, "employees.xlsx");

    // 获取特定功能
    if (excelService is IExcel<ISheet> npoiExcel)
    {
        // 使用NPOI特定的功能
        npoiExcel.ListToFile(data, "employees.xlsx", styleAction: sheet => {
            // 应用特定于NPOI的样式
        });
    }

    // 类型匹配更简单的方法
    var npoi = new NpoiExcel();
    npoi.ListToFile(data, "employees.xlsx", styleAction: sheet => {
        // 应用特定于NPOI的样式
    });
}
