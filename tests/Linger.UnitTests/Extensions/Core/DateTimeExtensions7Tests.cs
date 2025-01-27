using System.Globalization;

namespace Linger.UnitTests.Extensions.Core;

/// <summary>
/// 测试 GetYearWeekCount 方法的功能
/// </summary>
public class DateTimeExtensions7Tests
{
    private readonly ITestOutputHelper _outputHelper;

    public DateTimeExtensions7Tests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        // 重置默认文化信息
        CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
        CultureInfo.CurrentUICulture = new CultureInfo("en-US", false);
    }

    [Theory]
    [InlineData("en-US", 2023, 53)] // 美国文化
    [InlineData("de-DE", 2023, 52)] // 德国文化(ISO 8601)
    [InlineData("fr-FR", 2023, 52)] // 法国文化
    [InlineData("zh-CN", 2023, 53)] // 中国文化
    public void GetYearWeekCount_WithDifferentCultures_ReturnsExpectedWeeks(string cultureName, int year, int expected)
    {
        // Arrange
        var culture = CultureInfo.GetCultureInfo(cultureName);

        // Act
        var result = DateTimeExtensions.GetWeekCountOfYear(year, culture);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("de-DE", 2015, 53)] // ISO 8601, 2015年有53周
    [InlineData("de-DE", 2016, 52)]
    [InlineData("de-DE", 2020, 53)]
    public void GetYearWeekCount_WithISOStandard_ReturnsCorrectWeeks(string cultureName, int year, int expected)
    {
        // Arrange
        var culture = CultureInfo.GetCultureInfo(cultureName);

        // Act
        var result = DateTimeExtensions.GetWeekCountOfYear(year, culture);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetYearWeekCount_WithCustomCulture_ReturnsExpectedWeeks()
    {
        // Arrange
        var culture = new CultureInfo("en-US");
        culture.DateTimeFormat.FirstDayOfWeek = DayOfWeek.Monday;
        culture.DateTimeFormat.CalendarWeekRule = CalendarWeekRule.FirstFourDayWeek;

        // Act
        var result = DateTimeExtensions.GetWeekCountOfYear(2023, culture);

        // Assert
        Assert.Equal(52, result);
    }

    [Fact]
    public void GetYearWeekCount_WithNullCulture_UsesDefaultCulture()
    {
        // Arrange
        var year = 2023;
        var expectedWeeks = DateTimeExtensions.GetWeekCountOfYear(year, ExtensionMethodSetting.DefaultCulture);

        // Act
        var result = DateTimeExtensions.GetWeekCountOfYear(year);

        // Assert
        Assert.Equal(expectedWeeks, result);
    }

    [Theory]
    [InlineData(0)]  // 小于最小值
    [InlineData(10000)] // 大于最大值
    public void GetYearWeekCount_WithInvalidYear_ThrowsArgumentOutOfRangeException(int year)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            DateTimeExtensions.GetWeekCountOfYear(year));
        Assert.Equal("year", exception.ParamName);
    }

    [Fact]
    public void GetYearWeekCount_WithYearEndingInWeek1_ReturnsCorrectCount()
    {
        // Arrange
        var culture = CultureInfo.GetCultureInfo("de-DE"); // ISO 8601
        var year = 2016; // 2016-12-31 falls in week 1 of 2017

        // Act
        var result = DateTimeExtensions.GetWeekCountOfYear(year, culture);

        // Assert
        Assert.Equal(52, result);
    }

    /// <summary>
    /// 测试文化信息为null时是否使用默认文化信息
    /// </summary>
    [Fact]
    public void GetYearWeekCount_NullCulture_UsesDefaultCulture()
    {
        var result = DateTimeExtensions.GetWeekCountOfYear(2023, null);
        var expectedResult = DateTimeExtensions.GetWeekCountOfYear(2023, ExtensionMethodSetting.DefaultCulture);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("en-US", "2023-01-01", 1)]  // 美国文化 - 年初
    [InlineData("en-US", "2023-12-31", 53)] // 美国文化 - 年末
    [InlineData("de-DE", "2023-01-01", 52)] // 德国文化(ISO 8601) - 跨年周
    [InlineData("de-DE", "2023-12-31", 52)] // 德国文化(ISO 8601) - 年末
    [InlineData("fr-FR", "2023-01-01", 52)] // 法国文化 - 年初
    [InlineData("fr-FR", "2023-12-31", 52)] // 法国文化 - 年末
    [InlineData("zh-CN", "2023-01-01", 1)]  // 中国文化 - 年初
    [InlineData("zh-CN", "2023-12-31", 53)] // 中国文化 - 年末
    public void WeekOfYear_DifferentCultures_ReturnsCorrectWeek(
        string cultureName, string dateStr, int expectedWeek)
    {
        // Arrange
        var culture = CultureInfo.GetCultureInfo(cultureName);
        var date = DateTime.Parse(dateStr);

        // Act
        var result = date.WeekNumberOfYear(culture);

        // Assert
        Assert.Equal(expectedWeek, result);
    }

    [Theory]
    [InlineData("de-DE", "2020-12-28", 53)] // ISO周历年末
    [InlineData("de-DE", "2021-01-03", 53)] // ISO周跨年
    [InlineData("de-DE", "2021-01-04", 1)]  // ISO周新年
    public void WeekOfYear_ISOWeeks_HandlesSpecialCases(
        string cultureName, string dateStr, int expectedWeek)
    {
        // Arrange
        var culture = CultureInfo.GetCultureInfo(cultureName);
        var date = DateTime.Parse(dateStr);

        // Act
        var result = date.WeekNumberOfYear(culture);

        // Assert
        Assert.Equal(expectedWeek, result);
    }

    public static IEnumerable<object[]> GetMidYearWeekData2()
    {
        yield return new object[] { "en-US", "2023-07-01", 26 };
        yield return new object[] { "de-DE", "2023-07-01", 26 };
        yield return new object[] { "fr-FR", "2023-07-01", 26 };
#if NETFRAMEWORK
        // Framework使用ISO 8601标准
        yield return new object[] { "zh-CN", "2023-07-01", 27 };
#else
        // Core使用简化的日历周计算
        yield return new object[] { "zh-CN", "2023-07-01", 26 };
#endif
    }

    [Theory]
    [MemberData(nameof(GetMidYearWeekData2))]
    public void WeekOfYear_MidYear_ReturnsCorrectWeek(string cultureName, string dateStr, int expectedWeek)
    {
    // Arrange
    var culture = new CultureInfo(cultureName, false);
    // 先克隆DateTimeFormat，然后再设置给culture
    var dateTimeFormat = (DateTimeFormatInfo)CultureInfo.InvariantCulture.DateTimeFormat.Clone();
    culture = (CultureInfo)culture.Clone();
    culture.DateTimeFormat = dateTimeFormat;
        var date = DateTime.Parse(dateStr);

        _outputHelper.WriteLine($"Culture: {cultureName}");
        _outputHelper.WriteLine($"Date: {dateStr}");
        _outputHelper.WriteLine($"Runtime: {Environment.Version}");
        _outputHelper.WriteLine($"FirstDayOfWeek:{culture.DateTimeFormat.FirstDayOfWeek}");
        _outputHelper.WriteLine($"CalendarWeekRule:{culture.DateTimeFormat.CalendarWeekRule}");

        // Act
        var result = date.WeekNumberOfYear(culture);

        // Assert
        Assert.Equal(expectedWeek, result);
    }

    [Fact]
    public void WeekOfYear_CustomFirstDayOfWeek_ReturnsCorrectWeek()
    {
        // Arrange
        var culture = new CultureInfo("en-US");
        culture.DateTimeFormat.FirstDayOfWeek = DayOfWeek.Monday;
        culture.DateTimeFormat.CalendarWeekRule = CalendarWeekRule.FirstFourDayWeek;
        var date = new DateTime(2023, 1, 1);

        // Act
        var result = date.WeekNumberOfYear(culture);

        // Assert
        Assert.Equal(52, result); // 应该属于上一年的最后一周
    }

    [Fact]
    public void WeekOfYear_NullCulture_UsesDefaultCulture()
    {
        // Arrange
        var date = new DateTime(2023, 1, 1);
        var expectedResult = date.WeekNumberOfYear(ExtensionMethodSetting.DefaultCulture);

        // Act
        var result = date.WeekNumberOfYear(null);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("en-US", 2023, "2023-01-01")] // 美国文化 1月1日
    [InlineData("de-DE", 2023, "2023-01-02")] // 德国文化(ISO 8601) 1月2日
    [InlineData("fr-FR", 2023, "2023-01-02")] // 法国文化 1月2日
    [InlineData("zh-CN", 2023, "2023-01-01")]
    public void GetStartEndDayOfFirstWeek_DifferentCultures_ReturnsCorrectDate(
        string cultureName, int year, string expectedDateStr)
    {
        // Arrange
        var culture = CultureInfo.GetCultureInfo(cultureName);
        var expectedDate = DateTime.Parse(expectedDateStr);

        // Act
        var result = DateTimeExtensions.GetStartEndDayOfFirstWeek(year, culture);

        // Assert
        Assert.Equal(expectedDate, result.Start);
    }

    [Theory]
    [InlineData(2010, DayOfWeek.Sunday, CalendarWeekRule.FirstDay, "2010-01-01", "2010-01-02")]
    [InlineData(2010, DayOfWeek.Sunday, CalendarWeekRule.FirstFourDayWeek, "2010-01-03", "2010-01-09")]
    [InlineData(2010, DayOfWeek.Sunday, CalendarWeekRule.FirstFullWeek, "2010-01-03", "2010-01-09")]
    [InlineData(2010, DayOfWeek.Monday, CalendarWeekRule.FirstDay, "2010-01-01", "2010-01-03")]
    [InlineData(2010, DayOfWeek.Monday, CalendarWeekRule.FirstFourDayWeek, "2010-01-04", "2010-01-10")]
    [InlineData(2010, DayOfWeek.Monday, CalendarWeekRule.FirstFullWeek, "2010-01-04", "2010-01-10")]

    [InlineData(2013, DayOfWeek.Sunday, CalendarWeekRule.FirstDay, "2013-01-01", "2013-01-05")]
    [InlineData(2013, DayOfWeek.Sunday, CalendarWeekRule.FirstFourDayWeek, "2013-01-01", "2013-01-05")]
    [InlineData(2013, DayOfWeek.Sunday, CalendarWeekRule.FirstFullWeek, "2013-01-06", "2013-01-12")]
    [InlineData(2013, DayOfWeek.Monday, CalendarWeekRule.FirstDay, "2013-01-01", "2013-01-06")]
    [InlineData(2013, DayOfWeek.Monday, CalendarWeekRule.FirstFourDayWeek, "2013-01-01", "2013-01-06")]
    [InlineData(2013, DayOfWeek.Monday, CalendarWeekRule.FirstFullWeek, "2013-01-07", "2013-01-13")]

    [InlineData(2024, DayOfWeek.Sunday, CalendarWeekRule.FirstDay, "2024-01-01", "2024-01-06")]
    [InlineData(2024, DayOfWeek.Sunday, CalendarWeekRule.FirstFourDayWeek, "2024-01-01", "2024-01-06")]
    [InlineData(2024, DayOfWeek.Sunday, CalendarWeekRule.FirstFullWeek, "2024-01-07", "2024-01-13")]
    [InlineData(2024, DayOfWeek.Monday, CalendarWeekRule.FirstDay, "2024-01-01", "2024-01-07")]
    [InlineData(2024, DayOfWeek.Monday, CalendarWeekRule.FirstFourDayWeek, "2024-01-01", "2024-01-07")]
    [InlineData(2024, DayOfWeek.Monday, CalendarWeekRule.FirstFullWeek, "2024-01-01", "2024-01-07")]

    public void GetStartEndDayOfFirstWeek_WithSpecificRules_ReturnsCorrectDate(
        int year,
        DayOfWeek firstDayOfWeek,
        CalendarWeekRule calendarWeekRule,
        string expectedStartDateStr, string expectedEndDateStr)
    {
        // Arrange
        var expectedStartDate = DateTime.Parse(expectedStartDateStr);
        var expectedEndDate = DateTime.Parse(expectedEndDateStr);

        // Act
        var result = DateTimeExtensions.GetStartEndDayOfFirstWeek(year, firstDayOfWeek, calendarWeekRule);

        // Assert
        Assert.Equal(expectedStartDate, result.Start);
        Assert.Equal(expectedEndDate, result.End.Date);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10000)]
    public void GetFirstWeekStartOfYear_WithInvalidYear_ThrowsArgumentOutOfRangeException(int year)
    {
        // Arrange
        var firstDayOfWeek = DayOfWeek.Monday;
        var calendarWeekRule = CalendarWeekRule.FirstDay;

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            DateTimeExtensions.GetStartEndDayOfFirstWeek(year, firstDayOfWeek, calendarWeekRule));
        Assert.Equal("year", exception.ParamName);
    }

    [Fact]
    public void GetFirstWeekStartOfYear_NullCulture_UsesDefaultCulture()
    {
        // Arrange
        var year = 2023;
        var expectedDate = DateTimeExtensions.GetStartEndDayOfFirstWeek(year, ExtensionMethodSetting.DefaultCulture);

        // Act
        var result = DateTimeExtensions.GetStartEndDayOfFirstWeek(year, null);

        // Assert
        Assert.Equal(expectedDate, result);
    }

    [Theory]
    [InlineData("en-US", 2023, 1, "2023-01-01", "2023-01-07")] // 美国文化
    [InlineData("de-DE", 2023, 1, "2023-01-02", "2023-01-08")] // 德国文化(ISO 8601)
    [InlineData("fr-FR", 2023, 1, "2023-01-02", "2023-01-08")] // 法国文化
