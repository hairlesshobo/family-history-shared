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
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoxHollow.FHM.Shared.Utilities;

public class MediainfoUtils
{
    private IServiceProvider _services;
    private ILogger _logger;

    public MediainfoUtils(IServiceProvider services)
    {
        _services = services;

        _logger = _services.GetRequiredService<ILogger<MediainfoUtils>>();
    }

    /// <summary>
    ///     Gets the MediaInfo as a JsonNode object, optionally using and creating
    ///     a `.mediainfo` sidecar, if useSidecar is enabled
    /// </summary>
    /// <param name="filePath">
    ///     Full path to the media file to read the mediainfo for
    /// </param>
    /// <param name="useSidecar">
    ///     If true (default), it will attempt to read a .mediainfo sidecar, if present, and 
    ///     create a sidecar if it does not already exist
    /// </param>
    /// <param name="refreshSidecar">
    ///     If true, the mediainfo sidecar will be refreshed, even if it already exists
    /// </param>
    /// <returns>Task that returns the JsonNode</returns>
    public async Task<JsonNode> GetMediainfoAsync(string filePath, bool useSidecar = true, bool refreshSidecar = false)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);

        // attempt to load an existing mediainfo sidecar, if present
        string sidecarPath = $"{filePath}.mediainfo";

        // Sidecar exists and we are instructed to use it
        if (useSidecar && !refreshSidecar && File.Exists(sidecarPath))
        {
            _logger.LogDebug("Using existing raw mediainfo sidecar");

            using (var sidecarFileHandle = File.Open(sidecarPath, FileMode.Open, FileAccess.Read))
                return await JsonSerializer.DeserializeAsync<JsonNode>(sidecarFileHandle);
        }
        else
        {
            _logger.LogDebug("Calling mediainfo");

            using (var process = new Process())
            {
                process.StartInfo.FileName = "mediainfo";
                process.StartInfo.ArgumentList.Add("--Output=JSON");
                process.StartInfo.ArgumentList.Add(filePath);
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();

                JsonNode mediainfo = await JsonSerializer.DeserializeAsync<JsonNode>(process.StandardOutput.BaseStream);

                if (useSidecar)
                {
                    _logger.LogDebug("Writing new raw mediainfo sidecar");
                    await File.WriteAllTextAsync(sidecarPath, mediainfo.ToJsonString(Static.DefaultJso));
                }

                await process.WaitForExitAsync();

                return mediainfo;
            }
        }
    }
}