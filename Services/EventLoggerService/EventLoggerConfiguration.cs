using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace FoxHollow.FHM.Shared.Services;

public sealed class EventLoggerConfiguration
{
    public int EventId { get; set; }

    public Dictionary<LogLevel, ConsoleColor> LogLevelToColorMap { get; set; } = new()
    {
        [LogLevel.Information] = ConsoleColor.Green
    };
}