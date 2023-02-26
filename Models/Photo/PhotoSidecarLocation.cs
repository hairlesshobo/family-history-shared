using System;
using YamlDotNet.Serialization;

namespace FoxHollow.FHM.Shared.Models;

public class PhotoSidecarLocation
{
    // latitude: 
    // longitude:
    // name: Jeff's House, Jacksonville, FL

    public string Name { get; set; }
    public Nullable<double> Latitude { get; set; } = null;
    public Nullable<double> Longitude { get; set; } = null;
}