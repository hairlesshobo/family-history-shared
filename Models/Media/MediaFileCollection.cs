//==========================================================================
//  Family History Manager - https://code.foxhollow.cc/fhm/
//
//  A cross platform tool to help organize and preserve all types
//  of family history
//==========================================================================
//  Copyright (c) 2020-2023 Steve Cross <flip@foxhollow.cc>
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using FoxHollow.FHM.Shared.Storage;
using FoxHollow.FHM.Shared.Utilities;

namespace FoxHollow.FHM.Shared.Models;

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
    ///     StorageDirectory object in which the media file collection resides
    /// </summary>
    [JsonIgnore]
    public StorageDirectory Directory { get; internal set; }

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
    /// <param name="directory">StorageDirectory in which the collection resides</param>
    internal MediaFileCollection(string collectionName, StorageDirectory directory)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
            throw new ArgumentException($"'{nameof(collectionName)}' cannot be null or whitespace.", nameof(collectionName));

        if (directory == null)
            throw new ArgumentException($"'{nameof(directory)}' cannot be null", nameof(directory));

        this.Name = collectionName;
        this.Directory = directory; //new DirectoryInfo(directory);
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
        var sp = this.Directory.Provider;

        StorageDirectory newDir;

        if (sp.Exists(newDirPath))
            newDir = sp.GetDirectory(newDirPath);
        else
        {
            // if instructed to do so, create the directory
            if (createDir)
                newDir = sp.CreateDirectory(newDirPath);
            else
                // TODO: Create StorageDirectoryNotFoundException()
                throw new DirectoryNotFoundException($"Unable to move collection, directory does not exist: {newDirPath}");
        }

        var errors = new List<KeyValuePair<string, string>>();
        var actions = new List<Action>();

        foreach (var entry in this.Entries)
        {
            // TODO: convert to PathUtils equivalent
            var destFilePath = Path.Combine(newDir.Path, entry.FileEntry.Name);

            if (sp.Exists(destFilePath))
                errors.Add(new KeyValuePair<string, string>("PathAlreadyExists", destFilePath));

            actions.Add(() =>
            {
                entry.FileEntry.MoveTo(destFilePath, false);
                entry.Path = entry.FileEntry.Path;
                entry.RelativeDepth = PathUtils.GetRelativeDepth(this.RootDirectoryPath, entry.FileEntry.Directory.Path);
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

        this.Directory = newDir;
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

    // TODO: use storage manager to pull a file with preceeding repo id?
    // public static MediaFileCollection FromFile(string filePath)
    //     => MediaFileCollection.FromFile(new FileInfo(filePath));

    public static MediaFileCollection FromFile(StorageFile file)
        => MediaFileCollection.FromFile(file, new string[] { });

    // TODO: Finish implementing
    //
    // still needs:
    //   - move MediaFileEntry setup code to static MediaFileEntry method
    //   - implement include, exclude logic in MediaFileEntry method
    //   - implement media type prioritization
    public static MediaFileCollection FromFile(StorageFile fileInfo, string[] includeExtensions)
    {
        var collectionName = MediaFileCollection.GetCollectionName(fileInfo.Name);
        var collection = new MediaFileCollection(collectionName, fileInfo.Directory);

        var fileEntries = new List<StorageFile>();
        fileEntries.Add(fileInfo);
        fileEntries.AddRange(fileInfo.Directory.ListFiles(collectionName)
                                             .Where(x => !String.Equals(x.Path, fileInfo.Path)));

        foreach (var fileEntry in fileEntries)
        {
            Console.WriteLine(fileEntry);

            var mediaFileEntry = new MediaFileEntry()
            {
                Name = fileEntry.Name,
                Path = fileEntry.Path,
                RootPath = null, //this.RootDirectory,
                RelativeDepth = 0, //PathUtils.GetRelativeDepth(this.RootDirectory, directory)
                FileEntry = fileEntry
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
                if (!includeExtensions.Any(x => mediaFileEntry.FileEntry.Extension.TrimStart('.') == x))
                {
                    mediaFileEntry.Ignored = true;
                    continue;
                }
            }

            collection.Entries.Add(mediaFileEntry);
        }

        return collection;
    }
}