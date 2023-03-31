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
using FoxHollow.FHM.Shared.Models;
using FoxHollow.FHM.Shared.Storage;
using FoxHollow.FHM.Shared.Utilities;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoxHollow.FHM.Shared.Classes;

/// <summary>
///     Class used to walk a media tree when organizing it
/// </summary>
public class MediaTreeWalker
{
    private IServiceProvider _services;
    private ILogger _logger;

    /// <summary>
    ///     Root Directory from which tree walking takes place
    /// </summary>
    public StorageDirectory RootDirectory { get; private set; }

    /// <summary>
    ///     If true, the tree walker will recurse into subdirectories. Default: true
    /// </summary>
    public bool Recursive { get; set; } = true;

    /// <summary>
    ///     If true, the tree walker will ignore files beginning with a "."
    /// </summary>
    public bool IgnoreHiddenFiles { get; set; } = true;

    /// <summary>
    ///     List of paths that are included. If any entries exist in this list,
    ///     then only paths that match an entry in this list will be processed.
    /// </summary>
    public List<string> IncludePaths { get; set; }

    /// <summary>
    ///     List of paths that are to be excluded from processing. Important note:
    ///     anything listed here that matches up with an existing collection will
    ///     still be included in the collection, but the Ignored flag will be set
    ///     to true. This is done so that sidecar files are still included with
    ///     valid media
    /// </summary>
    public List<string> ExcludePaths { get; set; }

    /// <summary>
    ///     If this list is populated, only collections that contain at least one
    ///     file ending in a provided extension will be returned to the caller.
    /// </summary>
    public List<string> IncludeExtensions { get; set; }


    /// <summary>
    ///     Constructor that is called by the <see cref="MediaTreeWalkerFactory" />
    /// </summary>
    /// <param name="services">DI service provider</param>
    /// <param name="rootDir">Root directory where walking should begin</param>
    internal MediaTreeWalker(IServiceProvider services, StorageDirectory rootDir)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _logger = _services.GetRequiredService<ILogger<MediaTreeWalker>>();

        if (rootDir == null)
            throw new ArgumentException($"'{nameof(rootDir)}' cannot be null", nameof(rootDir));

        // if (!Directory.Exists(rootDir))
        //     throw new DirectoryNotFoundException(rootDir);


