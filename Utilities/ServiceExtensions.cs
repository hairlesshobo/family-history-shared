using System;
using FoxHollow.FHM.Shared.Classes;
using FoxHollow.FHM.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FoxHollow.FHM.Shared.Utilities;

public static class ServiceExtensions
{
    public static void AddFhmServices(this ServiceCollection collection)
    {
        collection.AddScoped<MediaTreeWalkerFactory>();
        collection.AddScoped<RawVideoUtils>();
        collection.AddScoped<PhotoUtils>();
        collection.AddScoped<MediainfoUtils>();
        collection.AddScoped<CamProfileService>();
    }
}