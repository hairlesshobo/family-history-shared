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

using FoxHollow.FHM.Shared.Classes;

namespace FoxHollow.FHM.Shared.Models;

public class PhotoSidecarInformation_V1
{
    public FlexibleDate TakenDate { get; set; }
    public string Caption { get; set; }
    public string Note { get; set; }
    public string[] People { get; set; }
    public PhotoSidecarLocation_V1 Location { get; set; } = new PhotoSidecarLocation_V1();
}
