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