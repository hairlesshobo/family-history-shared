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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using FoxHollow.FHM.Shared.Models.Video;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoxHollow.FHM.Shared.Services;

public class CamProfileService
{
    private IServiceProvider _services;
    private ILogger _logger;
    private List<CameraProfile> _profiles;

    public IReadOnlyList<CameraProfile> Profiles => (IReadOnlyList<CameraProfile>)_profiles;

    public IReadOnlyList<string> CamNames => (IReadOnlyList<string>)this.Profiles.Select(x => x.Name);

    public CamProfileService(IServiceProvider services)
    {
        _services = services;
        _logger = _services.GetRequiredService<ILogger<CamProfileService>>();

        _profiles = this.LoadCameraProfiles();
    }

    private List<CameraProfile> LoadCameraProfiles()
    {
        var profiles = new List<CameraProfile>();
        var profileDirPath = Path.Join(SysInfo.ConfigRoot, "profiles");
        var filesPaths = Directory.GetFiles(profileDirPath, "*.json");

        foreach (var filePath in filesPaths)
        {
            using (var fileHandle = File.OpenRead(filePath))
            {
                var camProfile = JsonSerializer.Deserialize<CameraProfile>(fileHandle, Static.DefaultJso);
                profiles.Add(camProfile);
            }
        }

        return profiles;
    }
}