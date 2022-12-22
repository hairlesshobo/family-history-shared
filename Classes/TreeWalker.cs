using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using FoxHollow.FHM.Shared.Utilities;
using FoxHollow.FHM.Shared.Exceptions;

namespace FoxHollow.FHM.Shared.Classes
{
    public class TreeWalkerFactory
    {
        private IServiceProvider _services;

        public TreeWalkerFactory(IServiceProvider services)
        {
            _services = services;
        }

        public TreeWalker GetWalker(string rootDir)
            => new TreeWalker(_services, rootDir);
    }

    public class TreeWalker
    {
        private IServiceProvider _services;
        private ILogger _logger;

        public string RootDirectory { get; private set; }
        public List<string> IncludePaths { get; set; }
        public List<string> ExcludePaths { get; set; }
        public List<string> Extensions { get; set; }
        public bool GroupSidecars { get; set; } = true;

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
            this.Extensions = new List<string>();
        }

        public async IAsyncEnumerable<FileSystemEntry> StartScanAsync()
        {
            await foreach (var entry in ScanDirectoryAsync(this.RootDirectory))
                yield return entry;
        }

        private async IAsyncEnumerable<FileSystemEntry> ScanDirectoryAsync(string directory, int depth = 0)
        {
            // Pre-populate a list of directories and file. For effeciency, you would think it
            // best to use IEnumerable but in this case, we need to pull the full list into
            // memory up front. This is done to allow renaming of the current directory
            // without processing duplicates or missing files, and also prevents exceptions
            // from being thown due to missing files if a directory is renamed from within
            // without special handling.
            var dirEntries = Directory.GetDirectories(directory)
                                      .Order()
                                      .Select(x => new FileSystemEntry(x, FileEntryType.Directory, depth))
                                      .ToList();


            var fileEntriesFullList = Directory.GetFiles(directory)
                                               .Order()
                                               .Select(x => new FileSystemEntry(x, FileEntryType.File, depth))
                                               .ToList();

            var fileEntries = new List<FileSystemEntry>();

            foreach (var fileEntry in fileEntriesFullList)
            {
                // If any include paths were provided, lets make sure that this file
                // path matches, otherwise we skip to the next file
                if (this.IncludePaths.Count() > 0)
                    if (!this.IncludePaths.Any(x => fileEntry.Path.StartsWith(x)))
                        continue;

                // lets make sure this file path doesn't match any provided excludes
                if (this.ExcludePaths.Any(x => fileEntry.Path.Contains(x)))
                    continue;

                fileEntry.FileInfo = new FileInfo(fileEntry.Path);

                // If we are filtering by extension, lets make sure the file uses that extension
                if (this.Extensions.Count() > 0)
                    if (!this.Extensions.Any(x => fileEntry.FileInfo.Extension.TrimStart('.') == x))
                        continue;

                fileEntries.Add(fileEntry);
            }

            // Populate a list of entries that are not used
            var fileEntriesUnmatched = fileEntriesFullList.Where(x => !fileEntries.Contains(x)).ToList();

            foreach (var fileEntry in fileEntries)
            {
                fileEntry.RelatedFiles = fileEntriesUnmatched.Where(x => x.CollectionName == fileEntry.CollectionName)
                                                             .ToList();
            }            

            // iterate through each directory
            foreach (var dirEntry in dirEntries)
            {
                // recurse into directories
                if (dirEntry.EntryType == FileEntryType.Directory)
                    await foreach (var subEntry in ScanDirectoryAsync(dirEntry.Path, depth + 1))
                        yield return subEntry;
            }


            // Lambda that is passed on each yield that allows the current directory to be renamed
            // without throwing off iteration
            Action<FileSystemEntry, string> renameDirLambda = (entry, newName) =>
            {
                if (newName.Contains('/') || newName.Contains('\\'))
                    throw new ArgumentException("When renaming a media directory, only the new name must be provided.. not a full path!");

                var oldPath = entry.FileInfo.Directory.FullName;
                var newPath = PathUtils.CleanPath(Path.Combine(entry.FileInfo.Directory.Parent.FullName, newName));

                if (File.Exists(newPath) || Directory.Exists(newPath))
                    throw new PathAlreadyExistsException(newPath);

                _logger.LogInformation($"Renaming '{oldPath}' to '{newPath}'");

                Directory.Move(oldPath, newPath);
                fileEntries.ForEach(x => x.Path = x.Path.Replace(oldPath, newPath));
            };


            // iterate through each file entry
            foreach (var entry in fileEntries)
            {
                entry.RenameDir = (newName) => renameDirLambda(entry, newName);

                yield return entry;
            }
        }

        public class FileSystemEntry
        {
            public string Path { get; internal set; }
            public string CollectionName { get; private set; }
            public FileEntryType EntryType { get; private set; }
            public int RelativeDepth { get; private set; }
            public string RootPath { get; private set; }
            public FileInfo FileInfo { get; internal set; }
            public Action<string> RenameDir { get; internal set; } = (discard) => {};
            public List<FileSystemEntry> RelatedFiles { get; internal set; } = new List<FileSystemEntry>();

            public FileSystemEntry(string path, FileEntryType entryType, int depth)
            {
                this.Path = path;
                this.EntryType = entryType;
                this.RelativeDepth = depth;
                
                var cleanPath = PathUtils.CleanPath(path);
                var lastDirSeparator = cleanPath.LastIndexOf('/');
                var fileName = cleanPath.Substring(lastDirSeparator + 1);
                var firstDecimal = fileName.IndexOf('.');

                if (firstDecimal == -1)
                    firstDecimal = fileName.Length;
                
                this.CollectionName = fileName.Substring(0, firstDecimal);
            }

            public override string ToString()
                => System.IO.Path.GetFileName(this.Path);
        }

        public enum FileEntryType
        {
            File,
            Directory
        }
    }
}