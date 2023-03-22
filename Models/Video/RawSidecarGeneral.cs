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

namespace FoxHollow.FHM.Shared.Models.Video;

public class RawSidecarGeneral
{
    //   file_name: Tape 1-2022_11_24-16_45_01.m2t
    //   container_format: MPEG-TS / HDV 1080i
    //   size: 395517220 # read from filesystem
    //   capture_dtm: 2022-11-24T16:45:01 # UTC
    //   duration: 00:02:01.48 # hh:mm:ss.ss
    public string FileName { get; set; }
    public DateTime FileModifyDtm { get; set; }
    public string ContainerFormat { get; set; }
    public long Size { get; set; }
    public DateTime CaptureDtm { get; set; }
    public TimeSpan Duration { get; set; }
}