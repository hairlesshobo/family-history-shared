using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FoxHollow.FHM.Shared.Models
{
    /// <summary>
    ///     Collection containing one or more media files and any related sidecar files
    /// </summary>
    public class MediaFileCollection
    {
        /// <summary>
        ///     The name of the collection. This is the name of the root file without any extensions
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     DirectoryInfo object in which the media file collection resides
        /// </summary>
        public DirectoryInfo Directory { get; internal set; }

        /// <summary>
        ///     List of media file entries that belong to this collection. This is any media file,
        ///     metadata file, or sidecar file that share the same base name
        /// </summary>
        public List<MediaFileEntry> Entries { get; private set; } = new List<MediaFileEntry>();

        /// <summary>
        ///     Function that is called to move the entire collection to a new directory
        /// </summary>
        public Action<string> MoveCollection { get; internal set; } = (_) => {};

        /// <summary>
        ///     Function that is calle to rename the directory in which the collection resides.
        ///     This MUST be used instead of calling directory rename functions directly. This is
        ///     because this function is designed to "hotswap" the directory paths in the iteration
        ///     and keeps the app from throwing exceptions due to missing files
        /// </summary>
        public Action<string> RenameDir { get; internal set; } = (_) => {};

        /// <summary>
        ///     Constructor used to initialize a new media file collection, to be called by TreeWalker
        /// </summary>
        /// <param name="collectionName">Name of the collection</param>
        /// <param name="directoryPath">Full path to the directory in which the collection resides</param>
        internal MediaFileCollection(string collectionName, string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(collectionName))
                throw new ArgumentException($"'{nameof(collectionName)}' cannot be null or whitespace.", nameof(collectionName));

            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new ArgumentException($"'{nameof(directoryPath)}' cannot be null or whitespace.", nameof(directoryPath));

            this.Name = collectionName;
            this.Directory = new DirectoryInfo(directoryPath);
        }

        /// <summary>
        ///     Custom string representation
        /// </summary>
        /// <returns>CollectionName[number of entries]</returns>
        public override string ToString()
            => $"{this.Name}[{this.Entries.Count()}]";
    }
}