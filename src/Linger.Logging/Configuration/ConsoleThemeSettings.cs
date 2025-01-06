namespace Linger.Logging.Configuration;

/// <summary>
/// 控制台主题设置
/// </summary>
public class ConsoleThemeSettings
{
    public ConsoleColor TextColor { get; set; } = ConsoleColor.White;
    public ConsoleColor DebugColor { get; set; } = ConsoleColor.Gray;
    public ConsoleColor InformationColor { get; set; } = ConsoleColor.White;
    public ConsoleColor WarningColor { get; set; } = ConsoleColor.Yellow;
    public ConsoleColor ErrorColor { get; set; } = ConsoleColor.Red;
    public ConsoleColor FatalColor { get; set; } = ConsoleColor.DarkRed;
}
