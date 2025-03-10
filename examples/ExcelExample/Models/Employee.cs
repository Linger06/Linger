using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Linger.Excel.Contracts.Attributes;

namespace ExcelExample.Models
{
    public class Employee
    {
        [ExcelColumn(ColumnName = "员工编号", Index = 0)]
        [Required]
        public string Id { get; set; }
        
        [ExcelColumn(ColumnName = "姓名", Index = 1)]
        [Required]
        public string Name { get; set; }
        
        [ExcelColumn(ColumnName = "性别", Index = 2)]
        public string Gender { get; set; }
        
        [ExcelColumn(ColumnName = "年龄", Index = 3)]
        public int Age { get; set; }
        
        [ExcelColumn(ColumnName = "入职日期", Index = 4)]
        [DisplayName("入职日期")]
        public DateTime JoinDate { get; set; }
        
        [ExcelColumn(ColumnName = "部门", Index = 5)]
        public string Department { get; set; }
        
        [ExcelColumn(ColumnName = "职位", Index = 6)]
        public string Position { get; set; }
        
        [ExcelColumn(ColumnName = "基本工资", Index = 7)]
        [DisplayName("基本工资")]
        public decimal BaseSalary { get; set; }
        
        [ExcelColumn(ColumnName = "是否在职", Index = 8)]
        public bool IsActive { get; set; }
        
        // 此字段不导出到Excel
        public string Remarks { get; set; }
    }
}
