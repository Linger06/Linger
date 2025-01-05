// 创建Serilog配置
var config = new LingerLoggerConfiguration
{
    LoggerType = LoggerType.Serilog,
    SoftwareName = "MyApp",
    WriteToTempPath = true,
    SerilogOptions = new SerilogOptions
    {
        EnableStructuredLogging = true,
        MinimumLevel = LogLevel.Debug,
        EnableSeqLogging = true,
        SeqServerUrl = "http://localhost:5341",
        ConsoleTheme = new ConsoleThemeSettings
        {
            ErrorColor = ConsoleColor.Red,
            WarningColor = ConsoleColor.Yellow
        },
        CustomProperties = new Dictionary<string, string>
        {
            ["Environment"] = "Development",
            ["Application"] = "MyApp"
        }
    }
};

// 创建NLog配置
var nlogConfig = new LingerLoggerConfiguration
{
    LoggerType = LoggerType.NLog,
    NLogOptions = new NLogOptions
    {
        ConfigFilePath = "nlog.config",
        AutoReload = true,
        EnableInternalLog = true,
        InternalLogFile = "nlog-internal.log"
    }
};

// 创建Log4Net配置
var log4netConfig = new LingerLoggerConfiguration
{
    LoggerType = LoggerType.Log4Net,
    Log4NetOptions = new Log4NetOptions
    {
        ConfigFilePath = "log4net.config",
        WatchConfig = true,
        UseFullyQualifiedLoggerName = true
    }
};


// 使用Microsoft.Extensions.Logging
var msConfig = new LingerLoggerConfiguration
{
    LoggerType = LoggerType.MicrosoftLogging,
    EnableConsoleLogging = true,
    FileSizeLimitMB = 10,
    RetainedFileDays = 7
};

// 初始化
var config = new LingerLoggerConfiguration
{
    LoggerType = LoggerType.Serilog,
    SoftwareName = "MyApp",
    WriteToTempPath = true
};
LingerLoggerFactory.Initialize(config);

// 在类中使用
public class LocalFileSystem : ILocalFileSystem
{
    private readonly ILingerLogger _logger;

    public LocalFileSystem(string rootDirectoryPath)
    {
        _logger = LingerLoggerFactory.CreateLogger<LocalFileSystem>();
        // ...
    }

    public async Task<UploadedInfo> UploadAsync(...)
    {
        _logger.Information("开始上传文件: {FileName}", sourceFileName);
        try
        {
            // ...
        }
        catch (Exception ex)
        {
            _logger.Error("上传失败", ex);
            throw;
        }
    }
}

// Program.cs
public class Program
{
    public static void Main(string[] args)
    {
        // 加载配置
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("logging.json", optional: false)
            .Build()
            .Get<LingerLoggerConfiguration>();

        LingerLoggerFactory.Initialize(config);

        var builder = WebApplication.CreateBuilder(args);
        
        // 注册日志服务
        builder.Services.AddSingleton(typeof(ILingerLogger<>), typeof(LingerLogger<>));
        
        // ... 其他配置
    }
}

// 使用依赖注入
public class WeatherController : ControllerBase
{
    private readonly ILingerLogger<WeatherController> _logger;

    public WeatherController(ILingerLogger<WeatherController> logger)
    {
        _logger = logger;
    }
}


// App.xaml.cs
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        var config = LoadConfiguration();
        LingerLoggerFactory.Initialize(config);
        
        // 设置全局异常处理
        SetupExceptionHandling();
        
        base.OnStartup(e);
    }

    private LingerLoggerConfiguration LoadConfiguration()
    {
        var configPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, 
            "logging.json"
        );
        
        if (File.Exists(configPath))
        {
            return JsonSerializer.Deserialize<LingerLoggerConfiguration>(
                File.ReadAllText(configPath)
            );
        }

        return new LingerLoggerConfiguration
        {
            LoggerType = LoggerType.Serilog,
            WriteToTempPath = true,
            SoftwareName = "MyWpfApp"
        };
    }
}

public class MyLibraryClass
{
    private readonly ILingerLogger _logger;
    
    public MyLibraryClass()
    {
        _logger = LingerLoggerFactory.CreateLogger<MyLibraryClass>();
    }

    public void DoSomething()
    {
        _logger.Information("执行操作");
    }
}

// logging.json
{
  "LoggerType": "Serilog",
  "SoftwareName": "MyApp",
  "WriteToTempPath": true,
  "EnableConsoleLogging": true,
  "RetainedFileDays": 7,
  "FileSizeLimitMB": 10,
  
  "SerilogOptions": {
    "MinimumLevel": "Debug",
    "EnableStructuredLogging": true,
    "EnableSeqLogging": false,
    "ConsoleTheme": {
      "ErrorColor": "Red",
      "WarningColor": "Yellow"
    },
    "CustomProperties": {
      "Environment": "Development",
      "Application": "MyApp"
    }
  }
}

await _logger.LogOperationAsync("保存文件",
    async () => await SaveFileAsync(path),
    path);


    // 配置Serilog
LingerLogger.Configure(options => options
    .UseSerilog(config =>
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
    .SetWriteToTempPath(true)
    .SetEnableConsoleLogging(true)
    .SetFileSizeLimit(10)
    .SetRetainedDays(7)
);

// 配置NLog
LingerLogger.Configure(options => options
    .UseNLog(config =>
    {
        config.ConfigFilePath = "nlog.config";
        config.AutoReload = true;
        config.EnableInternalLog = true;
    })
    .SetSoftwareName("MyApp")
);

// 配置Log4Net
LingerLogger.Configure(options => options
    .UseLog4Net(config =>
    {
        config.ConfigFilePath = "log4net.config";
        config.WatchConfig = true;
    })
    .SetLogPath("Logs")
);

// 配置Microsoft.Extensions.Logging
LingerLogger.Configure(options => options
    .UseMicrosoftLogging()
    .SetEnableConsoleLogging(true)
    .SetFileSizeLimit(10)
    .SetRetainedDays(7)
);

