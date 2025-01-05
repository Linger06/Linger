using Serilog.Events;

namespace Linger.Logging;

public class SerilogConfig : ILoggingConfiguration
{
    public Uri LoggingEndpoint { get; set; } = new("http://localhost:9200");
    public bool EnableConsoleLogging { get; } = true;
    public bool EnableElasticLogging { get; set; } = false;
    public bool WriteToTempPath { get; set; } = false;
    public string SoftwareName { get; set; } = "Log";
    public LogEventLevel ElasticLoggingLevel { get; set; } = LogEventLevel.Debug;
}