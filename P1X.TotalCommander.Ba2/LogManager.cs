using Microsoft.Extensions.Logging;

namespace P1X.TotalCommander.Ba2;

public static class LogManager
{
    private static ILogger? _globalLogger;
    private static ILoggerFactory? _loggerFactory;

    public static void SetLoggerFactory(ILoggerFactory loggerFactory, string categoryName)
    {
        _loggerFactory = loggerFactory;
        _globalLogger = loggerFactory.CreateLogger(categoryName);
    }

    public static ILogger? Logger => _globalLogger;

    public static ILogger<T> GetLogger<T>() where T : class => (_loggerFactory ?? throw new InvalidOperationException()).CreateLogger<T>();
    public static ILogger GetLogger(string categoryName) => (_loggerFactory ?? throw new InvalidOperationException()).CreateLogger(categoryName);
}