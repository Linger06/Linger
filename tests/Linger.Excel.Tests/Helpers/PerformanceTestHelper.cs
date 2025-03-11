using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Linger.Excel.Contracts;
using Microsoft.Extensions.Logging;
using Moq;

namespace Linger.Excel.Tests.Helpers
{
    public static class PerformanceTestHelper
    {
        /// <summary>
        /// 创建启用了性能监控的Excel选项
        /// </summary>
        public static ExcelOptions CreatePerformanceEnabledOptions(int threshold = 10)
        {
            return new ExcelOptions
            {
                EnablePerformanceMonitoring = true,
                PerformanceThreshold = threshold,
                ParallelProcessingThreshold = 100 // 降低并行阈值以便于测试
            };
        }

        /// <summary>
        /// 创建可捕获日志的Mock Logger
        /// </summary>
        public static (Mock<ILogger<T>> MockLogger, List<string> LogMessages) CreateMockLogger<T>()
        {
            var logMessages = new List<string>();
            var mockLogger = new Mock<ILogger<T>>();

            mockLogger.Setup(l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            )).Callback<LogLevel, EventId, object, Exception, Func<object, Exception, string>>(
                (level, id, state, ex, formatter) => 
                {
                    logMessages.Add(formatter(state, ex));
                }
            );
            
            return (mockLogger, logMessages);
        }

        /// <summary>
        /// 生成测试用的大型数据表
        /// </summary>
        public static DataTable GenerateLargeDataTable(int rowCount)
        {
            var dt = new DataTable("TestTable");
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Date", typeof(DateTime));
            dt.Columns.Add("Value", typeof(decimal));
            dt.Columns.Add("IsActive", typeof(bool));

            for (int i = 0; i < rowCount; i++)
            {
                dt.Rows.Add(
                    i,
                    $"Item {i}",
                    DateTime.Now.AddDays(-i),
                    100.50m + i,
                    i % 2 == 0
                );
            }

            return dt;
        }

        /// <summary>
        /// 测量函数执行时间
        /// </summary>
        public static TimeSpan MeasureExecutionTime(Action action)
        {
            var sw = Stopwatch.StartNew();
            action();
            sw.Stop();
            return sw.Elapsed;
        }
    }
}
