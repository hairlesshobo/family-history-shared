using System.IO;
using System.Text.Json.Serialization;
using FoxHollow.FHM.Shared.Classes;

namespace FoxHollow.FHM.Shared.Models;

public class MediaFileEntry
{
    /// <summary>
    ///     Name of the file
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    ///     Full path to the MediaFile
    /// </summary>
    public string Path { get; internal set; }

    /// <summary>
    ///     Folder depth this file exists, relative to the root directory of the 
    ///     <see cref="FoxHollow.FHM.Shared.Classes.MediaTreeWalker" /> instance
    /// </summary>
    public int RelativeDepth { get; internal set; }

    /// <summary>
    ///     Root path the <see cref="FoxHollow.FHM.Shared.Classes.MediaTreeWalker" /> operates on
    /// </summary>
    public string RootPath { get; internal set; }

    /// <summary>
    ///     <see cref="System.IO.FileInfo" /> object describing the media file
    /// </summary>
    [JsonIgnore]
    public FileInfo FileInfo { get; internal set; }

    /// <summary>
    ///     Flag that indicates whether this media file entry is considered to be "ignored"
    ///     by the <see cref="FoxHollow.FHM.Shared.Classes.MediaTreeWalker" />.
    /// </summary>
    public bool Ignored { get; internal set; }

    /// <summary>
    ///     Collection to which this media file belongs
    /// </summary>
    [JsonIgnore]
    public MediaFileCollection Collection { get; internal set; }

    /// <summary>
    ///     Custom string representation
    /// </summary>
    /// <returns>Name of the media file</returns>
    public override string ToString()
        => System.IO.Path.GetFileName(this.Path);

    public void RefreshFileInfo()
    {
        if (FileInfo != null)
            this.FileInfo = new FileInfo(this.FileInfo.FullName);
    }
}