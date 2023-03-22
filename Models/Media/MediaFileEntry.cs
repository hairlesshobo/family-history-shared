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

using System.IO;
using System.Text.Json.Serialization;

namespace FoxHollow.FHM.Shared.Models;

public class MediaFileEntry
{
    /// <summary>
    ///     Name of the file
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    ///     Full path to the MediaFile
    /// </summary>
    public string Path { get; internal set; }

    /// <summary>
    ///     Folder depth this file exists, relative to the root directory of the 
    ///     <see cref="FoxHollow.FHM.Shared.Classes.MediaTreeWalker" /> instance
    /// </summary>
    public int RelativeDepth { get; internal set; }

    /// <summary>
    ///     Root path the <see cref="FoxHollow.FHM.Shared.Classes.MediaTreeWalker" /> operates on
    /// </summary>
    public string RootPath { get; internal set; }

    /// <summary>
    ///     <see cref="System.IO.FileInfo" /> object describing the media file
    /// </summary>
    [JsonIgnore]
    public FileInfo FileInfo { get; internal set; }

    /// <summary>
    ///     Flag that indicates whether this media file entry is considered to be "ignored"
    ///     by the <see cref="FoxHollow.FHM.Shared.Classes.MediaTreeWalker" />.
    /// </summary>
    public bool Ignored { get; internal set; }

    /// <summary>
    ///     Collection to which this media file belongs
    /// </summary>
    [JsonIgnore]
    public MediaFileCollection Collection { get; internal set; }

    /// <summary>
    ///     Custom string representation
    /// </summary>
    /// <returns>Name of the media file</returns>
    public override string ToString()
        => System.IO.Path.GetFileName(this.Path);

    public void RefreshFileInfo()
    {
        if (FileInfo != null)
            this.FileInfo = new FileInfo(this.FileInfo.FullName);
    }
}