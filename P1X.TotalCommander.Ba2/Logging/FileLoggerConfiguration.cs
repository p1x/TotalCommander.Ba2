using Microsoft.Extensions.Logging;

namespace P1X.TotalCommander.Ba2;

public class FileLoggerConfiguration
{
    public LogLevel LogLevel { get; set; }
    public string? FilePath { get; set; }
}