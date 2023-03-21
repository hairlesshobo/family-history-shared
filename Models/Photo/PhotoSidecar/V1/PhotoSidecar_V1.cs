using System;
using System.IO;
using FoxHollow.FHM.Shared.Services;
using FoxHollow.FHM.Shared.Utilities.Serialization;

namespace FoxHollow.FHM.Shared.Models;

public class PhotoSidecar_V1
{
    // private string PhotoPath { get; set; }
    // private string PhotoSidecarPath => SidecarUtilsService.GetSidecarPath(this.PhotoPath);
    private bool NewSidecar { get; set; } = false;

    #region Public Properties
    public uint Version { get; set; } = 1;
    public DateTime GeneratedDtm { get; set; } = DateTime.UtcNow;

    public PhotoSidecarInformation_V1 Information { get; set; } = new PhotoSidecarInformation_V1();
    public PhotoSidecarGeneral_V1 General { get; set; } = new PhotoSidecarGeneral_V1();
    public PhotoSidecarFormat_V1 Format { get; set; } = new PhotoSidecarFormat_V1();
    public PhotoSidecarHash_V1 Hash { get; set; } = new PhotoSidecarHash_V1();
    public PhotoSidecarPreviews_V1 Previews { get; set; } = new PhotoSidecarPreviews_V1();
    #endregion Public Properties

    public PhotoSidecar_V1() { }

    public PhotoSidecar_V1(string photoPath)
    {
        // this.PhotoPath = photoPath;
        this.NewSidecar = true;
    }
}