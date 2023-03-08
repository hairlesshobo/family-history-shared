using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace FoxHollow.FHM.Shared.Services;

public static class EventLoggerExtensions
{
    public static ILoggingBuilder AddEventLogger(
        this ILoggingBuilder builder)
    {
        builder.AddConfiguration();

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, EventLoggerProvider>());

        LoggerProviderOptions.RegisterProviderOptions
            <EventLoggerConfiguration, EventLoggerProvider>(builder.Services);

        return builder;
    }

    public static ILoggingBuilder AddEventLogger(
        this ILoggingBuilder builder,
        Action<EventLoggerConfiguration> configure)
    {
        builder.AddEventLogger();
        builder.Services.Configure(configure);

        return builder;
    }
}