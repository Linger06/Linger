namespace Linger.Extensions.Core;

/// <summary>
/// <see cref="decimal"/> extensions for type conversion operations.
/// Implements the "Four Methods" pattern: ToTarget, ToTargetOrNull, ToTargetOrDefault, TryToTarget.
/// </summary>
public static class DecimalExtensions
{
    #region decimal -> int (Int32)

    /// <summary>
    /// 【标准布尔控流派】整个 decimal -> int 矩阵的唯一实质逻辑落地点（唯一核心大漏斗）。
    /// </summary>
    public static bool TryToInt(this decimal value, out int result)
    {
        result = 0;
        // 纯数学取模拦截小数，配合原生值域位宽高速比对，0 局部变量分配
        if (value % 1 != 0 || value < int.MinValue || value > int.MaxValue)
            return false;

        result = (int)value; // 纯内存级安全强转
        return true;
    }

    /// <summary>
    /// 【严格转换派】原生复盘。直接复用 Try 漏斗，失败时独立进行精准异常分类。
    /// </summary>
    public static int ToInt(this decimal value)
    {
        // 1. 成功路径：秒级闪过，0 堆栈开销
        if (value.TryToInt(out var result)) return result;

        // 2. 失败路径（二次复盘）：不重复调用外部方法，原地进行无损异常拆分
        if (value % 1 != 0)
            throw new InvalidCastException($"The value cannot be converted to Int32 because it contains a fractional part. value={value}");

        throw new OverflowException($"The value is outside the range of Int32. value={value}");
    }

    public static int? ToIntOrNull(this decimal value) => value.TryToInt(out var r) ? r : null;
    public static int ToIntOrDefault(this decimal value, int defaultValue = 0) => value.TryToInt(out var r) ? r : defaultValue;

    #endregion

    #region decimal -> long (Int64)

    /// <summary>
    /// 【标准布尔控流派】整个 decimal -> long 矩阵的唯一实质逻辑落地点（唯一核心大漏斗）。
    /// </summary>
    public static bool TryToLong(this decimal value, out long result)
    {
        result = 0L;
        if (value % 1 != 0 || value < long.MinValue || value > long.MaxValue)
            return false;

        result = (long)value;
        return true;
    }

    /// <summary>
    /// 【严格转换派】方案 A 原生复盘。
    /// </summary>
    public static long ToLong(this decimal value)
    {
        if (value.TryToLong(out var result)) return result;

        if (value % 1 != 0)
            throw new InvalidCastException($"The value cannot be converted to Int64 because it contains a fractional part. value={value}");

        throw new OverflowException($"The value is outside the range of Int64. value={value}");
    }

    public static long? ToLongOrNull(this decimal value) => value.TryToLong(out var r) ? r : null;
    public static long ToLongOrDefault(this decimal value, long defaultValue = 0L) => value.TryToLong(out var r) ? r : defaultValue;

    #endregion

    #region decimal -> short (Int16)

    /// <summary>
    /// 【标准布尔控流派】整个 decimal -> short 矩阵的唯一实质逻辑落地点（唯一核心大漏斗）。
    /// </summary>
    public static bool TryToShort(this decimal value, out short result)
    {
        result = 0;
        if (value % 1 != 0 || value < short.MinValue || value > short.MaxValue)
            return false;

        result = (short)value;
        return true;
    }

    /// <summary>
    /// 【严格转换派】原生复盘。
    /// </summary>
    public static short ToShort(this decimal value)
    {
        if (value.TryToShort(out var result)) return result;

        if (value % 1 != 0)
            throw new InvalidCastException($"The value cannot be converted to Int16 because it contains a fractional part. value={value}");

        throw new OverflowException($"The value is outside the range of Int16. value={value}");
    }

    public static short? ToShortOrNull(this decimal value) => value.TryToShort(out var r) ? r : null;
    public static short ToShortOrDefault(this decimal value, short defaultValue = 0) => value.TryToShort(out var r) ? r : defaultValue;

    #endregion

    #region Decimal Financial Helpers (财务高精度专项质检垫片)

    /// <summary>
    /// 【极简高性能纯数学质检】在原生硬件轨道无开销判定当前数值是否为纯整数
    /// </summary>
    public static bool IsInteger(this decimal value)
    {
        return value % 1 == 0;
    }

    /// <summary>
    /// 【全局唯一去零真理落地点】利用十进制因子缩放归一化机制，0 堆内存分配、绝对无损抹除尾随零
    /// </summary>
    public static decimal DeleteZero(this decimal value)
    {
        return value / 1.0000000000000000000000000000m;
    }

    /// <summary>
    /// 【彻底根除科学计数法大坑】单行复用唯一核心：先抹除数值尾随零，再强行锁定定点数十进制文本输出
    /// </summary>
    public static string ToStringDeleteZero(this decimal value)
    {
        // 1. 调用唯一数值核心 DeleteZero() 抹除尾随零
        // 2. 锁定 "0.############################" 纯十进制定点数输出，彻底粉碎任何科学计数法变身可能
        return value.DeleteZero().ToString("0.############################", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// 将可空十进制数值安全地转化为无尾随零的标准财务保真文本
    /// </summary>
    public static string? ToStringDeleteZero(this decimal? value)
    {
        // 单行流水线：直通核心，安全处理 null
        return value?.ToStringDeleteZero();
    }

    #endregion
}
