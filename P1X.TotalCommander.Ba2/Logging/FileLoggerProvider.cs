using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace P1X.TotalCommander.Ba2;

public class FileLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, ILogger> _loggers = new();
    private readonly BlockingCollection<string> _queue = new();
    private readonly Thread? _thread;
    private readonly FileLoggerConfiguration _configuration;

    private bool _killThread;

    public FileLoggerProvider(FileLoggerConfiguration configuration)
    {
        _configuration = configuration;
        var filePath = _configuration.FilePath;
        if (filePath == null)
            return;
		
        _thread = new Thread(() =>
        {
            var fileDate = DateOnly.FromDateTime(DateTime.Now);
            var streamWriter = File.AppendText(GetPath(filePath));
            try
            {
                foreach (var message in _queue.GetConsumingEnumerable())
                {
                    if (_killThread)
                        return;

                    if (DateOnly.FromDateTime(DateTime.Now) != fileDate)
                    {
                        streamWriter?.Dispose();
                        streamWriter = File.AppendText(GetPath(filePath));
                    }

                    streamWriter.WriteLine(message);
                    streamWriter.Flush(); 
                }
            }
            finally
            {
                streamWriter?.Dispose();
            }
        })
        {
            Priority = ThreadPriority.BelowNormal,
            IsBackground = true
        };
		    
        _thread.Start();
    }

    private static string GetPath(string filePath) => filePath.Replace("<date>", DateOnly.FromDateTime(DateTime.Now).ToString("yyyyMMdd"));

    public void Dispose()
    {
        _killThread = true;
        _thread?.Join();
    }

    public ILogger CreateLogger(string categoryName) => _loggers.GetOrAdd(categoryName, n => new FileLogger(n, _configuration, x => _queue.Add(x)));
}