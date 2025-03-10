using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Linger.Excel.Contracts.Attributes;

namespace Linger.Excel.Tests.Models;

/// <summary>
/// 测试用的人员模型类
/// </summary>
public class TestPerson
{
    [ExcelColumn(ColumnName = "编号", Index = 0)]
    [Required]
    public int Id { get; set; }
    
    [ExcelColumn(ColumnName = "姓名", Index = 1)]
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [ExcelColumn(ColumnName = "生日", Index = 2)]
    [DisplayName("出生日期")]
    public DateTime Birthday { get; set; }
    
    [ExcelColumn(ColumnName = "薪资", Index = 3)]
    public decimal Salary { get; set; }
    
    [ExcelColumn(ColumnName = "是否在职", Index = 4)]
    public bool IsActive { get; set; }
    
    // 此属性不导出到Excel
    public string? Remark { get; set; }
    
    // 可为空的属性
    [ExcelColumn(ColumnName = "部门", Index = 5)]
    public string? Department { get; set; }
    
    [ExcelColumn(ColumnName = "入职日期", Index = 6)]
    public DateTime? JoinDate { get; set; }
}

/// <summary>
/// 测试用枚举类型
/// </summary>
public enum TestLevel
{
    Low = 0,
    Medium = 1,
    High = 2
}