        this.RootDirectory = rootDir;
        this.IncludePaths = new List<string>();
        this.ExcludePaths = new List<string>();
        this.IncludeExtensions = new List<string>();
    }


    /// <summary>
    ///     Start walking the media directory tree and grouping related files by their name.
    ///     This is used to help keep sidecar files and multiple copies of the same media file,
    ///     such as .CR2, .JPG, .xmp, etc files together in a single "collection".
    /// </summary>
    /// <returns>Collections of media files, grouped by the collection name.</returns>
    public async IAsyncEnumerable<MediaFileCollection> StartScanAsync()
    {
        await foreach (var collection in ScanDirectoryAsync(this.RootDirectory))
            yield return collection;
    }


    /// <summary>
    ///     Recursive function used for walking the media tree
    /// </summary>
    /// <param name="directory">Directory to scan</param>
    /// <returns>Collections of media files, group by the collection name</returns>
    private async IAsyncEnumerable<MediaFileCollection> ScanDirectoryAsync(StorageDirectory directory)
    {
        List<StorageDirectory> dirEntries = new List<StorageDirectory>();
        List<StorageFile> fileEntries = new List<StorageFile>();
        
        // TODO: Add WithCancellation() here?
        await foreach (var entry in directory.ListDirectoryAsync())
        {
            if (entry is StorageDirectory)
                dirEntries.Add((StorageDirectory)entry);
            else
                fileEntries.Add((StorageFile)entry);
        }
            
        if (this.Recursive)
        {
            // First we recurse into sub directories before we begin to process files in this directory
            // we use .ToList() to ensure that the list of directories doesn't change while we are
            // looping.
            var dirs = dirEntries.OrderBy(x => x.Name)
                                  .ToList();

            // iterate through each directory
            foreach (var dir in dirs)
            {
                // recurse into directories
                await foreach (var subCollection in ScanDirectoryAsync(dir))
                    yield return subCollection;
            }
        }

        // TODO: extract the below logic to a separate "GroupByCollection" method or similar that accepts a directory path

        // Pre-populate a list of files. For effeciency, you would think it
        // best to use IEnumerable but in this case, we need to pull the full list into
        // memory up front. This is done to allow renaming of the current directory
        // without processing duplicates or missing files, and also prevents exceptions
        // from being thown due to missing files if a directory is renamed from within
        // without special handling.
        var files = fileEntries.OrderBy(x => x.Name).ToList();

        if (this.IgnoreHiddenFiles)
            files = files.Where(x => !x.Name.StartsWith(".")).ToList();

        // start with an empty list
        var collections = new List<MediaFileCollection>();
        var mediaFileEntries = new List<MediaFileEntry>();

        // Loop through all files looking for exclusions, inclusions, collection
        // grouping and file processing
        foreach (var fileEntry in files)
        {
            var collectionName = MediaFileCollection.GetCollectionName(fileEntry.Path);

            // look for an existing collection
            var collection = collections.FirstOrDefault(x => x.Name == collectionName);

            // it doesn't exist, so create it and add it to our list of collections
            if (collection == null)
            {
                collection = new MediaFileCollection(collectionName, directory);
                collection.RootDirectoryPath = this.RootDirectory;
                collections.Add(collection);
            }

            // TODO: Move the below to shared logic in MediaFileEntry class

            var mediaFileEntry = new MediaFileEntry()
            {
                Name = Path.GetFileName(fileEntry),
                Path = fileEntry,
                RootPath = this.RootDirectory,
                RelativeDepth = PathUtils.GetRelativeDepth(this.RootDirectory, directory)
            };

            mediaFileEntries.Add(mediaFileEntry);

            // Add the current file entry to the identified (or created) collection
            collection.Entries.Add(mediaFileEntry);
            mediaFileEntry.Collection = collection;


            // If any include paths were provided, lets make sure that this file
            // path matches, otherwise we skip to the next file
            if (this.IncludePaths.Count() > 0)
            {
                if (!this.IncludePaths.Any(x => mediaFileEntry.Path.StartsWith(x)))
                {
                    // this is in case we change the ignore logic in the future, w can know for each
                    // entry whether it was ignored or not
                    mediaFileEntry.Ignored = true;
                    continue;
                }
            }

            // Generate the FileInfo object for this entry. We do this after the "IncludePaths"
            // filter above for performance reasons
            mediaFileEntry.FileEntry = new FileInfo(mediaFileEntry.Path);

            // lets make sure this file path doesn't match any provided excludes
            if (this.ExcludePaths.Any(x => mediaFileEntry.Path.Contains(x)))
            {
                mediaFileEntry.Ignored = true;
                continue;
            }

            // If we are filtering by extension, lets make sure the file uses that extension
            if (this.IncludeExtensions.Count() > 0)
            {
                if (!this.IncludeExtensions.Any(x => mediaFileEntry.FileEntry.Extension.TrimStart('.') == x))
                {
                    mediaFileEntry.Ignored = true;
                    continue;
                }
            }
        }

        //
        // iterate through each file entry and yield execution back to the caller
        foreach (var collection in collections)
        {
            // If there are no non-ignored files in this collection, we do not
            // need to emit it back to the caller
            if (collection.Entries.Where(x => !x.Ignored).Count() == 0)
                continue;

            yield return collection;
        }
    }

    /// <summary>
    ///     Function used to walk all raw home video scenes
    /// </summary>
    /// <returns>Collections of media files, group by the collection name</returns>
    public IEnumerable<MediaFileRawScene> FindRawVideoScenes()
    {
        // First we recurse into sub directories before we begin to process files in this directory
        // we use .ToList() to ensure that the list of directories doesn't change while we are
        // looping.
        var dirPaths = Directory.GetDirectories(this.RootDirectory, "*", new EnumerationOptions() { RecurseSubdirectories = true })
                                .Order()
                                .Where(x => PathUtils.GetRelativeDepth(this.RootDirectory, x) == 3)
                                .ToList();


        // If any include paths were configured, Lets filter to only scenes matching the includes 
        if (this.IncludePaths.Count() > 0)
            dirPaths = dirPaths.Where(x => this.IncludePaths.Any(y => x.StartsWith(y))).ToList();


        // filter out any configured excludes
        dirPaths = dirPaths.Where(x => !this.ExcludePaths.Any(y => x.Contains(y))).ToList();

        foreach (var dirPath in dirPaths)
            yield return new MediaFileRawScene(_services, this.RootDirectory, dirPath);
    }
}