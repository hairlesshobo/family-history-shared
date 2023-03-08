using System;
using Microsoft.Extensions.Logging;

namespace FoxHollow.FHM.Shared.Services;

public interface IEventLoggerEventService
{
    void RegisterLogDestination(Action<string, string> callback);
    void UnregisterLogDestination(Action<string, string> callback);

    void WriteLog(string level, string message);
}