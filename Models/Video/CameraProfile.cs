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
using System.Text.Json.Serialization;

namespace FoxHollow.FHM.Shared.Models.Video;

public class CameraProfile
{
    /*     
        "id": "sony_hvr_z1u",
        "commission_date": null,
        "decommission_date": null,
        "name": "Sony HVR-Z1U",
        "capture_person": "Steve Cross",
        "hints": []
    */

    [JsonPropertyName("id")]
    public string ID { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("commission_date")]
    public Nullable<DateTime> CommissionDate { get; set; }

    [JsonPropertyName("decommission_date")]
    public Nullable<DateTime> DecommissionDate { get; set; }

    [JsonPropertyName("capture_person")]
    public string CapturePerson { get; set; }

    [JsonPropertyName("hints")]
    public List<CameraHint> Hints { get; set; } = new List<CameraHint>();
}