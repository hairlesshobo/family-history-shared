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