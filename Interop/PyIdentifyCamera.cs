using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FoxHollow.FHM.Shared.Interop.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoxHollow.FHM.Shared.Interop;

/// <summary>
///     Make a call to the Python interop process to identify the camera used to
///     capture the specified video
/// </summary>
public class PyIdentifyCamera : PythonInterop<IdentifyCameraProgress, IdentifyCameraResult>
{
    /// <summary>
    ///     Create a new instance of the camera identifier
    /// </summary>
    /// <param name="services">DI services container</param>
    /// <param name="mediaFilePath">Path to the video file to be identified</param>
    public PyIdentifyCamera(IServiceProvider services, string mediaFilePath)
        : base(services, "identify-camera", Path.Combine(SysInfo.ConfigRoot, "profiles"), mediaFilePath)
    {
        _logger = services.GetRequiredService<ILogger<PyIdentifyCamera>>();
    }

    /// <inheritdoc />
    override public async Task<IdentifyCameraResult> RunAsync(CancellationToken ctk)
        => await this.RunInternalAsync(ctk);
}