using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FoxHollow.FHM.Shared.Interop.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoxHollow.FHM.Shared.Interop
{
    public class PyIdentifyCamera : PythonInterop<IdentifyCameraProgress, IdentifyCameraResult>
    {
        public PyIdentifyCamera(IServiceProvider services, string mediaFilePath) 
            : base(services, "identify-camera", Path.Combine(SysInfo.ConfigRoot, "profiles"), mediaFilePath)
        {
            _logger = services.GetRequiredService<ILogger<PyIdentifyCamera>>();
        }

        override public async Task<IdentifyCameraResult>RunAsync(CancellationToken ctk)
            => await this.RunInternalAsync(ctk);
    }
}