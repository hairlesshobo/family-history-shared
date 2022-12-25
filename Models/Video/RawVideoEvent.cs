using System;
using System.Collections.Generic;
using System.Security;

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