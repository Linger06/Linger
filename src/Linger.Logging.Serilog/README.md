// 1. 添加包引用
using Linger.Logging;
using Linger.Logging.Serilog;

// 2. 配置日志
LingerLogger.Configure(options =>
{
    options.UseSerilog(config =>
    {
        config.MinimumLevel = LogLevel.Debug;
        config.EnableStructuredLogging = true;
        config.ConsoleTheme = new ConsoleThemeSettings
        {
            ErrorColor = ConsoleColor.Red,
            WarningColor = ConsoleColor.Yellow
        };
    })
    .SetSoftwareName("MyApp")
    .SetWriteToTempPath(true);
});

// 3. 使用日志
var logger = LingerLoggerFactory.CreateLogger<Program>();
logger.Information("应用程序启动");
