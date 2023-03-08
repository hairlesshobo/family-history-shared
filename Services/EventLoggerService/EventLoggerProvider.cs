using System;
using System.Collections.Concurrent;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoxHollow.FHM.Shared.Services;

#nullable enable

[UnsupportedOSPlatform("browser")]
[ProviderAlias("TextBox")]
public sealed class EventLoggerProvider : ILoggerProvider
{
    private readonly IDisposable? _onChangeToken;
    private EventLoggerConfiguration _currentConfig;
    private IServiceProvider _services;
    private readonly ConcurrentDictionary<string, EventLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);

    public EventLoggerProvider(
        IOptionsMonitor<EventLoggerConfiguration> config,
        IServiceProvider services)
    {
        _currentConfig = config.CurrentValue;
        _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
        _services = services;
    }

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new EventLogger(_services, name, GetCurrentConfig));

    private EventLoggerConfiguration GetCurrentConfig() => _currentConfig;

    public void Dispose()
    {
        _loggers.Clear();
        _onChangeToken?.Dispose();
    }
}