using System;
using System.Collections.Generic;
using System.IO;
using Archiver.Utilities.Shared;

namespace Archiver.Classes.Tape
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
                _FullPath = Helpers.CleanPath(value);

                this.Name = Helpers.GetFileName(_FullPath);
                this.RelativePath = Helpers.GetRelativePath(value).TrimEnd('/');
            }
        }
        public string RelativePath { get; set; }
        public List<TapeSourceDirectory> Directories { get; set; }
        public List<TapeSourceFile> Files { get; set; }

        public TapeSourceDirectory(string sourcePath)
        {
            this.Files = new List<TapeSourceFile>();
            this.Directories = new List<TapeSourceDirectory>();

            this.FullPath = Helpers.CleanPath(sourcePath);

            if (!Directory.Exists(this.FullPath))
                throw new DirectoryNotFoundException($"Source directory does not exist: {sourcePath}");
        }

        public TapeSourceDirectory()
        {
            this.Files = new List<TapeSourceFile>();
        }
    }
}