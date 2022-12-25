using System;

namespace FoxHollow.FHM.Shared.Models;

public class RawVideoEventSceneSummary
{
    // 	- camera: Sony HVR-Z1U
    // 	  videos: 23
    // 	  duration: 00:08:31.12 # hh:mm:ss.ss
    public string Camera { get; set; }
    public int Videos { get; set; }
    public TimeSpan Duration { get; set; }
}