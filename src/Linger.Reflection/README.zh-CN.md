# Linger.Reflection

`Linger.Reflection` 是从 `Linger.Utils` 拆分出的可选反射与表达式树功能包，用于需要运行时元数据的业务场景。

## 安装

```bash
dotnet add package Linger.Reflection
```

该包会传递依赖 `Linger.Utils`；使用这些能力时通常只需引用 `Linger.Reflection`。

## 目标框架

- .NET 10.0
- .NET 9.0
- .NET 8.0
- .NET Standard 2.0
- .NET Framework 4.7.2

## 包含能力

- Lambda 表达式组合、条件筛选和谓词帮助方法
- 按属性名称进行动态 `IQueryable` 排序
- 运行时属性读取、属性枚举和元数据缓存
- 枚举的 `DescriptionAttribute` 与 `DisplayAttribute` 读取
- `IEnumerable<T>.ToDataTable()` 反射转换
- `DataTable.ToList<T>()` 反射映射

## 使用示例

```csharp
using System.Data;
using System.Linq.Expressions;
using Linger.Extensions;
using Linger.Extensions.Collection;
using Linger.Extensions.Core;
using Linger.Extensions.Data;

Expression<Func<User, bool>> activeUsers = x => x.IsActive;
Expression<Func<User, bool>> adults = x => x.Age >= 18;
Expression<Func<User, bool>> filter = activeUsers.And(adults);

IQueryable<User> ordered = users.AsQueryable().CreateOrderBy(nameof(User.Name));

string? name = user.GetPropertyValue(nameof(User.Name))?.ToString();
string description = Status.Active.GetDescription();

DataTable table = users.ToDataTable();
List<User>? mappedUsers = table.ToList<User>();
```

## AOT 与裁剪

本包在运行时发现类型成员，因此不适用于 Native AOT 或对裁剪敏感的路径。DataTable 转换请改用 `Linger.Utils` 中的显式 mapper/factory 重载；其他运行时元数据场景应使用编译期投影替代。

## 依赖方向

`Linger.Reflection` 依赖 `Linger.Utils`，`Linger.Utils` 不依赖 `Linger.Reflection`。
