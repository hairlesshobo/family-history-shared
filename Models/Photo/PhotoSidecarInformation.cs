using System;
using System.Collections.Generic;
using FoxHollow.FHM.Shared.Classes;

namespace FoxHollow.FHM.Shared.Models;

public class PhotoSidecarInformation
{
    public FlexibleDate TakenDate { get; set; }
    public string Caption { get; set; }
    public string Note { get; set; }
    public string[] People { get; set; }
    public PhotoSidecarLocation Location { get; set; } = new PhotoSidecarLocation();
}
