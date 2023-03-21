using System;

namespace FoxHollow.FHM.Shared.Models;

public class PhotoSidecarFormat_V1
{
    public string Format { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Colorspace { get; set; }
    public string Compression { get; set; }
    // public int Quality { get; set; }
    // public int Dpi { get; set; }
    public int Pages { get; set; }
}