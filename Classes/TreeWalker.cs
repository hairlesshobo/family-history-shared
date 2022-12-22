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

            var fileEntries = Directory.GetFiles(directory)
                                       .Order()
                                       .Select(x => new FileSystemEntry(x, FileEntryType.File, depth))
                                       .ToList();


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

                // If any include paths were provided, lets make sure that this file
                // path matches, otherwise we skip to the next file
                if (this.IncludePaths.Count() > 0)
                    if (!this.IncludePaths.Any(x => entry.Path.StartsWith(x)))
                        continue;

                // lets make sure this file path doesn't match any provided excludes
                if (this.ExcludePaths.Any(x => entry.Path.Contains(x)))
                    continue;

                entry.FileInfo = new FileInfo(entry.Path);

                // If we are filtering by extension, lets make sure the file uses that extension
                if (this.Extensions.Count() > 0)
                    if (!this.Extensions.Any(x => entry.FileInfo.Extension.TrimStart('.') == x))
                        continue;

                yield return entry;
            }
        }

        public class FileSystemEntry
        {
            public string Path { get; internal set; }
            public FileEntryType EntryType { get; private set; }
            public int RelativeDepth { get; private set; }
            public string RootPath { get; private set; }
            public FileInfo FileInfo { get; internal set; }
            public Action<string> RenameDir { get; internal set; }

            public FileSystemEntry(string path, FileEntryType entryType, int depth)
            {
                this.Path = path;
                this.EntryType = entryType;
                this.RelativeDepth = depth;
            }
        }

        public enum FileEntryType
        {
            File,
            Directory
        }
    }
}