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

/// <summary>
///     Class that represents a sidecar file for a photo, version 1
/// </summary>
public class PhotoSidecar_V1
{
    // private string PhotoPath { get; set; }
    // private string PhotoSidecarPath => SidecarUtilsService.GetSidecarPath(this.PhotoPath);
    
    /// <summary>
    ///     Private field that represents that this sidecar is new and has not yet
    ///     been written to a file
    /// </summary>
    /// <value></value>
    private bool NewSidecar { get; set; } = false;

    #region Public Properties
    /// <summary>
    ///     The version of this sidecar
    /// </summary>
    public uint Version { get; set; } = 1;

    /// <summary>
    ///     Most recent date and time that this sidecar was generated
    /// </summary>
    public DateTime GeneratedDtm { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Object that describes the visual contents of this photograph
    /// </summary>
    public PhotoSidecarInformation_V1 Information { get; set; } = new PhotoSidecarInformation_V1();
    
    public PhotoSidecarGeneral_V1 General { get; set; } = new PhotoSidecarGeneral_V1();
    public PhotoSidecarFormat_V1 Format { get; set; } = new PhotoSidecarFormat_V1();
    public PhotoSidecarHash_V1 Hash { get; set; } = new PhotoSidecarHash_V1();
    public PhotoSidecarPreviews_V1 Previews { get; set; } = new PhotoSidecarPreviews_V1();
    #endregion Public Properties

    public PhotoSidecar_V1() { }

    public PhotoSidecar_V1(string photoPath)
    {
        // this.PhotoPath = photoPath;
        this.NewSidecar = true;
    }
}