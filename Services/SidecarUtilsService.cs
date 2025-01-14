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
using FoxHollow.FHM.Shared.Utilities.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoxHollow.FHM.Shared.Services;

/// <summary>
///     Utilities for reading/writing sidecar files
/// </summary>
public class SidecarUtilsService
{
    private IServiceProvider _services;
    private ILogger _logger;

    /// <summary>
    ///     Constructor that requires DI container
    /// </summary>
    /// <param name="services">DI container</param>
    public SidecarUtilsService(IServiceProvider services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _logger = _services.GetRequiredService<ILogger<SidecarUtilsService>>();
    }

    /// <summary>
    ///     Write sidecar to specified file
    /// </summary>
    /// <param name="sidecarObject">Sidecar to write</param>
    /// <param name="filePath">Output path</param>
    /// <param name="overwrite">If true, the sidecar will be overwritten. Default: false</param>
    public void WriteToFile(object sidecarObject, string filePath, bool overwrite = false)
        => Yaml.DumpToFile(sidecarObject, filePath, overwrite);

    /// <summary>
    ///     Write sidecar for specified media file
    /// </summary>
    /// <param name="sidecarObject">Sidecar object to write</param>
    /// <param name="mediaPath">Path to the media file this sidecar belongs to</param>
    public void WriteSidecar(object sidecarObject, string mediaPath)
    {
        if (String.IsNullOrWhiteSpace(mediaPath) || !File.Exists(mediaPath))
            throw new FileNotFoundException("Cannot write media sidecar because the media file does not exist");

        this.WriteToFile(sidecarObject, this.GetSidecarPath(mediaPath), true);
    }

    /// <summary>
    ///     Get the path to the sidecar for the specified media file
    /// </summary>
    /// <param name="mediaPath">Path to the media file</param>
    /// <returns>Full path to the sidecar file</returns>
    public string GetSidecarPath(string mediaPath)
        => (!String.IsNullOrWhiteSpace(mediaPath) ? $"{mediaPath}.yaml" : null);

    /// <summary>
    ///     Generate a sidecar object from an existing sidecar file
    /// </summary>
    /// <param name="mediaPath">Path to the media file sidecar to read</param>
    /// <typeparam name="TSidecar">Type of sidecar</typeparam>
    /// <returns>New sidedcar object</returns>
    public TSidecar FromExisting<TSidecar>(string mediaPath)
    {
        var existingSidecarPath = this.GetSidecarPath(mediaPath);

        if (!File.Exists(existingSidecarPath))
            return default(TSidecar);

        var sidecar = Yaml.LoadFromFile<TSidecar>(existingSidecarPath);
        // sidecar.PhotoPath = mediaPath;

        return sidecar;
    }
}