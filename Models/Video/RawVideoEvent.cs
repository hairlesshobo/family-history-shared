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

namespace FoxHollow.FHM.Shared.Models;

public class RawVideoEvent
{
    // date: 2022-11-24
    // start_time: 16:38:22
    // end_time: 19:07:31
    // complete: true
    // auto_process: true
    // title: "Thanksgiving at Jeff's House"
    // people: # should be an accumulation of all people found in all scenes
    // scenes:
    // 	- camera: Sony HVR-Z1U
    // 	  videos: 23
    // 	  duration: 00:08:31.12 # hh:mm:ss.ss

    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool Complete { get; set; }
    public bool AutoProcess { get; set; }
    public string Title { get; set; }
    public List<string> People { get; set; } = new List<string>();
    public List<RawVideoEventSceneSummary> Scenes { get; set; } = new List<RawVideoEventSceneSummary>();
}