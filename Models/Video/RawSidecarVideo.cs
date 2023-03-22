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

namespace FoxHollow.FHM.Shared.Models.Video;

public class RawSidecarVideo
{
    /*
        video_width: 1440
        video_height: 1080
        format: mpeg2
        format_name: HDV 1080i
        bitrate_mode: constant
        frame_rate: 29.970
        frame_count: 3628
        scan_type: interlaced
    */
    public uint VideoWidth { get; set; }
    public uint VideoHeight { get; set; }
    public string Format { get; set; }
    public string FormatName { get; set; }
    public BitrateMode BitrateMode { get; set; }
    public uint Bitrate { get; set; }
    public FramerateMode FramerateMode { get; set; }
    public double FrameRate { get; set; }
    public ulong FrameCount { get; set; }
    public ScanType ScanType { get; set; }
}