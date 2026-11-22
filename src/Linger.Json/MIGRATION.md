# Migration to Linger.Json

In Linger 2.0, JSON-specific APIs move from `Linger.Utils` to `Linger.Json`. The public namespaces and API names are unchanged.

| Previous package | API | Replacement package | Required action |
| --- | --- | --- | --- |
| `Linger.Utils` | `JsonDefaults` | `Linger.Json` | Add the `Linger.Json` package reference. |
| `Linger.Utils` | `DateTimeConverter`, `DateTimeNullConverter`, `DataTableJsonConverter`, `DataSetConverter`, `JsonStringConverter` | `Linger.Json` | Add the `Linger.Json` package reference. |
| `Linger.Utils` | `string.ToDataTable()` | `Linger.Json` | Add the `Linger.Json` package reference. |
| `Linger.Utils` | `DataTable.ToJsonString()` | `Linger.Json` | Add the `Linger.Json` package reference. |
| `Linger.Utils` | `JsonElement.JsonElementToDataTable()` | `Linger.Json` | Add the `Linger.Json` package reference. |

The removed `JsonExtensions.ToJsonString(...)` and `JsonExtensions.Deserialize<T>(...)` convenience wrappers are not part of `Linger.Json`. Use `System.Text.Json.JsonSerializer` directly, preferably with source-generated `JsonTypeInfo<T>` or `JsonSerializerContext` in Native AOT applications.

The development branch fixes lossy DataTable/DataSet JSON conversion. Byte arrays are written as Base64 text and read as strings; column names are preserved if trimming would collide or yield an empty key; mixed numeric columns may now be `object`. Exports also skip `Deleted` rows without changing source row states. Check column casts and binary-field decoding before upgrading.

Breaking changes for other packages are documented in the [Linger migration guide](../Linger/MIGRATION.md).
