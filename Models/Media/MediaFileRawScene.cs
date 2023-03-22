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
using System.Linq;
using FoxHollow.FHM.Shared.Models.Video;
using FoxHollow.FHM.Shared.Services;
using FoxHollow.FHM.Shared.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace FoxHollow.FHM.Shared.Models;

public class MediaFileRawScene
{
    public string Path { get; set; }
    public string RootRelativePath { get; set; }
    public DirectoryInfo DirectoryInfo { get; set; }
    public string CameraName { get; set; }
    public CameraProfile CameraProfile { get; set; }

    internal MediaFileRawScene(IServiceProvider services, string rootPath, string path)
    {
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException(path);

        var camProfileService = services.GetRequiredService<CamProfileService>();

        this.Path = PathUtils.CleanPath(path);
        this.RootRelativePath = PathUtils.GetRootRelativePath(rootPath, path);
        this.DirectoryInfo = new DirectoryInfo(this.Path);
        this.CameraName = this.DirectoryInfo.Name;
        this.CameraProfile = camProfileService.Profiles.FirstOrDefault(x => x.Name == this.CameraName);
    }
}