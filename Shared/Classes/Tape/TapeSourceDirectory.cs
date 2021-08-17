using System;
using System.Collections.Generic;
using System.IO;
using Archiver.Shared.Utilities;
using Newtonsoft.Json;

namespace Archiver.Shared.Classes.Tape
{
    public class TapeSourceDirectory
    {
        private string _FullPath;

        public string Name { get; set; }

        public string FullPath { 
            get
            {
                return _FullPath;
            } 
            set
            {
                _FullPath = PathUtils.CleanPath(value);

                this.Name = PathUtils.GetFileName(_FullPath);
                this.RelativePath = PathUtils.GetRelativePath(value).TrimEnd('/');
            }
        }
        public string RelativePath { get; set; }
        public List<TapeSourceDirectory> Directories { get; set; }
        public List<TapeSourceFile> Files { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }
        [JsonIgnore]
        public TapeDetail Tape { get; set; } = null;

        public TapeSourceDirectory(string sourcePath)
        {
            this.Files = new List<TapeSourceFile>();
            this.Directories = new List<TapeSourceDirectory>();

            this.FullPath = PathUtils.CleanPath(sourcePath);

            if (!Directory.Exists(this.FullPath))
                throw new DirectoryNotFoundException($"Source directory does not exist: {sourcePath}");

            // we go ahead and pull the last write time when we add the directory, no
            // need for seperate scanning for this info
            FileInfo dirInfo = new FileInfo(this.FullPath);

            this.LastWriteTimeUtc = dirInfo.LastWriteTimeUtc;
        }

        public TapeSourceDirectory()
        {
            this.Files = new List<TapeSourceFile>();
        }
    }
}