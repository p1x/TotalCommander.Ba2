using Microsoft.Extensions.Logging;

namespace P1X.TotalCommander.Ba2;

public class FileLogger(string name, FileLoggerConfiguration configuration, Action<string> append) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= configuration.LogLevel;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = $"{DateTime.Now:O} [{logLevel,-12}] {name} : {formatter(state, exception)}";
        if (exception != null)
            message += $"\r\n{exception}";
        
        append(message);
    }
}