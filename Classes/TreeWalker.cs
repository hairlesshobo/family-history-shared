using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace FoxHollow.FHM.Shared.Classes
{
    public class TreeWalker
    {
        public string RootDirectory { get; private set; }
        public List<string> IncludePaths { get; set; }
        public List<string> ExcludePaths { get; set; }
        public List<string> Extensions { get; set; }

        public TreeWalker(string rootDir)
        {
            this.RootDirectory = rootDir;
            this.IncludePaths = new List<string>();
            this.ExcludePaths = new List<string>();
            this.Extensions = new List<string>();

            if (!Directory.Exists(this.RootDirectory))
                throw new DirectoryNotFoundException(this.RootDirectory);
        }

        public async IAsyncEnumerable<FileSystemEntry> StartScanAsync()
        {
            await foreach (var entry in ScanDirectoryAsync(this.RootDirectory))
                yield return entry;
        }

        private async IAsyncEnumerable<FileSystemEntry> ScanDirectoryAsync(string directory, int depth = 0)
        {
            List<FileSystemEntry> entries = new List<FileSystemEntry>();

            // to start off, we populate our list of 

            entries.AddRange(Directory.GetDirectories(directory)
                                      .Order()
                                      .Select(x => new FileSystemEntry(x, FileEntryType.Directory, depth)));

            entries.AddRange(Directory.GetFiles(directory)
                                      .Order()
                                      .Select(x => new FileSystemEntry(x, FileEntryType.File, depth)));

            foreach (var entry in entries)
            {
                // recurse into directories
                if (entry.EntryType == FileEntryType.Directory)
                    await foreach (var subEntry in ScanDirectoryAsync(entry.Path, depth + 1))
                        yield return subEntry;

                // scan through all files in this directory
                else
                {
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
        }

        public class FileSystemEntry
        {
            public string Path { get; private set; }
            public FileEntryType EntryType { get; private set; }
            public int RelativeDepth { get; private set; }
            public string RootPath { get; private set; }
            public FileInfo FileInfo { get; internal set; }

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