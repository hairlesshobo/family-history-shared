using System;

namespace FoxHollow.FHM.Shared.Models;

public class PhotoSidecarPreviews_V1
{
    public string SourceImageMd5 { get; set; }
    public long SourceSize { get; set; }
    public DateTime GeneratedDtm { get; set; }
    public PhotoSidecarPreviewImage_V1 Thumbnail { get; set; } = new PhotoSidecarPreviewImage_V1();
    public PhotoSidecarPreviewImage_V1 Preview { get; set; } = new PhotoSidecarPreviewImage_V1();
}
