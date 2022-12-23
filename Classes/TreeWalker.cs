using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using FoxHollow.FHM.Shared.Exceptions;
using FoxHollow.FHM.Shared.Models;
using FoxHollow.FHM.Shared.Utilities;

namespace FoxHollow.FHM.Shared.Classes;

/// <summary>
///     Class used to walk a media tree when organizing it
/// </summary>
public class TreeWalker
{
    private IServiceProvider _services;
    private ILogger _logger;

    /// <summary>
    ///     Root Directory from which tree walking takes place
    /// </summary>
    public string RootDirectory { get; private set; }

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
    ///     Constructor that is called by the <see cref="TreeWalkerFactory" />
    /// </summary>
    /// <param name="services">DI service provider</param>
    /// <param name="rootDir">Root directory where walking should begin</param>
    internal TreeWalker(IServiceProvider services, string rootDir)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _logger = _services.GetRequiredService<ILogger<TreeWalker>>();

        if (string.IsNullOrWhiteSpace(rootDir))
            throw new ArgumentException($"'{nameof(rootDir)}' cannot be null or whitespace.", nameof(rootDir));
        
        if (!Directory.Exists(rootDir))
            throw new DirectoryNotFoundException(rootDir);


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
    private async IAsyncEnumerable<MediaFileCollection> ScanDirectoryAsync(string directory)
    {
        // First we recurse into sub directories before we begin to process files in this directory
        // we use .ToList() to ensure that the list of directories doesn't change while we are
        // looping.
        var dirPaths = Directory.GetDirectories(directory)
                                .Order()
                                .ToList();

        // iterate through each directory
        foreach (var dirPath in dirPaths)
        {
            // recurse into directories
            await foreach (var subCollection in ScanDirectoryAsync(dirPath))
                yield return subCollection;
        }


        // Pre-populate a list of files. For effeciency, you would think it
        // best to use IEnumerable but in this case, we need to pull the full list into
        // memory up front. This is done to allow renaming of the current directory
        // without processing duplicates or missing files, and also prevents exceptions
        // from being thown due to missing files if a directory is renamed from within
        // without special handling.
        var filePaths = Directory.GetFiles(directory)
                                            .Order()
                                            .ToList();
        // start with an empty list
        var collections = new List<MediaFileCollection>();
        var fileEntries = new List<MediaFileEntry>();

        // Loop through all files looking for exclusions, inclusions, collection
        // grouping and file processing
        foreach (var filePath in filePaths)
        {
            var fileEntry = new MediaFileEntry()
            {
                Path = filePath,
                RootPath = this.RootDirectory,
                RelativeDepth = PathUtils.GetRelativeDepth(this.RootDirectory, directory)
            };

            fileEntries.Add(fileEntry);

            var collectionName = this.GetCollectionName(fileEntry.Path);

            // look for an existing collection
            var collection = collections.FirstOrDefault(x => x.Name == collectionName);

            // it doesn't exist, so create it and add it to our list of collections
            if (collection == null)
            {
                collection = new MediaFileCollection(collectionName, directory);
                collections.Add(collection);
            }

            // Add the current file entry to the identified (or created) collection
            collection.Entries.Add(fileEntry);
            fileEntry.Collection = collection;


            // If any include paths were provided, lets make sure that this file
            // path matches, otherwise we skip to the next file
            if (this.IncludePaths.Count() > 0)
            {
                if (!this.IncludePaths.Any(x => fileEntry.Path.StartsWith(x)))
                {
                    // this is in case we change the ignore logic in the future, w can know for each
                    // entry whether it was ignored or not
                    fileEntry.Ignored = true;
                    continue;
                }
            }

            // Generate the FileInfo object for this entry. We do this after the "IncludePaths"
            // filter above for performance reasons
            fileEntry.FileInfo = new FileInfo(fileEntry.Path);

            // lets make sure this file path doesn't match any provided excludes
            if (this.ExcludePaths.Any(x => fileEntry.Path.Contains(x)))
            {
                fileEntry.Ignored = true;
                continue;
            }

            // If we are filtering by extension, lets make sure the file uses that extension
            if (this.IncludeExtensions.Count() > 0)
            {
                if (!this.IncludeExtensions.Any(x => fileEntry.FileInfo.Extension.TrimStart('.') == x))
                {
                    fileEntry.Ignored = true;
                    continue;
                }
            }
        }


        // Lambda that is passed on each yield that allows the current directory to be renamed
        // without throwing off iteration
        Action<MediaFileCollection, string> renameDirLambda = (collection, newName) =>
        {
            if (newName.Contains('/') || newName.Contains('\\'))
                throw new ArgumentException("When renaming a media directory, only the new name must be provided.. not a full path!");

            var oldPath = collection.Directory.FullName;
            var newPath = PathUtils.CleanPath(Path.Combine(collection.Directory.Parent.FullName, newName));

            if (File.Exists(newPath) || Directory.Exists(newPath))
                throw new PathAlreadyExistsException(newPath);

            _logger.LogInformation($"Renaming '{oldPath}' to '{newPath}'");

            Directory.Move(oldPath, newPath);

            var newDirInfo = new DirectoryInfo(newPath);

            fileEntries.ForEach(x => x.Path = x.Path.Replace(oldPath, newPath));
            collections.ForEach(x => x.Directory = newDirInfo);
        };

        Action<MediaFileCollection, string> moveCollectionLambda = (collection, newDirPath) =>
        {
            if (!Directory.Exists(newDirPath))
                throw new DirectoryNotFoundException($"Unable to move collection, directory does not exist: {newDirPath}");

            var newDirInfo = new DirectoryInfo(newDirPath);

            var errors = new List<KeyValuePair<string,string>>();
            var actions = new List<Action>();

            foreach (var entry in collection.Entries)
            {
                var destFilePath = Path.Combine(newDirInfo.FullName, entry.FileInfo.Name);

                if (File.Exists(destFilePath))
                    errors.Add(new KeyValuePair<string, string>("PathAlreadyExists", destFilePath));

                actions.Add(() =>
                {
                    entry.FileInfo.MoveTo(destFilePath, false);
                    entry.Path = entry.FileInfo.FullName;
                    entry.RelativeDepth = PathUtils.GetRelativeDepth(this.RootDirectory, entry.FileInfo.Directory.FullName);
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

            collection.Directory = newDirInfo;
        };


        // iterate through each file entry and yield execution back to the caller
        foreach (var collection in collections)
        {
            // If there are no non-ignored files in this collection, we do not
            // need to emit it back to the caller
            if (collection.Entries.Where(x => !x.Ignored).Count() == 0)
                continue;

            collection.RenameDir = (newName) => renameDirLambda(collection, newName);
            collection.MoveCollection = (newPath) => moveCollectionLambda(collection, newPath);

            yield return collection;
        }
    }

    /// <summary>
    ///     Given a full file path, this will extract the "collection name", that is the 
    ///     base filename without any extension
    /// </summary>
    /// <param name="filePath">Full file path</param>
    /// <returns>Collection name</returns>
    private string GetCollectionName(string filePath)
    {
        var cleanPath = PathUtils.CleanPath(filePath);
        var lastDirSeparator = cleanPath.LastIndexOf('/');
        var fileName = cleanPath.Substring(lastDirSeparator + 1);
        var firstDecimal = fileName.IndexOf('.');

        if (firstDecimal == -1)
            firstDecimal = fileName.Length;
        
        return fileName.Substring(0, firstDecimal);
    }
}