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

namespace FoxHollow.FHM.Shared.Models;

public class PhotoSidecarPreviewImage_V1
{
    /// <summary>
    ///     File extension this preview image is stored as
    /// </summary>
    public string FileExtension { get; set; }

    /// <summary>
    ///     MIME Type used for this preview image
    /// </summary>
    public string Format { get; set; }

    /// <summary>
    ///     Width of the preview image
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    ///     Height of the preview image
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    ///     Size, in bytes, of the preview image
    /// </summary>
    public long Size { get; set; }
}
