using System;

namespace FoxHollow.FHM.Shared.Models;

public class PhotoSidecarPreviews
{
    public string SourceImageMd5 { get; set; }
    public long SourceSize { get; set; }
    public DateTime GeneratedDtm { get; set; }
    public PhotoSidecarPreviewImage Thumbnail { get; set; } = new PhotoSidecarPreviewImage();
    public PhotoSidecarPreviewImage Preview { get; set; } = new PhotoSidecarPreviewImage();
}
