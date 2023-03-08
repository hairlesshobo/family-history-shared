using System;

namespace FoxHollow.FHM.Shared.Services;

public class EventLoggerEventService : IEventLoggerEventService
{
    private event Action<string, string> _onLogMessage;

    public EventLoggerEventService()
    {
        _onLogMessage += delegate { };
    }

    public void RegisterLogDestination(Action<string, string> callback)
    {
        _onLogMessage += callback;
    }

    public void UnregisterLogDestination(Action<string, string> callback)
    {
        _onLogMessage -= callback;
    }

    public void WriteLog(string level, string message)
        => _onLogMessage(level, message);
}