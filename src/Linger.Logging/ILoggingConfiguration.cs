using Serilog.Events;

namespace Linger.Logging;

public interface ILoggingConfiguration
{
    Uri LoggingEndpoint { get; }
    bool EnableElasticLogging { get; }
    LogEventLevel ElasticLoggingLevel { get; }

    /// <summary>We only want Console logging to be set for debugging in general, always should be disabled for tests.</summary>
    bool EnableConsoleLogging { get; }

    /// <summary>
    ///     Used within Desktop apps to write to the Temp directory, When false will write to the application directory
    ///     (used for services)
    /// </summary>
    bool WriteToTempPath { get; }

    string SoftwareName { get; }
}