#if NETFRAMEWORK
    [InlineData("zh-CN", 2023, 1, "2023-01-01", "2023-01-01")] // 中国文化
#else
    [InlineData("zh-CN", 2023, 1, "2023-01-01", "2023-01-07")] // 中国文化
#endif
    public void GetFirstEndDayOfWeek_DifferentCultures_ReturnsCorrectDates(
    string cultureName, int year, int weekNumber, string expectedStart, string expectedEnd)
    {
        //在.NET Core/.NET 5 + 中，中国文化（zh - CN）的 FirstDayOfWeek 默认设置为 Sunday。
        //这是因为：
        //1.  .NET Core 重新实现了 Globalization 功能，使用了 ICU 库（International Components for Unicode）
        //2.ICU 库中对中国地区的默认设置是以周日作为一周的开始
        //这就解释了为什么同样的代码在不同的.NET 运行时中会有不同的行为：
        //•	.NET Framework：FirstDayOfWeek = Monday，Framework: CalendarWeekRule = FirstFourDayWeek(第一周最少要有4天)（基于 Windows NLS）
        //•	.NET Core /.NET 5 +：FirstDayOfWeek = Sunday，CalendarWeekRule = FirstDay(第一周从第一天开始)（基于 ICU）

        // Arrange
        var culture = CultureInfo.GetCultureInfo(cultureName);
        var expectedStartDate = DateTime.Parse(expectedStart);
        var expectedEndDate = DateTime.Parse(expectedEnd);

        // Act
#if NET40
    var result = DateTimeExtensions.GetFirstEndDayOfWeek(year, weekNumber, culture);
    var start = result.Item1;
    var end = result.Item2;
#else
        var (start, end) = DateTimeExtensions.GetStartEndDayOfWeek(year, weekNumber, culture);
#endif

        // Assert
        Assert.Equal(expectedStartDate, start.Date);
        Assert.Equal(expectedEndDate, end.Date);
    }

    [Fact]
    public void GetFirstEndDayOfWeek_CustomFirstDayOfWeek_ReturnsCorrectDates()
    {
        // Arrange
        var culture = new CultureInfo("en-US");
        culture.DateTimeFormat.FirstDayOfWeek = DayOfWeek.Monday;
        culture.DateTimeFormat.CalendarWeekRule = CalendarWeekRule.FirstFourDayWeek;

        // Act
#if NET40
    var result = DateTimeExtensions.GetFirstEndDayOfWeek(2023, 1, culture);
    var start = result.Item1;
    var end = result.Item2;
#else
        var (start, end) = DateTimeExtensions.GetStartEndDayOfWeek(2023, 1, culture);
#endif

        // Assert
        Assert.Equal(DayOfWeek.Monday, start.DayOfWeek);
        Assert.Equal(6, (end - start).Days);
    }

    [Theory]
    [MemberData(nameof(GetMidYearWeekData))]
    public void GetFirstEndDayOfWeek_MidYearWeeks_ReturnsCorrectDates(
        string cultureName, int year, int weekNumber, DateTime expectedStart, DateTime expectedEnd)
    {
        // Arrange
        var culture = CultureInfo.GetCultureInfo(cultureName);

        // Act
#if NET40
    var result = DateTimeExtensions.GetFirstEndDayOfWeek(year, weekNumber, culture);
    var start = result.Item1;
    var end = result.Item2;
#else
        var (start, end) = DateTimeExtensions.GetStartEndDayOfWeek(year, weekNumber, culture);
#endif

        // Assert
        Assert.Equal(expectedStart, start.Date);
        Assert.Equal(expectedEnd, end.Date);
    }

    public static IEnumerable<object[]> GetMidYearWeekData()
    {
        yield return new object[] { "en-US", 2023, 26, new DateTime(2023, 6, 25), new DateTime(2023, 7, 1) };
        yield return new object[] { "de-DE", 2023, 26, new DateTime(2023, 6, 26), new DateTime(2023, 7, 2) };
        yield return new object[] { "fr-FR", 2023, 26, new DateTime(2023, 6, 26), new DateTime(2023, 7, 2) };
    }

    /// <summary>
    /// 测试有效输入时是否返回正确的日期范围
    /// </summary>
    /// <param name="year">年份</param>
    /// <param name="weekNumber">周数</param>
    [Theory]
    [InlineData(2023, 1)]  // 第一周测试
    [InlineData(2023, 52)] // 最后一周测试
    public void GetFirstEndDayOfWeek_ValidInput_ReturnsCorrectRange(int year, int weekNumber)
    {
#if NET40
        var result = DateTimeExtensions.GetFirstEndDayOfWeek(year, weekNumber);
        var start = result.Item1;
        var end = result.Item2;
#else
        var (start, end) = DateTimeExtensions.GetStartEndDayOfWeek(year, weekNumber);
#endif

        Assert.Equal(6, (end - start).Days);
        Assert.Equal(DayOfWeek.Sunday, start.DayOfWeek);
        Assert.Equal(DayOfWeek.Saturday, end.DayOfWeek);
    }

    /// <summary>
    /// 测试无效输入时是否抛出正确的异常
    /// </summary>
    /// <param name="year">年份</param>
    /// <param name="weekNumber">周数</param>
    [Theory]
    [InlineData(0, 1)]     // 无效年份
    [InlineData(10000, 1)] // 无效年份
    [InlineData(2023, 0)]  // 无效周数
    [InlineData(2023, 54)] // 超出周数范围
    public void GetFirstEndDayOfWeek_InvalidInput_ThrowsArgumentOutOfRangeException(int year, int weekNumber)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DateTimeExtensions.GetStartEndDayOfWeek(year, weekNumber));
    }

    /// <summary>
    /// 测试文化信息为null时是否使用默认文化信息
    /// </summary>
    [Fact]
    public void GetFirstEndDayOfWeek_NullCulture_UsesDefaultCulture()
    {
#if NET40
        var result = DateTimeExtensions.GetFirstEndDayOfWeek(2023, 1, null);
        var expected = DateTimeExtensions.GetFirstEndDayOfWeek(2023, 1, ExtensionMethodSetting.DefaultCulture);
        Assert.Equal(expected.Item1, result.Item1);
        Assert.Equal(expected.Item2, result.Item2);
#else
        var result = DateTimeExtensions.GetStartEndDayOfWeek(2023, 1, null);
        var expected = DateTimeExtensions.GetStartEndDayOfWeek(2023, 1, ExtensionMethodSetting.DefaultCulture);
        Assert.Equal(expected.Start, result.Start);
        Assert.Equal(expected.End, result.End);
#endif
    }

    [Fact]
    public void GetFirstEndDayOfWeek_ShouldReturnCorrectDates_ForFirstFullWeek()
    {
        // Arrange
        var cultureInfo = new CultureInfo("en-US");
        cultureInfo.DateTimeFormat.CalendarWeekRule = CalendarWeekRule.FirstFullWeek;
        cultureInfo.DateTimeFormat.FirstDayOfWeek = DayOfWeek.Sunday;

        var year = 2023;
        var weekNumber = 1;

        // Act
        var (start, end) = DateTimeExtensions.GetStartEndDayOfWeek(year, weekNumber, cultureInfo);

        // Assert
        Assert.Equal(new DateTime(2023, 1, 1), start);
        Assert.Equal(new DateTime(2023, 1, 7, 23, 59, 59, 999), end);
    }
}