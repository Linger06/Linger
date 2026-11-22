# 迁移到 Linger.Json

在 Linger 2.0 中，JSON 专用 API 从 `Linger.Utils` 拆分到 `Linger.Json`。公共命名空间和 API 名称不变。

| 原包 | API | 新包 | 所需操作 |
| --- | --- | --- | --- |
| `Linger.Utils` | `JsonDefaults` | `Linger.Json` | 添加 `Linger.Json` 包引用。 |
| `Linger.Utils` | `DateTimeConverter`、`DateTimeNullConverter`、`DataTableJsonConverter`、`DataSetConverter`、`JsonStringConverter` | `Linger.Json` | 添加 `Linger.Json` 包引用。 |
| `Linger.Utils` | `string.ToDataTable()` | `Linger.Json` | 添加 `Linger.Json` 包引用。 |
| `Linger.Utils` | `DataTable.ToJsonString()` | `Linger.Json` | 添加 `Linger.Json` 包引用。 |
| `Linger.Utils` | `JsonElement.JsonElementToDataTable()` | `Linger.Json` | 添加 `Linger.Json` 包引用。 |

已删除的 `JsonExtensions.ToJsonString(...)` 与 `JsonExtensions.Deserialize<T>(...)` 便捷包装方法不会迁入 `Linger.Json`。请直接使用 `System.Text.Json.JsonSerializer`；Native AOT 应用应优先使用源生成的 `JsonTypeInfo<T>` 或 `JsonSerializerContext`。

开发分支的 DataTable/DataSet JSON 已修复二进制、列名及混合数值的有损转换。`byte[]` 写为 Base64 文本，读回仍是字符串；列名裁剪发生碰撞或变为空键时保留原名；混合数值列可能为 `object`。导出还会跳过 `Deleted` 行，不改变源表行状态。升级前请检查按列类型强转和反序列化二进制的代码。

其他包的破坏性变更与 2.0 迁移说明见 [Linger 迁移指南](../Linger/MIGRATION.zh-CN.md)。
