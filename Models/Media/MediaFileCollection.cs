using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using FoxHollow.FHM.Shared.Utilities;
using Microsoft.VisualBasic;

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
        [JsonIgnore]
        public DirectoryInfo Directory { get; internal set; }

        /// <summary>
        ///     Root path in which the collection resides. This is the top level directory
        ///     that the TreeWalker is acting upon.
        /// </summary>
        public string RootDirectoryPath { get; internal set; }

        /// <summary>
        ///     List of media file entries that belong to this collection. This is any media file,
        ///     metadata file, or sidecar file that share the same base name
        /// </summary>
        public List<MediaFileEntry> Entries { get; private set; } = new List<MediaFileEntry>();

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
        ///     Move the entire media file collection to a new destination
        /// </summary>
        /// <param name="newDirPath">
        ///     Full path to the new directory where the collection should be moved
        /// </param>
        /// <param name="createDir">
        ///     If true, the destination directory will be created if it does not yet exist.
        ///     Default: false.
        /// </param>
        public void MoveCollection(string newDirPath, bool createDir = false)
        {
            // if instructed to do so, create the directory if it does not already exist
            if (createDir && !Path.Exists(newDirPath))
                System.IO.Directory.CreateDirectory(newDirPath);

            // throw an error if the path exists but it is not a directory
            if (!System.IO.Directory.Exists(newDirPath))
                throw new DirectoryNotFoundException($"Unable to move collection, directory does not exist: {newDirPath}");

            var newDirInfo = new DirectoryInfo(newDirPath);

            var errors = new List<KeyValuePair<string, string>>();
            var actions = new List<Action>();

            foreach (var entry in this.Entries)
            {
                var destFilePath = Path.Combine(newDirInfo.FullName, entry.FileInfo.Name);

                if (File.Exists(destFilePath))
                    errors.Add(new KeyValuePair<string, string>("PathAlreadyExists", destFilePath));

                actions.Add(() =>
                {
                    entry.FileInfo.MoveTo(destFilePath, false);
                    entry.Path = entry.FileInfo.FullName;
                    entry.RelativeDepth = PathUtils.GetRelativeDepth(this.RootDirectoryPath, entry.FileInfo.Directory.FullName);
                });
            }

            // no errors encountered, apply the actions
            if (errors.Count() == 0)
                actions.ForEach(x => x());
            else
            {
                throw new Exception(
                    "Cannot move collection, the following errors were encountered:" + Environment.NewLine +
                    string.Join(Environment.NewLine, errors.Select(x => $"  [{x.Key}] {x.Value}"))
                );
            }

            this.Directory = newDirInfo;
        }

        /// <summary>
        ///     Custom string representation
        /// </summary>
        /// <returns>CollectionName[number of entries]</returns>
        public override string ToString()
            => $"{this.Name}[{this.Entries.Count()}]";

        /// <summary>
        ///     Given a full file path, this will extract the "collection name", that is the 
        ///     base filename without any extensions
        /// </summary>
        /// <param name="filePath">Full file path</param>
        /// <returns>Collection name</returns>
        public static string GetCollectionName(string filePath)
        {
            var cleanPath = PathUtils.CleanPath(filePath);
            var lastDirSeparator = cleanPath.LastIndexOf('/');
            var fileName = cleanPath.Substring(lastDirSeparator + 1);
            var firstDecimal = fileName.IndexOf('.');

            if (firstDecimal == -1)
                firstDecimal = fileName.Length;

            return fileName.Substring(0, firstDecimal);
        }

        public static MediaFileCollection FromFile(string filePath)
            => MediaFileCollection.FromFile(new FileInfo(filePath));

        public static MediaFileCollection FromFile(FileInfo fileInfo)
            => MediaFileCollection.FromFile(fileInfo, new string[] { });

        // TODO: Finish implementing
        //
        // still needs:
        //   - move MediaFileEntry setup code to static MediaFileEntry method
        //   - implement include, exclude logic in MediaFileEntry method
        //   - implement media type prioritization
        public static MediaFileCollection FromFile(FileInfo fileInfo, string[] includeExtensions)
        {
            var collectionName = MediaFileCollection.GetCollectionName(fileInfo.Name);
            var collection = new MediaFileCollection(collectionName, fileInfo.Directory.FullName);

            var filePaths = new List<string>();
            filePaths.Add(fileInfo.FullName);
            filePaths.AddRange(System.IO.Directory
                                        .GetFiles(fileInfo.Directory.FullName, $"{collectionName}*")
                                        .Where(x => !String.Equals(x, fileInfo.FullName)));

            foreach (var filePath in filePaths)
            {
                Console.WriteLine(filePath);

                var fileEntry = new MediaFileEntry()
                {
                    Name = Path.GetFileName(filePath),
                    Path = filePath,
                    RootPath = null, //this.RootDirectory,
                    RelativeDepth = 0, //PathUtils.GetRelativeDepth(this.RootDirectory, directory)
                    FileInfo = new FileInfo(filePath)
                };

                // // If any include paths were provided, lets make sure that this file
                // // path matches, otherwise we skip to the next file
                // if (this.IncludePaths.Count() > 0)
                // {
                //     if (!this.IncludePaths.Any(x => fileEntry.Path.StartsWith(x)))
                //     {
                //         // this is in case we change the ignore logic in the future, w can know for each
                //         // entry whether it was ignored or not
                //         fileEntry.Ignored = true;
                //         continue;
                //     }
                // }

                // Generate the FileInfo object for this entry. We do this after the "IncludePaths"
                // filter above for performance reasons


                // // lets make sure this file path doesn't match any provided excludes
                // if (this.ExcludePaths.Any(x => fileEntry.Path.Contains(x)))
                // {
                //     fileEntry.Ignored = true;
                //     continue;
                // }

                // If we are filtering by extension, lets make sure the file uses that extension
                if (includeExtensions.Count() > 0)
                {
                    if (!includeExtensions.Any(x => fileEntry.FileInfo.Extension.TrimStart('.') == x))
                    {
                        fileEntry.Ignored = true;
                        continue;
                    }
                }

                collection.Entries.Add(fileEntry);
            }

            return collection;
        }
    }
}