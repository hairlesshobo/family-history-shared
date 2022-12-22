using System.IO;

namespace FoxHollow.FHM.Shared.Models;

public class MediaFileEntry
{
    /// <summary>
    ///     Full path to the MediaFile
    /// </summary>
    public string Path { get; internal set; }

    /// <summary>
    ///     Folder depth this file exists, relative to the root directory of the 
    ///     <see cref="FoxHollow.FHM.Shared.Classes.TreeWalker" /> instance
    /// </summary>
    public int RelativeDepth { get; private set; }

    /// <summary>
    ///     Root path the <see cref="FoxHollow.FHM.Shared.Classes.TreeWalker" /> operates on
    /// </summary>
    public string RootPath { get; internal set; }

    /// <summary>
    ///     <see cref="System.IO.FileInfo" /> object describing the media file
    /// </summary>
    public FileInfo FileInfo { get; internal set; }

    /// <summary>
    ///     Flag that indicates whether this media file entry is considered to be "ignored"
    ///     by the <see cref="FoxHollow.FHM.Shared.Classes.TreeWalker" />.
    /// </summary>
    public bool Ignored { get; internal set; }

    /// <summary>
    ///     Collection to which this media file belongs
    /// </summary>
    public MediaFileCollection Collection { get; internal set; }

    
    /// <summary>
    ///     Constructor to be called by <see cref="FoxHollow.FHM.Shared.Classes.TreeWalker" />
    /// </summary>
    /// <param name="path">Full path to the media file</param>
    /// <param name="depth">Directory depth, relative to the root of the tree walker instance</param>
    internal MediaFileEntry(string path, int depth)
    {
        this.Path = path;
        this.RelativeDepth = depth;
    }

    /// <summary>
    ///     Custom string representation
    /// </summary>
    /// <returns>Name of the media file</returns>
    public override string ToString()
        => System.IO.Path.GetFileName(this.Path);
}