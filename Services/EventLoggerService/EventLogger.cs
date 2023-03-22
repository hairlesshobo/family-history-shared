//==========================================================================
//  Family History Manager - https://code.foxhollow.cc/fhm/
//
//  A cross platform tool to help organize and preserve all types
//  of family history
//==========================================================================
//  Copyright (c) 2020-2023 Steve Cross <flip@foxhollow.cc>
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//==========================================================================

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
