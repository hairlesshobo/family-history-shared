using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoxHollow.FHM.Shared.Utilities;

public class PhotoUtils
{
    private IServiceProvider _services;
    private ILogger _logger;

    public PhotoUtils(IServiceProvider services)
    {
        _services = services;
        _logger = services.GetRequiredService<ILogger<PhotoUtils>>();
    }

}
