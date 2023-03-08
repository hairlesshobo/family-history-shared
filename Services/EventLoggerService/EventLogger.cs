using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoxHollow.FHM.Shared.Services;

#nullable enable

public sealed class EventLogger : ILogger
{
    private readonly string _name;
    private readonly Func<EventLoggerConfiguration> _getCurrentConfig;
    private IServiceProvider _services;
    private IEventLoggerEventService _loggerEventService;

    public EventLogger(IServiceProvider services, string name, Func<EventLoggerConfiguration> getCurrentConfig)
    {
        // => (_name, _getCurrentConfig) = (name, getCurrentConfig);

        _services = services;
        _name = name;
        _getCurrentConfig = getCurrentConfig;
        _loggerEventService = _services.GetRequiredService<IEventLoggerEventService>();
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) =>
        _getCurrentConfig().LogLevelToColorMap.ContainsKey(logLevel);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        EventLoggerConfiguration config = _getCurrentConfig();

        if (config.EventId == 0 || config.EventId == eventId.Id)
        {
            _loggerEventService.WriteLog("meow", $"{_name} - {formatter(state, exception)}" + Environment.NewLine);
        }


        //     Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
        //     Console.WriteLine($"[{eventId.Id,2}: {logLevel,-12}]");

        //     Console.ForegroundColor = originalColor;
        //     Console.Write($"     {_name} - ");

        //     Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
        //     Console.Write($"{formatter(state, exception)}");

        //     Console.ForegroundColor = originalColor;
        //     Console.WriteLine();
    }
}
