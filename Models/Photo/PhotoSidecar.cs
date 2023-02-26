using System;
using System.IO;
using FoxHollow.FHM.Shared.Utilities.Serialization;

namespace FoxHollow.FHM.Shared.Models;

public class PhotoSidecar
{
    private string PhotoPath { get; set; }
    private string PhotoSidecarPath => PhotoSidecar.GetSidecarPath(this.PhotoPath);
    private bool NewSidecar { get; set; } = false;

    #region Public Properties
    public uint Version { get; set; } = 1;
    public DateTime GeneratedDtm { get; set; } = DateTime.UtcNow;

    public PhotoSidecarInformation Information { get; set; } = new PhotoSidecarInformation();
    public PhotoSidecarGeneral General { get; set; } = new PhotoSidecarGeneral();
    public PhotoSidecarFormat Format { get; set; } = new PhotoSidecarFormat();
    public PhotoSidecarHash Hash { get; set; } = new PhotoSidecarHash();
    public PhotoSidecarPreviews Previews { get; set; } = new PhotoSidecarPreviews();
    #endregion Public Properties

    public PhotoSidecar() { }

    public PhotoSidecar(string photoPath)
    {
        this.PhotoPath = photoPath;
        this.NewSidecar = true;
    }

    public bool IsNewSidecar() => this.NewSidecar;

    public void WriteToFile(string filePath, bool overwrite = false)
        => Yaml.DumpToFile(this, filePath, overwrite);

    public void WriteSidecar()
    {
        if (String.IsNullOrWhiteSpace(PhotoPath) || !File.Exists(PhotoPath))
            throw new FileNotFoundException("Cannot write photo sidecar because the photo file does not exist");

        this.WriteToFile(this.PhotoSidecarPath, true);
    }

    private static string GetSidecarPath(string photoPath)
        => (!String.IsNullOrWhiteSpace(photoPath) ? $"{photoPath}.yaml" : null);

    public static PhotoSidecar FromExisting(string photoPath)
    {
        var existingSidecarPath = PhotoSidecar.GetSidecarPath(photoPath);

        if (!File.Exists(existingSidecarPath))
            return null;

        var sidecar = Yaml.LoadFromFile<PhotoSidecar>(existingSidecarPath);
        sidecar.PhotoPath = photoPath;

        return sidecar;
    }
}