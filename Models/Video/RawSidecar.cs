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

namespace FoxHollow.FHM.Shared.Models.Video;

public class RawSidecar
{
    internal string RawMediaPath { get; set; }
    private string RawSidecarPath => (!String.IsNullOrWhiteSpace(this.RawMediaPath) ? $"{this.RawMediaPath}.yaml" : null);
    private bool NewSidecar { get; set; } = false;

    public uint Version { get; set; } = 1;
    public DateTime GenerationDtm { get; set; } = DateTime.UtcNow;
    public RawSidecarGeneral General { get; set; } = new RawSidecarGeneral();
    public RawSidecarVideo Video { get; set; } = new RawSidecarVideo();
    public RawSidecarAudio Audio { get; set; } = new RawSidecarAudio();
    public RawSidecarHash Hash { get; set; } = new RawSidecarHash();
    public RawSidecarDetails Details { get; set; } = new RawSidecarDetails();

    public void WriteToFile(string filePath, bool overwrite = false)
        => Yaml.DumpToFile(this, filePath, overwrite);

    public void WriteSidecar()
    {
        if (String.IsNullOrWhiteSpace(RawMediaPath) || !File.Exists(RawMediaPath))
            throw new FileNotFoundException("Cannot write raw sidecar because the media file does not exist");

        this.WriteToFile(this.RawSidecarPath, true);
    }
}