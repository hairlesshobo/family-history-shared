using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FoxHollow.FHM.Shared.Interop.Models;

namespace FoxHollow.FHM.Shared.Interop
{
    public class PyIdentifyCamera : PythonInterop<IdentifyCameraProgress, IdentifyCameraResult>
    {
        public PyIdentifyCamera(string mediaFilePath) 
            : base("identify-camera", Path.Combine(SysInfo.ConfigRoot, "profiles"), mediaFilePath)
        {

        }

        override public async Task<IdentifyCameraResult>RunAsync(CancellationToken ctk)
            => await this.RunInternalAsync(ctk);
    }
}