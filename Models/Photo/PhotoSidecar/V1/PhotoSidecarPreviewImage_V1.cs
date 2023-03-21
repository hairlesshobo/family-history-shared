using System;

namespace FoxHollow.FHM.Shared.Models;

public class PhotoSidecarPreviewImage_V1
{
    /// <summary>
    ///     File extension this preview image is stored as
    /// </summary>
    public string FileExtension { get; set; }

    /// <summary>
    ///     MIME Type used for this preview image
    /// </summary>
    public string Format { get; set; }

    /// <summary>
    ///     Width of the preview image
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    ///     Height of the preview image
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    ///     Size, in bytes, of the preview image
    /// </summary>
    public long Size { get; set; }
}
