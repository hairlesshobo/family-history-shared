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

namespace FoxHollow.FHM.Shared.Models;

public class PhotoSidecar_V2
{
    // private string PhotoPath { get; set; }
    // private string PhotoSidecarPath => SidecarUtilsService.GetSidecarPath(this.PhotoPath);
    private bool NewSidecar { get; set; } = false;

    #region Public Properties
    public uint Version { get; set; } = 2;
    public DateTime GeneratedDtm { get; set; } = DateTime.UtcNow;

    public PhotoSidecarInformation_V1 Information { get; set; } = new PhotoSidecarInformation_V2();
    public PhotoSidecarGeneral_V1 General { get; set; } = new PhotoSidecarGeneral_V2();
    public PhotoSidecarFormat_V1 Format { get; set; } = new PhotoSidecarFormat_V2();
    public PhotoSidecarHash_V1 Hash { get; set; } = new PhotoSidecarHash_V2();
    public PhotoSidecarInferred_V2 Inferred { get; set; } = new PhotoSidecarInferred_V2();
    public PhotoSidecarPreviews_V1 Previews { get; set; } = new PhotoSidecarPreviews_V2();
    #endregion Public Properties

    public PhotoSidecar_V2() { }

    public PhotoSidecar_V2(string photoPath)
    {
        // this.PhotoPath = photoPath;
        this.NewSidecar = true;
    }
}